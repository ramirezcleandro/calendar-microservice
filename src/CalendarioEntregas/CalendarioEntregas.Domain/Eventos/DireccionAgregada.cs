using CalendarioEntregas.Domain.Abstractions;

namespace CalendarioEntregas.Domain.Eventos
{
    public record DireccionAgregada(
        Guid CalendarioId,
        Guid DireccionId,
        DateOnly Fecha,
        string Direccion,
        double Latitud,
        double Longitud
    ) : IDomainEvent;
}
