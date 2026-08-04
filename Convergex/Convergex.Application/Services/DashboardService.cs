using Convergex.Application.DTOs.Dashboard;
using Convergex.Application.Interfaces;
using Convergex.Domain.Enums;
using System.Globalization;

namespace Convergex.Application.Services;

public class DashboardService : IDashboardService
{
    private readonly IConversionRepository _conversionRepository;
    private readonly ICurrencyRepository _currencyRepository;
    private readonly IExchangeRateRepository _exchangeRateRepository;

    public DashboardService(
        IConversionRepository conversionRepository,
        ICurrencyRepository currencyRepository,
        IExchangeRateRepository exchangeRateRepository)
    {
        _conversionRepository = conversionRepository;
        _currencyRepository = currencyRepository;
        _exchangeRateRepository = exchangeRateRepository;
    }

    public async Task<DashboardSummaryDto> GetSummaryAsync(CancellationToken cancellationToken = default)
    {
        var today = DateTime.UtcNow.Date;
        var yesterday = today.AddDays(-1);
        var chartStart = today.AddDays(-6);

        var totalConversions = await _conversionRepository.CountAsync(cancellationToken);
        var conversionsToday = await _conversionRepository.CountBetweenAsync(today, today.AddDays(1), cancellationToken);
        var conversionsYesterday = await _conversionRepository.CountBetweenAsync(yesterday, today, cancellationToken);
        var activeCurrencies = await _currencyRepository.CountActiveAsync(cancellationToken);
        var recent = await _conversionRepository.GetRecentAsync(5, cancellationToken);
        var dailyCounts = await _conversionRepository.GetDailyCountsAsync(chartStart, cancellationToken);
        var typeCounts = await _conversionRepository.GetCountsByTypeAsync(cancellationToken);
        var latestRate = await _exchangeRateRepository.GetLatestAsync(cancellationToken);
        var topRates = await _exchangeRateRepository.GetTopAsync(4, cancellationToken);

        var culture = new CultureInfo("es-CR");
        var chartPoints = Enumerable.Range(0, 7)
            .Select(offset =>
            {
                var date = DateOnly.FromDateTime(chartStart.AddDays(offset));
                dailyCounts.TryGetValue(date, out var count);
                return new ConversionChartPointDto
                {
                    Date = date,
                    Label = culture.TextInfo.ToTitleCase(
                        date.ToDateTime(TimeOnly.MinValue).ToString("ddd", culture).TrimEnd('.')),
                    Count = count
                };
            })
            .ToList();

        var currencyCount = typeCounts.GetValueOrDefault(ConversionType.Currency);
        var unitCount = typeCounts.GetValueOrDefault(ConversionType.Unit);
        var typedTotal = Math.Max(currencyCount + unitCount, 1);

        var todayGrowth = conversionsYesterday == 0
            ? (conversionsToday > 0 ? 100m : 0m)
            : Math.Round((conversionsToday - conversionsYesterday) * 100m / conversionsYesterday, 1);

        var variations = new[] { 0.45m, -0.18m, 0.72m, -0.31m };
        var mappedRates = topRates.Select((rate, index) => new ExchangeRateSummaryDto
        {
            BaseCode = rate.BaseCurrency.Code,
            TargetCode = rate.TargetCurrency.Code,
            Rate = rate.Rate,
            VariationPercent = variations[index % variations.Length],
            UpdatedAt = rate.UpdatedAt
        }).ToList();

        return new DashboardSummaryDto
        {
            TotalConversions = totalConversions,
            ConversionsToday = conversionsToday,
            ConversionsYesterday = conversionsYesterday,
            ActiveCurrencies = activeCurrencies,
            ActiveUnits = 24,
            TodayGrowthPercent = todayGrowth,
            LatestExchangeRate = latestRate is null
                ? null
                : new ExchangeRateSummaryDto
                {
                    BaseCode = latestRate.BaseCurrency.Code,
                    TargetCode = latestRate.TargetCurrency.Code,
                    Rate = latestRate.Rate,
                    VariationPercent = mappedRates.FirstOrDefault()?.VariationPercent ?? 0.45m,
                    UpdatedAt = latestRate.UpdatedAt
                },
            ChartPoints = chartPoints,
            TypeShares =
            [
                new ConversionTypeShareDto
                {
                    Type = "Moneda",
                    Count = currencyCount,
                    Percentage = Math.Round(currencyCount * 100m / typedTotal, 1)
                },
                new ConversionTypeShareDto
                {
                    Type = "Unidad",
                    Count = unitCount,
                    Percentage = Math.Round(unitCount * 100m / typedTotal, 1)
                }
            ],
            RecentConversions = recent.Select(c => new RecentConversionDto
            {
                Id = c.Id,
                Type = c.Type.ToString(),
                FromCode = c.FromCode,
                ToCode = c.ToCode,
                Amount = c.Amount,
                Result = c.Result,
                UserName = c.UserName,
                CreatedAt = c.CreatedAt
            }).ToList(),
            TopRates = mappedRates
        };
    }
}
