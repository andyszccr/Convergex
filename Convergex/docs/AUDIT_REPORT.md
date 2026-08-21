# Auditoría técnica — Convergex

**Fecha:** 21 de agosto de 2026
**Alcance:** Auditoría, depuración y prueba exhaustiva de la solución completa (Domain, Application, Persistence, Infrastructure, Web, Tests).
**Entorno de prueba:** .NET SDK 10.0.400, Windows, ejecución local (`dotnet run --project Convergex.Web`), base de datos SQLite (`convergex.db`).

## 1. Resumen general

El proyecto es una aplicación ASP.NET Core MVC (.NET 10) con arquitectura limpia (Domain / Application / Persistence / Infrastructure / Web) para conversión de monedas y unidades, con auditoría, reportes (PDF/Excel), autenticación por cookies y sincronización automática de tasas de cambio.

La base de código está, en general, **bien estructurada y con buenas prácticas de seguridad ya implementadas** (autorización por roles, `[ValidateAntiForgeryToken]` en todos los POST, hashing de contraseñas con `PasswordHasher<T>`, protección `Url.IsLocalUrl` contra open-redirect, consultas parametrizadas vía EF Core). Sin embargo, se encontraron **9 defectos reales** — incluyendo uno **crítico** que rompía la interfaz visual para el 100% de los usuarios en un navegador real — y **6 hallazgos adicionales** documentados como recomendaciones que requieren una decisión de producto/infraestructura antes de corregirse.

Todos los bugs corregibles dentro del alcance de este proyecto fueron **corregidos y verificados** (compilación limpia, pruebas manuales end-to-end en navegador y por HTTP).

**Estado final de compilación:** `dotnet build` → **0 errores, 0 advertencias** (antes: 10 advertencias, incluida una vulnerabilidad de seguridad de severidad alta en una dependencia NuGet).

## 2. Bugs encontrados y corregidos

### 🔴 Crítico — Todo el CSS/JS se rompía en cualquier navegador real
- **Síntoma:** cada página se renderizaba sin ningún estilo (sin Bootstrap, sin layout, sin colores) — texto plano apilado verticalmente.
- **Causa raíz:** al enviar la cabecera `Accept-Encoding: gzip` (lo que hace *todo* navegador real), el middleware de activos estáticos de ASP.NET Core 10 (`MapStaticAssets`) devolvía `Content-Length: 0` para los archivos precomprimidos (`bootstrap.min.css`, `login.css`, etc.), aunque los archivos `.gz` en disco eran válidos y de tamaño correcto. Confirmado con `curl -H "Accept-Encoding: gzip"` y reproducido tras una recompilación limpia (no era un problema de caché).
- **Corrección:** se deshabilitó la precompresión de activos estáticos (`<CompressionEnabled>false</CompressionEnabled>` en `Convergex.Web.csproj`), la vía de solución oficial de ASP.NET Core para este escenario. Verificado visualmente en Chrome: el login ahora se ve exactamente como describe el `README.md` (panel con imagen de fondo, tarjeta de formulario, logo, colores corporativos).
- **Archivo:** `Convergex.Web/Convergex.Web.csproj`

### 🟠 Alto — Archivos de código fuente con contenido intercambiado y nombre mal escrito
- **Síntoma:** `Convergex.Web/Middleware/GloabalExceptionMiddleware.cs` (nombre con error tipográfico, "Gloabal") contenía en realidad la clase `ErrorViewModel`, mientras que `Convergex.Web/Models/ErrorViewModel.cs` contenía la clase `GlobalExceptionMiddleware`. Compilaba correctamente porque C# resuelve por espacio de nombres/clase, no por nombre de archivo — pero es un defecto real de higiene y mantenibilidad que además ocultaba el siguiente bug.
- **Corrección:** se restauró cada clase en el archivo que le corresponde; se creó `Convergex.Web/Middleware/GlobalExceptionMiddleware.cs` (nombre corregido) y se eliminó el archivo mal nombrado.
- **Archivos:** `Convergex.Web/Middleware/GlobalExceptionMiddleware.cs` (nuevo), `Convergex.Web/Middleware/GloabalExceptionMiddleware.cs` (eliminado), `Convergex.Web/Models/ErrorViewModel.cs`

### 🟠 Alto — Texto en español corrupto (mojibake) en logs y página de error
- **Síntoma:** mensajes como `"Excepci�n no controlada"`, `"An�nimo"`, `"Iniciando aplicaci�n"` en los logs de Serilog y en la página de error 500 mostrada al usuario. Los caracteres acentuados originales ya estaban perdidos de forma irrecuperable en el archivo (sustituidos por el carácter de reemplazo Unicode).
- **Corrección:** se restauró el texto en español correcto (Excepción, Método, Anónimo, aplicación, respondió, terminó, tardó, Ocurrió) en `Program.cs` y en `GlobalExceptionMiddleware.cs`, verificando que ambos archivos quedaran codificados en UTF‑8 válido.
- **Archivos:** `Convergex.Web/Program.cs`, `Convergex.Web/Middleware/GlobalExceptionMiddleware.cs`

