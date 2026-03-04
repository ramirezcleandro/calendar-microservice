using CalendarioEntregas.Domain.Repositories;
using CalendarioEntregas.Domain.Abstractions;
using CalendarioEntregas.Infrastructure.Messaging.Consumers;
using CalendarioEntregas.Infrastructure.Messaging.IntegrationEvents;
using CalendarioEntregas.Infrastructure.Outbox;
using CalendarioEntregas.Infrastructure.Persistence;
using CalendarioEntregas.Infrastructure.Repositories;
using MassTransit;
using RabbitMQ.Client;
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

            // MassTransit con RabbitMQ
            services.AddMassTransit(x =>
            {
                // Consumer: escucha eventos de otros microservicios
                x.AddConsumer<PlanAlimenticioCreadoConsumer>();

                x.UsingRabbitMq((ctx, cfg) =>
                {
                    cfg.Host(
                        configuration["RabbitMq:Host"] ?? "localhost",
                        configuration["RabbitMq:VirtualHost"] ?? "/",
                        h =>
                        {
                            h.Username(configuration["RabbitMq:Username"] ?? "admin");
                            h.Password(configuration["RabbitMq:Password"] ?? "admin");
                        });

                    // Convención de exchanges: exchange único "calendario", tipo topic, durable
                    cfg.Message<CalendarioCreadoIntegrationEvent>(x => x.SetEntityName("calendario"));
                    cfg.Publish<CalendarioCreadoIntegrationEvent>(x => { x.ExchangeType = ExchangeType.Topic; x.Durable = true; });

                    cfg.Message<DireccionAgregadaIntegrationEvent>(x => x.SetEntityName("calendario"));
                    cfg.Publish<DireccionAgregadaIntegrationEvent>(x => { x.ExchangeType = ExchangeType.Topic; x.Durable = true; });

                    cfg.Message<DireccionModificadaIntegrationEvent>(x => x.SetEntityName("calendario"));
                    cfg.Publish<DireccionModificadaIntegrationEvent>(x => { x.ExchangeType = ExchangeType.Topic; x.Durable = true; });

                    cfg.Message<EntregaCanceladaIntegrationEvent>(x => x.SetEntityName("calendario"));
                    cfg.Publish<EntregaCanceladaIntegrationEvent>(x => { x.ExchangeType = ExchangeType.Topic; x.Durable = true; });

                    cfg.Message<EntregaReactivadaIntegrationEvent>(x => x.SetEntityName("calendario"));
                    cfg.Publish<EntregaReactivadaIntegrationEvent>(x => { x.ExchangeType = ExchangeType.Topic; x.Durable = true; });

                    cfg.Message<CalendarioDesactivadoIntegrationEvent>(x => x.SetEntityName("calendario"));
                    cfg.Publish<CalendarioDesactivadoIntegrationEvent>(x => { x.ExchangeType = ExchangeType.Topic; x.Durable = true; });

                    cfg.ConfigureEndpoints(ctx);
                });
            });

            services.AddHostedService<OutboxProcessorService>();

            return services;
        }
    }
}
