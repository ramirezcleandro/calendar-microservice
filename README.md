# Microservicio: Calendario de Entregas

Gestiona los calendarios de entrega de alimentos para pacientes del sistema NutriCenter. Cuando un plan alimenticio es creado, este microservicio genera automáticamente el calendario correspondiente y publica los cambios al resto del sistema a través de RabbitMQ.

**Stack:** .NET 8 · PostgreSQL · RabbitMQ · MassTransit · Docker

---

## Levantar el sistema

```bash
cd src/CalendarioEntregas
docker-compose up -d
```

| Recurso | URL |
|---------|-----|
| Swagger | http://localhost:7020/swagger |
| RabbitMQ Management | http://localhost:15672 (guest/guest) |

---

## Autenticación

Todos los endpoints requieren `Bearer Token`. El token se valida con:

```
Issuer:   nurtricenter-auth
Audience: nurtricenter-api
Key:      nurtricenter-clave-secreta-de-al-menos-32-caracteres
```

Incluir en cada request:
```
Authorization: Bearer <token>
```

---

## Mensajería con RabbitMQ

Este microservicio usa **RabbitMQ** como broker de mensajes para comunicarse con el resto del sistema. La integración está construida sobre **MassTransit**, que maneja la conexión, serialización y ruteo de mensajes automáticamente.

### Por qué RabbitMQ

En una arquitectura de microservicios, los servicios no se llaman directamente entre sí. En cambio, publican eventos que otros servicios consumen de forma asíncrona. Esto desacopla los servicios: el calendario no sabe quién escucha sus eventos, y no le importa si el consumidor está disponible en ese momento.

### Eventos que este microservicio PUBLICA

Otros microservicios pueden suscribirse a estos eventos desde RabbitMQ.

| Evento | Cuándo se genera | Payload |
|--------|-----------------|---------|
| `CalendarioCreadoIntegrationEvent` | Se crea un calendario | `CalendarioId, PacienteId, PlanAlimenticioId, FechaInicio, FechaFin` |
| `DireccionAgregadaIntegrationEvent` | Se agrega una dirección de entrega | `CalendarioId, DireccionId, Fecha, Direccion, Latitud, Longitud` |
| `DireccionModificadaIntegrationEvent` | Se modifica una dirección | `CalendarioId, DireccionId, Fecha, NuevaDireccion, NuevaLatitud, NuevaLongitud` |
| `EntregaCanceladaIntegrationEvent` | Se marca un día como no entrega | `CalendarioId, DireccionId, Fecha` |
| `EntregaReactivadaIntegrationEvent` | Se reactiva una entrega cancelada | `CalendarioId, DireccionId, Fecha` |
| `CalendarioDesactivadoIntegrationEvent` | Se desactiva el calendario | `CalendarioId, PacienteId` |

### Eventos que este microservicio CONSUME

| Evento | Publicado por | Acción automática |
|--------|--------------|-------------------|
| `PlanAlimenticioCreadoIntegrationEvent` | MS Planes Alimenticios | Crea el calendario de entregas automáticamente |

### Contrato requerido del evento entrante

El microservicio de Planes Alimenticios **debe publicar exactamente este tipo**:

```csharp
public record PlanAlimenticioCreadoIntegrationEvent(
    Guid PlanAlimenticioId,
    Guid PacienteId,
    DateOnly FechaInicio,
    DateOnly FechaFin,
    DateTime OccurredOnUtc
);
```

> El nombre del `record` debe ser idéntico en ambos microservicios. MassTransit rutea por nombre de tipo.

### Cómo conectar otro microservicio al RabbitMQ

Para suscribirse a los eventos desde otro microservicio, agregar en su `docker-compose.yml`:

```yaml
services:
  mi-microservicio:
    # ...
    environment:
      - RabbitMq__Host=rabbitmq
      - RabbitMq__Username=guest
      - RabbitMq__Password=guest
    networks:
      - nutricenter_net

networks:
  nutricenter_net:
    external: true
```

Y en su código (.NET con MassTransit):

```csharp
services.AddMassTransit(x =>
{
    x.AddConsumer<MiConsumer>();

    x.UsingRabbitMq((ctx, cfg) =>
    {
        cfg.Host("rabbitmq", "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });
        cfg.ConfigureEndpoints(ctx); // crea la queue automáticamente
    });
});

public class MiConsumer : IConsumer<DireccionAgregadaIntegrationEvent>
{
    public async Task Consume(ConsumeContext<DireccionAgregadaIntegrationEvent> context)
    {
        var evento = context.Message;
        // usar evento.Fecha, evento.Latitud, evento.Longitud, etc.
    }
}
```

---

## Patrón Transactional Outbox

### El problema

Publicar un evento a RabbitMQ y guardar el dato en la BD son dos operaciones distintas. Si el proceso muere entre una y la otra, el evento se pierde o se publica sin que el dato haya sido persistido. El resultado: inconsistencia entre servicios.