### 🟠 Medio-Alto — Inyección de fórmulas en exportación a Excel (CSV/Excel Formula Injection)
- **Síntoma:** el generador de reportes Excel (`ClosedXmlReportGenerator`) escribía texto controlado por el usuario (nombre completo, IP, detalle de auditoría, etc.) directamente en las celdas sin sanear. Un usuario con un nombre como `=cmd|'/c calc'!A1` podía terminar ejecutando código en la máquina del administrador que abriera el reporte exportado.
- **Corrección:** se sanea cualquier valor de texto que comience con `=`, `+`, `-`, `@`, tab o retorno de carro, anteponiendo una comilla simple (mecanismo estándar de Excel para forzar interpretación como texto).
- **Archivo:** `Convergex.Infrastructure/Reporting/ClosedXmlReportGenerator.cs`

### 🟠 Medio — IDOR en Historial de conversiones
- **Síntoma:** `HistoryController` solo exigía `[Authorize]` (cualquier usuario autenticado, sin restricción de rol) y aceptaba un parámetro `userName` libre en la URL/formulario. Cualquier usuario — incluido el rol de menor privilegio — podía ver el historial completo de conversiones (montos, monedas, fechas) de **cualquier otro usuario**, o de todos, con solo omitir el filtro.
- **Corrección:** para usuarios sin rol `Administrador`, el controlador ahora ignora cualquier `userName` recibido y fuerza el filtro al usuario autenticado actual. El campo de filtro "Usuario" de la vista también se oculta para no-administradores (evita mostrar un control que sería ignorado silenciosamente).
- **Archivos:** `Convergex.Web/Controllers/HistoryController.cs`, `Convergex.Web/Views/History/Index.cshtml`

### 🟡 Medio — Redondeo de conversión de unidades ignoraba la configuración por unidad
- **Síntoma:** `UnitConversionService` redondeaba siempre el resultado a 6 decimales fijos con redondeo estándar, ignorando los campos `DecimalPrecision` y `RoundingMode` configurados por unidad (que sí se usaban para *formatear* en otras pantallas, pero no para calcular el resultado guardado).
- **Corrección:** el resultado ahora se redondea usando `DecimalFormatter.Round(...)` con la precisión y el modo de redondeo configurados en la unidad de destino — el mismo helper ya usado en el resto de la aplicación.
- **Archivo:** `Convergex.Application/Services/UnitConversionService.cs`

### 🟡 Medio — Reportes sin límite de memoria (riesgo de agotamiento de recursos)
- **Síntoma:** `GetConversionsReportAsync` y `GetUsersActivityReportAsync` cargaban **la tabla completa de conversiones** en memoria sin ningún límite, a diferencia de `GetRatesReportAsync`/`GetAuditReportAsync`, que ya aplicaban un tope de 5000 filas (`ExportRowCap`). Con un historial grande, esto es un riesgo de uso excesivo de memoria/DoS al generar reportes.
- **Corrección:** ambos métodos ahora usan el mismo tope `ExportRowCap` vía `GetHistoryPagedAsync`, consistente con el resto de reportes.
- **Archivo:** `Convergex.Application/Services/ReportService.cs`

### 🟡 Bajo-Medio — Consultas `FirstOrDefault` sin `OrderBy` (resultado no determinístico)
- **Síntoma:** EF Core advertía en cada request: *"The query uses the 'First'/'FirstOrDefault' operator without 'OrderBy'... This may lead to unpredictable results"*. Se localizó en dos puntos: `SystemSettingRepository.GetAsync()` (tabla de configuración) y `ExchangeRateRepository.GetActiveByPairAsync()` (podría devolver una tasa "Activa" distinta en cada ejecución si existiera más de una para el mismo par de monedas).
- **Corrección:** se agregó `OrderBy`/`OrderByDescending` explícito en ambas consultas (mismo patrón ya usado en `GetLatestActiveAsync`).
- **Archivos:** `Convergex.Persistence/Repositories/SystemSettingRepository.cs`, `Convergex.Persistence/Repositories/ExchangeRateRepository.cs`

