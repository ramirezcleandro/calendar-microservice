using CalendarioEntregas.Application;
using CalendarioEntregas.Infrastructure;
using CalendarioEntregas.Infrastructure.Persistence;
using Joselct.Communication.RabbitMQ.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Prometheus;
using Serilog;
using Serilog.Sinks.Grafana.Loki;
using System.Text;

const string ServiceName = "calendario-entregas";

var lokiUri = Environment.GetEnvironmentVariable("Loki__Uri");

var loggerConfig = new LoggerConfiguration()
	.Enrich.FromLogContext()
	.Enrich.WithMachineName()
	.Enrich.WithThreadId()
	.Enrich.WithProperty("Service", ServiceName)
	.WriteTo.Console(outputTemplate:
		"[{Timestamp:yyyy-MM-dd HH:mm:ss.fff} {Level:u3}] [{Service}] [{TraceId} {SpanId}] {Message:lj} {Properties:j}{NewLine}{Exception}");

if (!string.IsNullOrWhiteSpace(lokiUri))
{
	loggerConfig = loggerConfig.WriteTo.GrafanaLoki(
		lokiUri,
		labels: new[]
		{
			new LokiLabel { Key = "service", Value = ServiceName },
			new LokiLabel { Key = "app", Value = ServiceName }
		},
		propertiesAsLabels: new[] { "level" });
}

Log.Logger = loggerConfig.CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog();

builder.WebHost.ConfigureKestrel(options =>
{
	var port = int.Parse(Environment.GetEnvironmentVariable("PORT") ?? "7020");
	options.ListenAnyIP(port);
});

// OpenTelemetry — tracing distribuido propagado por HTTP y por Joselct/RabbitMQ
var otlpEndpoint = Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT");
builder.Services.AddOpenTelemetry()
	.ConfigureResource(r => r.AddService(ServiceName))
	.WithTracing(tracing =>
	{
		tracing
			.AddAspNetCoreInstrumentation()
			.AddHttpClientInstrumentation()
			.AddEntityFrameworkCoreInstrumentation(o => o.SetDbStatementForText = true);

		if (builder.Environment.IsDevelopment())
			tracing.AddConsoleExporter();

		if (!string.IsNullOrWhiteSpace(otlpEndpoint))
			tracing.AddOtlpExporter(o => o.Endpoint = new Uri(otlpEndpoint));
	})
	.AddRabbitMqInstrumentation();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Swagger con soporte para Bearer Token
builder.Services.AddSwaggerGen(c =>
{
	c.SwaggerDoc("v1", new OpenApiInfo { Title = "CalendarioEntregas API", Version = "v1" });

	c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
	{
		Name = "Authorization",
		Type = SecuritySchemeType.Http,
		Scheme = "Bearer",
		BearerFormat = "JWT",
		In = ParameterLocation.Header,
		Description = "Ingrese el token JWT. Ejemplo: Bearer {token}"
	});

	c.AddSecurityRequirement(new OpenApiSecurityRequirement
	{
		{
			new OpenApiSecurityScheme
			{
				Reference = new OpenApiReference
				{
					Type = ReferenceType.SecurityScheme,
					Id = "Bearer"
				}
			},
			Array.Empty<string>()
		}
	});
});

builder.Configuration
	.AddJsonFile("appsettings.json", optional: true)
	.AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
	.AddEnvironmentVariables();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Autenticación JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
	.AddJwtBearer(options =>
	{
		options.TokenValidationParameters = new TokenValidationParameters
		{
			ValidateIssuer = true,
			ValidateAudience = true,
			ValidateLifetime = true,
			ValidateIssuerSigningKey = true,
			ValidIssuer = builder.Configuration["Jwt:Issuer"],
			ValidAudience = builder.Configuration["Jwt:Audience"],
			IssuerSigningKey = new SymmetricSecurityKey(
				Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
		};
	});

builder.Services.AddAuthorization();
builder.Services.AddHealthChecks();

builder.Services.AddCors(options =>
{
	options.AddPolicy("AllowAll",
		policy =>
		{
			policy
				.AllowAnyOrigin()
				.AllowAnyHeader()
				.AllowAnyMethod();
		});
});

var app = builder.Build();

try
{
	// Aplicar migraciones automáticamente
	using (var scope = app.Services.CreateScope())
	{
		var dbContext = scope.ServiceProvider.GetRequiredService<CalendarioDbContext>();
		dbContext.Database.Migrate();
		Log.Information("Base de datos migrada correctamente");
	}

	app.UseSerilogRequestLogging();

	// Metricas Prometheus (requests, duraciones, status codes).
	app.UseHttpMetrics();

	// Swagger disponible en todos los ambientes
	app.UseSwagger();
	app.UseSwaggerUI();

	app.UseCors("AllowAll");

	app.UseAuthentication();
	app.UseAuthorization();

	app.MapHealthChecks("/health");
	// Endpoint /metrics que Prometheus scrape cada 15s.
	app.MapMetrics();
	app.MapControllers();

	app.Run();
}
catch (Exception ex)
{
	Log.Fatal(ex, "El microservicio {Service} terminó inesperadamente", ServiceName);
	throw;
}
finally
{
	Log.CloseAndFlush();
}
