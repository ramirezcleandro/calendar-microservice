namespace CalendarioEntregas.Infrastructure.Messaging.IntegrationEvents.ReceivedEvents
{
    /// <summary>
    /// Contrato del evento publicado por el microservicio de Planes Alimenticios.
    /// Al recibir este evento, se crea automáticamente el calendario de entregas.
    /// IMPORTANTE: Este record debe coincidir exactamente con el que publica el otro microservicio.
    /// </summary>
    public record PlanAlimenticioCreadoIntegrationEvent(
        Guid PlanAlimenticioId,
        Guid PacienteId,
        DateOnly FechaInicio,
        DateOnly FechaFin,
        DateTime OccurredOnUtc
    );
}
