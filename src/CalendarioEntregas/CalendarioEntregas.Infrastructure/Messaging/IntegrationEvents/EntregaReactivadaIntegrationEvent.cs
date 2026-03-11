using Joselct.Communication.Contracts.Messages;

namespace CalendarioEntregas.Infrastructure.Messaging.IntegrationEvents
{
	public record EntregaReactivadaIntegrationEvent(
		Guid CalendarioId,
		Guid DireccionId,
		DateOnly Fecha
	) : IntegrationMessage;
}
