using Convergex.Application.DTOs.Reports;

namespace Convergex.Application.Interfaces;

public interface IReportService
{
    Task<ConversionsReportDto> GetConversionsReportAsync(ConversionsReportFilterDto filter, CancellationToken cancellationToken = default);
    Task<RatesReportDto> GetRatesReportAsync(RatesReportFilterDto filter, CancellationToken cancellationToken = default);
    Task<AuditReportDto> GetAuditReportAsync(AuditReportFilterDto filter, CancellationToken cancellationToken = default);
    Task<UsersActivityReportDto> GetUsersActivityReportAsync(UsersActivityReportFilterDto filter, CancellationToken cancellationToken = default);

    ReportDocument BuildConversionsDocument(ConversionsReportDto report, string generatedByUserName);
    ReportDocument BuildRatesDocument(RatesReportDto report, string generatedByUserName);
    ReportDocument BuildAuditDocument(AuditReportDto report, string generatedByUserName);
    ReportDocument BuildUsersActivityDocument(UsersActivityReportDto report, string generatedByUserName);
}
