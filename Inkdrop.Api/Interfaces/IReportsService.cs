using Inkdrop.Api.Core;
using Inkdrop.Api.DTOs.Responses;

namespace Inkdrop.Api.Interfaces;

public interface IReportsService
{
    Task<ServiceResult<ReportResponse>> GetPageVolumeAsync(
        DateTime? startDate,
        DateTime? endDate,
        Guid? printerId = null,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<ReportResponse>> GetTonerConsumptionAsync(
        DateTime? startDate,
        DateTime? endDate,
        Guid? printerId = null,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<List<PredictiveMetricResponse>>> GetPredictiveAnalysisAsync(
        DateTime? startDate,
        DateTime? endDate,
        Guid? printerId = null,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<ExecutiveSummaryResponse>> GetExecutiveSummaryAsync(
        DateTime? startDate,
        DateTime? endDate,
        Guid? printerId = null,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<PrintedPagesResponse>> GetPrintedPagesAsync(
        DateTime? startDate,
        DateTime? endDate,
        Guid? printerId,
        CancellationToken cancellationToken = default);
}

public record ReportResponse(
    List<string> Labels,
    List<ReportDataset> Datasets,
    ReportSummary Summary
);

public record ReportDataset(
    string Label,
    List<double> Data,
    string? Color = null
);

public record ReportSummary(
    double TotalValue,
    double AverageConsumption,
    string Trend,
    string Unit
);

public record ExecutiveSummaryResponse(
    double TotalPages,
    double AvgFleetHealth,
    string TopConsumerPrinter,
    int CriticalTonerCount
);

public record PredictiveMetricResponse(
    Guid PrinterId,
    string PrinterName,
    string Color,
    int EstimatedDaysRemaining,
    DateTime EstimatedDate,
    double Confidence
);
