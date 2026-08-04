# Convergex — Issues v2.0 (Extras y siguientes módulos)

Documento de planificación para la **versión 2.0** del Product Backlog.

Complementa `issues.md` (v1.0) y sirve para decidir qué implementar como **extras** o siguientes sprints.

---

## 1. Estado actual (v1.0 entregada)

| Módulo | Estado | Notas |
|--------|--------|--------|
| 1. Dashboard | Completado | KPIs, gráficos, actividad, layout |
| 2. Autenticación y Usuarios | Completado | Login, logout, perfil, roles, CRUD, recuperar clave |
| 3. Conversión de Monedas | Completado (funcional) | Conversión, monedas, tasas, historial básico, validaciones |

Ya cubierto parcialmente por el Módulo 3 (aunque en el backlog v1.0 aparezcan aparte):

- Gestión de monedas (CRUD + activo/inactivo)
- Tasas de cambio (CRUD + consulta de par, tasa inversa)
- Historial básico (listado + filtros por tipo, usuario y fechas)

---

## 2. Módulos del backlog v1.0 aún pendientes

### Prioridad alta (core del producto)

| # | Módulo | Por qué conviene |
|---|--------|------------------|
| 4 | Conversión de Unidades | Completa el valor de monedas + unidades |
| 7 | Gestión de Unidades | Catálogo necesario para el módulo 4 |
| 5 | Historial avanzado | Paginación, detalle y búsqueda más completa |
| 8 | Gestión de Tasas (ampliada) | Historial de tasas + actualización automática |
| 12 | Integración API | Tasas en tiempo real |

### Prioridad media (calidad y trazabilidad)

| # | Módulo | Por qué conviene |
|---|--------|------------------|
| 9 | Auditoría | Trazabilidad académica / empresarial |
| 13 | Logs (Serilog) | Diagnóstico y buenas prácticas |
| 10 | Reportes PDF/Excel | Entregable muy visible para demos |
| 11 | Configuración | Tema, idioma, moneda default, zona horaria |
| 14 | CI/CD + Testing | Automatización y evidencia de calidad |

---

## 3. Extras sugeridos para Issues v2.0

### 3.1 Extras de producto

| Extra | Descripción | Esfuerzo |
|-------|-------------|----------|
| Favoritos de conversión | Guardar pares frecuentes (USD→CRC, m→km) | Bajo |
| Calculadora rápida (widget) | Mini convertidor en Dashboard | Bajo |
| Comparador de tasas | Ver varias monedas contra una base | Medio |
| Alertas de tasa | Aviso si USD/CRC sube o baja X% | Medio |
| Conversión múltiple | Un monto → varias monedas destino | Medio |
| Modo offline / última tasa conocida | Usar caché si falla la API | Medio |
| Favoritos / recientes en UI | Últimas 5 conversiones del usuario | Bajo |
| Exportar historial CSV | Descarga rápida sin PDF | Bajo |

### 3.2 Extras de unidades (ampliación del Módulo 4)

| Extra | Descripción | Esfuerzo |
|-------|-------------|----------|
| Más categorías | Temperatura, tiempo, área, velocidad | Medio |
| Factores editables | Admin puede ajustar factores de conversión | Medio |
| Unidades personalizadas | El usuario define unidades propias | Alto |

### 3.3 Extras de seguridad y usuarios

| Extra | Descripción | Esfuerzo |
|-------|-------------|----------|
| Login Google / Microsoft real | OAuth (hoy solo UI deshabilitada) | Alto |
| Bloqueo por intentos fallidos | Anti fuerza bruta | Bajo |
| 2FA simple (email/código) | Capa extra de seguridad | Alto |
| Sesiones activas | Ver/cerrar sesiones | Medio |
| Políticas de contraseña | Complejidad, expiración | Bajo |
| Auditoría de accesos | Quién entró y cuándo | Bajo |

### 3.4 Extras técnicos / DevOps

