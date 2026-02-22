using CalendarioEntregas.Domain.Repositories;
using CalendarioEntregas.Domain.Abstractions;
using CalendarioEntregas.Infrastructure.Persistence;
using CalendarioEntregas.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;

namespace CalendarioEntregas.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("CalendarioDatabase")
                ?? throw new InvalidOperationException("Connection string 'CalendarioDatabase' no encontrada.");

            // Política de reintentos con backoff exponencial
            var retryPolicy = Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(
                    retryCount: 5,
                    sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                    onRetry: (exception, timespan, retryCount, context) =>
                    {
                        Console.WriteLine($"Retry {retryCount} después de {timespan.TotalSeconds}s: {exception.Message}");
                    }
                );

            services.AddDbContext<CalendarioDbContext>(options =>
                options.UseNpgsql(connectionString)
            );

            services.AddScoped<ICalendarioEntregaRepository, CalendarioEntregaRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            return services;
        }
    }
}
