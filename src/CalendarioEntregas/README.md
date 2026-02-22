# Microservicio Calendario de Entregas

Este es un microservicio independiente que gestiona calendarios de entregas para pacientes en el sistema de Nur-tricenter.

## Características

- ✅ Creación de calendarios de entrega basados en planes alimenticios
- ✅ Agregación de direcciones de entrega por día
- ✅ Modificación de direcciones con validación de 2 días de anticipación
- ✅ Cancelación de entregas con validación de 2 días de anticipación
- ✅ Reactivación de entregas canceladas con validación de 2 días de anticipación
- ✅ Consulta de próxima entrega
- ✅ Consulta de direcciones activas
- ✅ Desactivación de calendarios
- ✅ Consulta de estado del calendario y próximas entregas
- ✅ Geolocalización (Latitud/Longitud) para cada dirección

## Estructura del Proyecto

### Domain (Dominio - Reglas de Negocio)
- **Agregados**: `CalendarioEntrega`, `Direccion`
- **Value Objects**: `Latitud`, `Longitud`
- **Eventos de Dominio**: `CalendarioCreado`, `DireccionAgregada`, `DireccionModificada`, `EntregaCancelada`
- **Repositorios**: `ICalendarioEntregaRepository`

### Application (Casos de Uso)
- **Commands**:
  - `CreateCalendarioCommand`: Crea un nuevo calendario
  - `AgregarDireccionCommand`: Agrega una dirección a un día
  - `ModificarDireccionCommand`: Modifica una dirección (con validación de 2 días)
  - `MarcarDiaNoEntregaCommand`: Marca un día como no entregable
  - `ReactivarEntregaCommand`: Reactiva un día marcado como no entregable
  - `DesactivarCalendarioCommand`: Desactiva un calendario

- **Queries**:
  - `GetCalendarioQuery`: Obtiene detalles de un calendario
  - `ListarCalendariosQuery`: Lista calendarios (con filtro opcional por paciente)
  - `ObtenerProximaEntregaQuery`: Obtiene la próxima entrega activa
  - `ObtenerDireccionesActivasQuery`: Obtiene todas las direcciones activas desde hoy

### Infrastructure (Persistencia)
- **DbContext**: `CalendarioDbContext` (PostgreSQL)
- **Repository**: `CalendarioEntregaRepository`
- **Unit of Work**: Para transacciones

### WebApi (API REST)
- **Controller**: `CalendarioController` con endpoints protegidos

## Endpoints

### Crear Calendario
```
POST /api/calendario/crear
Content-Type: application/json

{
  "pacienteId": "guid",
  "planAlimenticioId": "guid",
  "fechaInicio": "2025-02-01",
  "fechaFin": "2025-02-15"
}
```

### Agregar Dirección
```
POST /api/calendario/{calendarioId}/agregar-direccion
Content-Type: application/json

{
  "fecha": "2025-02-05",
  "direccion": "Av. Principal 123",
  "referencias": "Frente al parque",
  "latitud": -12.0464,
  "longitud": -77.0428
}
```

### Modificar Dirección (2 días de anticipación requeridos)
```
PUT /api/calendario/{calendarioId}/modificar-direccion
Content-Type: application/json

{
  "fecha": "2025-02-05",
  "nuevaDireccion": "Av. Nueva 456",
  "referencias": "Cerca de la estación",
  "latitud": -12.0500,
  "longitud": -77.0500
}
```

### Marcar No Entrega (2 días de anticipación requeridos)
```
POST /api/calendario/{calendarioId}/marcar-no-entrega
Content-Type: application/json

{
  "fecha": "2025-02-05"
}
```

### Reactivar Entrega (2 días de anticipación requeridos)
```
POST /api/calendario/{calendarioId}/reactivar-entrega
Content-Type: application/json

{
  "fecha": "2025-02-05"
}
```

### Obtener Próxima Entrega
```
GET /api/calendario/{calendarioId}/proxima-entrega
```

Respuesta:
```json
{
  "id": "guid",
  "fecha": "2025-02-05",
  "direccion": "Av. Principal 123",
  "referencias": "Frente al parque",
  "latitud": -12.0464,
  "longitud": -77.0428,
  "esEntregaActiva": true,
  "diasRestantes": 5
}
```

