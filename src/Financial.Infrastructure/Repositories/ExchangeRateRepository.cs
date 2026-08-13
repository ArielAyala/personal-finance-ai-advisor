using Financial.Application.Interfaces;
using Financial.Domain.Entities;
using Financial.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Financial.Infrastructure.Repositories;

public class ExchangeRateRepository : IExchangeRateRepository
{
    private readonly FinancialDbContext _context;

    public ExchangeRateRepository(FinancialDbContext context)
    {
        _context = context;
    }

    public async Task<ExchangeRate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.ExchangeRates
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<ExchangeRate?> GetRateAsync(string sourceCurrency, string targetCurrency, DateTime date, CancellationToken cancellationToken = default)
    {
        return await _context.ExchangeRates
            .Where(e => e.SourceCurrency == sourceCurrency 
                && e.TargetCurrency == targetCurrency 
                && e.Date.Date == date.Date)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<ExchangeRate?> GetLatestRateAsync(string sourceCurrency, string targetCurrency, CancellationToken cancellationToken = default)
    {
        return await _context.ExchangeRates
            .Where(e => e.SourceCurrency == sourceCurrency && e.TargetCurrency == targetCurrency)
            .OrderByDescending(e => e.Date)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IEnumerable<ExchangeRate>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.ExchangeRates
            .OrderByDescending(e => e.Date)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ExchangeRate>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        return await _context.ExchangeRates
            .Where(e => e.Date >= startDate && e.Date <= endDate)
            .OrderBy(e => e.Date)
            .ToListAsync(cancellationToken);
    }

    public async Task<ExchangeRate> CreateAsync(ExchangeRate exchangeRate, CancellationToken cancellationToken = default)
    {
        exchangeRate.CreatedAt = DateTime.UtcNow;
        _context.ExchangeRates.Add(exchangeRate);
        await _context.SaveChangesAsync(cancellationToken);
        return exchangeRate;
    }

    public async Task UpdateAsync(ExchangeRate exchangeRate, CancellationToken cancellationToken = default)
    {
        _context.ExchangeRates.Update(exchangeRate);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var exchangeRate = await GetByIdAsync(id, cancellationToken);
        if (exchangeRate != null)
        {
            _context.ExchangeRates.Remove(exchangeRate);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
