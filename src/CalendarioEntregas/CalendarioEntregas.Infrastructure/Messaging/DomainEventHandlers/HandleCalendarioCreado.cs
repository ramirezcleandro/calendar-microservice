using CalendarioEntregas.Domain.Eventos;
using CalendarioEntregas.Infrastructure.Messaging.IntegrationEvents;
using Joselct.Outbox.Core.Entities;
using Joselct.Outbox.Core.Interfaces;
using MediatR;

namespace CalendarioEntregas.Infrastructure.Messaging.DomainEventHandlers
{
	internal class HandleCalendarioCreado : INotificationHandler<CalendarioCreado>
	{
		private readonly IOutboxRepository _outboxRepository;

		public HandleCalendarioCreado(IOutboxRepository outboxRepository)
		{
			_outboxRepository = outboxRepository;
		}

		public async Task Handle(CalendarioCreado e, CancellationToken ct)
		{
			var integrationEvent = new CalendarioCreadoIntegrationEvent(
				e.CalendarioId, e.PacienteId, e.PlanAlimenticioId,
				e.FechaInicio, e.FechaFin);

			await _outboxRepository.AddAsync(
				OutboxMessage.CreateWithCurrentTrace(integrationEvent), ct);
		}
	}
}
