using Joselct.Communication.Contracts.Messages;

namespace CalendarioEntregas.Infrastructure.Messaging.IntegrationEvents.ReceivedEvents
{
	// RequiereCatering tiene default true para que los mensajes emitidos por versiones
	// anteriores del productor (sin el campo) mantengan el comportamiento original.
	public record PlanAlimenticioCreadoIntegrationEvent(
		Guid PlanId,
		Guid PacienteId,
		Guid NutricionistaId,
		DateTime FechaInicio,
		int Duracion,
		bool RequiereCatering = true
	) : IntegrationMessage;
}
