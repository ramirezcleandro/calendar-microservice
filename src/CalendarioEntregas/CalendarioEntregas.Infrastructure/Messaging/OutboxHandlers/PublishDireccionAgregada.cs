using CalendarioEntregas.Infrastructure.Messaging.IntegrationEvents;
using Joselct.Communication.Contracts.Services;
using Joselct.Outbox.MediatR.Notifications;
using MediatR;

namespace CalendarioEntregas.Infrastructure.Messaging.OutboxHandlers
{
	internal class PublishDireccionAgregada
		: INotificationHandler<OutboxMessageNotification<DireccionAgregadaIntegrationEvent>>
	{
		private readonly IExternalPublisher _publisher;

		public PublishDireccionAgregada(IExternalPublisher publisher) => _publisher = publisher;

		public Task Handle(OutboxMessageNotification<DireccionAgregadaIntegrationEvent> notification, CancellationToken ct)
			=> _publisher.PublishAsync(notification.Content, destination: "calendar", routingKey: "calendar.addressadded", ct: ct);
	}
}
