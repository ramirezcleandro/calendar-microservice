namespace CalendarioEntregas.Infrastructure.Messaging.IntegrationEvents
{
    /// <summary>
    /// Evento de integración publicado a RabbitMQ cuando se modifica una dirección de entrega.
    /// </summary>
    public record DireccionModificadaIntegrationEvent(
        Guid CalendarioId,
        Guid DireccionId,
        DateOnly Fecha,
        string NuevaDireccion,
        double NuevaLatitud,
        double NuevaLongitud,
        DateTime OccurredOnUtc
    );
}
