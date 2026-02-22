# 🔬 Verificación Post-Implementación

## 1. Compilación

```bash
# Compilar solución completa
dotnet build src/CalendarioEntregas/CalendarioEntregas.sln

# Output esperado: ✓ Build succeeded
```

## 2. Tests

```bash
# Ejecutar todos los tests
dotnet test tests/CalendarioEntregas.Tests/

# Output esperado:
# ✓ 29 passed in X ms
# - CreateCalendarioHandlerTests: 2/2 ✓
# - AgregarDireccionHandlerTests: 4/4 ✓
# - ModificarDireccionHandlerTests: 5/5 ✓
# - MarcarDiaNoEntregaHandlerTests: 2/2 ✓
# - ReactivarEntregaHandlerTests: 4/4 ✓
# - ObtenerProximaEntregaHandlerTests: 5/5 ✓
# - ObtenerDireccionesActivasHandlerTests: 4/4 ✓
# - DesactivarCalendarioHandlerTests: 3/3 ✓
```

## 3. Migraciones

```bash
# Asegurarse que PostgreSQL está ejecutándose
# Puerto: 5432 (o el configurado en appsettings.json)

# Aplicar migraciones 
dotnet ef database update \
  --project src/CalendarioEntregas/CalendarioEntregas.Infrastructure \
  --context CalendarioDbContext

# Output esperado:
# Applying migration '20250221000000_InitialCreate'.
# Done.
```

## 4. Verificar Base de Datos

```sql
-- Conectarse a PostgreSQL
psql -h localhost -U postgres -d calendario_db

-- Listar tablas
\dt

-- Output esperado:
--  public | calendarios  | table | postgres
--  public | direcciones  | table | postgres

-- Ver estructura de tablas
\d calendarios
\d direcciones

-- Verificar índice único
SELECT * FROM information_schema.table_constraints 
WHERE table_name = 'direcciones' AND constraint_type = 'UNIQUE';
```

## 5. Ejecutar Servidor

```bash
# Iniciar el API
dotnet run --project src/CalendarioEntregas/CalendarioEntregas.WebApi

# Output esperado:
# info: Microsoft.Hosting.Lifetime[14]
#      Now listening on: http://localhost:7020
#      Now listening on: https://localhost:7021
# Application started. Press Ctrl+C to exit.
```

## 6. Verificar Swagger

Abrir navegador: `http://localhost:7020/swagger`

### Endpoints a probar:

#### 6.1 Crear Calendario
```bash
POST /api/calendario/crear

Body:
{
  "pacienteId": "550e8400-e29b-41d4-a716-446655440000",
  "planAlimenticioId": "550e8400-e29b-41d4-a716-446655440001",
  "fechaInicio": "2025-02-25",
  "fechaFin": "2025-03-25"
}

Response esperado (201):
{
  "calendarioId": "a1b2c3d4-e5f6-47g8-h9i0-j1k2l3m4n5o6"
}
```

#### 6.2 Agregar Dirección
```bash
POST /api/calendario/{calendarioId}/agregar-direccion

Body:
{
  "fecha": "2025-03-01",
  "direccion": "Av. Principal 123, Dept 501",
  "referencias": "Frente a la estación",
  "latitud": -12.0464,
  "longitud": -77.0428
}

Response esperado (200):
{
  "direccionId": "b2c3d4e5-f6g7-48h9-i0j1-k2l3m4n5o6p7"
}
```

#### 6.3 Obtener Calendario
```bash
GET /api/calendario/{calendarioId}

Response esperado (200):
{
  "id": "a1b2c3d4-e5f6-47g8-h9i0-j1k2l3m4n5o6",
  "pacienteId": "550e8400-e29b-41d4-a716-446655440000",
  "planAlimenticioId": "550e8400-e29b-41d4-a716-446655440001",
  "fechaInicio": "2025-02-25",
  "fechaFin": "2025-03-25",
  "activo": true,
  "porcentajeCompletado": 0,
  "direcciones": [
    {
      "id": "b2c3d4e5-f6g7-48h9-i0j1-k2l3m4n5o6p7",
      "fecha": "2025-03-01",
      "direccion": "Av. Principal 123, Dept 501",
      "referencias": "Frente a la estación",
      "latitud": -12.0464,
      "longitud": -77.0428,
      "esEntregaActiva": true,
      "diasRestantes": 9
    }
  ]
}
```

#### 6.4 Obtener Próxima Entrega
```bash
GET /api/calendario/{calendarioId}/proxima-entrega

Response esperado (200):
{
  "id": "b2c3d4e5-f6g7-48h9-i0j1-k2l3m4n5o6p7",
  "fecha": "2025-03-01",
  "direccion": "Av. Principal 123, Dept 501",
  "referencias": "Frente a la estación",
  "latitud": -12.0464,
  "longitud": -77.0428,
  "esEntregaActiva": true,
  "diasRestantes": 9
}
```

#### 6.5 Obtener Direcciones Activas
```bash
GET /api/calendario/{calendarioId}/direcciones-activas

Response esperado (200):
[
  {
    "id": "b2c3d4e5-f6g7-48h9-i0j1-k2l3m4n5o6p7",
    "fecha": "2025-03-01",
    "direccion": "Av. Principal 123, Dept 501",
    ...
  }
]
```

