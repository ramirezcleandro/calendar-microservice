using CalendarioEntregas.Domain.Eventos;
using CalendarioEntregas.Infrastructure.Messaging.IntegrationEvents;
using Joselct.Outbox.Core.Entities;
using Joselct.Outbox.Core.Interfaces;
using MediatR;

namespace CalendarioEntregas.Infrastructure.Messaging.DomainEventHandlers
{
	internal class HandleDireccionModificada : INotificationHandler<DireccionModificada>
	{
		private readonly IOutboxRepository _outboxRepository;

		public HandleDireccionModificada(IOutboxRepository outboxRepository)
		{
			_outboxRepository = outboxRepository;
		}

		public async Task Handle(DireccionModificada e, CancellationToken ct)
		{
			var integrationEvent = new DireccionModificadaIntegrationEvent(
				e.CalendarioId, e.DireccionId, e.Fecha,
				e.NuevaDireccion, e.NuevaLatitud, e.NuevaLongitud);

			await _outboxRepository.AddAsync(
				OutboxMessage.CreateWithCurrentTrace(integrationEvent), ct);
		}
	}
}
