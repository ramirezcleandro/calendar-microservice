using CalendarioEntregas.Domain.Agregados;
using CalendarioEntregas.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CalendarioEntregas.Infrastructure.Repositories
{
    public class CalendarioEntregaRepository : ICalendarioEntregaRepository
    {
        private readonly Persistence.CalendarioDbContext _context;

        public CalendarioEntregaRepository(Persistence.CalendarioDbContext context)
        {
            _context = context;
        }

        public async Task<CalendarioEntrega?> GetByIdAsync(Guid id)
        {
            return await _context.Calendarios
                .Include(c => c.Direcciones)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<CalendarioEntrega?> GetByPacienteIdAsync(Guid pacienteId)
        {
            return await _context.Calendarios
                .Include(c => c.Direcciones)
                .FirstOrDefaultAsync(c => c.PacienteId == pacienteId && c.Activo);
        }

        public async Task<IEnumerable<CalendarioEntrega>> GetAllAsync()
        {
            return await _context.Calendarios
                .Include(c => c.Direcciones)
                .Where(c => c.Activo)
                .ToListAsync();
        }

        public async Task AddAsync(CalendarioEntrega calendario)
        {
            await _context.Calendarios.AddAsync(calendario);
        }

        public async Task UpdateAsync(CalendarioEntrega calendario)
        {
            _context.Calendarios.Update(calendario);
            await Task.CompletedTask;
        }

        public async Task DeleteAsync(Guid id)
        {
            var calendario = await GetByIdAsync(id);
            if (calendario != null)
            {
                _context.Calendarios.Remove(calendario);
            }
        }
    }
}
