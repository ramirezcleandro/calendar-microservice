using CalendarioEntregas.Domain.Abstractions;
using CalendarioEntregas.Domain.Eventos;
using CalendarioEntregas.Infrastructure.Messaging.IntegrationEvents;
using CalendarioEntregas.Infrastructure.Persistence;
using Joselct.Outbox.Core.Entities;
using Joselct.Outbox.Core.Interfaces;

namespace CalendarioEntregas.Infrastructure
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly CalendarioDbContext _context;
        private readonly IOutboxRepository _outboxRepository;

        public UnitOfWork(CalendarioDbContext context, IOutboxRepository outboxRepository)
        {
            _context = context;
            _outboxRepository = outboxRepository;
        }

        public async Task CommitAsync(CancellationToken cancellationToken = default)
        {
            // 1. Recopilar domain events antes de guardar
            var domainEvents = _context.ChangeTracker
                .Entries<AggregateRoot>()
                .SelectMany(e => e.Entity.DomainEvents)
                .ToList();

            _context.ChangeTracker
                .Entries<AggregateRoot>()
                .ToList()
                .ForEach(e => e.Entity.ClearDomainEvents());

            // 2. Guardar el aggregate
            await _context.SaveChangesAsync(cancellationToken);

            // 3. Convertir domain events a integration events y guardar en outbox
            foreach (var domainEvent in domainEvents)
                await SaveIntegrationEventAsync(domainEvent, cancellationToken);

            // 4. Guardar los mensajes del outbox
            if (domainEvents.Any())
                await _context.SaveChangesAsync(cancellationToken);
        }

        private async Task SaveIntegrationEventAsync(IDomainEvent domainEvent, CancellationToken ct)
        {
            OutboxMessage? outboxMessage = domainEvent switch
            {
                CalendarioCreado e => OutboxMessage.CreateWithCurrentTrace(
                    new CalendarioCreadoIntegrationEvent(
                        e.CalendarioId, e.PacienteId, e.PlanAlimenticioId,
                        e.FechaInicio, e.FechaFin)),

                DireccionAgregada e => OutboxMessage.CreateWithCurrentTrace(
                    new DireccionAgregadaIntegrationEvent(
                        e.CalendarioId, e.DireccionId, e.Fecha,
                        e.Direccion, e.Latitud, e.Longitud)),

                DireccionModificada e => OutboxMessage.CreateWithCurrentTrace(
                    new DireccionModificadaIntegrationEvent(
                        e.CalendarioId, e.DireccionId, e.Fecha,
                        e.NuevaDireccion, e.NuevaLatitud, e.NuevaLongitud)),

                EntregaCancelada e => OutboxMessage.CreateWithCurrentTrace(
                    new EntregaCanceladaIntegrationEvent(
                        e.CalendarioId, e.DireccionId, e.Fecha)),

                EntregaReactivada e => OutboxMessage.CreateWithCurrentTrace(
                    new EntregaReactivadaIntegrationEvent(
                        e.CalendarioId, e.DireccionId, e.Fecha)),

                CalendarioDesactivado e => OutboxMessage.CreateWithCurrentTrace(
                    new CalendarioDesactivadoIntegrationEvent(
                        e.CalendarioId, e.PacienteId)),

                _ => null
            };

            if (outboxMessage is not null)
                await _outboxRepository.AddAsync(outboxMessage, ct);
        }

        public Task RollbackAsync(CancellationToken cancellationToken = default)
        {
            _context.ChangeTracker.Clear();
            return Task.CompletedTask;
        }

        public void Dispose() => _context.Dispose();
    }
}
