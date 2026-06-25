using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using UniformWMS.Web.Models;

namespace UniformWMS.Web.Services;

// ─── Base API Service ─────────────────────────────────────────────────────────

public abstract class ApiServiceBase
{
    protected readonly HttpClient _http;
    protected readonly ILocalStorageService _storage;

    protected static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true
    };

    protected ApiServiceBase(HttpClient http, ILocalStorageService storage)
    {
        _http = http;
        _storage = storage;
    }

    protected async Task SetAuthHeaderAsync()
    {
        var token = await _storage.GetItemAsStringAsync("access_token");
        if (!string.IsNullOrEmpty(token))
            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
    }

    protected async Task<T?> GetAsync<T>(string url)
    {
        await SetAuthHeaderAsync();
        var response = await _http.GetFromJsonAsync<ApiResponse<T>>(url, JsonOpts);
        return response != null && response.Success ? response.Data : default;
    }

    protected async Task<ApiResponse<T>?> PostAsync<T>(string url, object body)
    {
        await SetAuthHeaderAsync();
        var res = await _http.PostAsJsonAsync(url, body);
        return await res.Content.ReadFromJsonAsync<ApiResponse<T>>(JsonOpts);
    }

    protected async Task<ApiResponse<T>?> PutAsync<T>(string url, object body)
    {
        await SetAuthHeaderAsync();
        var res = await _http.PutAsJsonAsync(url, body);
        return await res.Content.ReadFromJsonAsync<ApiResponse<T>>(JsonOpts);
    }

    protected async Task<ApiResponse?> DeleteAsync(string url)
    {
        await SetAuthHeaderAsync();
        var res = await _http.DeleteAsync(url);
        return await res.Content.ReadFromJsonAsync<ApiResponse>(JsonOpts);
    }
}

// ─── Auth Service ─────────────────────────────────────────────────────────────

public class AuthApiService : ApiServiceBase
{
    private readonly JwtSecurityTokenHandler _jwtHandler = new();

    public AuthApiService(HttpClient http, ILocalStorageService storage)
        : base(http, storage) { }

    public async Task<(bool Success, string? Error)> LoginAsync(LoginRequest request)
    {
        var res = await PostAsync<TokenResponse>("api/auth/login", request);
        if (res == null || !res.Success || res.Data == null)
            return (false, res?.Errors.FirstOrDefault() ?? "Đăng nhập thất bại.");

        await _storage.SetItemAsStringAsync("access_token", res.Data.AccessToken);
        await _storage.SetItemAsStringAsync("refresh_token", res.Data.RefreshToken);
        await _storage.SetItemAsync("token_data", res.Data);
        return (true, null);
    }

    public async Task LogoutAsync()
    {
        var refreshToken = await _storage.GetItemAsStringAsync("refresh_token");
        if (!string.IsNullOrEmpty(refreshToken))
        {
            try { await PostAsync<object>("api/auth/logout", refreshToken); } catch { }
        }
        await _storage.RemoveItemAsync("access_token");
        await _storage.RemoveItemAsync("refresh_token");
        await _storage.RemoveItemAsync("token_data");
    }

    public async Task<bool> TryRefreshAsync()
    {
        var accessToken = await _storage.GetItemAsStringAsync("access_token");
        var refreshToken = await _storage.GetItemAsStringAsync("refresh_token");
        if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(refreshToken))
            return false;

        var res = await PostAsync<TokenResponse>("api/auth/refresh",
            new { AccessToken = accessToken, RefreshToken = refreshToken });

        if (res == null || !res.Success || res.Data == null) return false;

        await _storage.SetItemAsStringAsync("access_token", res.Data.AccessToken);
        await _storage.SetItemAsStringAsync("refresh_token", res.Data.RefreshToken);
        await _storage.SetItemAsync("token_data", res.Data);
        return true;
    }

    public async Task<TokenResponse?> GetStoredTokenAsync()
        => await _storage.GetItemAsync<TokenResponse>("token_data");

    public async Task<bool> ChangePasswordAsync(ChangePasswordRequest request)
    {
        var res = await PostAsync<object>("api/auth/change-password", request);
        return res?.Success ?? false;
    }
}

public class RefreshTokenHandler : DelegatingHandler
{
    private readonly ILocalStorageService _storage;
    private readonly AuthApiService _authService;
    private readonly SemaphoreSlim _refreshLock = new(1, 1);

