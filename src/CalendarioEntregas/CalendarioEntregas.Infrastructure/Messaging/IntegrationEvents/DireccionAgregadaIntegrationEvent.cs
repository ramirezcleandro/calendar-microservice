using Joselct.Communication.Contracts.Messages;

namespace CalendarioEntregas.Infrastructure.Messaging.IntegrationEvents
{
    public record DireccionAgregadaIntegrationEvent(
        Guid CalendarioId,
        Guid DireccionId,
        DateOnly Fecha,
        string Direccion,
        double Latitud,
        double Longitud
    ) : IntegrationMessage;
}
