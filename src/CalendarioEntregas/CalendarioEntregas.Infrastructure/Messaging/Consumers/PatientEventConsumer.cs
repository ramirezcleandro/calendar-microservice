using CalendarioEntregas.Infrastructure.Messaging.IntegrationEvents.ReceivedEvents;
using Joselct.Communication.Contracts.Services;
using Microsoft.Extensions.Logging;

namespace CalendarioEntregas.Infrastructure.Messaging.Consumers
{
    public class PatientEventConsumer : IIntegrationMessageConsumer<PatientIntegrationEvent>
    {
        private readonly ILogger<PatientEventConsumer> _logger;

        public PatientEventConsumer(ILogger<PatientEventConsumer> logger)
        {
            _logger = logger;
        }

        public Task HandleAsync(PatientIntegrationEvent message, CancellationToken ct = default)
        {
            _logger.LogInformation("Evento de paciente recibido: PatientId={PatientId}", message.PatientId);
            return Task.CompletedTask;
        }
    }
}
