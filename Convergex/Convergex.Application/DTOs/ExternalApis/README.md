# Integración API Externa de Tasas de Cambio - TDC

## Descripción
Implementación segura para consumir la API de tasas de cambio TDC (http://apis.gometa.org/tdc/tdc.json) en el módulo de conversión de monedas de Convergex.

## Arquitectura

### Componentes Creados

1. **ExternalExchangeRateDto.cs** - DTO para mapear la respuesta JSON de la API externa
2. **IExternalExchangeRateService.cs** - Interfaz del servicio
3. **ExternalExchangeRateService.cs** - Implementación del servicio con manejo seguro de HTTP
4. **README.md** - Documentación completa

### Flujo de Datos

```
Usuario solicita conversión USD↔CRC
    ↓
CurrencyConversionService.ConvertAsync()
    ↓
ResolveRateAsync()
    ↓
1. Busca tasa directa en BD
2. Busca tasa inversa en BD  
3. Si no existe, consulta API externa TDC
    ↓
Retorna tasa y guarda conversión en historial
```

## Características de Seguridad

### 1. Configuración Segura de HttpClient
- Timeout de 10 segundos para evitar bloqueos
- Headers personalizados (Accept, User-Agent)
- Uso de `using` statement para disposal adecuado
- Inyección de dependencias con HttpClientFactory

### 2. Validaciones Múltiples
- Validación de código HTTP de respuesta (2xx)
- Validación de datos nulos después de deserialización
- Validación de valores numéricos (tasas > 0)
- Validación de fechas no vacías

### 3. Manejo de Excepciones Específico
- `HttpRequestException` - Errores de conexión
- `TaskCanceledException` - Timeouts
- `JsonException` - Errores de deserialización
- `Exception` - Catch-all para errores inesperados

### 4. Logging Completo
- Log de inicio de consulta con URL de API
- Log de éxito con detalles de tasas (compra/venta)
- Log de advertencias para datos inválidos
- Log de errores con excepción completa

## Configuración

### appsettings.json

```json
{
  "ExternalApis": {
    "TdcRateUrl": "http://apis.gometa.org/tdc/tdc.json"
  }
}
```

La URL de la API se configura en `appsettings.json` para:
- Facilitar cambios sin modificar código
- Permitir diferentes URLs por ambiente (dev/prod)
- Mejorar seguridad al centralizar configuraciones

## Uso

### En el Controlador (CurrencyConversionController.cs)

```csharp
// El servicio se inyecta automáticamente via DI
private readonly IExternalExchangeRateService _externalExchangeRateService;

// Se usa para obtener tasas actualizadas de USD/CRC
var externalRate = await _externalExchangeRateService.GetTdcRateAsync();
if (externalRate != null)
{
    model.ExternalCompraRate = externalRate.Compra;
    model.ExternalVentaRate = externalRate.Venta;
    model.ExternalRateDate = externalRate.VentaDate;
    model.HasExternalRate = true;
}
```

### En la Vista (Index.cshtml)

```html
<!-- Muestra la fuente de la tasa -->
<strong>Tasa actual: 537.50</strong>
<span class="badge bg-info">API Externa (TDC)</span>

<!-- Muestra ambos precios cuando hay datos de API externa -->
<div class="alert alert-info">
    <strong>Tasas TDC del 2026-08-08:</strong><br/>
    <span>Compra: <strong>535.00</strong></span>
    <span>Venta: <strong>537.50</strong></span>
</div>
```

## Monedas Soportadas por API Externa

Actualmente la integración soporta:
- **USD → CRC**: Usa tasa de venta de la API (para comprar USD con CRC)
- **CRC → USD**: Usa tasa inversa de compra (1/compra) (para vender USD por CRC)

Para otras combinaciones de monedas, se requiere tasa en base de datos.

## Registro en DI

```csharp
// En DependencyInjection.cs
services.AddHttpClient<IExternalExchangeRateService, ExternalExchangeRateService>();
```

Esto configura:
- HttpClient con pooling automático
- Inyección de ILogger automática
- Inyección de IConfiguration para leer configuración
- Lifetime transiente para el servicio

## Validaciones Implementadas

1. **Response.IsSuccessStatusCode** - Verifica código 2xx
2. **data != null** - Verifica que la deserialización fue exitosa
3. **data.Compra > 0 && data.Venta > 0** - Verifica tasas válidas
4. **!string.IsNullOrWhiteSpace(fechas)** - Verifica fechas presentes

## Manejo de Errores

Si la API falla:
- Se registra el error en logs con detalles
- Se retorna null
- El sistema continúa funcionando con tasas de BD
- El usuario ve mensaje: "No hay tasa de cambio disponible"

## Características de la Vista

### Badge de Fuente
Muestra el origen de la tasa:
- "Base de datos" - Tasa directa encontrada
- "Base de datos (inversa)" - Tasa inversa calculada
- "API Externa (TDC)" - Tasa obtenida de la API

### Panel de Tasas TDC
Cuando se usa la API externa, se muestra un panel informativo con:
- Fecha de la tasa
- Precio de compra (para vender USD)
- Precio de venta (para comprar USD)

## ViewModel

### Propiedades Agregadas

```csharp
public class CurrencyConversionViewModel
{
    // ... propiedades existentes ...
    
    // Precios de la API externa para USD/CRC
    public decimal? ExternalCompraRate { get; set; }
    public decimal? ExternalVentaRate { get; set; }
    public string? ExternalRateDate { get; set; }
    public bool HasExternalRate { get; set; }
}
```

## Próximos Pasos

- [ ] Agregar cache de tasas externas para reducir llamadas
- [ ] Soportar más pares de monedas desde API externa
- [ ] Implementar retry policy con Polly
- [ ] Agregar métricas de uso de API externa
- [ ] Agregar botón de actualización manual de tasas
- [ ] Implementar fallback a API cuando BD no tiene tasa (actualmente solo para display)

## Notas de Implementación

- La API TDC solo proporciona tasas para USD/CRC
- El sistema primero busca en BD, luego consulta la API externa
- Las tasas de la API se usan para display en tiempo real
- Las conversiones siempre se guardan en historial con la tasa aplicada
- El servicio es thread-safe y usa HttpClientFactory para pooling