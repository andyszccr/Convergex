# 🚀 Convergex - Product Backlog (Issues)

## 📖 Descripción

Este documento contiene los módulos principales del sistema **Convergex**, organizados como Product Backlog para GitHub Issues y GitHub Projects.

Proyecto desarrollado con:

- ASP.NET Core MVC (.NET 10)
- Clean Architecture
- SOLID
- Entity Framework Core
- SQLite (Desarrollo)
- SQL Server (Producción)
- GitHub Actions (CI/CD)
- xUnit
- Bootstrap 5
- Serilog
- QuestPDF / ClosedXML (reportes)

---

## 📊 Resumen de avance

| Módulo | Estado |
|--------|--------|
| 1. Dashboard | Completado |
| 2. Autenticación y Usuarios | Completado |
| 3. Conversión de Monedas | Completado |
| 4. Conversión de Unidades | Completado |
| 5. Historial de Conversiones | Completado |
| 6. Gestión de Monedas | Completado |
| 7. Gestión de Unidades | Completado |
| 8. Gestión de Tasas de Cambio | Completado |
| 9. Auditoría | Completado |
| 10. Reportes | Completado |
| 11. Configuración | Completado |
| 12. Integración API | Completado |
| 13. Logs | Completado |
| 14. CI/CD | Pendiente |

Ver extras y siguientes ideas en [`README-Issues-v2.0.md`](./README-Issues-v2.0.md).

---

# 📌 Módulo 1 - Dashboard

## Objetivo

Mostrar información general del sistema mediante indicadores y gráficos.

### Funcionalidades

- Dashboard principal
- Tarjetas informativas
- Gráfico de conversiones
- Últimas conversiones
- Última tasa de cambio
- Accesos rápidos

### Issues

- [x] Crear Dashboard
- [x] Cards informativas
- [x] Gráfico de conversiones
- [x] Tabla de últimas conversiones
- [x] Mostrar tasa de cambio
- [x] Responsive

---

# 📌 Módulo 2 - Autenticación y Usuarios

## Objetivo

Gestionar el acceso al sistema.

### Funcionalidades

- Login
- Logout
- Perfil
- Roles
- Cambio de contraseña

### CRUD Usuarios

- Crear usuario
- Editar usuario
- Eliminar usuario
- Consultar usuarios

### Issues

- [x] Login
- [x] Logout
- [x] CRUD Usuarios
- [x] Gestión de Roles
- [x] Perfil de usuario
- [x] Recuperar contraseña

---

# 📌 Módulo 3 - Conversión de Monedas

## Objetivo

Permitir convertir valores entre distintas monedas utilizando tasas de cambio.

### Funcionalidades

- Seleccionar moneda origen
- Seleccionar moneda destino
- Consultar tasa
- Ejecutar conversión
- Guardar historial

### Issues

- [x] CRUD Monedas
- [x] Obtener tasas
- [x] Conversión
- [x] Validaciones
- [x] Historial
- [x] Mensajes de éxito

---

# 📌 Módulo 4 - Conversión de Unidades

## Objetivo

Realizar conversiones entre distintas unidades de medida.

### Categorías

- Longitud
- Peso
- Volumen

### Issues

- [x] CRUD Unidades
- [x] Conversión Longitud
- [x] Conversión Peso
- [x] Conversión Volumen
- [x] Historial

---

# 📌 Módulo 5 - Historial de Conversiones

## Objetivo

Consultar todas las conversiones realizadas.

### Funcionalidades

- Listado
- Búsqueda
- Filtros
- Detalle

### Issues

- [x] Tabla Historial
- [x] Buscar por usuario
- [x] Buscar por fecha
- [x] Buscar por tipo
- [x] Paginación

---

# 📌 Módulo 6 - Gestión de Monedas

## Objetivo

Administrar el catálogo de monedas.

### Funcionalidades

- Crear
- Editar
- Eliminar
- Activar
- Desactivar

