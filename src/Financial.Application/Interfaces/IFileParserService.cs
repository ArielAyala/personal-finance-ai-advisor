using Financial.Application.DTOs.Import;

namespace Financial.Application.Interfaces;

public interface IFileParserService
{
    Task<ImportPreviewDto> ParseFileAsync(Stream fileStream, string fileName, Guid userId, CancellationToken cancellationToken = default);
    bool CanParse(string fileName);
}
