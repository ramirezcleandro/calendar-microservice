using CalendarioEntregas.Domain.Abstractions;

namespace CalendarioEntregas.Domain.Eventos
{
    public record DireccionModificada(
        Guid CalendarioId,
        Guid DireccionId,
        DateOnly Fecha,
        string NuevaDireccion,
        double NuevaLatitud,
        double NuevaLongitud
    ) : IDomainEvent;
}
