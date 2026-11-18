# CLAUDE.md

Instrucciones para Claude Code (agente de IA) al trabajar en este repositorio.

## Proyecto

Microservicio **Calendario de Entregas** del caso de estudio NutriCenter (catering nutricional).
Cuando se crea un plan alimenticio, genera el calendario de entregas y publica cambios vía RabbitMQ.

- .NET 8, Clean Architecture + DDD + CQRS (MediatR)
- `src/CalendarioEntregas/CalendarioEntregas.Domain` – agregados, value objects, domain events
- `src/CalendarioEntregas/CalendarioEntregas.Application` – comandos y queries
- `src/CalendarioEntregas/CalendarioEntregas.Infrastructure` – EF Core (PostgreSQL), Outbox, RabbitMQ (Joselct.*), Consul
- `src/CalendarioEntregas/CalendarioEntregas.WebApi` – controllers, expuesto por el API Gateway en `/api/calendario/*`
- `tests/CalendarioEntregas.Tests` – xUnit

## Comandos

```bash
dotnet restore calendar-service.sln
dotnet build calendar-service.sln
dotnet test calendar-service.sln
dotnet format calendar-service.sln                        # aplica .editorconfig
dotnet format whitespace calendar-service.sln --verify-no-changes
```

## Convenciones obligatorias

- **Indentación con tabs**, nunca espacios (`.editorconfig`). Ejecutar `dotnet format` antes de dar un cambio por terminado.
- Respetar las reglas de `SonarAnalyzer.CSharp` (`Directory.Build.props`); no introducir warnings nuevos.
- Lógica de negocio en Domain/Application; Infrastructure solo integra.
- Todo cambio en Domain/Application lleva prueba unitaria (la CI exige ≥ 80 % de cobertura de líneas en esas capas).
- Mensajes de commit en formato Conventional Commits (`feat:`, `fix:`, `chore:`…), en español.
- No editar migraciones de EF existentes; crear una nueva.

## Calidad y CI

- **Husky + lint-staged** (`npm install` instala los hooks):
  - `pre-commit`: `dotnet format` sobre los `.cs` en staging + pruebas unitarias.
  - `pre-push`: verifica formato (tabs) y compila con los analizadores de Sonar.
- **GitHub Actions** (`.github/workflows/ci.yml`): restore → SonarCloud begin → build → test con cobertura → SonarCloud end → gate de cobertura 80 %.
- **CD** (`.github/workflows/cd.yml`): tras CI exitosa en `main`, construye la imagen Docker y despliega.
- No usar `--no-verify` para saltarse los hooks.
