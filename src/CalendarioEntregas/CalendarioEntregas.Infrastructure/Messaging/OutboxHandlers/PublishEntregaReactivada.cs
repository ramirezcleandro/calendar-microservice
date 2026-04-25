using CalendarioEntregas.Infrastructure.Messaging.IntegrationEvents;
using Joselct.Communication.Contracts.Services;
using Joselct.Outbox.MediatR.Notifications;
using MediatR;

namespace CalendarioEntregas.Infrastructure.Messaging.OutboxHandlers
{
	internal class PublishEntregaReactivada
		: INotificationHandler<OutboxMessageNotification<EntregaReactivadaIntegrationEvent>>
	{
		private readonly IExternalPublisher _publisher;

		public PublishEntregaReactivada(IExternalPublisher publisher) => _publisher = publisher;

		public Task Handle(OutboxMessageNotification<EntregaReactivadaIntegrationEvent> notification, CancellationToken ct)
			=> _publisher.PublishAsync(notification.Content, destination: "calendar", routingKey: "calendar.deliveryreactivated", ct: ct);
	}
}
