using Consul;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CalendarioEntregas.Infrastructure.External.Consul;

internal sealed class ConsulHostedService : IHostedService
{
	private readonly IConsulClient _consulClient;
	private readonly ConsulOptions _options;
	private readonly ILogger<ConsulHostedService> _logger;
	private string? _registrationId;

	public ConsulHostedService(
		IConsulClient consulClient,
		IOptions<ConsulOptions> options,
		ILogger<ConsulHostedService> logger)
	{
		_consulClient = consulClient;
		_options = options.Value;
		_logger = logger;
	}

	public async Task StartAsync(CancellationToken cancellationToken)
	{
		_registrationId = $"{_options.ServiceName}-{_options.ServiceAddress}-{_options.ServicePort}";

		var registration = new AgentServiceRegistration
		{
			ID = _registrationId,
			Name = _options.ServiceName,
			Address = _options.ServiceAddress,
			Port = _options.ServicePort,
			Tags = _options.Tags,
			Check = new AgentServiceCheck
			{
				HTTP = $"http://{_options.ServiceAddress}:{_options.ServicePort}{_options.HealthCheckEndpoint}",
				Interval = TimeSpan.FromSeconds(10),
				Timeout = TimeSpan.FromSeconds(5),
				DeregisterCriticalServiceAfter = TimeSpan.FromMinutes(1)
			}
		};

		_logger.LogInformation(
			"Registrando servicio {ServiceName} en Consul ({Address}:{Port})",
			_options.ServiceName, _options.ServiceAddress, _options.ServicePort);

		try
		{
			await _consulClient.Agent.ServiceDeregister(_registrationId, cancellationToken);
			await _consulClient.Agent.ServiceRegister(registration, cancellationToken);

			_logger.LogInformation(
				"Servicio registrado correctamente en Consul con ID {RegistrationId}",
				_registrationId);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Falló el registro del servicio en Consul");
		}
	}

	public async Task StopAsync(CancellationToken cancellationToken)
	{
		if (_registrationId is null) return;

		_logger.LogInformation("Des-registrando servicio {ServiceId} de Consul", _registrationId);

		try
		{
			await _consulClient.Agent.ServiceDeregister(_registrationId, cancellationToken);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Falló el des-registro del servicio en Consul");
		}
	}
}
