using CalendarioEntregas.Infrastructure.Messaging.IntegrationEvents;
using Joselct.Communication.Contracts.Services;
using Joselct.Outbox.MediatR.Notifications;
using MediatR;

namespace CalendarioEntregas.Infrastructure.Messaging.OutboxHandlers
{
	internal class PublishCalendarioCreado
		: INotificationHandler<OutboxMessageNotification<CalendarioCreadoIntegrationEvent>>
	{
		private readonly IExternalPublisher _publisher;

		public PublishCalendarioCreado(IExternalPublisher publisher) => _publisher = publisher;

		public Task Handle(OutboxMessageNotification<CalendarioCreadoIntegrationEvent> notification, CancellationToken ct)
			=> _publisher.PublishAsync(notification.Content, destination: "calendar", routingKey: "calendar.created", ct: ct);
	}
}
