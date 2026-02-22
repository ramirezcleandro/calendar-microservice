using CalendarioEntregas.Domain.Agregados;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace CalendarioEntregas.Infrastructure.Persistence
{
    public class CalendarioDbContext : DbContext
    {
        public DbSet<CalendarioEntrega> Calendarios { get; set; } = null!;
        public DbSet<Direccion> Direcciones { get; set; } = null!;

        public CalendarioDbContext(DbContextOptions<CalendarioDbContext> options) : base(options)
        {
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
        }
    }
}
