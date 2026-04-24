using CalendarioEntregas.Domain.Eventos;
using CalendarioEntregas.Infrastructure.Messaging.IntegrationEvents;
using Joselct.Outbox.Core.Entities;
using Joselct.Outbox.Core.Interfaces;
using MediatR;

namespace CalendarioEntregas.Infrastructure.Messaging.DomainEventHandlers
{
	internal class HandleDireccionAgregada : INotificationHandler<DireccionAgregada>
	{
		private readonly IOutboxRepository _outboxRepository;

		public HandleDireccionAgregada(IOutboxRepository outboxRepository)
		{
			_outboxRepository = outboxRepository;
		}

		public async Task Handle(DireccionAgregada e, CancellationToken ct)
		{
			var integrationEvent = new DireccionAgregadaIntegrationEvent(
				e.CalendarioId, e.DireccionId, e.Fecha,
				e.Direccion, e.Latitud, e.Longitud);

			await _outboxRepository.AddAsync(
				OutboxMessage.CreateWithCurrentTrace(integrationEvent), ct);
		}
	}
}
