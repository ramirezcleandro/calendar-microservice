namespace CalendarioEntregas.Infrastructure.External.Consul;

public class ConsulOptions
{
	public const string SectionName = "Consul";

	public string Host { get; set; } = "http://localhost:8500";
	public string ServiceName { get; set; } = "calendario-webapi";
	public string ServiceAddress { get; set; } = "localhost";
	public int ServicePort { get; set; } = 7020;
	public string[] Tags { get; set; } = ["dotnet", "api", "calendario", "metrics"];
	public string HealthCheckEndpoint { get; set; } = "/health";
}
