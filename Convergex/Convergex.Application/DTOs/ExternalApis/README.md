# Integración API Externa de Tasas de Cambio

## Descripción
Implementación segura para consumir la API de tasas de cambio TDC (http://apis.gometa.org/tdc/tdc.json) en el módulo de conversión de monedas.

## Arquitectura

### Componentes Creados

1. **ExternalExchangeRateDto.cs** - DTO para mapear la respuesta JSON de la API externa
2. **IExternalExchangeRateService.cs** - Interfaz del servicio
3. **ExternalExchangeRateService.cs** - Implementación del servicio con manejo seguro de HTTP

### Flujo de Datos

```
Usuario solicita conversión
    ↓
CurrencyConversionService.ConvertAsync()
    ↓
ResolveRateAsync()
    ↓
1. Busca tasa directa en BD
2. Busca tasa inversa en BD
3. Si no existe, consulta API externa (solo USD/CRC)
    ↓
Retorna tasa y guarda conversión en historial
```

## Características de Seguridad

### 1. Configuración Segura de HttpClient
- Timeout de 10 segundos para evitar bloqueos
- Headers personalizados (Accept, User-Agent)
- Uso de `using` statement para disposal adecuado

### 2. Validaciones Múltiples
- Validación de código HTTP de respuesta
- Validación de datos nulos
- Validación de valores numéricos (tasas > 0)
- Validación de fechas no vacías

### 3. Manejo de Excepciones Específico
- `HttpRequestException` - Errores de conexión
- `TaskCanceledException` - Timeouts
- `JsonException` - Errores de deserialización
- `Exception` - Catch-all para errores inesperados

### 4. Logging Completo
- Log de inicio de consulta
- Log de éxito con detalles de tasas
- Log de advertencias para datos inválidos
- Log de errores con excepción completa

## Uso

### En el Controlador
```csharp
// El servicio seinyecta automáticamente via DI
private readonly IExternalExchangeRateService _externalExchangeRateService;

// Se usa para obtener tasas actualizadas de USD/CRC
var rate = await _externalExchangeRateService.GetTdcRateAsync();
```

### En la Vista
```html
<!-- Muestra la fuente de la tasa -->
<strong>Tasa actual: 537.50</strong>
<span class="badge bg-info">API Externa (TDC)</span>
```

## Monedas Soportadas por API Externa

Actualmente la integración soporta:
- **USD → CRC**: Usa tasa de venta de la API
- **CRC → USD**: Usa tasa inversa de compra (1/compra)

Para otras combinaciones, se requiere tasa en base de datos.

## Registro en DI

```csharp
// En DependencyInjection.cs
services.AddHttpClient<IExternalExchangeRateService, ExternalExchangeRateService>();
```

Esto configura:
- HttpClient con pooling
- Inyección de ILogger automática
- Lifetime transiente para el servicio

## Validaciones Implementadas

1. **Response.IsSuccessStatusCode** - Verifica código 2xx
2. **data != null** - Verifica que la deserialización fue exitosa
3. **data.Compra > 0 && data.Venta > 0** - Verifica tasas válidas
4. **!string.IsNullOrWhiteSpace(fechas)** - Verifica fechas presentes

## Manejo de Errores

Si la API falla:
- Se registra el error en logs
- Se retorna null
- El sistema continúa funcionando con tasas de BD
- El usuario ve mensaje: "No hay tasa de cambio disponible"

## Próximos Pasos

- [ ] Agregar cache de tasas externas para reducir llamadas
- [ ] Soportar más pares de monedas desde API externa
- [ ] Agregar configuración de URL en appsettings.json
- [ ] Implementar retry policy con Polly
- [ ] Agregar métricas de uso de API externa