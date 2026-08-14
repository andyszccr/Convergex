# Convergex - Conversor de Unidades

Sistema de conversión de monedas y unidades desarrollado con ASP.NET Core 10.0.

## Características

- ✅ Conversión de monedas en tiempo real (USD, CRC y más)
- ✅ Integración con API externa de tasas de cambio (TDC)
- ✅ Historial de conversiones con auditoría
- ✅ Sistema de autenticación seguro
- ✅ Diseño responsive con mockup profesional
- ✅ Validaciones completas de datos
- ✅ Logging y manejo de errores robusto

## Tecnologías

- **Framework:** ASP.NET Core 10.0
- **Base de Datos:** SQLite con Entity Framework Core
- **Autenticación:** Cookies con ASP.NET Core Identity
- **Frontend:** Bootstrap 5, Bootstrap Icons, CSS personalizado
- **Arquitectura:** Clean Architecture (Domain, Application, Infrastructure, Persistence, Web)

## Estructura del Proyecto

```
Convergex/
├── Convergex.Domain/           # Entidades y reglas de negocio
├── Convergex.Application/      # Servicios, DTOs, Interfaces
├── Convergex.Persistence/      # Repositorios y DbContext
├── Convergex.Infrastructure/   # Servicios externos, configuración
├── Convergex.Web/              # API, Controladores, Vistas, CSS
└── Convergex.Tests/            # Pruebas unitarias
```

## Módulos Principales

### 1. Conversión de Monedas (`/CurrencyConversion`)

**Funcionalidad:**
- Conversión entre diferentes monedas
- Integración automática con API TDC para tasas USD/CRC
- Fallback a base de datos cuando la API no está disponible
- Historial completo de conversiones

**Características de Seguridad:**
- Validación de tasas antes de usar
- Timeout de 10 segundos en llamadas a API externa
- Manejo de excepciones específico (HttpRequestException, TaskCanceledException, JsonException)
- Logging completo de operaciones
- URL de API configurada en appsettings.json

**Uso:**
1. Selecciona moneda origen y destino
2. Ingresa el monto a convertir
3. El sistema busca la tasa en BD o consulta la API externa
4. Se muestra el resultado y se guarda en el historial

**Monedas soportadas:**
- USD ↔ CRC (con API externa TDC)
- Otras combinaciones (solo con tasas en BD)

### 2. Autenticación (`/Account`)

**Funcionalidad:**
- Login con email y contraseña
- Registro de nuevos usuarios
- Recuperación de contraseña
- Sesiones persistentes con "Recordarme"

**Diseño:**
- Fondo de pantalla del mockup oficial
- Logo transparente de Convergex
- Interfaz limpia y profesional
- Responsive para móviles y desktop

**Credenciales de demo (solo local):**
- Email: `admin@convergex.com`
- Contraseña: *(definida en el seeding/configuración local; no se versiona en el repositorio)*
### 3. Dashboard (`/Dashboard`)

**Funcionalidad:**
- Vista general de conversiones
- Estadísticas de uso
- Acceso rápido a módulos

### 4. Historial (`/History`)

**Funcionalidad:**
- Ver todas las conversiones realizadas
- Filtrar por fecha y tipo
- Exportar datos

## Configuración

### Requisitos Previos

- .NET 10.0 SDK
- SQLite (incluido en el proyecto)

### Instalación

```bash
# Clonar el repositorio
git clone https://github.com/andyszccr/Convergex.git
cd Convergex

# Restaurar paquetes
dotnet restore

# Ejecutar la aplicación
dotnet run --project Convergex.Web
```

### Configuración de API Externa

La URL de la API de tasas de cambio se configura en `Convergex.Web/appsettings.json`:

```json
{
  "ExternalApis": {
    "TdcRateUrl": "http://apis.gometa.org/tdc/tdc.json"
  }
}
```

## Diseño UI/UX

### Login
- **Fondo:** Mockup oficial de Convergex (`01 mockup de login.png`)
- **Logo:** Logo transparente de Convergex (PNG)
- **Colores:** Azul navy (#0B1F3A), azul royal (#2563EB), celeste (#38BDF8)
- **Tipografía:** DM Sans (cuerpo), Space Grotesk (títulos)
- **Layout:** Panel izquierdo con fondo e información, panel derecho con formulario
- **Características:**
  - Overlay oscuro para legibilidad del texto
  - Logo con sombra para destacar
  - Diseño responsive (mobile-first)
  - Iconos de características en el panel izquierdo

### Panel de Conversión
- Diseño limpio y moderno
- Badges informativos de fuente de tasas
- Validaciones visuales en tiempo real
- Resultados destacados con formato profesional

## Seguridad

- ✅ Autenticación con cookies seguras
- ✅ Encriptación de contraseñas
- ✅ Validación de datos en cliente y servidor
- ✅ Protección contra CSRF (AntiForgeryToken)
- ✅ Timeout en llamadas a API externa
- ✅ Logging de eventos de seguridad
- ✅ Sesiones con expiración configurable (8 horas)

## Próximas Mejoras

- [ ] Cache de tasas externas para reducir llamadas
- [ ] Soporte para más pares de monedas desde API externa
- [ ] Implementar retry policy con Polly
- [ ] Agregar métricas de uso de API externa
- [ ] Botón de actualización manual de tasas
- [ ] Exportar historial a PDF/Excel
- [ ] Modo oscuro/claro
- [ ] Multiidioma (i18n)

## Licencia

© 2026 Convergex. Todos los derechos reservados.

## Demo

Accede a la aplicación en: `http://localhost:5167`

**Credenciales de prueba:**
- Email: `admin@convergex.com`
- Contraseña: `Admin123!`

---

**Desarrollado con** ❤️ **por el equipo de Convergex**