using CsvHelper;
using CsvHelper.Configuration;
using Financial.Application.DTOs.Import;
using Financial.Application.Interfaces;
using System.Globalization;

namespace Financial.Application.Services.Import;

public class CsvParserService : IFileParserService
{
    public bool CanParse(string fileName)
    {
        return fileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase);
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
            using var reader = new StreamReader(fileStream);
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HeaderValidated = null,
                MissingFieldFound = null,
                HasHeaderRecord = true
            };

            using var csv = new CsvReader(reader, config);
            
            await csv.ReadAsync();
            csv.ReadHeader();

            int rowNumber = 1;
            while (await csv.ReadAsync())
            {
                rowNumber++;
                var transaction = ParseRow(csv, rowNumber);
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
                Message = $"Error parsing CSV file: {ex.Message}"
            });
        }

        return preview;
    }

    private TransactionImportDto ParseRow(CsvReader csv, int rowNumber)
    {
        var transaction = new TransactionImportDto
        {
            RowNumber = rowNumber,
            IsValid = true,
            ValidationMessages = new List<string>()
        };

        try
        {
            // Expected CSV format: Date,Amount,Currency,Merchant,Category,Account,Notes
            transaction.Date = ParseDate(csv.GetField("Date") ?? csv.GetField(0));
            transaction.Amount = ParseDecimal(csv.GetField("Amount") ?? csv.GetField(1));
            transaction.Currency = csv.GetField("Currency") ?? csv.GetField(2) ?? "PYG";
            transaction.Merchant = csv.GetField("Merchant") ?? csv.GetField(3);
            transaction.Category = csv.GetField("Category") ?? csv.GetField(4);
            transaction.Account = csv.GetField("Account") ?? csv.GetField(5);
            transaction.Notes = csv.GetField("Notes") ?? csv.GetField(6);

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

    private DateTime ParseDate(string? dateString)
    {
        if (string.IsNullOrWhiteSpace(dateString))
            return default;

        if (DateTime.TryParse(dateString, out var date))
            return date;

        return default;
    }

    private decimal ParseDecimal(string? amountString)
    {
        if (string.IsNullOrWhiteSpace(amountString))
            return 0;

        // Remove currency symbols and spaces
        amountString = amountString.Replace("Gs.", "").Replace("$", "").Replace(",", "").Trim();

        if (decimal.TryParse(amountString, NumberStyles.Any, CultureInfo.InvariantCulture, out var amount))
            return amount;

        return 0;
    }
}
