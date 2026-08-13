using Financial.Application.Interfaces;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using System.Text;

namespace Financial.Application.Services;

/// <summary>
/// Service for extracting text from various file types
/// </summary>
public class TextExtractionService : ITextExtractionService
{
    private readonly ILogger<TextExtractionService> _logger;

    public TextExtractionService(ILogger<TextExtractionService> logger)
    {
        _logger = logger;
        // Set EPPlus license context
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
    }

    public async Task<string> ExtractTextAsync(Stream fileStream, string fileName, string contentType, CancellationToken cancellationToken = default)
    {
        try
        {
            if (contentType.Contains("pdf") || fileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            {
                return await ExtractFromPdfAsync(fileStream, cancellationToken);
            }
            else if (contentType.Contains("excel") || contentType.Contains("spreadsheet") ||
                     fileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase) ||
                     fileName.EndsWith(".xls", StringComparison.OrdinalIgnoreCase))
            {
                return await ExtractFromExcelAsync(fileStream, cancellationToken);
            }
            else if (contentType.Contains("text") || fileName.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
            {
                return await ExtractFromTextAsync(fileStream, cancellationToken);
            }
            else
            {
                throw new NotSupportedException($"File type '{contentType}' is not supported for text extraction");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting text from file: {FileName}", fileName);
            throw;
        }
    }

    public bool SupportsFileType(string contentType, string fileName)
    {
        var lowerFileName = fileName.ToLowerInvariant();
        var lowerContentType = contentType.ToLowerInvariant();

        return lowerContentType.Contains("pdf") ||
               lowerContentType.Contains("excel") ||
               lowerContentType.Contains("spreadsheet") ||
               lowerContentType.Contains("text") ||
               lowerFileName.EndsWith(".pdf") ||
               lowerFileName.EndsWith(".xlsx") ||
               lowerFileName.EndsWith(".xls") ||
               lowerFileName.EndsWith(".txt");
    }

    private async Task<string> ExtractFromPdfAsync(Stream fileStream, CancellationToken cancellationToken)
    {
        var sb = new StringBuilder();

        using (var pdfReader = new PdfReader(fileStream))
        using (var pdfDoc = new PdfDocument(pdfReader))
        {
            for (int i = 1; i <= pdfDoc.GetNumberOfPages(); i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var page = pdfDoc.GetPage(i);
                var strategy = new SimpleTextExtractionStrategy();
                var text = PdfTextExtractor.GetTextFromPage(page, strategy);
                sb.AppendLine(text);
            }
        }

        return await Task.FromResult(sb.ToString().Trim());
    }

    private async Task<string> ExtractFromExcelAsync(Stream fileStream, CancellationToken cancellationToken)
    {
        var sb = new StringBuilder();

        using (var package = new ExcelPackage(fileStream))
        {
            foreach (var worksheet in package.Workbook.Worksheets)
            {
                cancellationToken.ThrowIfCancellationRequested();

                sb.AppendLine($"Sheet: {worksheet.Name}");

                var start = worksheet.Dimension.Start;
                var end = worksheet.Dimension.End;

                for (int row = start.Row; row <= end.Row; row++)
                {
                    var rowValues = new List<string>();
                    for (int col = start.Column; col <= end.Column; col++)
                    {
                        var cell = worksheet.Cells[row, col];
                        if (cell.Value != null)
                        {
                            rowValues.Add(cell.Value.ToString() ?? string.Empty);
                        }
                    }

                    if (rowValues.Any())
                    {
                        sb.AppendLine(string.Join(" | ", rowValues));
                    }
                }

                sb.AppendLine();
            }
        }

        return await Task.FromResult(sb.ToString().Trim());
    }

    private async Task<string> ExtractFromTextAsync(Stream fileStream, CancellationToken cancellationToken)
    {
        using (var reader = new StreamReader(fileStream, Encoding.UTF8))
        {
            return await reader.ReadToEndAsync(cancellationToken);
        }
    }
}
