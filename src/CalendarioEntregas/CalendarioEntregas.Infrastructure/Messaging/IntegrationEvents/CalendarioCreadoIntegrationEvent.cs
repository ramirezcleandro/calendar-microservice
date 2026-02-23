namespace CalendarioEntregas.Infrastructure.Messaging.IntegrationEvents
{
    /// <summary>
    /// Evento de integración publicado a RabbitMQ cuando se crea un calendario de entregas.
    /// Otros microservicios (ej: Notificaciones) pueden suscribirse a este evento.
    /// </summary>
    public record CalendarioCreadoIntegrationEvent(
        Guid CalendarioId,
        Guid PacienteId,
        Guid PlanAlimenticioId,
        DateOnly FechaInicio,
        DateOnly FechaFin,
        DateTime OccurredOnUtc
    );
}
