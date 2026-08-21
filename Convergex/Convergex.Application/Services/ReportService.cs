using Convergex.Application.DTOs.Reports;
using Convergex.Application.Interfaces;
using Convergex.Domain.Enums;

namespace Convergex.Application.Services;

public class ReportService : IReportService
{
    private const int ExportRowCap = 5000;

    private readonly IConversionRepository _conversionRepository;
    private readonly IExchangeRateRepository _exchangeRateRepository;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IUserRepository _userRepository;

    public ReportService(
        IConversionRepository conversionRepository,
        IExchangeRateRepository exchangeRateRepository,
        IAuditLogRepository auditLogRepository,
        IUserRepository userRepository)
    {
        _conversionRepository = conversionRepository;
        _exchangeRateRepository = exchangeRateRepository;
        _auditLogRepository = auditLogRepository;
        _userRepository = userRepository;
    }

    public async Task<ConversionsReportDto> GetConversionsReportAsync(
        ConversionsReportFilterDto filter, CancellationToken cancellationToken = default)
    {
        var (items, _) = await _conversionRepository.GetHistoryPagedAsync(
            filter.Type, filter.UserName, filter.FromUtc, filter.ToUtc, 1, ExportRowCap, cancellationToken);

        if (!string.IsNullOrWhiteSpace(filter.CurrencyCode))
        {
            var code = filter.CurrencyCode.Trim().ToUpperInvariant();
            items = items.Where(c => c.FromCode == code || c.ToCode == code).ToList();
        }

        var rows = items.Select(c => new ConversionsReportRowDto
        {
            Id = c.Id,
            CreatedAt = c.CreatedAt,
            TypeName = c.Type == ConversionType.Currency ? "Moneda" : "Unidad",
            FromCode = c.FromCode,
            ToCode = c.ToCode,
            Amount = c.Amount,
            RateApplied = c.RateApplied,
            Result = c.Result,
            UserName = c.UserName
        }).ToList();

        var totalsByCurrency = items
            .GroupBy(c => c.FromCode)
            .Select(g => new CurrencyTotalDto
            {
                CurrencyCode = g.Key,
                Operations = g.Count(),
                TotalAmount = g.Sum(x => x.Amount)
            })
            .OrderByDescending(x => x.TotalAmount)
            .ToList();

        return new ConversionsReportDto
        {
            Items = rows,
            TotalOperations = items.Count,
            TotalAmount = items.Sum(c => c.Amount),
            TotalResult = items.Sum(c => c.Result),
            AverageRate = items.Count > 0 ? Math.Round(items.Average(c => c.RateApplied), 6) : 0,
            TotalsByCurrency = totalsByCurrency
        };
    }

    public async Task<RatesReportDto> GetRatesReportAsync(
        RatesReportFilterDto filter, CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _exchangeRateRepository.GetHistoryAsync(
            filter.BaseCurrencyId, filter.TargetCurrencyId, filter.FromUtc, filter.ToUtc, filter.Source,
            1, ExportRowCap, cancellationToken);

        var rows = new List<RatesReportRowDto>(items.Count);
        foreach (var item in items)
        {
            var previous = await _exchangeRateRepository.GetPreviousAsync(
                item.BaseCurrencyId, item.TargetCurrencyId, item.EffectiveAt, item.Id, cancellationToken);

            decimal? variation = null;
            if (previous is not null)
            {
                var previousMid = (previous.BuyRate + previous.SellRate) / 2;
                var currentMid = (item.BuyRate + item.SellRate) / 2;
                if (previousMid != 0)
                {
                    variation = Math.Round((currentMid - previousMid) * 100m / previousMid, 2);
                }
            }

            rows.Add(new RatesReportRowDto
            {
                EffectiveAt = item.EffectiveAt,
                Pair = $"{item.BaseCurrency.Code}/{item.TargetCurrency.Code}",
                BuyRate = item.BuyRate,
                SellRate = item.SellRate,
                SourceName = item.Source == ExchangeRateSource.Api ? "API" : "Manual",
                StatusName = item.Status switch
                {
                    ExchangeRateStatus.Active => "Activa",
                    ExchangeRateStatus.Inactive => "Inactiva",
                    _ => "Histórica"
                },
                VariationPercent = variation
            });
        }

        var spreadPercents = items
            .Where(i => i.BuyRate != 0)
            .Select(i => (i.SellRate - i.BuyRate) * 100m / i.BuyRate)
            .ToList();
        var variations = rows.Where(r => r.VariationPercent.HasValue).Select(r => r.VariationPercent!.Value).ToList();

        return new RatesReportDto
        {
            Items = rows,
            TotalRecords = totalCount,
            AverageBuyRate = items.Count > 0 ? Math.Round(items.Average(i => i.BuyRate), 6) : 0,
            AverageSellRate = items.Count > 0 ? Math.Round(items.Average(i => i.SellRate), 6) : 0,
            AverageSpreadPercent = spreadPercents.Count > 0 ? Math.Round(spreadPercents.Average(), 2) : 0,
            MaxVariationPercent = variations.Count > 0 ? variations.Max() : null,
            MinVariationPercent = variations.Count > 0 ? variations.Min() : null
        };
    }

