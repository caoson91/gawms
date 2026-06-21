using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using UniformWMS.Application.Common.Mappings;
using UniformWMS.Application.Features.Auth;
using UniformWMS.Application.Features.IssuanceOrders;
using UniformWMS.Application.Features.UniformItems;

namespace UniformWMS.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddAutoMapper(cfg =>
        {
            cfg.AddProfile<MappingProfile>();
        });

        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUniformItemService, UniformItemService>();
        services.AddScoped<IIssuanceOrderService, IssuanceOrderService>();

        return services;
    }
}
