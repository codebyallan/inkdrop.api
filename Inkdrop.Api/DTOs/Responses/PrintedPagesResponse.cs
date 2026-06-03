namespace Inkdrop.Api.DTOs.Responses;

public record PrintedPagesResponse(
    double TotalPages,
    double? MonoPages,
    double? ColorPages,
    DateTime PeriodStart,
    DateTime PeriodEnd,
    List<PrinterPageBreakdown> ByPrinter
);

public record PrinterPageBreakdown(
    Guid PrinterId,
    string PrinterName,
    double TotalPages,
    double? MonoPages,
    double? ColorPages
);
