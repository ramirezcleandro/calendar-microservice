using CalendarioEntregas.Domain.Repositories;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace CalendarioEntregas.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddMediatR(config =>
            {
                config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            });

            return services;
        }
    }
}
