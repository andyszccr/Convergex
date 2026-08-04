# <img src="docs/logo.png" width="45"> Convergex

> **Conversor Inteligente de Monedas y Unidades**

![.NET](https://img.shields.io/badge/.NET-10-512BD4?style=for-the-badge)
![ASP.NET MVC](https://img.shields.io/badge/ASP.NET-Core_MVC-5C2D91?style=for-the-badge)
![SQLite](https://img.shields.io/badge/SQLite-003B57?style=for-the-badge)
![SQL Server](https://img.shields.io/badge/SQL_Server-CC2927?style=for-the-badge)
![EF Core](https://img.shields.io/badge/Entity_Framework_Core-68217A?style=for-the-badge)
![GitHub Actions](https://img.shields.io/badge/GitHub_Actions-2088FF?style=for-the-badge)

---

# 📖 Descripción

**Convergex** es una aplicación web desarrollada en **ASP.NET Core MVC** que permite realizar conversiones de **monedas** y **unidades de medida** de forma rápida, precisa y segura.

El sistema está diseñado para apoyar plataformas de comercio electrónico, empresas y usuarios que necesitan convertir valores monetarios y unidades físicas, manteniendo un historial completo de las operaciones realizadas y un registro de auditoría para garantizar la trazabilidad.

El proyecto fue desarrollado siguiendo principios de **Clean Architecture**, **SOLID** y buenas prácticas de desarrollo de software para facilitar su mantenimiento, escalabilidad y evolución.

---

# 🎯 Objetivo

Desarrollar un motor inteligente de conversión que permita:

- 💱 Conversión de monedas
- 📏 Conversión de unidades
- 📜 Historial de conversiones
- 📝 Auditoría de operaciones
- 📊 Dashboard con estadísticas
- 📈 Reportes
- 🔒 Gestión de usuarios

---

# ✨ Características

- Conversión de monedas en tiempo real
- Conversión de unidades físicas
- Historial de conversiones
- Gestión de tasas de cambio
- Dashboard interactivo
- Reportes PDF y Excel
- Gestión de usuarios
- Auditoría
- Arquitectura escalable
- Código basado en SOLID

---

# 🏗 Arquitectura

El proyecto sigue una arquitectura limpia (**Clean Architecture**) separando las responsabilidades en diferentes capas.

```

Presentation (MVC)
│
├── Controllers
├── Views
├── ViewModels
│
▼
Application
│
├── DTOs
├── Services
├── Interfaces
├── Validators
│
▼
Domain
│
├── Entities
├── Interfaces
├── Enums
├── Value Objects
│
▼
Infrastructure
│
├── Entity Framework Core
├── SQLite
├── SQL Server
├── APIs
├── Repositories
│
▼
Persistence
│
└── DbContext

```

---

# 💻 Tecnologías

| Tecnología | Uso |
|------------|------------------------------|
| ASP.NET Core MVC | Aplicación Web |
| .NET 10 | Framework |
| Entity Framework Core | ORM |
| SQLite | Base de datos desarrollo |
| SQL Server | Base de datos producción |
| Bootstrap 5 | Interfaz |
| AutoMapper | Mapeo de objetos |
| FluentValidation | Validaciones |
| Serilog | Logs |
| xUnit | Pruebas |
| GitHub Actions | CI/CD |

---

# 📦 Módulos

## 🏠 Dashboard

Visualización general del sistema mediante indicadores, gráficos y estadísticas.

---

## 👤 Usuarios

- Login
- Logout
- CRUD Usuarios
- Roles
- Perfil

---

## 💱 Conversión de Monedas

- Conversión entre monedas
- Tasas de cambio
- Registro de conversiones

---

## 📏 Conversión de Unidades

- Longitud
- Peso
- Volumen

---

## 📜 Historial

Consulta todas las conversiones realizadas.

---

## 🌎 Monedas

Administración del catálogo de monedas.

---

## 📐 Unidades

Administración de unidades físicas.

---

## 💲 Tasas de Cambio

Gestión y actualización de tasas.

---

## 📝 Auditoría

Registro de eventos del sistema.

---

## 📊 Reportes

Generación de reportes en PDF y Excel.

---

## ⚙ Configuración

Configuraciones generales del sistema.

---

# 📂 Estructura del Proyecto

```

Convergex.sln

│

├── Convergex.Web

├── Convergex.Application

├── Convergex.Domain

├── Convergex.Infrastructure

├── Convergex.Persistence

└── Convergex.Tests

```

---

# 🎨 Paleta de Colores

La identidad visual de **Convergex** está inspirada en aplicaciones empresariales modernas, transmitiendo confianza, precisión e innovación.

| Elemento | Color | Hex |
|----------|---------|---------|
| 🔵 Azul Principal | ![#2563EB](https://via.placeholder.com/15/2563EB/000000?text=+) | `#2563EB` |
| 🔷 Azul Oscuro | ![#1E3A8A](https://via.placeholder.com/15/1E3A8A/000000?text=+) | `#1E3A8A` |
| 🟢 Verde Éxito | ![#10B981](https://via.placeholder.com/15/10B981/000000?text=+) | `#10B981` |
| ⚪ Blanco | ![#FFFFFF](https://via.placeholder.com/15/FFFFFF/000000?text=+) | `#FFFFFF` |
| ⚪ Gris Fondo | ![#F8FAFC](https://via.placeholder.com/15/F8FAFC/000000?text=+) | `#F8FAFC` |
| ⚫ Gris Bordes | ![#CBD5E1](https://via.placeholder.com/15/CBD5E1/000000?text=+) | `#CBD5E1` |
| ⚫ Texto Principal | ![#334155](https://via.placeholder.com/15/334155/000000?text=+) | `#334155` |
| ⚫ Texto Secundario | ![#64748B](https://via.placeholder.com/15/64748B/000000?text=+) | `#64748B` |
| 🔴 Error | ![#EF4444](https://via.placeholder.com/15/EF4444/000000?text=+) | `#EF4444` |
| 🟠 Advertencia | ![#F59E0B](https://via.placeholder.com/15/F59E0B/000000?text=+) | `#F59E0B` |

---

# 🎨 Gradiente Principal

```css
background: linear-gradient(
135deg,
#2563EB,
#1E3A8A
);
```

---

# 📋 Principios de Desarrollo

El proyecto fue desarrollado aplicando:

- ✅ Clean Architecture
- ✅ SOLID
- ✅ Repository Pattern
- ✅ Unit of Work
- ✅ Dependency Injection
- ✅ DTO Pattern
- ✅ Service Layer
- ✅ CI/CD
- ✅ GitFlow

---

# 🚀 Roadmap

## Sprint 1

- Arquitectura
- Login
- Usuarios
- Dashboard
- SQLite

## Sprint 2

- Conversión de Monedas
- Monedas
- Tasas de Cambio

## Sprint 3

- Conversión de Unidades
- Historial
- Auditoría

## Sprint 4

- Reportes
- Configuración
- Testing
- CI/CD

---

# 🔄 Flujo del Sistema

```

Usuario

↓

MVC Controller

↓

Application Service

↓

Repository

↓

SQLite

↓

Resultado

↓

Vista MVC

```

---

# 👥 Equipo

- Scrum Master / Analista
- Backend Developer
- Database Developer
- Frontend Developer / QA

---

# 📄 Licencia

Proyecto desarrollado con fines académicos para el curso de **Paradigmas de Programación** de la **Universidad Fidélitas**.

---

<div align="center">

### 🚀 Convergex

**Conversor Inteligente de Monedas y Unidades**

Desarrollado con ❤️ utilizando ASP.NET Core MVC, Clean Architecture y SOLID.

</div>