namespace CalendarioEntregas.Application.Calendario.Queries
{
    public record DireccionDto(
        Guid Id,
        DateOnly Fecha,
        string Direccion,
        string Referencias,
        double Latitud,
        double Longitud,
        bool EsEntregaActiva,
        int DiasRestantes
    );

    public record CalendarioDto(
        Guid Id,
        Guid PacienteId,
        Guid PlanAlimenticioId,
        DateOnly FechaInicio,
        DateOnly FechaFin,
        bool Activo,
        int PorcentajeCompletado,
        IEnumerable<DireccionDto> Direcciones
    );
}
