# Convergex — Issues v2.0 (Extras y siguientes módulos)

Documento de planificación para la **versión 2.0** del Product Backlog.

Complementa [`issues.md`](./issues.md) (v1.0) y sirve para decidir qué implementar como **extras** o siguientes sprints.

---

## 1. Estado actual (revisión agosto 2026)

| Módulo v1.0 | Estado | Notas |
|-------------|--------|--------|
| 1. Dashboard | Completado | KPIs, gráficos, actividad, layout |
| 2. Autenticación y Usuarios | Completado | Login, logout, perfil, roles, CRUD, recuperar clave |
| 3. Conversión de Monedas | Completado | Conversión + historial |
| 4. Conversión de Unidades | Completado | Longitud, peso, volumen |
| 5. Historial | Completado | Filtros + paginación |
| 6. Gestión de Monedas | Completado | CRUD + activo/inactivo |
| 7. Gestión de Unidades | Completado | CRUD + categorías |
| 8. Tasas de Cambio | Completado | CRUD + sync API + historial de tasas |
| 9. Auditoría | Completado | Logs de eventos + consulta |
| 10. Reportes | Completado | PDF / Excel (historial, auditoría, usuarios, tasas) |
| 11. Configuración | Completado | Idioma, tema, moneda default, zona horaria |
| 12. Integración API | Completado | Cliente HTTP + sync + caché |
| 13. Logs | Completado | Serilog + middleware de excepciones |
| 14. CI/CD | Pendiente | Falta GitHub Actions y suite xUnit |

**Conclusión:** el backlog core de v1.0 está ~93% completo. Lo pendiente fuerte es **CI/CD + tests automatizados**.

---

## 2. Qué queda del backlog v1.0

### Prioridad inmediata

| # | Ítem | Por qué |
|---|------|---------|
| 14 | GitHub Actions (build + test) | Automatiza calidad en cada push |
| 14 | Suite xUnit real | Evidencia académica y regresión |
| 14 | Publicar artefactos | Entregable de despliegue |

### Mejoras menores sobre módulos ya hechos

| Ítem | Descripción | Esfuerzo |
|------|-------------|----------|
| Detalle de conversión | Vista `History/Details/{id}` | Bajo |
| Detalle de auditoría UI | Vista dedicada más rica (ya hay action Details) | Bajo |
| Alertas de tasa | Notificar variación % | Medio |
| Actualización programada de tasas | Background service / cron | Medio |

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
| Modo offline / última tasa conocida | Usar caché si falla la API | Bajo (parcialmente cubierto) |
| Exportar historial CSV | Descarga rápida sin PDF | Bajo |

### 3.2 Extras de unidades

| Extra | Descripción | Esfuerzo |
|-------|-------------|----------|
| Más categorías | Temperatura, tiempo, área, velocidad | Medio |
| Unidades personalizadas por usuario | Definir unidades propias | Alto |

### 3.3 Extras de seguridad

| Extra | Descripción | Esfuerzo |
|-------|-------------|----------|
| Login Google / Microsoft real | OAuth (UI existe, falta wiring) | Alto |
| Bloqueo por intentos fallidos | Anti fuerza bruta | Bajo |
| 2FA simple | Código por email | Alto |
| Políticas de contraseña | Complejidad / expiración | Bajo |

### 3.4 Extras técnicos / DevOps

| Extra | Descripción | Esfuerzo |
|-------|-------------|----------|
| GitHub Actions CI | Build + test en cada PR | Medio |
| Suite xUnit | Auth, conversión, tasas, auditoría | Medio |
| SQL Server producción | Connection string por entorno | Medio |
| Health checks | Endpoint `/health` | Bajo |
| Docker Compose | App + SQL Server | Medio |
| Swagger / Minimal API | API pública de Convergex | Medio |

### 3.5 Extras de UX

| Extra | Descripción | Esfuerzo |
|-------|-------------|----------|
| Pulido UI módulos funcionales | Homogeneizar diseño monedas/unidades/reportes | Medio |
| Dark mode aplicado global | Ya hay setting de tema; completar CSS | Medio |
| Toasts | Notificaciones no bloqueantes | Bajo |
| Onboarding primer uso | Tour corto al primer login | Medio |

---

## 4. Propuesta de Issues v2.0 (checklist)

### Epic A — CI/CD y calidad (recomendado siguiente)

- [ ] Configurar GitHub Actions
- [ ] Job de build
- [ ] Ejecutar tests en CI
- [ ] Publicar artefactos
- [ ] Suite xUnit (Auth, Conversión monedas, Unidades, Tasas)

### Epic B — Pulido de historial y auditoría

- [ ] Vista detalle de conversión
- [ ] Mejorar detalle de auditoría
- [ ] Export CSV del historial

### Epic C — Tasas inteligentes (ampliación)

- [ ] Alertas de variación
- [ ] Actualización programada (hosted service)
- [ ] Comparador multi-moneda

### Epic D — Seguridad avanzada

- [ ] Bloqueo por intentos fallidos
- [ ] OAuth Google / Microsoft
- [ ] Políticas de contraseña

### Epic E — Extensiones de producto

- [ ] Widget convertidor en Dashboard
- [ ] Pares favoritos
- [ ] Categorías extra de unidades (temperatura, tiempo)
- [ ] Dark mode completo

### Epic F — Plataforma

- [ ] Health checks
- [ ] Docker Compose
- [ ] SQL Server en producción documentado
- [ ] Minimal API / Swagger

---

## 5. Roadmap sugerido v2.0

```text
Sprint 5  →  CI/CD + xUnit (cerrar Módulo 14)
Sprint 6  →  Pulido UI + detalle historial/auditoría + CSV
Sprint 7  →  Alertas de tasa + hosted sync + favoritos
Sprint 8  →  OAuth / seguridad + Docker + health checks
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

1. GitHub Actions + tests xUnit (cerrar v1.0)
2. Vista detalle de historial
3. Widget convertidor en Dashboard
4. Alertas de variación de tasa
5. Dark mode completo / pulido UI

---

## 7. Cómo usar este documento

1. Elige un Epic (A–F)
2. Crea GitHub Issues a partir de los checklists
3. Mueve a Project Board: Backlog → Ready → In Progress → Done
4. Cuando un ítem v2.0 se complete, márcalo aquí y sincroniza con `issues.md`

---

## 8. Relación con v1.0

| Documento | Uso |
|-----------|-----|
| [`issues.md`](./issues.md) | Backlog original / estado v1.0 |
| **`README-Issues-v2.0.md`** | Extras + priorización de lo que sigue |
| [`../README.md`](../README.md) | Visión general del proyecto |

---

Convergex v2.0 — Conversor inteligente de monedas y unidades  
Universidad Fidélitas — Paradigmas de Programación
