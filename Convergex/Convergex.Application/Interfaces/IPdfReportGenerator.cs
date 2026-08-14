using Convergex.Application.DTOs.Reports;

namespace Convergex.Application.Interfaces;

public interface IPdfReportGenerator
{
    byte[] Generate(ReportDocument document);
}
