# Arquitectura e Integración del Microservicio CalendarioEntregas

## Descripción General

El microservicio CalendarioEntregas es responsable de gestionar los calendarios de entrega de planes alimenticios para los pacientes de Nur-tricenter. Implementa una arquitectura de **Clean Architecture** con patrones **CQRS** y **Event Sourcing**.

## Decisiones Arquitectónicas

### 1. Clean Architecture (Capas)

```
Domain (Dominio)
    ↓
Application (Casos de Uso)
    ↓
Infrastructure (Persistencia)
    ↓
WebApi (API REST)
```

- **Domain**: Contiene la lógica de negocio pura, sin dependencias externas
- **Application**: Implementa los casos de uso usando MediatR (CQRS)
- **Infrastructure**: Implementa la persistencia con Entity Framework Core
- **WebApi**: Expone los endpoints REST

### 2. CQRS (Command Query Responsibility Segregation)

- **Commands**: Modifican el estado (CreateCalendario, AgregarDireccion, MarcarNoEntrega, etc.)
- **Queries**: Consultan sin modificar (GetCalendario, ListarCalendarios, ObtenerProximaEntrega, etc.)

### 3. Agregados

- **CalendarioEntrega**: Agregado raíz que contiene la lógica del calendario
- **Direccion**: Entidad que pertenece al agregado CalendarioEntrega

### 4. Value Objects

- **Latitud**: Valor entre -90 y 90
- **Longitud**: Valor entre -180 y 180

### 5. Domain Events

Aunque están definidos, los eventos de dominio no se persisten ni se publican. Para una implementación completa, se necesitaría:
- Persistencia de eventos (Event Store)
- Publicación a un broker (RabbitMQ, Kafka)

## Validaciones de Negocio

### Regla de 2 Días de Anticipación

Cualquier modificación a una dirección (cambiar dirección, marcar como no entrega, reactivar) requiere **2 días de anticipación**.

```csharp
// Cálculo correcto de días
var diasHasta = (Fecha.ToDateTime(TimeOnly.MinValue) - hoy.ToDateTime(TimeOnly.MinValue)).Days;
return diasHasta >= 2;
```

### Unicidad de Direcciones por Día

Una calendario no puede tener más de una dirección por fecha.

```csharp
entity.HasIndex(d => new { d.CalendarioId, d.Fecha }).IsUnique();
```

## Patrones Usados

### Repository Pattern
```csharp
public interface ICalendarioEntregaRepository
{
    Task<CalendarioEntrega?> GetByIdAsync(Guid id);
    Task<CalendarioEntrega?> GetByPacienteIdAsync(Guid pacienteId);
    Task<IEnumerable<CalendarioEntrega>> GetAllAsync();
    Task AddAsync(CalendarioEntrega calendario);
    Task UpdateAsync(CalendarioEntrega calendario);
    Task DeleteAsync(Guid id);
}
```

### Unit of Work Pattern
```csharp
public interface IUnitOfWork
{
    Task CommitAsync(CancellationToken cancellationToken = default);
    Task RollbackAsync(CancellationToken cancellationToken = default);
    void Dispose();
}
```

### Result Pattern (Railway Oriented Programming)
```csharp
public class Result<T>
{
    public bool IsSuccess { get; set; }
    public bool IsFailure => !IsSuccess;
    public T? Value { get; set; }
    public Error? Error { get; set; }
}
```

## Integración con Otros Microservicios

### Eventos que Este Servicio Produce

El servicio define eventos pero no los publica. Para integración futura:

```csharp
public record CalendarioCreado(/*...*/) : IDomainEvent;
public record DireccionAgregada(/*...*/) : IDomainEvent;
public record DireccionModificada(/*...*/) : IDomainEvent;
public record EntregaCancelada(/*...*/) : IDomainEvent;
```

### Cómo Consumir Estos Eventos

1. Implementar `IDomainEventHandler<TDomainEvent>`
2. Registrar en DependencyInjection
3. MediatR publicará automáticamente los eventos

Ejemplo:
```csharp
public class EnviarNotificacionCalendarioCreado : IDomainEventHandler<CalendarioCreado>
{
    public async Task Handle(CalendarioCreado @event, CancellationToken cancellationToken)
    {
        // Enviar notificación al paciente
        // Crear tarea de preparación de comida
    }
}
```

### APIs que Necesita Este Servicio

Para una implementación completa, se necesita información de:

1. **Pacientes**: ID, nombre, correo, teléfono
   - Referencia: `PacienteId` en CalendarioEntrega

2. **Planes Alimenticios**: ID, nombre, descripción, recetas
   - Referencia: `PlanAlimenticioId` en CalendarioEntrega

3. **Entregas**: Seguimiento del estado de entregas, geolocalización del repartidor
   - Relacionado con `Direccion` (contiene geolocalización)

## Tipos de Consultas

### Consultas Simples (Sin Estado)
```
GET /api/calendario/{calendarioId}
GET /api/calendario?pacienteId={pacienteId}
```

### Consultas Complejas (Con Filtros Temporales)
```
GET /api/calendario/{calendarioId}/proxima-entrega
GET /api/calendario/{calendarioId}/direcciones-activas
```

## Ciclo de Vida de un Calendario

```
1. CREACIÓN
   CreateCalendarioCommand
   → Nuevo calendario sin direcciones
   → Evento: CalendarioCreado

2. CONFIGURACIÓN
   AgregarDireccionCommand (múltiples veces)
   → Se agregan direcciones día a día
   → Evento: DireccionAgregada

3. CAMBIOS
   ModificarDireccionCommand (requiere 2 días)
   → Cambiar dirección
   → Evento: DireccionModificada

4. CANCELACIONES TEMPORALES
   MarcarDiaNoEntregaCommand (requiere 2 días)
   → Marcar un día como no entrega
   → Evento: EntregaCancelada

   ReactivarEntregaCommand (requiere 2 días)
   → Reactivar un día marcado como no entrega

5. FINALIZACIÓN
   DesactivarCalendarioCommand
   → Calendario inactivo
   → No aparece en listados
```

## Mejoras Futuras

1. **Persistencia de Eventos**: Implementar Event Store
2. **Publicación de Eventos**: Integrar con RabbitMQ/Kafka
3. **Optimización de Rutas**: Calcular ruta óptima con geolocalización
4. **Notificaciones**: Integrar con servicio de notificaciones
5. **Auditoria Completa**: Registrar todos los cambios con usuario y timestamp
6. **Importación en Lote**: Cargar calendarios desde CSV
7. **Sincronización**: Sincronizar con calendario de Google/Outlook
8. **Alertas**: Notificar cambios a repartidores
9. **Estadísticas**: Dashboard de entregas completadas vs canceladas
10. **Split Testing**: Versiones alternativas de mensajes de notificación

## Referencias

- [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [CQRS Pattern](https://martinfowler.com/bliki/CQRS.html)
- [Repository Pattern](https://martinfowler.com/eaaCatalog/repository.html)
- [Domain-Driven Design](https://en.wikipedia.org/wiki/Domain-driven_design)
