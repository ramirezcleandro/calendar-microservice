using CalendarioEntregas.Domain.Eventos;
using CalendarioEntregas.Infrastructure.Messaging.IntegrationEvents;
using Joselct.Outbox.Core.Entities;
using Joselct.Outbox.Core.Interfaces;
using MediatR;

namespace CalendarioEntregas.Infrastructure.Messaging.DomainEventHandlers
{
	internal class HandleCalendarioDesactivado : INotificationHandler<CalendarioDesactivado>
	{
		private readonly IOutboxRepository _outboxRepository;

		public HandleCalendarioDesactivado(IOutboxRepository outboxRepository)
		{
			_outboxRepository = outboxRepository;
		}

		public async Task Handle(CalendarioDesactivado e, CancellationToken ct)
		{
			var integrationEvent = new CalendarioDesactivadoIntegrationEvent(
				e.CalendarioId, e.PacienteId);

			await _outboxRepository.AddAsync(
				OutboxMessage.CreateWithCurrentTrace(integrationEvent), ct);
		}
	}
}
