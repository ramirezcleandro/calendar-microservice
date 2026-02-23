namespace CalendarioEntregas.Infrastructure.Messaging.IntegrationEvents
{
    /// <summary>
    /// Evento de integración publicado a RabbitMQ cuando se agrega una dirección de entrega.
    /// </summary>
    public record DireccionAgregadaIntegrationEvent(
        Guid CalendarioId,
        Guid DireccionId,
        DateOnly Fecha,
        string Direccion,
        double Latitud,
        double Longitud,
        DateTime OccurredOnUtc
    );
}
