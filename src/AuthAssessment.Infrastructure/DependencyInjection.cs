using AuthAssessment.Application.Interfaces;
using AuthAssessment.Application.UseCases;
using AuthAssessment.Infrastructure.Persistence;
using AuthAssessment.Infrastructure.Repositories;
using AuthAssessment.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AuthAssessment.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<ITokenService, JwtTokenService>();
        services.AddScoped<IPasswordHasher, BCryptPasswordHasher>();

        services.AddScoped<AuthUseCase>();
        services.AddScoped<TransactionUseCase>();
        services.AddScoped<UserUseCase>();

        return services;
    }
}
