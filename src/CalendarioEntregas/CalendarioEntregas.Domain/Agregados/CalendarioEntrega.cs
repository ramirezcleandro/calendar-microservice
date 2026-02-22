using CalendarioEntregas.Domain.ValueObjects;

namespace CalendarioEntregas.Domain.Agregados
{
    /// <summary>
    /// Agregado raíz que representa un calendario de entregas de un paciente
    /// </summary>
    public class CalendarioEntrega
    {
        public Guid Id { get; private set; }
        public Guid PacienteId { get; private set; }
        public Guid PlanAlimenticioId { get; private set; }
        public DateOnly FechaInicio { get; private set; }
        public DateOnly FechaFin { get; private set; }
        public DateTime FechaCreacion { get; private set; }
        public bool Activo { get; private set; }

        private readonly List<Direccion> _direcciones = new();
        public IReadOnlyList<Direccion> Direcciones => _direcciones.AsReadOnly();

        private CalendarioEntrega() { }

        public CalendarioEntrega(
            Guid pacienteId,
            Guid planAlimenticioId,
            DateOnly fechaInicio,
            DateOnly fechaFin)
        {
            if (fechaFin <= fechaInicio)
            {
                throw new ArgumentException("La fecha fin debe ser posterior a la fecha inicio");
            }

            Id = Guid.NewGuid();
            PacienteId = pacienteId;
            PlanAlimenticioId = planAlimenticioId;
            FechaInicio = fechaInicio;
            FechaFin = fechaFin;
            FechaCreacion = DateTime.UtcNow;
            Activo = true;
        }

        /// <summary>
        /// Agrega una dirección de entrega para un día específico
        /// </summary>
        public void AgregarDireccion(
            DateOnly fecha,
            string direccion,
            string referencias,
            Latitud latitud,
            Longitud longitud)
        {
            ValidarFechaEnRango(fecha);

            if (_direcciones.Any(d => d.Fecha == fecha))
            {
                throw new InvalidOperationException($"Ya existe una dirección registrada para el día {fecha}");
            }

            var nuevaDireccion = new Direccion(Id, fecha, direccion, referencias, latitud, longitud);
            _direcciones.Add(nuevaDireccion);
        }

        /// <summary>
        /// Obtiene la dirección para una fecha específica
        /// </summary>
        public Direccion? ObtenerDireccion(DateOnly fecha)
        {
            return _direcciones.FirstOrDefault(d => d.Fecha == fecha);
        }

        /// <summary>
        /// Obtiene todas las direcciones activas desde una fecha en adelante
        /// </summary>
        public IEnumerable<Direccion> ObtenerDireccionesActivas(DateOnly desdeHoy)
        {
            return _direcciones
                .Where(d => d.Fecha >= desdeHoy && d.EsEntregaActiva)
                .OrderBy(d => d.Fecha);
        }

        /// <summary>
        /// Obtiene la dirección más próxima para entregar
        /// </summary>
        public Direccion? ObtenerProximaEntrega()
        {
            var hoy = DateOnly.FromDateTime(DateTime.Today);
            return _direcciones
                .Where(d => d.Fecha >= hoy && d.EsEntregaActiva)
                .OrderBy(d => d.Fecha)
                .FirstOrDefault();
        }

        /// <summary>
        /// Valida que la fecha esté dentro del rango del calendario
        /// </summary>
        private void ValidarFechaEnRango(DateOnly fecha)
        {
            if (fecha < FechaInicio || fecha > FechaFin)
            {
                throw new ArgumentException($"La fecha {fecha} está fuera del rango del calendario ({FechaInicio} a {FechaFin})");
            }
        }

        /// <summary>
        /// Obtiene el estado actual del calendario (porcentaje completado)
        /// </summary>
        public int ObtenerPorcentajeCompletado()
        {
            var totalDias = FechaFin.DayNumber - FechaInicio.DayNumber + 1;
            var diasEntregados = _direcciones.Count(d => !d.EsEntregaActiva || d.Fecha < DateOnly.FromDateTime(DateTime.Today));

            return totalDias > 0 ? (diasEntregados * 100) / totalDias : 0;
        }

        /// <summary>
        /// Desactiva el calendario
        /// </summary>
        public void Desactivar()
        {
            Activo = false;
        }
    }
}
