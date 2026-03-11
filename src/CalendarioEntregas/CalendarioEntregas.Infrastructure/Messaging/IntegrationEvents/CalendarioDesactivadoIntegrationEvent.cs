using Joselct.Communication.Contracts.Messages;

namespace CalendarioEntregas.Infrastructure.Messaging.IntegrationEvents
{
	public record CalendarioDesactivadoIntegrationEvent(
		Guid CalendarioId,
		Guid PacienteId
	) : IntegrationMessage;
}
