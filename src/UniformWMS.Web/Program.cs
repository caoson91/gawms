using Blazored.LocalStorage;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Components.Authorization;
using UniformWMS.Web.Components;
using UniformWMS.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddBlazoredLocalStorage();

// Auth state provider
builder.Services.AddScoped<AuthenticationStateProvider, JwtAuthStateProvider>();
builder.Services.AddScoped<JwtAuthStateProvider>();

// API Base URL
var apiBaseUrl = builder.Configuration["ApiSettings:BaseUrl"] ?? "http://localhost:53475";

// Register all API services with typed HttpClient
builder.Services.AddHttpClient<AuthApiService>(c => c.BaseAddress = new Uri(apiBaseUrl));
builder.Services.AddHttpClient<CategoryApiService>(c => c.BaseAddress = new Uri(apiBaseUrl));
builder.Services.AddHttpClient<SupplierApiService>(c => c.BaseAddress = new Uri(apiBaseUrl));
builder.Services.AddHttpClient<EmployeeApiService>(c => c.BaseAddress = new Uri(apiBaseUrl));
builder.Services.AddHttpClient<UniformItemApiService>(c => c.BaseAddress = new Uri(apiBaseUrl));
builder.Services.AddHttpClient<IssuanceOrderApiService>(c => c.BaseAddress = new Uri(apiBaseUrl));
builder.Services.AddHttpClient<ReturnOrderApiService>(c => c.BaseAddress = new Uri(apiBaseUrl));
builder.Services.AddHttpClient<PurchaseOrderApiService>(c => c.BaseAddress = new Uri(apiBaseUrl));
builder.Services.AddHttpClient<UserApiService>(c => c.BaseAddress = new Uri(apiBaseUrl));
builder.Services.AddHttpClient<RoleApiService>(c => c.BaseAddress = new Uri(apiBaseUrl));

// when adding the HttpClient used by your Api services
builder.Services.AddTransient<RefreshTokenHandler>();
builder.Services.AddHttpClient("ApiClient", client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
})
.AddHttpMessageHandler<RefreshTokenHandler>();

// Example of registering typed client or resolving named client for ApiServiceBase
builder.Services.AddScoped(sp =>
{
    var factory = sp.GetRequiredService<IHttpClientFactory>();
    return factory.CreateClient("ApiClient");
});

// Blazor Server với custom JwtAuthStateProvider không dùng middleware auth pipeline.
// Vẫn cần đăng ký IAuthenticationService nhưng phải chỉ định scheme rõ để tránh
// lỗi "No DefaultChallengeScheme". Việc redirect login do <RedirectToLogin> đảm nhận.
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = "NoOp";
    options.DefaultChallengeScheme = "NoOp";
    options.DefaultForbidScheme = "NoOp";
})
.AddScheme<AuthenticationSchemeOptions, NoOpAuthenticationHandler>("NoOp", _ => { });

builder.Services.AddAuthorizationCore();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

//app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.UseAuthentication();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();