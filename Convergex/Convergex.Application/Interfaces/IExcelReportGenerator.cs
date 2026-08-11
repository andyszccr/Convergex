using Convergex.Application.DTOs.Reports;

namespace Convergex.Application.Interfaces;

public interface IExcelReportGenerator
{
    byte[] Generate(ReportDocument document);
}
