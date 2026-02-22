using CalendarioEntregas.Domain.Abstractions;

namespace CalendarioEntregas.Infrastructure
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly Persistence.CalendarioDbContext _context;

        public UnitOfWork(Persistence.CalendarioDbContext context)
        {
            _context = context;
        }

        public async Task CommitAsync(CancellationToken cancellationToken = default)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task RollbackAsync(CancellationToken cancellationToken = default)
        {
            await _context.DisposeAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
