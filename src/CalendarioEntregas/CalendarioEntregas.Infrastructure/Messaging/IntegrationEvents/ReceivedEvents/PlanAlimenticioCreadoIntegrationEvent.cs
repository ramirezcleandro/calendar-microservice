using Joselct.Communication.Contracts.Messages;

namespace CalendarioEntregas.Infrastructure.Messaging.IntegrationEvents.ReceivedEvents
{
	public record PlanAlimenticioCreadoIntegrationEvent(
		Guid PlanId,
		Guid PacienteId,
		Guid NutricionistaId,
		DateTime FechaInicio,
		int Duracion
	) : IntegrationMessage;
}
