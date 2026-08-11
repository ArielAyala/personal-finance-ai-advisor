using Financial.Application.DTOs.Import;

namespace Financial.Application.Interfaces;

public interface IImportService
{
    Task<ImportPreviewDto> PreviewImportAsync(Stream fileStream, string fileName, Guid userId, CancellationToken cancellationToken = default);
    Task<ImportResultDto> ExecuteImportAsync(ImportRequestDto request, Guid userId, CancellationToken cancellationToken = default);
    Task<bool> ValidateFileAsync(Stream fileStream, string fileName, CancellationToken cancellationToken = default);
}
