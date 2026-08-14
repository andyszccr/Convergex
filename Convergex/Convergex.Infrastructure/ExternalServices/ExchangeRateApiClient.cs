using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Convergex.Application.DTOs.ExchangeRates;
using Convergex.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace Convergex.Infrastructure.ExternalServices;

public class ExchangeRateApiClient : IExchangeRateProviderClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ExchangeRateApiClient> _logger;

    public ExchangeRateApiClient(HttpClient httpClient, ILogger<ExchangeRateApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<ExternalRateQuoteResult> GetLatestRatesAsync(
        string baseCode,
        IReadOnlyCollection<string> targetCodes,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await _httpClient.GetAsync(baseCode, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return new ExternalRateQuoteResult
                {
                    Success = false,
                    ErrorMessage = $"El proveedor externo respondió con estado {(int)response.StatusCode}."
                };
            }

            var payload = await response.Content.ReadFromJsonAsync<OpenErApiResponse>(cancellationToken: cancellationToken);
            if (payload is null || payload.Rates is null || !string.Equals(payload.Result, "success", StringComparison.OrdinalIgnoreCase))
            {
                return new ExternalRateQuoteResult { Success = false, ErrorMessage = "Respuesta inválida del proveedor externo de tasas de cambio." };
            }

            var filtered = payload.Rates
                .Where(kv => targetCodes.Contains(kv.Key, StringComparer.OrdinalIgnoreCase))
                .ToDictionary(kv => kv.Key, kv => kv.Value);

            return new ExternalRateQuoteResult
            {
                Success = true,
                BaseCode = payload.BaseCode ?? baseCode,
                Rates = filtered
            };
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
        {
            _logger.LogWarning(ex, "Fallo al consultar el proveedor externo de tasas de cambio para la base {BaseCode}.", baseCode);
            return new ExternalRateQuoteResult
            {
                Success = false,
                ErrorMessage = "No se pudo contactar al proveedor externo de tasas de cambio (red o tiempo de espera agotado)."
            };
        }
    }

    private class OpenErApiResponse
    {
        [JsonPropertyName("result")]
        public string? Result { get; set; }

        [JsonPropertyName("base_code")]
        public string? BaseCode { get; set; }

        [JsonPropertyName("rates")]
        public Dictionary<string, decimal>? Rates { get; set; }
    }
}
