using CalendarioEntregas.Infrastructure.Messaging.IntegrationEvents;
using Joselct.Communication.Contracts.Services;
using Joselct.Outbox.MediatR.Notifications;
using MediatR;

namespace CalendarioEntregas.Infrastructure.Messaging.OutboxHandlers
{
	internal class PublishCalendarioDesactivado
		: INotificationHandler<OutboxMessageNotification<CalendarioDesactivadoIntegrationEvent>>
	{
		private readonly IExternalPublisher _publisher;

		public PublishCalendarioDesactivado(IExternalPublisher publisher) => _publisher = publisher;

		public Task Handle(OutboxMessageNotification<CalendarioDesactivadoIntegrationEvent> notification, CancellationToken ct)
			=> _publisher.PublishAsync(notification.Content, destination: "calendar", routingKey: "calendar.deactivated", ct: ct);
	}
}
