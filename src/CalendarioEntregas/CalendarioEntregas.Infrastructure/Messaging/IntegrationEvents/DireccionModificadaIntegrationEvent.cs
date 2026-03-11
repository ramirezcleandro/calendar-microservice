using Joselct.Communication.Contracts.Messages;

namespace CalendarioEntregas.Infrastructure.Messaging.IntegrationEvents
{
	public record DireccionModificadaIntegrationEvent(
		Guid CalendarioId,
		Guid DireccionId,
		DateOnly Fecha,
		string NuevaDireccion,
		double NuevaLatitud,
		double NuevaLongitud
	) : IntegrationMessage;
}
