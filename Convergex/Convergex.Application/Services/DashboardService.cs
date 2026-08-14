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
    private readonly IUnitRepository _unitRepository;

    public DashboardService(
        IConversionRepository conversionRepository,
        ICurrencyRepository currencyRepository,
        IExchangeRateRepository exchangeRateRepository,
        IUnitRepository unitRepository)
    {
        _conversionRepository = conversionRepository;
        _currencyRepository = currencyRepository;
        _exchangeRateRepository = exchangeRateRepository;
        _unitRepository = unitRepository;
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
        var activeUnits = await _unitRepository.CountActiveAsync(cancellationToken);
        var recent = await _conversionRepository.GetRecentAsync(5, cancellationToken);
        var dailyCounts = await _conversionRepository.GetDailyCountsAsync(chartStart, cancellationToken);
        var typeCounts = await _conversionRepository.GetCountsByTypeAsync(cancellationToken);
        var latestRate = await _exchangeRateRepository.GetLatestActiveAsync(cancellationToken);
        var topRates = await _exchangeRateRepository.GetTopActiveAsync(4, cancellationToken);

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

        var mappedRates = new List<ExchangeRateSummaryDto>(topRates.Count);
        foreach (var rate in topRates)
        {
            mappedRates.Add(await BuildSummaryAsync(rate, cancellationToken));
        }

        return new DashboardSummaryDto
        {
            TotalConversions = totalConversions,
            ConversionsToday = conversionsToday,
            ConversionsYesterday = conversionsYesterday,
            ActiveCurrencies = activeCurrencies,
            ActiveUnits = activeUnits,
            TodayGrowthPercent = todayGrowth,
            LatestExchangeRate = latestRate is null
                ? null
                : await BuildSummaryAsync(latestRate, cancellationToken),
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

    private async Task<ExchangeRateSummaryDto> BuildSummaryAsync(Domain.Entities.ExchangeRate rate, CancellationToken cancellationToken)
    {
        var previous = await _exchangeRateRepository.GetPreviousAsync(
            rate.BaseCurrencyId, rate.TargetCurrencyId, rate.EffectiveAt, rate.Id, cancellationToken);

        var currentMid = (rate.BuyRate + rate.SellRate) / 2;
        var variation = 0m;
        if (previous is not null)
        {
            var previousMid = (previous.BuyRate + previous.SellRate) / 2;
            if (previousMid != 0)
            {
                variation = Math.Round((currentMid - previousMid) * 100m / previousMid, 2);
            }
        }

        return new ExchangeRateSummaryDto
        {
            BaseCode = rate.BaseCurrency.Code,
            TargetCode = rate.TargetCurrency.Code,
            Rate = Math.Round(currentMid, 4),
            VariationPercent = variation,
            UpdatedAt = rate.EffectiveAt
        };
    }
}