### 🟢 Seguridad de dependencias — Vulnerabilidad NuGet de severidad alta
- **Síntoma:** `dotnet build` reportaba `NU1903`: `SQLitePCLRaw.lib.e_sqlite3` 2.1.11 tiene una vulnerabilidad conocida de severidad alta ([CVE‑2025‑6965](https://github.com/advisories/GHSA-2m69-gcr7-jv3q), corrupción de memoria en SQLite < 3.50.2).
- **Corrección:** se fijó la versión de `SQLitePCLRaw.bundle_e_sqlite3` a `2.1.13` (que empaqueta una versión de SQLite ya corregida) en los dos proyectos que referencian EF Core SQLite.
- **Archivos:** `Convergex.Persistence/Convergex.Persistence.csproj`, `Convergex.Infrastructure/Convergex.Infrastructure.csproj`

### 🟢 Limpieza — Paquete NuGet redundante y repositorio sin `.gitignore` para .NET
- Se eliminó `Microsoft.AspNetCore.Authentication.Cookies` de `Convergex.Web.csproj` (advertencia `NU1510`: el paquete ya viene incluido automáticamente con el SDK web).
- El repositorio no tenía reglas de `.gitignore` para artefactos de compilación (`bin/`, `obj/`, `.vs/`, `*.user`) ni para la base de datos SQLite local (`*.db*`) o los logs, por lo que cientos de archivos binarios y locales estaban versionados en git. Se agregaron las reglas al `.gitignore`. **Nota:** no se eliminaron del control de versiones los archivos ya rastreados (`git rm -r --cached bin/ obj/ ...`) porque es una operación más disruptiva que excede el alcance de "corrección de bugs"; se recomienda hacerlo como una tarea deliberada aparte.
- **Archivos:** `Convergex.Web/Convergex.Web.csproj`, `.gitignore`

## 3. Hallazgos documentados (no corregidos — requieren decisión de producto/infraestructura)

| # | Hallazgo | Severidad | Por qué no se corrigió automáticamente |
|---|---|---|---|
| 1 | `AuthService.RequestPasswordResetAsync` devuelve el token de recuperación **directamente en la respuesta** mostrada en pantalla a quien lo solicite, sin verificar que sea el dueño de la cuenta. Cualquiera que conozca un correo puede tomar el control de esa cuenta. | Alta (si se despliega fuera de un entorno local) | Es un atajo de demo explícito (el propio código lo marca `"(demo)"` y el `README` lo documenta así) porque no hay proveedor de correo configurado. Removerlo sin agregar envío de correo real dejaría la función de "olvidé mi contraseña" completamente rota. **Recomendación:** integrar un proveedor SMTP/SendGrid y enviar el token por correo antes de cualquier despliegue fuera de `localhost`. |
| 2 | La API externa de tasas de cambio (`TdcRateUrl`, `apis.gometa.org`) está configurada sobre **HTTP** plano, no HTTPS. | Media | Es un proveedor de terceros de solo lectura y puede no ofrecer HTTPS; cambiar la URL requiere confirmar si el proveedor soporta TLS. **Recomendación:** priorizar el proveedor HTTPS ya integrado (`ExchangeRateApi`, `open.er-api.com`) como fuente principal y dejar el TDC como respaldo, o confirmar soporte HTTPS. |
| 3 | No existe una suite de pruebas automatizadas: `Convergex.Tests` es un proyecto vacío, sin ningún archivo `.cs` de pruebas ni referencia a xUnit/NUnit/MSTest. | Media | Crear una suite de pruebas es un trabajo de alcance propio (no un "bug"), fuera de lo que puede resolverse como corrección puntual. **Recomendación:** priorizar pruebas unitarias sobre `CurrencyConversionService`, `UnitConversionService`, `AuthService` y el `ReportService`, que concentran la lógica de negocio y el riesgo. |
| 4 | Las conversiones de **unidades** no se registran en el log de auditoría, mientras que las conversiones de **moneda** sí. | Baja | Inconsistencia de cobertura, no un defecto funcional. **Recomendación:** agregar el mismo `_auditService.LogAsync(...)` que usa `CurrencyConversionService`. |
| 5 | `GetUsersActivityReportAsync` empareja conversiones con usuarios comparando `UserName == FullName` (texto) en vez de una clave foránea real. Dos usuarios con el mismo nombre completo verían su actividad mezclada. | Media | Corregirlo de raíz requeriría agregar una FK `UserId` a `Conversion` y una migración de datos — cambio de modelo de datos que excede una corrección puntual. **Recomendación:** agregar `UserId` (nullable, para no romper datos existentes) a la entidad `Conversion`. |
| 6 | El campo de monto en los formularios de conversión llega precargado con el valor `"0.00"` en vez de estar vacío; si el usuario escribe sin seleccionar/borrar primero el contenido, el nuevo valor se inserta junto al existente (ej. "0.00" + "100" → "0.00100"). | Baja (UX) | Detectado durante las pruebas manuales; es un comportamiento estándar de un `<input>` con valor prellenado, no un error de lógica de conversión (la conversión en sí calculó correctamente sobre el valor final del campo). **Recomendación:** dejar el campo vacío por defecto o seleccionar todo el contenido al enfocar (`onfocus="this.select()"`). |

## 4. Pruebas ejecutadas

### Compilación
- `dotnet build Convergex.slnx` → **0 errores, 0 advertencias** (estado final, tras las correcciones).

### Pruebas unitarias/integración
- No aplica: el proyecto `Convergex.Tests` no contiene pruebas (ver hallazgo #3). No se inventaron pruebas nuevas por estar fuera del alcance de "corrección de bugs", pero se deja documentado como recomendación prioritaria.

### Pruebas funcionales manuales (navegador real + HTTP)
Realizadas contra la app corriendo en `http://localhost:5167` con la base de datos y el seed de datos de demo:

- ✅ Login con credenciales de demo (`admin@convergex.com` / `Admin123!`) — sesión, cookie y redirección correctas.
- ✅ Dashboard — gráficos, KPIs, actividad reciente y tasas principales se renderizan sin excepciones.
- ✅ Conversión de monedas (100 USD → CRC) — resultado matemáticamente correcto (100 × 446.08 = 44,608.00 CRC), guardado en historial.
- ✅ Conversión de unidades (5 km → m) — resultado correcto (5,000 m), guardado en historial.
- ✅ Historial, Reportes (los 4 sub-reportes), Auditoría, Monedas, Unidades, Tasas de cambio, Usuarios, Configuración — todas cargan con HTTP 200 y sin trazas de excepción.
- ✅ Exportación de reporte a Excel (`/Reports/Export?report=conversions&format=excel`) — genera un `.xlsx` válido tras el fix de saneamiento.
- ✅ Verificación de `[Authorize(Roles=...)]` en todos los controladores administrativos (Usuarios, Auditoría, Reportes, gestión de Monedas/Unidades/Tasas) mediante inspección de código y pruebas de ruta.
- ✅ Verificación de que los formularios POST rechazan peticiones sin token antifalsificación (`400 Bad Request`).
- ✅ Verificación visual completa en Chrome (antes/después del fix de CSS): el login, dashboard y formularios se ven exactamente como describe el `README.md`.

## 5. Recomendaciones adicionales de optimización

1. **Eliminar del control de versiones** `bin/`, `obj/`, `.vs/` y los archivos `.db`/`.db-shm`/`.db-wal` ya rastreados (`git rm -r --cached`), ahora que el `.gitignore` los excluye hacia adelante.
2. **Agregar pruebas automatizadas** (xUnit + `Microsoft.EntityFrameworkCore.InMemory` o SQLite en memoria) cubriendo como mínimo: cálculo de conversión de moneda/unidad, autenticación y flujo de reseteo de contraseña, generación de reportes.
3. **Agregar `UserId` (FK)** a la entidad `Conversion` para relacionar de forma confiable las conversiones con el usuario que las realizó, en vez de depender del texto `UserName`.
4. **Enviar el token de recuperación de contraseña por correo real** (o al menos exigir una segunda verificación) antes de considerar el flujo apto para producción.
5. Considerar **paginación en el listado de Historial también a nivel de exportación PDF**, ya que `QuestPdfReportGenerator` no fue objeto de este hallazgo pero comparte el mismo patrón de entrada (`ReportDocument`) que ya está acotado por `ExportRowCap` tras esta corrección.
6. El mensaje `"Failed to determine the https port for redirect"` que aparece en el log al ejecutar en `http://localhost` con `UseHttpsRedirection()` activo es benigno en desarrollo, pero conviene configurar `https_port` explícitamente (o el perfil `https` de `launchSettings.json`) para producción.

## 6. Archivos modificados en esta auditoría

```
.gitignore
Convergex.Application/Services/ReportService.cs
Convergex.Application/Services/UnitConversionService.cs
Convergex.Infrastructure/Convergex.Infrastructure.csproj
Convergex.Infrastructure/Reporting/ClosedXmlReportGenerator.cs
Convergex.Persistence/Convergex.Persistence.csproj
Convergex.Persistence/Repositories/ExchangeRateRepository.cs
Convergex.Persistence/Repositories/SystemSettingRepository.cs
Convergex.Web/Controllers/HistoryController.cs
Convergex.Web/Convergex.Web.csproj
Convergex.Web/Middleware/GlobalExceptionMiddleware.cs   (nuevo; reemplaza el mal nombrado)
Convergex.Web/Middleware/GloabalExceptionMiddleware.cs  (eliminado)
Convergex.Web/Models/ErrorViewModel.cs
Convergex.Web/Program.cs
Convergex.Web/Views/History/Index.cshtml
```

La arquitectura general (Clean Architecture por capas) y los mecanismos de seguridad ya existentes (cookies, hashing de contraseñas, autorización por roles, anti-forgery) se mantuvieron intactos, tal como lo requería el alcance de esta auditoría.
