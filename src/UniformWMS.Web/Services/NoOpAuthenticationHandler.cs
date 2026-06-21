using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace UniformWMS.Web.Services;

/// <summary>
/// Handler xác thực "rỗng" dành cho Blazor Server.
/// Blazor Server quản lý auth state qua JwtAuthStateProvider + LocalStorage,
/// không qua HTTP middleware pipeline. Handler này chỉ để thỏa mãn yêu cầu
/// IAuthenticationService của ASP.NET Core DI — không làm gì thực sự.
/// Việc redirect login do component <RedirectToLogin> đảm nhận.
/// </summary>
public class NoOpAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public NoOpAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder) { }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        => Task.FromResult(AuthenticateResult.NoResult());
}