using MediatR;

namespace CalendarioEntregas.Domain.Abstractions
{
	// Hereda INotification para que los eventos puedan publicarse por MediatR
	// desde el UnitOfWork, y cada evento tenga su propio handler
	// (Open/Closed Principle: agregar un evento no requiere modificar el UoW).
	public interface IDomainEvent : INotification
	{
	}
}
