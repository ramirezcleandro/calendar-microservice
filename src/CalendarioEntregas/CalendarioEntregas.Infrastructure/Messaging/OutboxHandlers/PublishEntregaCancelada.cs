using CalendarioEntregas.Infrastructure.Messaging.IntegrationEvents;
using Joselct.Communication.Contracts.Services;
using Joselct.Outbox.MediatR.Notifications;
using MediatR;

namespace CalendarioEntregas.Infrastructure.Messaging.OutboxHandlers
{
	internal class PublishEntregaCancelada
		: INotificationHandler<OutboxMessageNotification<EntregaCanceladaIntegrationEvent>>
	{
		private readonly IExternalPublisher _publisher;

		public PublishEntregaCancelada(IExternalPublisher publisher) => _publisher = publisher;

		public Task Handle(OutboxMessageNotification<EntregaCanceladaIntegrationEvent> notification, CancellationToken ct)
			=> _publisher.PublishAsync(notification.Content, destination: "calendar", routingKey: "calendar.deliverycancelled", ct: ct);
	}
}
