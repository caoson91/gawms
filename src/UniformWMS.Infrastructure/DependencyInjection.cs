using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UniformWMS.Application.Common.Interfaces;
using UniformWMS.Domain.Interfaces;
using UniformWMS.Infrastructure.Data;
using UniformWMS.Infrastructure.Data.Seeders;
using UniformWMS.Infrastructure.Repositories;
using UniformWMS.Infrastructure.Services;

namespace UniformWMS.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {

        services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(
                config.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly("UniformWMS.Infrastructure")));

        services.AddHttpContextAccessor();

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<ICodeGenerator, CodeGenerator>();
        services.AddScoped<DatabaseSeeder>();

        return services;
    }
}