| Extra | Descripción | Esfuerzo |
|-------|-------------|----------|
| Migraciones EF Core | Reemplazar EnsureCreated | Medio |
| SQL Server en producción | Connection string por entorno | Medio |
| Health checks | Endpoint /health para monitoreo | Bajo |
| Swagger / Minimal API interna | API propia de Convergex | Medio |
| Docker Compose | App + SQL Server listos | Medio |
| Tests xUnit reales | Auth, conversión, tasas | Medio |
| Seed demo rico | Datos realistas para presentaciones | Bajo |

### 3.5 Extras de UX / diseño

| Extra | Descripción | Esfuerzo |
|-------|-------------|----------|
| Diseño final Conversión/Monedas/Tasas | Funcional primero, UI después | Medio |
| Dark mode | Ligado a Configuración | Medio |
| Toasts / notificaciones | En vez de solo TempData | Bajo |
| Empty states ilustrados | Pantallas vacías más claras | Bajo |
| Onboarding primer uso | Tour corto al primer login | Medio |

---

## 4. Propuesta de Issues v2.0 (checklist)

### Epic A — Unidades (recomendado siguiente)

- [ ] CRUD Unidades (Longitud, Peso, Volumen)
- [ ] Conversión por categoría
- [ ] Guardar en historial (ConversionType.Unit)
- [ ] Validaciones y mensajes
- [ ] (Extra) Temperatura / Tiempo

### Epic B — Historial y reportes

- [ ] Paginación del historial
- [ ] Detalle de conversión
- [ ] Exportar CSV
- [ ] Reporte PDF
- [ ] Reporte Excel

### Epic C — Tasas inteligentes

- [ ] Historial de tasas (cambios en el tiempo)
- [ ] Cliente HTTP + API externa
- [ ] Job / botón Actualizar tasas
- [ ] Caché de tasas
- [ ] (Extra) Alertas de variación

### Epic D — Auditoría y logs

- [ ] Entidad AuditLog + registro de eventos
- [ ] Pantalla de consulta/filtros
- [ ] Serilog (archivo + consola)
- [ ] Middleware de excepciones

### Epic E — Plataforma

- [ ] Configuración general (tema, idioma, moneda default)
- [ ] Migraciones EF + SQL Server
- [ ] GitHub Actions (build + test)
- [ ] Suite xUnit mínima
- [ ] (Extra) Docker

### Epic F — Extras rápidos (wins de demo)

- [ ] Widget convertidor en Dashboard
- [ ] Pares favoritos
- [ ] Bloqueo por intentos de login
- [ ] Toasts de éxito/error
- [ ] Diseño UI de Conversión / Monedas / Tasas

---

## 5. Roadmap sugerido v2.0

```text
Sprint 5  →  Unidades (CRUD + conversión) + pulido historial
Sprint 6  →  API tasas + caché + historial de tasas
Sprint 7  →  Auditoría + Serilog + reportes CSV/PDF
Sprint 8  →  Configuración + CI/CD + tests + extras de demo
```

---

## 6. Criterios para elegir un extra

Implementa primero un extra si cumple al menos 2 de estos puntos:

1. Se ve bien en una demo académica
2. Reutiliza capas ya hechas (Domain / Application / Persistence)
3. Refuerza Clean Architecture o SOLID
4. Es usable en menos de 1 sprint
5. Diferencia a Convergex de un CRUD básico

### Top 5 recomendados ahora

1. Conversión de Unidades
2. Diseño UI del módulo de monedas/conversión
3. API externa de tasas + caché
4. Auditoría de acciones
5. Reportes PDF/Excel o export CSV

---

## 7. Cómo usar este documento

1. Elige un Epic (A–F)
2. Crea GitHub Issues a partir de los checklists
3. Mueve a Project Board: Backlog → Ready → In Progress → Done
4. Cuando un módulo v2.0 se complete, márcalo aquí y sincroniza con `issues.md`

---

## 8. Relación con v1.0

| Documento | Uso |
|-----------|-----|
| `issues.md` | Backlog original / estado v1.0 |
| `README-Issues-v2.0.md` | Extras + priorización de lo que sigue |

---

Convergex v2.0 — Conversor inteligente de monedas y unidades  
Universidad Fidélitas — Paradigmas de Programación
