using CalendarioEntregas.Domain.Abstractions;
using CalendarioEntregas.Infrastructure.Persistence;
using MediatR;

namespace CalendarioEntregas.Infrastructure
{
	public class UnitOfWork : IUnitOfWork
	{
		private readonly CalendarioDbContext _context;
		private readonly IMediator _mediator;

		public UnitOfWork(CalendarioDbContext context, IMediator mediator)
		{
			_context = context;
			_mediator = mediator;
		}

		public async Task CommitAsync(CancellationToken cancellationToken = default)
		{
			// 1. Recopilar domain events de los AggregateRoots trackeados.
			var domainEvents = _context.ChangeTracker
				.Entries<AggregateRoot>()
				.SelectMany(e => e.Entity.DomainEvents)
				.ToList();

			// 2. Limpiar los eventos para no reemitirlos en el próximo commit.
			_context.ChangeTracker
				.Entries<AggregateRoot>()
				.ToList()
				.ForEach(e => e.Entity.ClearDomainEvents());

			// 3. Persistir el aggregate.
			await _context.SaveChangesAsync(cancellationToken);

			// 4. Publicar cada domain event vía MediatR. Cada Handle<T> se encarga de
			//    traducir al integration event correspondiente y guardar en el outbox.
			//    Nuevo evento = nuevo handler, el UoW no cambia (Open/Closed Principle).
			foreach (var domainEvent in domainEvents)
				await _mediator.Publish(domainEvent, cancellationToken);

			// 5. Persistir los mensajes del outbox que quedaron en el ChangeTracker.
			if (domainEvents.Any())
				await _context.SaveChangesAsync(cancellationToken);
		}

		public Task RollbackAsync(CancellationToken cancellationToken = default)
		{
			_context.ChangeTracker.Clear();
			return Task.CompletedTask;
		}

		public void Dispose() => _context.Dispose();
	}
}
