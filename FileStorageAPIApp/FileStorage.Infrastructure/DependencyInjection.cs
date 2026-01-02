using FileStorage.Application.Interfaces;
using FileStorage.Infrastructure.AppDbContext;
using FileStorage.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
namespace FileStorage.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
        {
            services.AddScoped<IJwtTokenService, JwtTokenService>();

            // register DbContext with retry logic for transient failures
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"),
                    sql => 
                    {
                        sql.MigrationsAssembly("FileStorage.Infrastructure");
                        sql.EnableRetryOnFailure(
                            maxRetryCount: 5,
                            maxRetryDelay: TimeSpan.FromSeconds(30),
                            errorNumbersToAdd: null);
                    }));

            return services;
        }
    }
}
