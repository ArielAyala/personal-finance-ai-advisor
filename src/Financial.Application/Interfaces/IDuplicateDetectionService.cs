using Financial.Application.DTOs.Import;

namespace Financial.Application.Interfaces;

public interface IDuplicateDetectionService
{
    Task<List<TransactionImportDto>> DetectDuplicatesAsync(List<TransactionImportDto> transactions, Guid userId, CancellationToken cancellationToken = default);
}
