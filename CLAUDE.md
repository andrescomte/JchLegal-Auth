# CLAUDE.md — AuthService

Guía de contexto para Claude Code. Lee este archivo antes de cualquier análisis o implementación.

---

## Propósito del proyecto

AuthService es un **servicio de autenticación centralizado y reutilizable** para ecosistemas .NET.
Está diseñado para ser consumido por múltiples aplicaciones futuras como proveedor único de identidad.

Responsabilidades del servicio:
- Registro y gestión de usuarios
- Autenticación con JWT (access token de corta vida)
- Refresh tokens con rotación segura
- Gestión de roles y permisos
- Auditoría de acciones y registro de intentos de login

---

## Stack técnico

| Capa | Tecnología |
|---|---|
| Runtime | .NET 9 |
| Framework web | ASP.NET Core 9 (minimal hosting) |
| ORM | Entity Framework Core 9 + Npgsql |
| Base de datos | PostgreSQL 15+ (`citext`, `pgcrypto`) |
| Mensajería interna | MediatR 12 (CQRS) |
| Autenticación | JWT Bearer (`Microsoft.AspNetCore.Authentication.JwtBearer`) |
| Logging | NLog 5 via `NLog.Web.AspNetCore` |
| Documentación API | Swagger (Swashbuckle.AspNetCore 9) |
| Tests unitarios | xUnit + Moq |
| Tests funcionales | xUnit + `Microsoft.AspNetCore.Mvc.Testing` |
| Cobertura | coverlet |
| Contenedor | Docker (imagen base `mcr.microsoft.com/dotnet/aspnet`) |

---

## Arquitectura DDD

El proyecto sigue Domain-Driven Design con separación en 4 proyectos:

```
AuthService.ApplicationApi      ← Capa de presentación (entrada HTTP)
AuthService.Domain              ← Núcleo del dominio (sin dependencias externas*)
AuthService.Infrastructure      ← Persistencia, contexto EF, logging
AuthService.UnitTest            ← Tests unitarios (xUnit + Moq)
AuthService.FunctionalTest      ← Tests de integración (TestHost)
```

### Dependencias entre capas

```
ApplicationApi → Domain
ApplicationApi → Infrastructure
Infrastructure → Domain
UnitTest       → ApplicationApi + Domain + Infrastructure
FunctionalTest → ApplicationApi + Domain + Infrastructure + UnitTest
```

`Domain` no debe depender de ningún otro proyecto del solution.

> (*) Excepción actual conocida: `UserService` en Domain importa `Microsoft.IdentityModel.Tokens`
> para generar JWT. Esto viola DDD estricto y deberá moverse a Infrastructure en una refactorización futura.

---

## Estructura de carpetas

```
AuthService.ApplicationApi/
├── Application/
│   ├── Command/Auth/           ← Comandos (escriben estado): Authenticate, RefreshToken, Register
│   └── Query/RolesQuery/       ← Queries (solo lectura): RolesListHandler
├── Controllers/                ← UserController, RolesController
├── Program.cs                  ← Configuración DI, JWT, MediatR
└── appsettings.json

AuthService.Domain/
├── Helpers/                    ← JwtSettings (POCO de configuración)
├── Models/                     ← Entidades de dominio
├── Repository/                 ← Interfaces de repositorio (contratos)
├── SeedWork/                   ← Entity, IRepository, IEntityRepository, IAppLogger, IAggregateRoot
└── Services/                   ← IUserService + UserService (lógica de autenticación)

AuthService.Infrastructure/
├── Context/                    ← AuthDbContext (EF Core, mappings completos)
├── Logging/                    ← LoggerAdapter<T> : IAppLogger<T>
└── Repository/                 ← Implementaciones: UsersRepository, TokensRepository, RolesRepository

pg-init/
└── 00_create_auth_db.sql       ← Script SQL de inicialización (tablas, índices, seeds)
```

---

## Modelos de dominio

Todas las entidades heredan de `Entity` (SeedWork).

| Entidad | Tabla | Descripción |
|---|---|---|
| `Users` | `users` | Usuario del sistema. Campos: Id, Username, Email, CreatedAt, IsActive |
| `UserPasswords` | `user_passwords` | Hash de contraseña (1:1 con Users). Tiene FailedAttempts |
| `UserStatus` | `user_status` | Catálogo: NEW, ACTIVE, BLOCKED, DELETED |
| `UserStatusHistory` | `user_status_history` | Historial de estados del usuario |
| `Roles` | `roles` | Roles del sistema (code: ADMIN, USER). Tiene HomeMenuId |
| `UserRoles` | `user_roles` | Relación N:M usuario-rol. IsPrimary indica rol principal |
| `Tokens` | `tokens` | Refresh tokens: TokenHash (SHA-256), TokenType, IssuedAt, ExpiresAt, RevokedAt, Used |
| `Menus` | `menus` | Menús jerárquicos (parent_id autorreferenciado) |
| `LoginAttempts` | `login_attempts` | Registro de intentos de login (éxito/fallo, IP, user-agent) |
| `AuditLogs` | `audit_logs` | Auditoría de acciones, datos en JSONB |

