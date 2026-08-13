namespace Financial.Application.Interfaces;

public interface ITextExtractionService
{
    Task<string> ExtractTextAsync(Stream fileStream, string fileName, string contentType, CancellationToken cancellationToken = default);
    bool SupportsFileType(string contentType, string fileName);
}
