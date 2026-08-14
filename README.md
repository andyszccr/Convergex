# Convergex

> **Conversor Inteligente de Monedas y Unidades**

![.NET](https://img.shields.io/badge/.NET-10-512BD4?style=for-the-badge)
![ASP.NET MVC](https://img.shields.io/badge/ASP.NET-Core_MVC-5C2D91?style=for-the-badge)
![SQLite](https://img.shields.io/badge/SQLite-003B57?style=for-the-badge)
![EF Core](https://img.shields.io/badge/Entity_Framework_Core-68217A?style=for-the-badge)
![Serilog](https://img.shields.io/badge/Serilog-Logs-4B8BBE?style=for-the-badge)
![Bootstrap](https://img.shields.io/badge/Bootstrap-5-7952B3?style=for-the-badge)

---

## Descripción

**Convergex** es una aplicación web en **ASP.NET Core MVC (.NET 10)** para convertir **monedas** y **unidades de medida** de forma rápida, precisa y segura.

Incluye historial de operaciones, auditoría, reportes PDF/Excel, sincronización de tasas por API, configuración del sistema y un dashboard con indicadores.

El proyecto sigue **Clean Architecture**, **SOLID**, Repository Pattern y Dependency Injection, e integra varios **paradigmas de programación** (ver sección dedicada más abajo).

---

## Estado del proyecto (v1.0)

| Área | Estado |
|------|--------|
| Dashboard | Completado |
| Autenticación / Usuarios / Roles | Completado |
| Conversión de monedas | Completado |
| Conversión de unidades | Completado |
| Historial (filtros + paginación) | Completado |
| CRUD Monedas / Unidades / Tasas | Completado |
| API externa de tasas + sync | Completado |
| Auditoría | Completado |
| Reportes PDF / Excel | Completado |
| Configuración (tema, idioma, TZ, moneda) | Completado |
| Serilog + middleware de excepciones | Completado |
| CI/CD + suite xUnit | Pendiente |

Detalle de tickets: [`Documentación/issues.md`](Documentación/issues.md)  
Extras v2.0: [`Documentación/README-Issues-v2.0.md`](Documentación/README-Issues-v2.0.md)

---

## Objetivo

- Conversión de monedas con tasas locales y externas
- Conversión de unidades (longitud, peso, volumen)
- Historial y auditoría
- Dashboard con estadísticas
- Reportes exportables
- Gestión de usuarios y roles

---

## Arquitectura

```
Presentation (MVC)
│  Controllers · Views · ViewModels
▼
Application
│  DTOs · Services · Interfaces
▼
Domain
│  Entities · Enums
▼
Infrastructure
│  APIs externas · Report generators · DI
▼
Persistence
   DbContext · Repositories · Migrations · Seed
```

### Proyectos

```
Convergex/
├── Convergex.Web              → ASP.NET Core MVC
├── Convergex.Application      → Casos de uso
├── Convergex.Domain           → Entidades
├── Convergex.Infrastructure   → EF wiring, APIs, reportes
├── Convergex.Persistence      → DbContext, repositorios, migraciones
└── Convergex.Tests            → Unit testing (por completar)
```

---

## Tecnologías

| Tecnología | Uso |
|------------|-----|
| ASP.NET Core MVC | Aplicación web |
| .NET 10 | Framework |
| Entity Framework Core | ORM + migraciones |
| SQLite | Base de datos (desarrollo) |
| Bootstrap 5 | UI |
| Cookie Authentication | Sesiones |
| Serilog | Logs |
| QuestPDF / ClosedXML | Reportes PDF / Excel |
| HttpClient | API de tasas de cambio |
| xUnit | Pruebas (pendiente ampliar) |
| GitHub Actions | CI/CD (pendiente) |

---

## Módulos implementados

| Módulo | Descripción |
|--------|-------------|
| Dashboard | KPIs, gráficos, actividad reciente |
| Usuarios | Login, registro, logout, perfil, roles, CRUD |
| Conv. monedas | Origen/destino, tasa, historial |
| Conv. unidades | Longitud, peso, volumen |
| Monedas | Catálogo CRUD + activo/inactivo |
| Unidades | Catálogo CRUD por categoría |
| Tasas | CRUD, sync API, historial de tasas |
| Historial | Filtros por tipo/usuario/fecha + paginación |
| Auditoría | Registro y consulta de eventos |
| Reportes | Conversiones, tasas, auditoría, usuarios (PDF/Excel) |
| Configuración | Idioma, tema, moneda default, zona horaria |
| Logs | Serilog + middleware global de excepciones |

---

## Cómo ejecutar

```bash
cd Convergex
dotnet restore
dotnet run --project Convergex.Web
```

Abre la URL que muestre la consola (por ejemplo `https://localhost:7xxx`).

Puedes **crear una cuenta nueva** desde *Regístrate aquí* (rol Operador) o usar las credenciales demo:

### Credenciales demo

| Rol | Correo | Contraseña |
|-----|--------|------------|
| Administrador | `admin@convergex.com` | `Admin123!` |
| Operador | `operador@convergex.com` | `Operador123!` |

> Si la base SQLite no existe, se crea/migra y se siembra al arrancar.

---

## Flujo del sistema

```
Usuario → MVC Controller → Application Service → Repository → SQLite → Vista MVC
```

Las tasas externas se obtienen vía `HttpClient` en Infrastructure y se sincronizan desde el módulo de Tasas.

---

## Paleta de colores

| Elemento | Hex |
|----------|-----|
| Azul principal | `#2563EB` |
| Azul oscuro | `#1E3A8A` / `#0B1F3A` (sidebar) |
| Verde éxito | `#10B981` |
| Fondo | `#F1F5F9` / `#F8FAFC` |
| Texto | `#334155` |
| Texto secundario | `#64748B` |
| Error | `#EF4444` |
| Advertencia | `#F59E0B` |

```css
background: linear-gradient(135deg, #2563EB, #1E3A8A);
```

Logo: `Imagenes/Logo.png` · Login: `Imagenes/logo2.png`

---

## Principios de diseño

- Clean Architecture
- SOLID
- Repository Pattern
- Dependency Injection
- DTO + Service Layer
- Auditoría de cambios
- Logging estructurado

---

## Análisis de paradigmas de programación

Como analista se revisó el código fuente (capas **Domain**, **Application**, **Infrastructure**, **Persistence** y **Web**) e identificaron los siguientes paradigmas, además del orientado a objetos.

### 1. Programación funcional

- **LINQ**: uso intensivo de `Select`, `Where`, `GroupBy`, `Sum`, `Average`, `OrderBy`, `ToDictionary` en repositorios y servicios (p. ej. `ConversionRepository`, `ReportService`).
- **Tuplas**: retorno de múltiples valores con tuplas nombradas, p. ej. `(bool Success, string Message, CurrencyConversionResultDto? Result)` en `CurrencyConversionService` y `UnitConversionService`.
- **Switch expressions**: `switch` como expresión (no sentencia), p. ej. en `AuditSaveChangesInterceptor` y `ReportService`.
- **Métodos estáticos puros**: `TimeZoneHelper` y `ReportCell` (factory estático) sin estado mutable.
- **Inmutabilidad parcial**: `IReadOnlyList<T>`, `IReadOnlyDictionary<TKey, TValue>` y colecciones de solo lectura.

### 2. Programación declarativa

- **Atributos**: `[Authorize]`, `[HttpGet]`, `[HttpPost]`, `[ValidateAntiForgeryToken]` en controladores.
- **Configuración de servicios**: `AddScoped`, `AddHttpClient`, `AddAuthentication`, `AddAuthorization` en `Program.cs` y `DependencyInjection.cs`.
- **Fluent API de EF Core**: entidades y relaciones configuradas de forma declarativa.

### 3. Programación orientada a aspectos (AOP)

- **Interceptores**: `AuditSaveChangesInterceptor` (hereda de `SaveChangesInterceptor`) audita automáticamente al guardar sin invadir la lógica de negocio.
- **Middleware**: `GlobalExceptionMiddleware` centraliza excepciones y logging en el pipeline HTTP.
- **Autorización transversal**: `[Authorize]` aplica seguridad de forma transversal.

### 4. Programación asíncrona y concurrente

- **async/await**: flujo de datos asíncrono (`Task`, `ValueTask`, `CancellationToken`).
- **Servicios en segundo plano**: `ExchangeRateSyncBackgroundService` (`BackgroundService` + `PeriodicTimer`).
- **Paralelismo**: `Task.WhenAll` y operaciones concurrentes en repositorios donde aplica.

### 5. Programación por contratos (interfaces)

- **Abstracción**: 20+ interfaces (`ICurrencyRepository`, `ICurrencyConversionService`, `IReportService`, `IExcelReportGenerator`, `IPdfReportGenerator`, etc.).
- **Inyección de dependencias**: constructor injection en servicios, controladores y repositorios; el contenedor DI resuelve las dependencias.

### 6. Programación genérica

- Tipos genéricos: `IQueryable<T>`, `IReadOnlyList<T>`, `IReadOnlyDictionary<TKey, TValue>`, `Dictionary<Type, string[]>`, `List<T>`.
- Métodos genéricos de EF Core: `Set<T>()`, consultas tipadas en repositorios.

### 7. Programación por capas / Clean Architecture

- **Separación**: Domain (entidades), Application (casos de uso), Infrastructure (servicios externos), Persistence (datos), Web (presentación).
- **Dependencias invertidas**: las capas internas no dependen de las externas.

### 8. Programación reactiva / basada en eventos

- Interceptores que reaccionan al ciclo de vida de EF Core (`SavingChangesAsync` / `SavedChangesAsync`).
- Auditoría como reacción a cambios de estado de entidades.

### 9. Programación orientada a objetos (paradigma base)

- Clases, herencia (`BackgroundService`, `SaveChangesInterceptor`, `Controller`), encapsulación, polimorfismo y composición.

### Patrones de diseño identificados

| Patrón | Evidencia |
|--------|-----------|
| Repository | `ConversionRepository`, `CurrencyRepository`, etc. |
| Service Layer | `CurrencyConversionService`, `ReportService`, etc. |
| Strategy | `IExcelReportGenerator` / `ClosedXmlReportGenerator` y `IPdfReportGenerator` / `QuestPdfReportGenerator` |
| Observer | `AuditSaveChangesInterceptor` observa cambios de entidades |
| Factory | `ReportCell.Text()`, `ReportCell.Number()`, `ReportCell.DateAndTime()` |
| Facade | Servicios de aplicación con operaciones de alto nivel |
| Dependency Injection | Patrón central en toda la arquitectura |

### Resumen ejecutivo

| Paradigma | Evidencia |
|-----------|-----------|
| Orientado a objetos | Clases, herencia, interfaces, encapsulación |
| Funcional | LINQ, tuplas, switch expressions, métodos puros |
| Declarativo | Atributos, configuración de servicios, EF Core |
| Orientado a aspectos | Interceptores, middleware, `[Authorize]` |
| Asíncrono / concurrente | async/await, `BackgroundService`, `PeriodicTimer` |
| Por contratos | 20+ interfaces, DI |
| Genérico | Tipos genéricos en repositorios y servicios |
| Por capas | Clean Architecture (5 proyectos) |
| Reactivo / eventos | Interceptores de EF Core |

El proyecto es **predominantemente orientado a objetos**, con fuerte influencia de **programación funcional** (LINQ, tuplas, inmutabilidad) y **orientada a aspectos** (interceptores y middleware), sobre una **arquitectura limpia por capas** con inyección de dependencias.

---

## Roadmap

### Hecho (Sprints 1–4)

- Arquitectura, auth, dashboard
- Monedas, tasas, conversión monetaria
- Unidades y conversión física
- Historial, auditoría, reportes, configuración, API, logs

### Siguiente (v2.0)

1. CI/CD con GitHub Actions + suite xUnit
2. Pulido UI / dark mode completo
3. Extras: favoritos, alertas de tasa, OAuth

Ver plan detallado en [`Documentación/README-Issues-v2.0.md`](Documentación/README-Issues-v2.0.md).

---

## Equipo

- Scrum Master / Analista
- Backend Developer
- Database Developer
- Frontend Developer / QA

---

## Licencia

Proyecto académico — **Paradigmas de Programación**, Universidad Fidélitas.

---

<div align="center">

### Convergex

**Conversor inteligente de monedas y unidades**

ASP.NET Core MVC · Clean Architecture · Multi-paradigma · SOLID

</div>