#### 6.6 Marcar No Entrega (requiere 2 días)
```bash
POST /api/calendario/{calendarioId}/marcar-no-entrega

Body:
{
  "fecha": "2025-03-05"  # Al menos 2 días en el futuro
}

Response esperado (200):
{
  "mensaje": "Entrega marcada como no activa"
}

Response fallido (400):
{
  "error": "Calendario.CancelacionError",
  "mensaje": "No se puede cancelar la entrega. Requiere 2 días de anticipación."
}
```

#### 6.7 Modificar Dirección (requiere 2 días)
```bash
PUT /api/calendario/{calendarioId}/modificar-direccion

Body:
{
  "fecha": "2025-03-05",
  "nuevaDireccion": "Av. Nueva 456",
  "referencias": "Cerca de la plaza",
  "latitud": -12.05,
  "longitud": -77.05
}

Response esperado (200):
{
  "mensaje": "Dirección modificada exitosamente"
}
```

#### 6.8 Reactivar Entrega (requiere 2 días)
```bash
POST /api/calendario/{calendarioId}/reactivar-entrega

Body:
{
  "fecha": "2025-03-05"
}

Response esperado (200):
{
  "mensaje": "Entrega reactivada exitosamente"
}
```

#### 6.9 Desactivar Calendario
```bash
POST /api/calendario/{calendarioId}/desactivar

Response esperado (200):
{
  "mensaje": "Calendario desactivado exitosamente"
}
```

#### 6.10 Listar Calendarios
```bash
GET /api/calendario
GET /api/calendario?pacienteId=550e8400-e29b-41d4-a716-446655440000

Response esperado (200):
{
  "value": [
    {
      "id": "a1b2c3d4-e5f6-47g8-h9i0-j1k2l3m4n5o6",
      ...
    }
  ]
}
```

## 7. Validaciones a Probar

### 7.1 Validación de Días (2 días requeridos)

```bash
# Hoy es: 2025-02-21

# ❌ Marcar entrega de mañana (2025-02-22) - Solo 1 día
POST /marcar-no-entrega
Body: { "fecha": "2025-02-22" }
Response: 400 "No se puede cancelar la entrega. Requiere 2 días de anticipación."

# ✓ Marcar entrega de pasado mañana (2025-02-23) - 2 días
POST /marcar-no-entrega
Body: { "fecha": "2025-02-23" }
Response: 200 OK
```

### 7.2 Validación de Geolocalización

```bash
# ❌ Latitud fuera de rango
POST /agregar-direccion
Body: { ..., "latitud": -91 }
Response: 400 "La latitud debe estar entre -90 y 90 grados"

# ❌ Longitud fuera de rango
POST /agregar-direccion
Body: { ..., "longitud": 181 }
Response: 400 "La longitud debe estar entre -180 y 180 grados"

# ✓ Valores correctos
POST /agregar-direccion
Body: { ..., "latitud": -12.0474, "longitud": -77.0428 }
Response: 200 OK
```

### 7.3 Validación de Unicidad (una dirección por día)

```bash
# Primera dirección
POST /agregar-direccion
Body: { "fecha": "2025-03-01", ... }
Response: 200 OK

# Intento de agregar otra para el mismo día
POST /agregar-direccion
Body: { "fecha": "2025-03-01", ... }
Response: 400 "Ya existe una dirección registrada para el día 2025-03-01"
```

### 7.4 Validación de Fechas del Calendario

```bash
# Fecha fuera del rango
POST /agregar-direccion
Body: { "fecha": "2025-04-01", ... }  # Último día de calendar: 2025-03-25
Response: 400 "La fecha 2025-04-01 está fuera del rango del calendario..."
```

## 8. Checklist de Aceptación

- [ ] Compilación exitosa (sin warnings)
- [ ] 29/29 tests pasan
- [ ] BD con tablas creadas
- [ ] Servidor inicia en puerto 7020 y 7021
- [ ] Swagger accesible
- [ ] Crear calendario funciona
- [ ] Agregar dirección funciona
- [ ] Obtener próxima entrega funciona
- [ ] Reactivar entrega funciona
- [ ] Validación de 2 días funciona
- [ ] Validación de geolocalización funciona
- [ ] Validación de unicidad funciona
- [ ] Marcar no entrega funciona
- [ ] Desactivar calendario funciona
- [ ] Listar calendarios funciona

## 9. Documentación Disponible

1. **README.md**: Descripción general, endpoints, uso
2. **ARCHITECTURE.md**: Decisiones de diseño, patrones
3. **INTEGRATION_GUIDE.md**: Integración con otros servicios
4. **CHANGES.md**: Resumen de cambios realizados
5. **Este archivo**: Verificación post-implementación

## 10. Docker (Opcional)

```bash
# Construir imagen
docker-compose -f src/CalendarioEntregas/docker-compose.yml build

# Ejecutar con BD
docker-compose -f src/CalendarioEntregas/docker-compose.yml up

# Acceder
curl http://localhost:7020/api/calendario
```

##  11. Troubleshooting

### Error: "Connection string not found"
- Verificar `appsettings.json`
- Verificar que PostgreSQL está en puerto 5432
- Revisar credenciales (postgres/postgres)

### Error: "Migration not found"
- Ejecutar: `dotnet ef database update --context CalendarioDbContext`
- Asegurarse de estar en carpeta del proyecto

### Error: "Port 7020 already in use"
- Cambiar puerto en `Program.cs`
- O terminar proceso usando puerto

### Tests fallan
- Verificar que no hay BD real interferencia
- Verificar Moq está instalado
- Ejecutar: `dotnet restore`

---

**❌ Si algún punto falla → Revisar la documentación y CHANGES.md**

**✅ Si todos los puntos pasan → Implementación completa y lista para integración**
