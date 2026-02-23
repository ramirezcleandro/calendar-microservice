namespace CalendarioEntregas.Infrastructure.Messaging.IntegrationEvents
{
    /// <summary>
    /// Evento de integración publicado a RabbitMQ cuando se desactiva un calendario.
    /// Logística debe cerrar todas las órdenes de entrega pendientes del paciente.
    /// </summary>
    public record CalendarioDesactivadoIntegrationEvent(
        Guid CalendarioId,
        Guid PacienteId,
        DateTime OccurredOnUtc
    );
}