    public RefreshTokenHandler(ILocalStorageService storage, AuthApiService authService)
    {
        _storage = storage;
        _authService = authService;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // Attach access token
        var token = await _storage.GetItemAsStringAsync("access_token");
        if (!string.IsNullOrEmpty(token))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await base.SendAsync(request, cancellationToken);

        if (response.StatusCode != HttpStatusCode.Unauthorized)
            return response;

        // Try refresh (single concurrent refresh)
        await _refreshLock.WaitAsync(cancellationToken);
        try
        {
            // Check if another caller already refreshed the token
            var current = await _storage.GetItemAsStringAsync("access_token");
            if (current == token)
            {
                var refreshed = await _authService.TryRefreshAsync();
                if (!refreshed)
                    return response; // Refresh failed, return original 401
            }

            // Retry original request with new token
            var newToken = await _storage.GetItemAsStringAsync("access_token");
            if (!string.IsNullOrEmpty(newToken))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", newToken);

                // clone request for retry if needed (HttpRequestMessage can be sent only once)
                var newRequest = await CloneHttpRequestMessageAsync(request);
                return await base.SendAsync(newRequest, cancellationToken);
            }

            return response;
        }
        finally
        {
            _refreshLock.Release();
        }
    }

    private static async Task<HttpRequestMessage> CloneHttpRequestMessageAsync(HttpRequestMessage req)
    {
        var clone = new HttpRequestMessage(req.Method, req.RequestUri)
        {
            Version = req.Version
        };

        // copy content (if any)
        if (req.Content != null)
        {
            var ms = new System.IO.MemoryStream();
            await req.Content.CopyToAsync(ms);
            ms.Position = 0;
            clone.Content = new StreamContent(ms);

            foreach (var h in req.Content.Headers)
                clone.Content.Headers.TryAddWithoutValidation(h.Key, h.Value);
        }

        foreach (var header in req.Headers)
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);

        foreach (var prop in req.Options)
            clone.Options.Set(new System.Net.Http.HttpRequestOptionsKey<object>(prop.Key), prop.Value);

        return clone;
    }
}

// ─── Custom AuthStateProvider ─────────────────────────────────────────────────

public class JwtAuthStateProvider : AuthenticationStateProvider
{
    private readonly ILocalStorageService _storage;
    private readonly AuthApiService _authService;

    private static readonly AuthenticationState Anonymous =
        new(new ClaimsPrincipal(new ClaimsIdentity()));

    // Cache state sau khi đã load lần đầu thành công
    private AuthenticationState? _cachedState;
    private bool _initialized = false;

    public JwtAuthStateProvider(ILocalStorageService storage, AuthApiService authService)
    {
        _storage = storage;
        _authService = authService;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        // Trả về cached state nếu đã khởi tạo (tránh gọi JS nhiều lần)
        if (_initialized && _cachedState != null)
            return _cachedState;

        try
        {
            var token = await _storage.GetItemAsStringAsync("access_token");

            if (string.IsNullOrEmpty(token))
            {
                _initialized = true;
                _cachedState = Anonymous;
                return Anonymous;
            }

            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);

            // Nếu token sắp hết hạn (< 1 phút) thì thử refresh
            if (jwt.ValidTo < DateTime.UtcNow.AddMinutes(1))
            {
                var refreshed = await _authService.TryRefreshAsync();
                if (!refreshed)
                {
                    _initialized = true;
                    _cachedState = Anonymous;
                    return Anonymous;
                }
                token = await _storage.GetItemAsStringAsync("access_token");
                if (string.IsNullOrEmpty(token))
                {
                    _initialized = true;
                    _cachedState = Anonymous;
                    return Anonymous;
                }
                jwt = handler.ReadJwtToken(token);
            }

            var claims = jwt.Claims.ToList();
            var identity = new ClaimsIdentity(claims, "jwt");
            var state = new AuthenticationState(new ClaimsPrincipal(identity));

            _initialized = true;
            _cachedState = state;
            return state;
        }
        catch
        {
            // JS Interop chưa sẵn sàng (prerender) hoặc token lỗi
            // → Trả về Anonymous nhưng KHÔNG set _initialized
            // để lần sau khi JS sẵn sàng sẽ thử lại
            return Anonymous;
        }
    }

    public void NotifyAuthStateChanged()
    {
        // Reset cache để lần sau đọc lại từ storage
        _initialized = false;
        _cachedState = null;
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public void MarkAsLoggedOut()
    {
        _initialized = true;
        _cachedState = Anonymous;
        NotifyAuthenticationStateChanged(Task.FromResult(Anonymous));
    }
}