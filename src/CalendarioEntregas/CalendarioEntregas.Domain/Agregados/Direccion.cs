using CalendarioEntregas.Domain.ValueObjects;

namespace CalendarioEntregas.Domain.Agregados
{
    /// <summary>
    /// Representa una dirección de entrega para un día específico del calendario
    /// </summary>
    public class Direccion
    {
        public Guid Id { get; private set; }
        public Guid CalendarioId { get; private set; }
        public DateOnly Fecha { get; private set; }
        public string Direccion_Texto { get; private set; }
        public string Referencias { get; private set; }
        public Latitud Latitud { get; private set; }
        public Longitud Longitud { get; private set; }
        public bool EsEntregaActiva { get; private set; }
        public DateTime FechaCreacion { get; private set; }
        public DateTime? FechaUltimaModificacion { get; private set; }

        private Direccion() { }

        public Direccion(
            Guid calendarioId,
            DateOnly fecha,
            string direccion,
            string referencias,
            Latitud latitud,
            Longitud longitud)
        {
            Id = Guid.NewGuid();
            CalendarioId = calendarioId;
            Fecha = fecha;
            Direccion_Texto = direccion ?? throw new ArgumentNullException(nameof(direccion));
            Referencias = referencias ?? string.Empty;
            Latitud = latitud ?? throw new ArgumentNullException(nameof(latitud));
            Longitud = longitud ?? throw new ArgumentNullException(nameof(longitud));
            EsEntregaActiva = true;
            FechaCreacion = DateTime.UtcNow;
            FechaUltimaModificacion = null;
        }

        /// <summary>
        /// Modifica la dirección. Solo se permite si quedan al menos 2 días antes de la entrega
        /// </summary>
        public void Modificar(string nuevaDireccion, string referencias, Latitud latitud, Longitud longitud)
        {
            if (!PuedeModificarse())
            {
                throw new InvalidOperationException("No se puede modificar la dirección. Requiere 2 días de anticipación.");
            }

            Direccion_Texto = nuevaDireccion ?? throw new ArgumentNullException(nameof(nuevaDireccion));
            Referencias = referencias ?? string.Empty;
            Latitud = latitud ?? throw new ArgumentNullException(nameof(latitud));
            Longitud = longitud ?? throw new ArgumentNullException(nameof(longitud));
            FechaUltimaModificacion = DateTime.UtcNow;
        }

        /// <summary>
        /// Marca la dirección como no entregable. Solo se permite si quedan al menos 2 días
        /// </summary>
        public void MarcarNoEntrega()
        {
            if (!PuedeModificarse())
            {
                throw new InvalidOperationException("No se puede cancelar la entrega. Requiere 2 días de anticipación.");
            }

            EsEntregaActiva = false;
            FechaUltimaModificacion = DateTime.UtcNow;
        }

        /// <summary>
        /// Reactiva una dirección marcada como no entrega
        /// </summary>
        public void ReactivarEntrega()
        {
            if (!PuedeModificarse())
            {
                throw new InvalidOperationException("No se puede reactivar la entrega. Requiere 2 días de anticipación.");
            }

            EsEntregaActiva = true;
            FechaUltimaModificacion = DateTime.UtcNow;
        }

        /// <summary>
        /// Valida si la dirección puede ser modificada (deben quedar al menos 2 días)
        /// </summary>
        private bool PuedeModificarse()
        {
            var hoy = DateOnly.FromDateTime(DateTime.Today);
            var diasHasta = (Fecha.ToDateTime(TimeOnly.MinValue) - hoy.ToDateTime(TimeOnly.MinValue)).Days;
            return diasHasta >= 2;
        }

        /// <summary>
        /// Obtiene los días restantes hasta la entrega
        /// </summary>
        public int ObtenerDiasRestantes()
        {
            var hoy = DateOnly.FromDateTime(DateTime.Today);
            return (Fecha.ToDateTime(TimeOnly.MinValue) - hoy.ToDateTime(TimeOnly.MinValue)).Days;
        }
    }
}
