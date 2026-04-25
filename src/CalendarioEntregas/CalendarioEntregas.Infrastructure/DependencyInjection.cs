using CalendarioEntregas.Domain.Repositories;
using CalendarioEntregas.Domain.Abstractions;
using CalendarioEntregas.Infrastructure.External.Consul;
using CalendarioEntregas.Infrastructure.Messaging.Consumers;
using CalendarioEntregas.Infrastructure.Messaging.IntegrationEvents.ReceivedEvents;
using CalendarioEntregas.Infrastructure.Persistence;
using CalendarioEntregas.Infrastructure.Repositories;
using Consul;
using Joselct.Communication.RabbitMQ.Extensions;
using Joselct.Outbox.EFCore.Extensions;
using Joselct.Outbox.MediatR.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace CalendarioEntregas.Infrastructure
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
		{
			// Registrar OutboxHandlers (INotificationHandler) de esta assembly en MediatR
			services.AddMediatR(config =>
				config.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

			var connectionString = configuration.GetConnectionString("CalendarioDatabase")
				?? throw new InvalidOperationException("Connection string 'CalendarioDatabase' no encontrada.");

			services.AddDbContext<CalendarioDbContext>(options =>
				options.UseNpgsql(connectionString));

			services.AddScoped<ICalendarioEntregaRepository, CalendarioEntregaRepository>();
			services.AddScoped<IUnitOfWork, UnitOfWork>();

			// Outbox (Joselct)
			services.AddOutboxEfCore<CalendarioDbContext>();

			// RabbitMQ (Joselct) — publisher + connection manager
			services.AddRabbitMq(configuration);

			// Consumer: escucha meal-plan.plan desde ms-calendar-queue
			services.AddRabbitMqConsumer<PlanAlimenticioCreadoIntegrationEvent, PlanAlimenticioCreadoConsumer>(
				queueName: "ms-calendar-queue",
				exchangeName: "meal-plans",
				routingKey: "meal-plan.plan",
				declareQueue: false);

			// Worker del outbox (polling y dispatch vía MediatR)
			services.AddOutboxWorker();

			// Service discovery con Consul
			services.AddConsulServiceDiscovery(configuration);

			return services;
		}

		private static IServiceCollection AddConsulServiceDiscovery(
			this IServiceCollection services,
			IConfiguration configuration)
		{
			services.Configure<ConsulOptions>(configuration.GetSection(ConsulOptions.SectionName));

			services.AddSingleton<IConsulClient, ConsulClient>(sp =>
			{
				var options = sp.GetRequiredService<IOptions<ConsulOptions>>().Value;
				return new ConsulClient(config => config.Address = new Uri(options.Host));
			});

			services.AddHostedService<ConsulHostedService>();

			return services;
		}
	}
}
