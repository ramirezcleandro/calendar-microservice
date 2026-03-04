using Joselct.Communication.Contracts.Messages;

namespace CalendarioEntregas.Infrastructure.Messaging.IntegrationEvents.ReceivedEvents
{
    public record PatientIntegrationEvent(
        Guid PatientId,
        string FullName,
        string? PhoneNumber
    ) : IntegrationMessage;
}
