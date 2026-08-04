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

- [ ] Login
- [ ] Logout
- [ ] CRUD Usuarios
- [ ] Gestión de Roles
- [ ] Perfil de usuario
- [ ] Recuperar contraseña

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

- [ ] CRUD Monedas
- [ ] Obtener tasas
- [ ] Conversión
- [ ] Validaciones
- [ ] Historial
- [ ] Mensajes de éxito

---



# 📌 Módulo 4 - Conversión de Unidades



## Objetivo

Realizar conversiones entre distintas unidades de medida.

### Categorías

- Longitud
- Peso
- Volumen



### Issues

- [ ] CRUD Unidades
- [ ] Conversión Longitud
- [ ] Conversión Peso
- [ ] Conversión Volumen
- [ ] Historial

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

- [ ] Tabla Historial
- [ ] Buscar por usuario
- [ ] Buscar por fecha
- [ ] Buscar por tipo
- [ ] Paginación

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

- [ ] CRUD Monedas
- [ ] Validaciones
- [ ] Catálogo
- [ ] Estado Activo

---



# 📌 Módulo 7 - Gestión de Unidades



## Objetivo

Administrar las unidades de medida.

### Categorías

- Longitud
- Peso
- Volumen



### Issues

- [ ] CRUD Unidades
- [ ] Categorías
- [ ] Validaciones

---



# 📌 Módulo 8 - Gestión de Tasas de Cambio



## Objetivo

Administrar las tasas de cambio del sistema.

### Funcionalidades

- Actualización manual
- Actualización automática
- Historial de tasas



### Issues

- [ ] CRUD Tasas
- [ ] Actualizar tasas
- [ ] Historial
- [ ] Integración API

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

- [ ] Registrar eventos
- [ ] Tabla Auditoría
- [ ] Consulta Auditoría
- [ ] Buscar eventos

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

- [ ] Reporte Historial
- [ ] Reporte Auditoría
- [ ] Exportar PDF
- [ ] Exportar Excel

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

- [ ] Configuración General
- [ ] Idioma
- [ ] Tema
- [ ] Zona Horaria

---



# 📌 Módulo 12 - Integración API



## Objetivo

Consumir servicios externos para obtener tasas de cambio.

### Funcionalidades

- Consumo REST API
- Caché
- Manejo de errores



### Issues

- [ ] Cliente HTTP
- [ ] Obtener tasas
- [ ] Actualización automática
- [ ] Caché

---



# 📌 Módulo 13 - Logs



## Objetivo

Registrar errores y eventos técnicos del sistema.

### Funcionalidades

- Logs
- Excepciones
- Performance



### Issues

- [ ] Configurar Serilog
- [ ] Logs Globales
- [ ] Middleware Excepciones

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



## 🚀 Sprint 1

- Arquitectura Clean
- Configuración del proyecto
- SQLite
- Login
- Usuarios
- Dashboard

---



## 🚀 Sprint 2

- Conversión de Monedas
- Gestión de Monedas
- Tasas de Cambio
- API Externa

---



## 🚀 Sprint 3

- Conversión de Unidades
- Historial
- Auditoría
- Logs

---



## 🚀 Sprint 4

- Reportes
- Configuración
- CI/CD
- Testing
- Optimización
- Documentación

---



# 🏗 Arquitectura

