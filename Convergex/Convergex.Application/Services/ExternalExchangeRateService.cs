using System.Net.Http.Json;
using System.Text.Json;
using Convergex.Application.DTOs.ExternalApis;
using Convergex.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Convergex.Application.Services;

public class ExternalExchangeRateService : IExternalExchangeRateService, IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ExternalExchangeRateService> _logger;
    private readonly string _apiUrl;
    private bool _disposed;

    public ExternalExchangeRateService(
        HttpClient httpClient,
        ILogger<ExternalExchangeRateService> logger,
        IConfiguration configuration)
    {
        _httpClient = httpClient;
        _logger = logger;
        _apiUrl = configuration["ExternalApis:TdcRateUrl"] ?? "http://apis.gometa.org/tdc/tdc.json";

        // Configurar el cliente HTTP para mayor seguridad
        _httpClient.Timeout = TimeSpan.FromSeconds(10);
        _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "Convergex-App/1.0");
    }

    public async Task<ExternalExchangeRateDto?> GetTdcRateAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Consultando API de tasas de cambio TDC desde: {ApiUrl}", _apiUrl);

            using var response = await _httpClient.GetAsync(
                _apiUrl,
                cancellationToken);

            // Validar que la respuesta sea exitosa
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "API externa retornó código de error: {StatusCode} - {ReasonPhrase}",
                    (int)response.StatusCode,
                    response.ReasonPhrase);

                return null;
            }

            // Leer el contenido de forma segura
            var data = await response.Content.ReadFromJsonAsync<ExternalExchangeRateDto>(
                cancellationToken: cancellationToken);

            // Validar que los datos sean correctos
            if (data is null)
            {
                _logger.LogWarning("API externa retornó datos nulos");
                return null;
            }

            // Validaciones de negocio
            if (data.Compra <= 0 || data.Venta <= 0)
            {
                _logger.LogWarning(
                    "API externa retornó tasas inválidas: Compra={Compra}, Venta={Venta}",
                    data.Compra,
                    data.Venta);

                return null;
            }

            if (string.IsNullOrWhiteSpace(data.VentaDate) || string.IsNullOrWhiteSpace(data.CompraDate))
            {
                _logger.LogWarning("API externa retornó fechas inválidas");
                return null;
            }

            _logger.LogInformation(
                "Tasa TDC obtenida exitosamente: Compra={Compra}, Venta={Venta}, Fecha={Fecha}",
                data.Compra,
                data.Venta,
                data.VentaDate);

            return data;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error de conexión al consultar API externa de tasas de cambio");
            return null;
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Timeout al consultar API externa de tasas de cambio");
            return null;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Error al deserializar respuesta de API externa");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al consultar API externa de tasas de cambio");
            return null;
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _httpClient?.Dispose();
            _disposed = true;
        }

        GC.SuppressFinalize(this);
    }
}