---

## Patrón CQRS con MediatR

Cada operación sigue la estructura `Request → Handler → Response`:

```
Application/Command/Auth/
├── AuthenticateRequest.cs      IRequest<AuthenticateResponse?>
├── AuthenticateHandler.cs      IRequestHandler<AuthenticateRequest, AuthenticateResponse?>
├── AuthenticateResponse.cs     { Token, RefreshToken }
├── RefreshTokenRequest.cs      IRequest<AuthenticateResponse?>
└── RefreshTokenHandler.cs      IRequestHandler<RefreshTokenRequest, AuthenticateResponse?>

Application/Query/RolesQuery/
├── RolesListRequest.cs         IRequest<RolesListResponse?>
├── RolesListHandler.cs         IRequestHandler<RolesListRequest, RolesListResponse?>
└── RolesListResponse.cs        { List<Roles> }
```

**Convención de nombres:**
- Comandos (mutan estado): carpeta `Command/`, sufijo `Request/Handler/Response`
- Queries (solo lectura): carpeta `Query/`, sufijo `Request/Handler/Response`
- Handlers devuelven tipos nullable (`T?`) cuando el resultado puede no existir

---

## Interfaces de repositorio (contratos del dominio)

```csharp
// SeedWork — contrato base
IEntityRepository<T> : IRepository   { T Create(T entity); }

// Repositorios específicos
IUsersRepository  : IEntityRepository<Users>  { GetByEmailAsync, CreateUserAsync }
IRolesRepository  : IEntityRepository<Roles>  { ReadAll() }
ITokensRepository : IEntityRepository<Tokens> { CreateTokenAsync, GetByHashAsync, UpdateTokenAsync }
```

Los repositorios se registran en `Program.cs` con `AddScoped`.

---

## Flujo de autenticación actual

```
POST /api/User/authenticate
  → AuthenticateHandler.Handle()
  → UserService.UserAuth(email, password, ip, email)   ← ip está hardcodeada (bug conocido)
      1. GetByEmailAsync(email) — incluye UserPasswords + UserStatusHistory(last).Status
      2. Verifica Status.Name == "Activo"               ← bug: debería ser Status.Code == "ACTIVE"
      3. Compara PasswordHash == password               ← BUG CRÍTICO: texto plano, debe ser BCrypt
      4. Genera JWT (NameIdentifier + Name)             ← falta claims de roles
      5. Genera refresh token (RandomBytes → SHA-256 → almacena hash)
  ← AuthenticateResponse { Token, RefreshToken }

POST /api/User/refresh
  → RefreshTokenHandler.Handle()
  → UserService.RefreshToken(refreshToken)
      1. SHA-256 del token recibido → busca por hash
      2. Valida: !Used, RevokedAt == null, ExpiresAt > now
      3. Marca token anterior como Used + RevokedAt = now (rotación)
      4. Genera nuevo JWT + nuevo refresh token
  ← AuthenticateResponse { Token, RefreshToken }
```

---

## Convenciones de código

### Generales
- Lenguaje: C# con `Nullable=enable` en todos los proyectos
- `ImplicitUsings=enable` en todos los proyectos
- Nombres de clases, propiedades y métodos en **PascalCase**
- Campos privados con prefijo `_camelCase`
- Archivos `.cs` llevan el nombre exacto de la clase que contienen

### Controladores
- Heredar de `ControllerBase` (no de `Controller`)
- Decorar con `[ApiController]` y `[Route("api/[controller]")]`
- Endpoints públicos (login, register, refresh): `[AllowAnonymous]`
- Endpoints protegidos: `[Authorize]`
- Usar `[ProducesResponseType]` para documentar todos los códigos de respuesta
- No poner lógica en controladores — delegar todo al mediator

### Handlers (MediatR)
- Un handler por archivo, sin lógica de negocio directa
- Delegar al servicio de dominio correspondiente
- Retornar `null` cuando el resultado no existe (el controlador decide el código HTTP)
- El tipo de retorno de los handlers es siempre nullable: `IRequestHandler<TReq, TResp?>`

### Repositorios
- Las interfaces viven en `Domain/Repository/`
- Las implementaciones viven en `Infrastructure/Repository/`
- Nunca exponer `IQueryable` fuera del repositorio
- Los métodos async llevan sufijo `Async`

