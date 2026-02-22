# Guía de Integración - Microservicio CalendarioEntregas

## Resumen de Funcionalidades Implementadas

Según el enunciado del trabajo final, se implementaron **todas las funcionalidades requeridas**:

### ✅ Del Enunciado

1. **Crear calendario de entrega** basado en días contratados
   - `POST /api/calendario/crear` → CreateCalendarioCommand
   - Requiere: PacienteId, PlanAlimenticioId, FechaInicio, FechaFin

2. **Asignar dirección de entrega por día** (puede variar)
   - `POST /api/calendario/{calendarioId}/agregar-direccion` → AgregarDireccionCommand
   - Requiere: Fecha, Dirección, Referencias, Latitud, Longitud
   - Validación: Una sola dirección por día

3. **Modificar dirección** (con 2 días de anticipación)
   - `PUT /api/calendario/{calendarioId}/modificar-direccion` → ModificarDireccionCommand
   - Validación: Requiere 2 días de anticipación

4. **Marcar días sin entrega** (con 2 días de anticipación)
   - `POST /api/calendario/{calendarioId}/marcar-no-entrega` → MarcarDiaNoEntregaCommand
   - Validación: Requiere 2 días de anticipación

5. **Geolocalización** (Latitud/Longitud)
   - Se almacenan en cada Dirección
   - Validación: Latitud [-90, 90], Longitud [-180, 180]

6. **Consultar evolución** del calendario
   - `GET /api/calendario/{calendarioId}` → Obtiene estado completo con progreso
   - Incluye: PorcentajeCompletado, Direcciones con DíasRestantes

### ✅ Funcionalidades Adicionales Implementadas

7. **Reactivar entrega** (con 2 días de anticipación)
   - `POST /api/calendario/{calendarioId}/reactivar-entrega` → ReactivarEntregaCommand
   - Permite readecuar un día marcado como sin entrega

8. **Obtener próxima entrega**
   - `GET /api/calendario/{calendarioId}/proxima-entrega` → ObtenerProximaEntregaQuery
   - Retorna la próxima dirección activa a entregar

9. **Obtener direcciones activas**
   - `GET /api/calendario/{calendarioId}/direcciones-activas` → ObtenerDireccionesActivasQuery
   - Lista todas las direcciones activas desde hoy en adelante

10. **Desactivar calendario**
    - `POST /api/calendario/{calendarioId}/desactivar` → DesactivarCalendarioCommand
    - Finaliza un calendario cuando el paciente deja de contratar el servicio

11. **Listar calendarios**
    - `GET /api/calendario` → Todos los calendarios
    - `GET /api/calendario?pacienteId={id}` → Calendario de un paciente específico

## Matriz de Requisitos vs Implementación

| Requisito | Status | Endpoint/Handler | Validación |
|-----------|--------|-----------------|-----------|
| Crear calendario | ✅ | `POST /crear` | Fecha fin > inicio |
| Agregar dirección | ✅ | `POST /agregar-direccion` | Una por fecha, geolocalización válida |
| Modificar dirección | ✅ | `PUT /modificar-direccion` | 2 días anticipación |
| Marcar sin entrega | ✅ | `POST /marcar-no-entrega` | 2 días anticipación |
| Reactivar entrega | ✅ | `POST /reactivar-entrega` | 2 días anticipación |
| Consultar evolución | ✅ | `GET /{id}` | Incluye % completado |
| Geolocalización | ✅ | Latitud/Longitud | Rango válido |
| Próxima entrega | ✅ | `GET /proxima-entrega` | Solo activas |
| Direcciones activas | ✅ | `GET /direcciones-activas` | Desde hoy |
| Desactivar | ✅ | `POST /desactivar` | Status activo |

## Cobertura de Tests

Todos los handlers poseen tests exhaustivos:

