using CalendarioEntregas.Infrastructure.Messaging.IntegrationEvents;
using Joselct.Communication.Contracts.Services;
using Joselct.Outbox.MediatR.Notifications;
using MediatR;

namespace CalendarioEntregas.Infrastructure.Messaging.OutboxHandlers
{
	internal class PublishDireccionModificada
		: INotificationHandler<OutboxMessageNotification<DireccionModificadaIntegrationEvent>>
	{
		private readonly IExternalPublisher _publisher;

		public PublishDireccionModificada(IExternalPublisher publisher) => _publisher = publisher;

		public Task Handle(OutboxMessageNotification<DireccionModificadaIntegrationEvent> notification, CancellationToken ct)
			=> _publisher.PublishAsync(notification.Content, destination: "calendar", routingKey: "calendar.addressupdated", ct: ct);
	}
}