### Logging
- Usar `IAppLogger<T>` (interfaz del dominio), nunca `ILogger<T>` directamente en Domain
- `IAppLogger<T>` se inyecta en repositorios y servicios que necesiten logging
- Implementación: `LoggerAdapter<T>` en Infrastructure

### Seguridad
- Las contraseñas SIEMPRE se hashean con BCrypt antes de almacenar
- Los refresh tokens se almacenan como SHA-256 del token real (nunca el token en claro)
- El JWT debe incluir claims: `NameIdentifier`, `Name`, y `Role` por cada rol del usuario
- La IP del cliente se obtiene del `HttpContext`, nunca se hardcodea

---

## Configuración (`appsettings.json`)

```json
{
  "ConnectionStrings": {
    "MainConnectionString": "Host=host.docker.internal;Port=5432;Database=auth_db;Username=auth_admin;Password=..."
  },
  "Jwt": {
    "Key": "...",        // mínimo 32 caracteres
    "Issuer": "AuthService",
    "Audience": "AuthServiceUsers"
  }
}
```

La sección `Jwt` es obligatoria. Si falta, `Program.cs` lanza `InvalidOperationException` al arrancar.
`JwtSettings` se registra con `services.Configure<JwtSettings>(...)` e `IOptions<JwtSettings>` se inyecta en `UserService`.

---

## Base de datos

- Motor: PostgreSQL 15+
- Extensiones requeridas: `citext` (emails case-insensitive), `pgcrypto` (hashing)
- Script de inicialización: `pg-init/00_create_auth_db.sql`
- Datos semilla: roles `ADMIN` y `USER`, estados `NEW/ACTIVE/BLOCKED/DELETED`, menús de admin

**Convención de nombres en BD (snake_case):**
- Tablas: plural (`users`, `roles`, `tokens`)
- Columnas: `snake_case` (`user_id`, `created_at`, `is_active`)
- PKs: `id` siempre
- FKs: `{tabla_referenciada_singular}_id`
- Índices: `idx_{tabla}_{campo(s)}`

---

## Tests

### UnitTest
- Framework: xUnit + Moq
- Clase base: `UnitTestBase` (configura DI de test)
- Mocks: crear con `new Mock<IInterfaz>()`, acceder via `.Object`
- Los handlers se testean mockeando `IMediator`

### FunctionalTest
- Framework: xUnit + `Microsoft.AspNetCore.Mvc.Testing`
- Clase base: `IntegrationTestBase` (levanta `WebApplicationFactory`)
- Incluye `AutoAuthorizeMiddleware` para bypass de auth en tests

### Ejecutar tests
```bash
dotnet test AuthService.ApplicationApi.sln
```

---

## Bugs conocidos (pendientes de corregir)

| # | Archivo | Descripción | Severidad |
|---|---|---|---|
| 1 | `UserService.cs` | Contraseña comparada y almacenada en texto plano — debe usar BCrypt | CRÍTICO |
| 2 | `AuthenticateHandler.cs` | IP hardcodeada como `"123432"` — debe leer de `HttpContext` | ALTO |
| 3 | `UserService.cs` | `UserAuth` compara `Status.Name != "Activo"` — debe usar `Status.Code != "ACTIVE"` | ALTO |
| 4 | `UserService.cs` | JWT sin claims de roles — los consumidores no pueden autorizar por rol | ALTO |
| 5 | `Users.cs` | Propiedades `Rut` y `Dv` en el modelo sin columnas en la tabla SQL | MEDIO |
| 6 | `RolesController.cs` | Hereda de `Controller`, falta `[ApiController]`, `[Route]`, `[Authorize]` | MEDIO |
| 7 | `IUserService.cs` | Parámetro `userName` nunca usado en `UserAuth` | BAJO |

---

## Features faltantes (roadmap)

```
FASE 1 — Seguridad crítica
  ├── Hash de contraseñas con BCrypt
  ├── Claims de roles en el JWT
  ├── IP real desde HttpContext en AuthenticateHandler
  └── Corrección de Status.Code en UserService

FASE 2 — Features core faltantes
  ├── POST /api/User/register (registro de usuarios)
  ├── POST /api/User/logout (revocación de refresh token)
  └── Registro de LoginAttempts + bloqueo por intentos fallidos

FASE 3 — Gestión de roles
  ├── POST /api/Roles (crear rol)
  ├── PUT  /api/Roles/{id} (editar rol)
  ├── POST /api/User/{id}/roles (asignar rol a usuario)
  └── DELETE /api/User/{id}/roles/{roleId} (revocar rol)

FASE 4 — Calidad y DDD
  ├── IUnitOfWork para transacciones en CreateUserAsync
  ├── Mover generación de JWT de Domain a Infrastructure
  ├── Escribir AuditLogs en operaciones críticas
  └── Cambio y recuperación de contraseña
```
