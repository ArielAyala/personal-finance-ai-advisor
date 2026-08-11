using OfficeOpenXml;
using Financial.Application.DTOs.Import;
using Financial.Application.Interfaces;
using System.Globalization;

namespace Financial.Application.Services.Import;

public class ExcelParserService : IFileParserService
{
    public ExcelParserService()
    {
        // Set EPPlus license context (NonCommercial or Commercial)
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
    }

    public bool CanParse(string fileName)
    {
        return fileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase) ||
               fileName.EndsWith(".xls", StringComparison.OrdinalIgnoreCase);
    }

    public async Task<ImportPreviewDto> ParseFileAsync(Stream fileStream, string fileName, Guid userId, CancellationToken cancellationToken = default)
    {
        var preview = new ImportPreviewDto
        {
            BatchId = Guid.NewGuid(),
            FileName = fileName,
            Transactions = new List<TransactionImportDto>()
        };

        try
        {
            using var package = new ExcelPackage(fileStream);
            var worksheet = package.Workbook.Worksheets.FirstOrDefault();

            if (worksheet == null)
            {
                preview.Errors.Add(new ImportValidationError
                {
                    RowNumber = 0,
                    Field = "File",
                    Message = "No worksheet found in Excel file"
                });
                return preview;
            }

            // Assume first row is header
            int startRow = 2;
            int endRow = worksheet.Dimension?.End.Row ?? 0;

            for (int row = startRow; row <= endRow; row++)
            {
                var transaction = ParseRow(worksheet, row);
                preview.Transactions.Add(transaction);
            }

            preview.TotalRecords = preview.Transactions.Count;
            preview.ValidRecords = preview.Transactions.Count(t => t.IsValid);
            preview.InvalidRecords = preview.Transactions.Count(t => !t.IsValid);
        }
        catch (Exception ex)
        {
            preview.Errors.Add(new ImportValidationError
            {
                RowNumber = 0,
                Field = "File",
                Message = $"Error parsing Excel file: {ex.Message}"
            });
        }

        return await Task.FromResult(preview);
    }

    private TransactionImportDto ParseRow(ExcelWorksheet worksheet, int rowNumber)
    {
        var transaction = new TransactionImportDto
        {
            RowNumber = rowNumber,
            IsValid = true,
            ValidationMessages = new List<string>()
        };

        try
        {
            // Expected Excel format (columns): Date | Amount | Currency | Merchant | Category | Account | Notes
            transaction.Date = ParseDate(worksheet.Cells[rowNumber, 1].Value);
            transaction.Amount = ParseDecimal(worksheet.Cells[rowNumber, 2].Value);
            transaction.Currency = worksheet.Cells[rowNumber, 3].Value?.ToString() ?? "PYG";
            transaction.Merchant = worksheet.Cells[rowNumber, 4].Value?.ToString();
            transaction.Category = worksheet.Cells[rowNumber, 5].Value?.ToString();
            transaction.Account = worksheet.Cells[rowNumber, 6].Value?.ToString();
            transaction.Notes = worksheet.Cells[rowNumber, 7].Value?.ToString();

            // Validation
            if (transaction.Date == default)
            {
                transaction.IsValid = false;
                transaction.ValidationMessages.Add("Invalid or missing date");
            }

            if (transaction.Amount == 0)
            {
                transaction.IsValid = false;
                transaction.ValidationMessages.Add("Invalid or missing amount");
            }

            if (string.IsNullOrWhiteSpace(transaction.Currency))
            {
                transaction.IsValid = false;
                transaction.ValidationMessages.Add("Missing currency");
            }
        }
        catch (Exception ex)
        {
            transaction.IsValid = false;
            transaction.ValidationMessages.Add($"Parse error: {ex.Message}");
        }

        return transaction;
    }

    private DateTime ParseDate(object? value)
    {
        if (value == null)
            return default;

        if (value is DateTime dateTime)
            return dateTime;

        if (DateTime.TryParse(value.ToString(), out var date))
            return date;

        return default;
    }

    private decimal ParseDecimal(object? value)
    {
        if (value == null)
            return 0;

        if (value is decimal decimalValue)
            return decimalValue;

        if (value is double doubleValue)
            return (decimal)doubleValue;

        var stringValue = value.ToString()?.Replace("Gs.", "").Replace("$", "").Replace(",", "").Trim();

        if (decimal.TryParse(stringValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var amount))
            return amount;

        return 0;
    }
}
