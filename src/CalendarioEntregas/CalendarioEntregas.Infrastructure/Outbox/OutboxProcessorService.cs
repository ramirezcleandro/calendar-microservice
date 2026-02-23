using CalendarioEntregas.Domain.Eventos;
using CalendarioEntregas.Infrastructure.Messaging.IntegrationEvents;
using CalendarioEntregas.Infrastructure.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace CalendarioEntregas.Infrastructure.Outbox
{
    public class OutboxProcessorService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<OutboxProcessorService> _logger;
        private readonly TimeSpan _interval = TimeSpan.FromSeconds(5);

        public OutboxProcessorService(
            IServiceScopeFactory scopeFactory,
            ILogger<OutboxProcessorService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("OutboxProcessorService iniciado.");

            while (!stoppingToken.IsCancellationRequested)
            {
                await ProcessPendingMessages(stoppingToken);
                await Task.Delay(_interval, stoppingToken);
            }
        }

        private async Task ProcessPendingMessages(CancellationToken cancellationToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<CalendarioDbContext>();
            var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

            var pendingMessages = await dbContext.OutboxMessages
                .Where(m => m.ProcessedOnUtc == null)
                .OrderBy(m => m.OccurredOnUtc)
                .Take(20)
                .ToListAsync(cancellationToken);

            if (!pendingMessages.Any()) return;

            foreach (var message in pendingMessages)
            {
                try
                {
                    await PublishIntegrationEvent(publishEndpoint, message.Type, message.Payload, message.OccurredOnUtc, cancellationToken);

                    message.ProcessedOnUtc = DateTime.UtcNow;

                    _logger.LogInformation(
                        "Outbox: evento {Type} publicado a RabbitMQ (Id: {Id})",
                        message.Type, message.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Outbox: error publicando mensaje {Id} de tipo {Type}",
                        message.Id, message.Type);
                }
            }

            await dbContext.SaveChangesAsync(cancellationToken);
        }

        private static async Task PublishIntegrationEvent(
            IPublishEndpoint publishEndpoint,
            string eventType,
            string payload,
            DateTime occurredOnUtc,
            CancellationToken cancellationToken)
        {
            switch (eventType)
            {
                case nameof(CalendarioCreado):
                {
                    var domainEvent = JsonSerializer.Deserialize<CalendarioCreado>(payload)!;
                    await publishEndpoint.Publish(new CalendarioCreadoIntegrationEvent(
                        domainEvent.CalendarioId,
                        domainEvent.PacienteId,
                        domainEvent.PlanAlimenticioId,
                        domainEvent.FechaInicio,
                        domainEvent.FechaFin,
                        occurredOnUtc
                    ), cancellationToken);
                    break;
                }

                case nameof(DireccionAgregada):
                {
                    var domainEvent = JsonSerializer.Deserialize<DireccionAgregada>(payload)!;
                    await publishEndpoint.Publish(new DireccionAgregadaIntegrationEvent(
                        domainEvent.CalendarioId,
                        domainEvent.DireccionId,
                        domainEvent.Fecha,
                        domainEvent.Direccion,
                        domainEvent.Latitud,
                        domainEvent.Longitud,
                        occurredOnUtc
                    ), cancellationToken);
                    break;
                }

                case nameof(DireccionModificada):
                {
                    var domainEvent = JsonSerializer.Deserialize<DireccionModificada>(payload)!;
                    await publishEndpoint.Publish(new DireccionModificadaIntegrationEvent(
                        domainEvent.CalendarioId,
                        domainEvent.DireccionId,
                        domainEvent.Fecha,
                        domainEvent.NuevaDireccion,
                        domainEvent.NuevaLatitud,
                        domainEvent.NuevaLongitud,
                        occurredOnUtc
                    ), cancellationToken);
                    break;
                }

                case nameof(EntregaCancelada):
                {
                    var domainEvent = JsonSerializer.Deserialize<EntregaCancelada>(payload)!;
                    await publishEndpoint.Publish(new EntregaCanceladaIntegrationEvent(
                        domainEvent.CalendarioId,
                        domainEvent.DireccionId,
                        domainEvent.Fecha,
                        occurredOnUtc
                    ), cancellationToken);
                    break;
                }

                case nameof(EntregaReactivada):
                {
                    var domainEvent = JsonSerializer.Deserialize<EntregaReactivada>(payload)!;
                    await publishEndpoint.Publish(new EntregaReactivadaIntegrationEvent(
                        domainEvent.CalendarioId,
                        domainEvent.DireccionId,
                        domainEvent.Fecha,
                        occurredOnUtc
                    ), cancellationToken);
                    break;
                }

                case nameof(CalendarioDesactivado):
                {
                    var domainEvent = JsonSerializer.Deserialize<CalendarioDesactivado>(payload)!;
                    await publishEndpoint.Publish(new CalendarioDesactivadoIntegrationEvent(
                        domainEvent.CalendarioId,
                        domainEvent.PacienteId,
                        occurredOnUtc
                    ), cancellationToken);
                    break;
                }
            }
        }
    }
}
