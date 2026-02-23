namespace CalendarioEntregas.Infrastructure.Messaging.IntegrationEvents
{
    /// <summary>
    /// Evento de integración publicado a RabbitMQ cuando se cancela una entrega.
    /// </summary>
    public record EntregaCanceladaIntegrationEvent(
        Guid CalendarioId,
        Guid DireccionId,
        DateOnly Fecha,
        DateTime OccurredOnUtc
    );
}