- **CreateCalendarioHandlerTests**: 2 tests (éxito, validación)
- **AgregarDireccionHandlerTests**: 4 tests (éxito, calendario no existe, latitud inválida)
- **ModificarDireccionHandlerTests**: 5 tests (éxito, sin 2 días, calendario no existe, dirección no existe, latitud inválida)
- **MarcarDiaNoEntregaHandlerTests**: 2 tests (éxito, sin 2 días)
- **ReactivarEntregaHandlerTests**: 4 tests (éxito, sin 2 días, calendario no existe, dirección no existe)
- **ObtenerProximaEntregaHandlerTests**: 5 tests (retorna próxima, retorna null, inactiva, calendario no existe)
- **ObtenerDireccionesActivasHandlerTests**: 4 tests (retorna todas, excluye inactivas, lista vacía, calendario no existe)
- **DesactivarCalendarioHandlerTests**: 3 tests (éxito, ya desactivado, no existe)

**Total: 29 tests**

## Cálculos de Días - Importante

Se corrigió el cálculo de días para usar `TimeSpan` en lugar de `DayNumber`:

```csharp
// ❌ Incorrecto (viejo)
var diasHasta = Fecha.DayNumber - hoy.DayNumber;

// ✅ Correcto (nuevo)
var diasHasta = (Fecha.ToDateTime(TimeOnly.MinValue) - hoy.ToDateTime(TimeOnly.MinValue)).Days;
```

Esto es importante porque `DayNumber` retorna el número de días desde el 1 de enero del año actual, no la diferencia real entre fechas.

## DB Context Configuración

Se configura con índice único para garantizar una sola dirección por día por calendario:

```csharp
entity.HasIndex(d => new { d.CalendarioId, d.Fecha }).IsUnique();
```

## Migraciones Incluidas

- `20250221000000_InitialCreate`: Crea tablas Calendarios y Direcciones
- `CalendarioDbContextModelSnapshot`: Snapshot del modelo

## Próximos Pasos para Integración

### 1. Integración con Servicio de Pacientes
```csharp
// En ObtenerCalendario, verificar que el pacienteId existe
var paciente = await _pacientesService.GetPacienteAsync(calendario.PacienteId);
if (paciente == null) return error;
```

### 2. Integración con Servicio de Planes
```csharp
// Al crear calendario, validar que el plan existe
var plan = await _planesService.GetPlanAsync(createCommand.PlanAlimenticioId);
if (plan == null) return error;
```

### 3. Integración con Servicio de Entregas
```csharp
// Al marcar dirección como activa, crear orden de entrega
await _entregasService.CrearOrdenEntrega(
    calendarioId: direccion.CalendarioId,
    direccionId: direccion.Id,
    fecha: direccion.Fecha,
    geolocalización: (dirección.Latitud, dirección.Longitud)
);
```

### 4. Publicación de Eventos
```csharp
// En los handlers, después de guardar:
await _eventPublisher.PublishAsync(new CalendarioCreado(...));
await _eventPublisher.PublishAsync(new DireccionAgregada(...));
```

### 5. Notificaciones al Paciente
```csharp
// Cuando se crea calendario o hay cambios
await _notificacionesService.NotificarPaciente(
    pacienteId: calendario.PacienteId,
    mensaje: "Tu calendario ha sido creado/modificado"
);
```

## Notas de Seguridad

1. **Validación de Paciente**: Implementar autorización para que un paciente solo vea sus calendarios
2. **Auditoría**: Registrar quién hizo cada cambio (usuario + timestamp)
3. **Soft Delete**: Considerar usar soft delete en lugar de hard delete
4. **Rate Limiting**: Proteger endpoints contra fuerza bruta

## Notas de Performance

1. El cálculo de `ObtenerPorcentajeCompletado()` es O(n), considerar caché si hay muchas direcciones
2. Las queries cargan por Include, considerar usar proyección si solo se necesitan ciertos campos
3. La búsqueda de ObtenerProximaEntrega es O(n), considerar índice en EF si es crítica

## Documentación Generada

- ✅ `README.md`: Descripción general, endpoints, estructura
- ✅ `ARCHITECTURE.md`: Decisiones arquitectónicas, patrones, mejoras futuras
- ✅ Este documento: Requisitos vs implementación
