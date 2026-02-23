namespace CalendarioEntregas.Infrastructure.Messaging.IntegrationEvents
{
    /// <summary>
    /// Evento de integración publicado a RabbitMQ cuando se reactiva una entrega cancelada.
    /// Logística debe reactivar la orden de entrega correspondiente.
    /// </summary>
    public record EntregaReactivadaIntegrationEvent(
        Guid CalendarioId,
        Guid DireccionId,
        DateOnly Fecha,
        DateTime OccurredOnUtc
    );
}