### Issues

- [x] CRUD Monedas
- [x] Validaciones
- [x] Catálogo
- [x] Estado Activo

---

# 📌 Módulo 7 - Gestión de Unidades

## Objetivo

Administrar las unidades de medida.

### Categorías

- Longitud
- Peso
- Volumen

### Issues

- [x] CRUD Unidades
- [x] Categorías
- [x] Validaciones

---

# 📌 Módulo 8 - Gestión de Tasas de Cambio

## Objetivo

Administrar las tasas de cambio del sistema.

### Funcionalidades

- Actualización manual
- Actualización automática
- Historial de tasas

### Issues

- [x] CRUD Tasas
- [x] Actualizar tasas
- [x] Historial
- [x] Integración API

---

# 📌 Módulo 9 - Auditoría

## Objetivo

Registrar todas las acciones importantes realizadas dentro del sistema.

### Eventos

- Login
- Logout
- Conversión
- Eliminación
- Edición
- Creación

### Issues

- [x] Registrar eventos
- [x] Tabla Auditoría
- [x] Consulta Auditoría
- [x] Buscar eventos

---

# 📌 Módulo 10 - Reportes

## Objetivo

Generar reportes del sistema.

### Reportes

- Historial
- Auditoría
- Conversiones
- Usuarios

### Exportaciones

- PDF
- Excel

### Issues

- [x] Reporte Historial
- [x] Reporte Auditoría
- [x] Exportar PDF
- [x] Exportar Excel

---

# 📌 Módulo 11 - Configuración

## Objetivo

Configurar parámetros generales del sistema.

### Funcionalidades

- Tema
- Idioma
- Moneda por defecto
- Zona horaria

### Issues

- [x] Configuración General
- [x] Idioma
- [x] Tema
- [x] Zona Horaria

---

# 📌 Módulo 12 - Integración API

## Objetivo

Consumir servicios externos para obtener tasas de cambio.

### Funcionalidades

- Consumo REST API
- Caché
- Manejo de errores

### Issues

- [x] Cliente HTTP
- [x] Obtener tasas
- [x] Actualización automática
- [x] Caché

---

# 📌 Módulo 13 - Logs

## Objetivo

Registrar errores y eventos técnicos del sistema.

### Funcionalidades

- Logs
- Excepciones
- Performance

### Issues

- [x] Configurar Serilog
- [x] Logs Globales
- [x] Middleware Excepciones

---

# 📌 Módulo 14 - CI/CD

## Objetivo

Automatizar compilación, pruebas y despliegue.

### Funcionalidades

- Build automático
- Unit Testing
- Publicación

### Issues

- [ ] Configurar GitHub Actions
- [ ] Build
- [ ] Ejecutar Tests
- [ ] Publicar Artefactos

---

# 📅 Roadmap por Sprint

## 🚀 Sprint 1 — Completado

- Arquitectura Clean
- Configuración del proyecto
- SQLite
- Login
- Usuarios
- Dashboard

---

## 🚀 Sprint 2 — Completado

- Conversión de Monedas
- Gestión de Monedas
- Tasas de Cambio
- API Externa

---

## 🚀 Sprint 3 — Completado

- Conversión de Unidades
- Historial
- Auditoría
- Logs

---

## 🚀 Sprint 4 — Completado (parcial)

- Reportes PDF / Excel
- Configuración
- Testing (pendiente suite xUnit)
- CI/CD (pendiente GitHub Actions)
- Documentación

---

# 🏗 Arquitectura

```
Convergex.Web            → ASP.NET Core MVC (Presentation)
Convergex.Application    → Casos de uso / Services / DTOs
Convergex.Domain         → Entidades / Enums
Convergex.Infrastructure → APIs externas / DI / Report generators
Convergex.Persistence    → DbContext / Repositories / Migrations
Convergex.Tests          → Unit Testing (por completar)
```
