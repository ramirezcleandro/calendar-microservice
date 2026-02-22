using CalendarioEntregas.Domain.Agregados;

namespace CalendarioEntregas.Domain.Repositories
{
    public interface ICalendarioEntregaRepository
    {
        Task<CalendarioEntrega?> GetByIdAsync(Guid id);
        Task<CalendarioEntrega?> GetByPacienteIdAsync(Guid pacienteId);
        Task<IEnumerable<CalendarioEntrega>> GetAllAsync();
        Task AddAsync(CalendarioEntrega calendario);
        Task UpdateAsync(CalendarioEntrega calendario);
        Task DeleteAsync(Guid id);
    }
}
