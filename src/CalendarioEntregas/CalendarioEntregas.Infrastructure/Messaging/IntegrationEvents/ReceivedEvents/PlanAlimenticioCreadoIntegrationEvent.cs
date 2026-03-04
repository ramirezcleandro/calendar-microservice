namespace CalendarioEntregas.Infrastructure.Messaging.IntegrationEvents.ReceivedEvents
{
    /// <summary>
    /// Contrato del evento publicado por el microservicio de Planes Alimenticios.
    /// Al recibir este evento, se crea automáticamente el calendario de entregas.
    /// IMPORTANTE: Este record debe coincidir exactamente con el PlanMessage del otro microservicio.
    /// </summary>
    public record PlanAlimenticioCreadoIntegrationEvent(
        Guid PlanId,
        Guid PacienteId,
        Guid NutricionistaId,
        DateTime FechaInicio,
        int Duracion
    );
}
