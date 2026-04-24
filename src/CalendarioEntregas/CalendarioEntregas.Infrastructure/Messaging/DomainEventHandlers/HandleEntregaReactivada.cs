using CalendarioEntregas.Domain.Eventos;
using CalendarioEntregas.Infrastructure.Messaging.IntegrationEvents;
using Joselct.Outbox.Core.Entities;
using Joselct.Outbox.Core.Interfaces;
using MediatR;

namespace CalendarioEntregas.Infrastructure.Messaging.DomainEventHandlers
{
	internal class HandleEntregaReactivada : INotificationHandler<EntregaReactivada>
	{
		private readonly IOutboxRepository _outboxRepository;

		public HandleEntregaReactivada(IOutboxRepository outboxRepository)
		{
			_outboxRepository = outboxRepository;
		}

		public async Task Handle(EntregaReactivada e, CancellationToken ct)
		{
			var integrationEvent = new EntregaReactivadaIntegrationEvent(
				e.CalendarioId, e.DireccionId, e.Fecha);

			await _outboxRepository.AddAsync(
				OutboxMessage.CreateWithCurrentTrace(integrationEvent), ct);
		}
	}
}
