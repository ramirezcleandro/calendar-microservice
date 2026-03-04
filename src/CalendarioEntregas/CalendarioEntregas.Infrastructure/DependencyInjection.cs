using CalendarioEntregas.Domain.Repositories;
using CalendarioEntregas.Domain.Abstractions;
using CalendarioEntregas.Infrastructure.Messaging.Consumers;
using CalendarioEntregas.Infrastructure.Messaging.IntegrationEvents.ReceivedEvents;
using CalendarioEntregas.Infrastructure.Persistence;
using CalendarioEntregas.Infrastructure.Repositories;
using Joselct.Communication.RabbitMQ.Extensions;
using Joselct.Outbox.EFCore.Extensions;
using Joselct.Outbox.MediatR.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CalendarioEntregas.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("CalendarioDatabase")
                ?? throw new InvalidOperationException("Connection string 'CalendarioDatabase' no encontrada.");

            services.AddDbContext<CalendarioDbContext>(options =>
                options.UseNpgsql(connectionString));

            services.AddScoped<ICalendarioEntregaRepository, CalendarioEntregaRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Outbox (Joselct)
            services.AddOutboxEfCore<CalendarioDbContext>();

            // RabbitMQ (Joselct) — publisher + connection manager
            services.AddRabbitMq(configuration);

            // Consumer: escucha meal-plan.created desde ms-calendar-queue
            services.AddRabbitMqConsumer<PlanAlimenticioCreadoIntegrationEvent, PlanAlimenticioCreadoConsumer>(
                queueName: "ms-calendar-queue",
                exchangeName: "meal-plans",
                routingKey: "meal-plan.created",
                declareQueue: false);

            // Consumer: escucha patient.* desde ms-calendar-queue (requerido por el binding de la infra)
            services.AddRabbitMqConsumer<PatientIntegrationEvent, PatientEventConsumer>(
                queueName: "ms-calendar-queue",
                exchangeName: "patients",
                routingKey: "patient.*",
                declareQueue: false);

            // Worker del outbox (polling y dispatch vía MediatR)
            services.AddOutboxWorker();

            return services;
        }
    }
}