### La solución

El patrón Outbox resuelve esto: **el evento se guarda en la misma BD y en la misma transacción que el dato de negocio**, y se publica a RabbitMQ en un paso posterior. Así es imposible que uno se persista sin el otro.

### Implementación

El patrón se divide en tres piezas:

---

#### Pieza 1 — La tabla `OutboxMessages`

```
Infrastructure/Outbox/OutboxMessage.cs
```

Cada fila representa un evento pendiente de publicar:

| Columna | Tipo | Descripción |
|---------|------|-------------|
| `Id` | Guid | Identificador único |
| `Type` | string | Nombre del domain event (ej: `"CalendarioCreado"`) |
| `Payload` | string | El evento serializado en JSON |
| `OccurredOnUtc` | DateTime | Cuándo ocurrió |
| `ProcessedOnUtc` | DateTime? | `null` = pendiente · fecha = ya publicado |

---

#### Pieza 2 — Escritura al Outbox (mismo `SaveChanges`)

```
Infrastructure/Persistence/CalendarioDbContext.cs
```

Se sobreescribe `SaveChangesAsync()`. Antes de persistir, EF Core inspecciona todos los agregados y extrae sus `DomainEvents`:

```csharp
// Flujo dentro de SaveChangesAsync():
1. Recopilar DomainEvents de todos los AggregateRoot trackeados
2. Serializar cada evento a JSON → OutboxMessage
3. Insertar los OutboxMessages en la tabla
4. base.SaveChangesAsync()  ← todo en UNA SOLA transacción
```

Si la transacción falla, el dato de negocio y el evento se revierten juntos.

---

#### Pieza 3 — Lectura y publicación a RabbitMQ (background service)

```
Infrastructure/Outbox/OutboxProcessorService.cs
```

Un `BackgroundService` que se ejecuta cada **5 segundos**:

```
1. SELECT TOP 20 FROM OutboxMessages WHERE ProcessedOnUtc IS NULL
2. Para cada mensaje:
   a. Deserializar el JSON al tipo de evento correspondiente
   b. publishEndpoint.Publish(...) → RabbitMQ
   c. ProcessedOnUtc = DateTime.UtcNow
3. SaveChangesAsync() → marca todos como procesados
```

Si RabbitMQ está caído, el servicio logea el error y reintenta en el próximo ciclo. Los mensajes **nunca se pierden**.

---

### Flujo completo de un evento

```
[HTTP Request]
     │
     ▼
[Command Handler]
  calendario.AgregarDireccion(...)
  → Raise(new DireccionAgregada(...))   ← se acumula en memoria
     │
     ▼
[UnitOfWork.CommitAsync()]
  → SaveChangesAsync()
     ┌──────────────────────────────────────┐
     │  BEGIN TRANSACTION                    │
     │   INSERT INTO Direcciones (...)       │  ← dato de negocio
     │   INSERT INTO OutboxMessages (...)    │  ← evento serializado
     │  COMMIT                               │
     └──────────────────────────────────────┘
     │
     │  (cada 5 segundos)
     ▼
[OutboxProcessorService]
  SELECT OutboxMessages WHERE ProcessedOnUtc IS NULL
     │
     ▼
  publishEndpoint.Publish(DireccionAgregadaIntegrationEvent)
     │
     ▼
  RabbitMQ Exchange
     │
     ▼
  [Microservicio consumidor recibe el evento]
```

---

## Estructura del proyecto

```
CalendarioEntregas.Domain/
├── Agregados/          → CalendarioEntrega (AggregateRoot), Direccion
├── Eventos/            → DomainEvents (CalendarioCreado, DireccionAgregada, ...)
├── ValueObjects/       → Latitud, Longitud
└── Abstractions/       → AggregateRoot, IDomainEvent, Result, Error

CalendarioEntregas.Application/
└── Calendario/         → Commands y Queries (CQRS con MediatR)

CalendarioEntregas.Infrastructure/
├── Outbox/             → OutboxMessage.cs, OutboxProcessorService.cs
├── Messaging/
│   ├── Consumers/      → PlanAlimenticioCreadoConsumer.cs
│   └── IntegrationEvents/  → Contratos publicados y recibidos
├── Persistence/        → CalendarioDbContext.cs (escribe al Outbox en SaveChanges)
└── Repositories/

CalendarioEntregas.WebApi/
└── Controllers/        → CalendarioController (todos con [Authorize])
```

---

## Reglas de negocio

1. Solo se puede modificar, cancelar o reactivar una entrega con **2 días de anticipación**
2. Las direcciones deben estar dentro del rango de fechas del calendario
3. Solo puede haber **una dirección por fecha** en cada calendario
4. Latitud: `-90` a `90` · Longitud: `-180` a `180`

---

## Tests

```bash
dotnet test tests/CalendarioEntregas.Tests/
```

Cubre: creación, agregar dirección, modificar, cancelar, reactivar, desactivar, próxima entrega, direcciones activas.