    public async Task<AuditReportDto> GetAuditReportAsync(
        AuditReportFilterDto filter, CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _auditLogRepository.GetPagedAsync(
            filter.Keyword, filter.Action, filter.EntityName, filter.FromUtc, filter.ToUtc, 1, ExportRowCap, cancellationToken);

        var rows = items.Select(a => new AuditReportRowDto
        {
            Timestamp = a.Timestamp,
            UserName = a.UserName,
            ActionName = MapActionName(a.Action),
            EntityName = a.EntityName,
            EntityId = a.EntityId,
            IpAddress = a.IpAddress,
            StatusName = a.Status == AuditStatus.Success ? "Éxito" : "Fallido"
        }).ToList();

        var countsByAction = items
            .GroupBy(a => MapActionName(a.Action))
            .Select(g => new ActionCountDto { ActionName = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .ToList();

        return new AuditReportDto
        {
            Items = rows,
            TotalEvents = totalCount,
            SuccessCount = items.Count(a => a.Status == AuditStatus.Success),
            FailedCount = items.Count(a => a.Status == AuditStatus.Failed),
            CountsByAction = countsByAction
        };
    }

    public async Task<UsersActivityReportDto> GetUsersActivityReportAsync(
        UsersActivityReportFilterDto filter, CancellationToken cancellationToken = default)
    {
        var users = await _userRepository.GetAllAsync(cancellationToken);
        var (conversions, _) = await _conversionRepository.GetHistoryPagedAsync(
            null, null, filter.FromUtc, filter.ToUtc, 1, ExportRowCap, cancellationToken);

        var query = users.AsEnumerable();
        if (filter.RoleId.HasValue)
        {
            query = query.Where(u => u.RoleId == filter.RoleId.Value);
        }

        if (filter.IsActive.HasValue)
        {
            query = query.Where(u => u.IsActive == filter.IsActive.Value);
        }

        var rows = query
            .Select(u =>
            {
                var userConversions = conversions
                    .Where(c => string.Equals(c.UserName, u.FullName, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                return new UserActivityRowDto
                {
                    UserId = u.Id,
                    FullName = u.FullName,
                    Email = u.Email,
                    RoleName = u.Role.Name,
                    IsActive = u.IsActive,
                    LastLoginAt = u.LastLoginAt,
                    OperationsCount = userConversions.Count,
                    TotalAmount = userConversions.Sum(c => c.Amount)
                };
            })
            .OrderByDescending(r => r.OperationsCount)
            .ToList();

        return new UsersActivityReportDto
        {
            Items = rows,
            TotalUsers = rows.Count,
            ActiveUsers = rows.Count(r => r.IsActive),
            TotalOperations = rows.Sum(r => r.OperationsCount)
        };
    }

    public ReportDocument BuildConversionsDocument(ConversionsReportDto report, string generatedByUserName) => new()
    {
        Title = "Reporte de Conversiones",
        Subtitle = $"{report.TotalOperations} operación(es) registradas en el periodo consultado",
        GeneratedByUserName = generatedByUserName,
        Columns =
        [
            new ReportColumn { Header = "Fecha", Type = ReportColumnType.Date },
            new ReportColumn { Header = "Tipo" },
            new ReportColumn { Header = "De" },
            new ReportColumn { Header = "A" },
            new ReportColumn { Header = "Monto", Type = ReportColumnType.Currency },
            new ReportColumn { Header = "Tasa aplicada", Type = ReportColumnType.Number },
            new ReportColumn { Header = "Resultado", Type = ReportColumnType.Currency },
            new ReportColumn { Header = "Usuario" }
        ],
        Rows = report.Items.Select(r => (IReadOnlyList<ReportCell>)
        [
            ReportCell.DateAndTime(r.CreatedAt),
            ReportCell.Text(r.TypeName),
            ReportCell.Text(r.FromCode),
            ReportCell.Text(r.ToCode),
            ReportCell.Number(r.Amount),
            ReportCell.Number(r.RateApplied, "N6"),
            ReportCell.Number(r.Result, "N4"),
            ReportCell.Text(r.UserName)
        ]).ToList(),
        SummaryItems =
        [
            new ReportSummaryItem { Label = "Total de operaciones", Value = report.TotalOperations.ToString("N0") },
            new ReportSummaryItem { Label = "Monto total operado", Value = report.TotalAmount.ToString("N2") },
            new ReportSummaryItem { Label = "Resultado total", Value = report.TotalResult.ToString("N2") },
            new ReportSummaryItem { Label = "Tasa promedio", Value = report.AverageRate.ToString("N6") }
        ]
    };

    public ReportDocument BuildRatesDocument(RatesReportDto report, string generatedByUserName) => new()
    {
        Title = "Reporte de Historial de Tasas",
        Subtitle = $"{report.TotalRecords} registro(s) de tasas en el periodo consultado",
        GeneratedByUserName = generatedByUserName,
        Columns =
        [
            new ReportColumn { Header = "Vigente desde", Type = ReportColumnType.Date },
            new ReportColumn { Header = "Par" },
            new ReportColumn { Header = "Compra", Type = ReportColumnType.Number },
            new ReportColumn { Header = "Venta", Type = ReportColumnType.Number },
            new ReportColumn { Header = "Fuente" },
            new ReportColumn { Header = "Estado" },
            new ReportColumn { Header = "Variación %", Type = ReportColumnType.Number }
        ],
        Rows = report.Items.Select(r => (IReadOnlyList<ReportCell>)
        [
            ReportCell.DateAndTime(r.EffectiveAt),
            ReportCell.Text(r.Pair),
            ReportCell.Number(r.BuyRate, "N6"),
            ReportCell.Number(r.SellRate, "N6"),
            ReportCell.Text(r.SourceName),
            ReportCell.Text(r.StatusName),
            r.VariationPercent.HasValue ? ReportCell.Number(r.VariationPercent.Value, "N2") : ReportCell.Text("—")
        ]).ToList(),
        SummaryItems =
        [
            new ReportSummaryItem { Label = "Registros", Value = report.TotalRecords.ToString("N0") },
            new ReportSummaryItem { Label = "Compra promedio", Value = report.AverageBuyRate.ToString("N6") },
            new ReportSummaryItem { Label = "Venta promedio", Value = report.AverageSellRate.ToString("N6") },
            new ReportSummaryItem { Label = "Spread promedio", Value = $"{report.AverageSpreadPercent:N2}%" }
        ]
    };

    public ReportDocument BuildAuditDocument(AuditReportDto report, string generatedByUserName) => new()
    {
        Title = "Reporte de Auditoría",
        Subtitle = $"{report.TotalEvents} evento(s) registrados en el periodo consultado",
        GeneratedByUserName = generatedByUserName,
        Columns =
        [
            new ReportColumn { Header = "Fecha", Type = ReportColumnType.Date },
            new ReportColumn { Header = "Usuario" },
            new ReportColumn { Header = "Evento" },
            new ReportColumn { Header = "Módulo" },
            new ReportColumn { Header = "Entidad" },
            new ReportColumn { Header = "IP" },
            new ReportColumn { Header = "Estado" }
        ],
        Rows = report.Items.Select(r => (IReadOnlyList<ReportCell>)
        [
            ReportCell.DateAndTime(r.Timestamp),
            ReportCell.Text(r.UserName),
            ReportCell.Text(r.ActionName),
            ReportCell.Text(r.EntityName),
            ReportCell.Text(r.EntityId),
            ReportCell.Text(r.IpAddress),
            ReportCell.Text(r.StatusName)
        ]).ToList(),
        SummaryItems =
        [
            new ReportSummaryItem { Label = "Total de eventos", Value = report.TotalEvents.ToString("N0") },
            new ReportSummaryItem { Label = "Exitosos", Value = report.SuccessCount.ToString("N0") },
            new ReportSummaryItem { Label = "Fallidos", Value = report.FailedCount.ToString("N0") }
        ]
    };

    public ReportDocument BuildUsersActivityDocument(UsersActivityReportDto report, string generatedByUserName) => new()
    {
        Title = "Reporte de Usuarios y Actividad",
        Subtitle = $"{report.TotalUsers} usuario(s), {report.ActiveUsers} activo(s)",
        GeneratedByUserName = generatedByUserName,
        Columns =
        [
            new ReportColumn { Header = "Usuario" },
            new ReportColumn { Header = "Correo" },
            new ReportColumn { Header = "Rol" },
            new ReportColumn { Header = "Estado" },
            new ReportColumn { Header = "Último acceso", Type = ReportColumnType.Date },
            new ReportColumn { Header = "Operaciones", Type = ReportColumnType.Number },
            new ReportColumn { Header = "Monto total", Type = ReportColumnType.Currency }
        ],
        Rows = report.Items.Select(r => (IReadOnlyList<ReportCell>)
        [
            ReportCell.Text(r.FullName),
            ReportCell.Text(r.Email),
            ReportCell.Text(r.RoleName),
            ReportCell.Text(r.IsActive ? "Activo" : "Inactivo"),
            r.LastLoginAt.HasValue ? ReportCell.DateAndTime(r.LastLoginAt.Value) : ReportCell.Text("—"),
            ReportCell.Number(r.OperationsCount, "N0"),
            ReportCell.Number(r.TotalAmount)
        ]).ToList(),
        SummaryItems =
        [
            new ReportSummaryItem { Label = "Total de usuarios", Value = report.TotalUsers.ToString("N0") },
            new ReportSummaryItem { Label = "Usuarios activos", Value = report.ActiveUsers.ToString("N0") },
            new ReportSummaryItem { Label = "Operaciones procesadas", Value = report.TotalOperations.ToString("N0") }
        ]
    };

    private static string MapActionName(AuditAction action) => action switch
    {
        AuditAction.Create => "Creación",
        AuditAction.Update => "Edición",
        AuditAction.Delete => "Eliminación",
        AuditAction.Login => "Inicio de sesión",
        AuditAction.Logout => "Cierre de sesión",
        AuditAction.LoginFailed => "Inicio fallido",
        AuditAction.Conversion => "Conversión",
        _ => action.ToString()
    };
}
