using CalendarioEntregas.Domain.Abstractions;
using CalendarioEntregas.Domain.Agregados;
using CalendarioEntregas.Infrastructure.Outbox;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace CalendarioEntregas.Infrastructure.Persistence
{
    public class CalendarioDbContext : DbContext
    {
        public DbSet<CalendarioEntrega> Calendarios { get; set; } = null!;
        public DbSet<Direccion> Direcciones { get; set; } = null!;
        public DbSet<OutboxMessage> OutboxMessages { get; set; } = null!;

        public CalendarioDbContext(DbContextOptions<CalendarioDbContext> options) : base(options)
        {
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // Recopilar todos los domain events de los agregados tracked
            var outboxMessages = ChangeTracker
                .Entries<AggregateRoot>()
                .SelectMany(entry => entry.Entity.DomainEvents)
                .Select(domainEvent => new OutboxMessage
                {
                    Id = Guid.NewGuid(),
                    Type = domainEvent.GetType().Name,
                    Payload = JsonSerializer.Serialize(domainEvent, domainEvent.GetType()),
                    OccurredOnUtc = DateTime.UtcNow
                })
                .ToList();

            // Limpiar los domain events de los agregados
            ChangeTracker
                .Entries<AggregateRoot>()
                .ToList()
                .ForEach(entry => entry.Entity.ClearDomainEvents());

            // Agregar los mensajes al Outbox
            if (outboxMessages.Any())
                OutboxMessages.AddRange(outboxMessages);

            // Persistir todo en una sola transacción
            return await base.SaveChangesAsync(cancellationToken);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configurar CalendarioEntrega
            modelBuilder.Entity<CalendarioEntrega>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedNever();
                entity.Property(e => e.PacienteId).IsRequired();
                entity.Property(e => e.PlanAlimenticioId).IsRequired();
                entity.Property(e => e.FechaInicio).IsRequired();
                entity.Property(e => e.FechaFin).IsRequired();
                entity.Property(e => e.FechaCreacion).IsRequired();
                entity.Property(e => e.Activo).IsRequired();

                entity.HasMany(e => e.Direcciones)
                    .WithOne()
                    .HasForeignKey(d => d.CalendarioId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.ToTable("Calendarios");
            });

            // Configurar Direccion
            modelBuilder.Entity<Direccion>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedNever();
                entity.Property(e => e.CalendarioId).IsRequired();
                entity.Property(e => e.Fecha).IsRequired();
                entity.Property(e => e.Direccion_Texto).IsRequired();
                entity.Property(e => e.Referencias).IsRequired();
                entity.Property(e => e.EsEntregaActiva).IsRequired();
                entity.Property(e => e.FechaCreacion).IsRequired();

                // Value Objects
                entity.OwnsOne(d => d.Latitud, builder =>
                {
                    builder.Property(l => l.Valor).HasColumnName("Latitud");
                });

                entity.OwnsOne(d => d.Longitud, builder =>
                {
                    builder.Property(l => l.Valor).HasColumnName("Longitud");
                });

                entity.HasIndex(d => new { d.CalendarioId, d.Fecha }).IsUnique();

                entity.ToTable("Direcciones");
            });

            // Configurar OutboxMessage
            modelBuilder.Entity<OutboxMessage>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedNever();
                entity.Property(e => e.Type).IsRequired();
                entity.Property(e => e.Payload).IsRequired();
                entity.Property(e => e.OccurredOnUtc).IsRequired();
                entity.Property(e => e.ProcessedOnUtc);

                entity.HasIndex(e => e.ProcessedOnUtc);

                entity.ToTable("OutboxMessages");
            });
        }
    }
}
