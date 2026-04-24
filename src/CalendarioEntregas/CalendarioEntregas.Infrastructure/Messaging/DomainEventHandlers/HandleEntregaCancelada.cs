using CalendarioEntregas.Domain.Eventos;
using CalendarioEntregas.Infrastructure.Messaging.IntegrationEvents;
using Joselct.Outbox.Core.Entities;
using Joselct.Outbox.Core.Interfaces;
using MediatR;

namespace CalendarioEntregas.Infrastructure.Messaging.DomainEventHandlers
{
	internal class HandleEntregaCancelada : INotificationHandler<EntregaCancelada>
	{
		private readonly IOutboxRepository _outboxRepository;

		public HandleEntregaCancelada(IOutboxRepository outboxRepository)
		{
			_outboxRepository = outboxRepository;
		}

		public async Task Handle(EntregaCancelada e, CancellationToken ct)
		{
			var integrationEvent = new EntregaCanceladaIntegrationEvent(
				e.CalendarioId, e.DireccionId, e.Fecha);

			await _outboxRepository.AddAsync(
				OutboxMessage.CreateWithCurrentTrace(integrationEvent), ct);
		}
	}
}
