using Joselct.Communication.Contracts.Messages;

namespace CalendarioEntregas.Infrastructure.Messaging.IntegrationEvents
{
    public record CalendarioCreadoIntegrationEvent(
        Guid CalendarioId,
        Guid PacienteId,
        Guid PlanAlimenticioId,
        DateOnly FechaInicio,
        DateOnly FechaFin
    ) : IntegrationMessage;
}
