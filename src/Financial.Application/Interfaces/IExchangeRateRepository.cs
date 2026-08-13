using Financial.Domain.Entities;

namespace Financial.Application.Interfaces;

public interface IExchangeRateRepository
{
    Task<ExchangeRate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ExchangeRate?> GetRateAsync(string sourceCurrency, string targetCurrency, DateTime date, CancellationToken cancellationToken = default);
    Task<ExchangeRate?> GetLatestRateAsync(string sourceCurrency, string targetCurrency, CancellationToken cancellationToken = default);
    Task<IEnumerable<ExchangeRate>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<ExchangeRate>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<ExchangeRate> CreateAsync(ExchangeRate exchangeRate, CancellationToken cancellationToken = default);
    Task UpdateAsync(ExchangeRate exchangeRate, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