### Obtener Direcciones Activas
```
GET /api/calendario/{calendarioId}/direcciones-activas
```

### Desactivar Calendario
```
POST /api/calendario/{calendarioId}/desactivar
```

### Obtener Calendario
```
GET /api/calendario/{calendarioId}
```

### Listar Calendarios
```
GET /api/calendario?pacienteId={pacienteId}
```

## Estructura de Carpetas

```
src/CalendarioEntregas/
├── CalendarioEntregas.Domain/
│   ├── Agregados/
│   ├── ValueObjects/
│   ├── Eventos/
│   └── Repositories/
├── CalendarioEntregas.Application/
│   ├── Calendario/
│   │   ├── CreateCalendario/
│   │   ├── AgregarDireccion/
│   │   ├── ModificarDireccion/
│   │   ├── MarcarDiaNoEntrega/
│   │   └── Queries/
│   └── DependencyInjection.cs
├── CalendarioEntregas.Infrastructure/
│   ├── Persistence/
│   ├── Repositories/
│   ├── DependencyInjection.cs
│   └── UnitOfWork.cs
└── CalendarioEntregas.WebApi/
    ├── Controllers/
    ├── appsettings.json
    └── Program.cs

tests/CalendarioEntregas.Tests/
├── Calendario/
│   ├── CreateCalendarioHandlerTests.cs
│   ├── AgregarDireccionHandlerTests.cs
│   └── MarcarDiaNoEntregaHandlerTests.cs
```

## Tecnologías

- **.NET 8.0**
- **Entity Framework Core** (PostgreSQL)
- **MediatR** (CQRS)
- **xUnit** (Testing)
- **Moq** (Mocking)
- **FluentAssertions** (Assertions)

## Reglas de Negocio

1. **Validación de Días**: Solo se pueden modificar direcciones o cancelar entregas con **2 días de anticipación**
2. **Rango de Fechas**: Las direcciones deben estar dentro del rango de fechas del calendario
3. **Unicidad**: Solo puede haber una dirección por fecha en cada calendario
4. **Validación Geográfica**: 
   - Latitud debe estar entre -90 y 90
   - Longitud debe estar entre -180 y 180

## Configuración

### Variables de Entorno
```
ConnectionStrings__CalendarioDatabase=Host=localhost;Port=5432;Database=calendario_db;Username=postgres;Password=postgres;
ASPNETCORE_ENVIRONMENT=Development
```

### appsettings.json
```json
{
  "ConnectionStrings": {
    "CalendarioDatabase": "Host=localhost;Port=5432;Database=calendario_db;Username=postgres;Password=postgres;"
  }
}
```

## Ejecución

### Local
```bash
dotnet run --project src/CalendarioEntregas/CalendarioEntregas.WebApi
```

### Con Docker
```bash
docker-compose -f src/CalendarioEntregas/docker-compose.yml up
```

## Tests

```bash
dotnet test tests/CalendarioEntregas.Tests/
```

### Tests Disponibles
- `CreateCalendarioHandlerTests`: Pruebas para creación de calendarios
- `AgregarDireccionHandlerTests`: Pruebas para agregar direcciones
- `ModificarDireccionHandlerTests`: Pruebas para modificar direcciones
- `MarcarDiaNoEntregaHandlerTests`: Pruebas para marcar días sin entrega
- `ReactivarEntregaHandlerTests`: Pruebas para reactivar entregas
- `ObtenerProximaEntregaHandlerTests`: Pruebas para obtener próxima entrega
- `ObtenerDireccionesActivasHandlerTests`: Pruebas para obtener direcciones activas
- `DesactivarCalendarioHandlerTests`: Pruebas para desactivar calendarios

## Migraciones de Base de Datos

Se incluye una migración inicial `InitialCreate` que crea las tablas `Calendarios` y `Direcciones`.

### Aplicar Migraciones Localmente
```bash
# Crear la base de datos con las migraciones
dotnet ef database update --project src/CalendarioEntregas/CalendarioEntregas.Infrastructure --context CalendarioDbContext

# O crear una nueva migración
dotnet ef migrations add NombreMigracion --project src/CalendarioEntregas/CalendarioEntregas.Infrastructure --context CalendarioDbContext
```

## Autor
Microservicio desarrollado como parte del Trabajo Final - Sistema de Gestión Integral de Nur-tricenter
