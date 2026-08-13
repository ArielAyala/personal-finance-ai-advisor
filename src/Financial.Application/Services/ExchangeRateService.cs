using Financial.Application.DTOs;
using Financial.Application.Interfaces;
using Financial.Application.Mappers;
using Financial.Domain.Entities;

namespace Financial.Application.Services;

public class ExchangeRateService
{
    private readonly IExchangeRateRepository _exchangeRateRepository;

    public ExchangeRateService(IExchangeRateRepository exchangeRateRepository)
    {
        _exchangeRateRepository = exchangeRateRepository;
    }

    public async Task<IEnumerable<ExchangeRateDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var rates = await _exchangeRateRepository.GetAllAsync(cancellationToken);
        return ExchangeRateMapper.ToDtoList(rates);
    }

    public async Task<ExchangeRateDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var rate = await _exchangeRateRepository.GetByIdAsync(id, cancellationToken);
        return rate != null ? ExchangeRateMapper.ToDto(rate) : null;
    }

    public async Task<ExchangeRateDto?> GetRateAsync(string sourceCurrency, string targetCurrency, DateTime date, CancellationToken cancellationToken = default)
    {
        var rate = await _exchangeRateRepository.GetRateAsync(sourceCurrency, targetCurrency, date, cancellationToken);
        return rate != null ? ExchangeRateMapper.ToDto(rate) : null;
    }

    public async Task<ExchangeRateDto?> GetLatestRateAsync(string sourceCurrency, string targetCurrency, CancellationToken cancellationToken = default)
    {
        var rate = await _exchangeRateRepository.GetLatestRateAsync(sourceCurrency, targetCurrency, cancellationToken);
        return rate != null ? ExchangeRateMapper.ToDto(rate) : null;
    }

    public async Task<decimal?> ConvertCurrencyAsync(decimal amount, string sourceCurrency, string targetCurrency, DateTime? date = null, CancellationToken cancellationToken = default)
    {
        // If source and target are the same, return the amount as is
        if (sourceCurrency == targetCurrency)
            return amount;

        ExchangeRate? rate;
        
        if (date.HasValue)
        {
            rate = await _exchangeRateRepository.GetRateAsync(sourceCurrency, targetCurrency, date.Value, cancellationToken);
        }
        else
        {
            rate = await _exchangeRateRepository.GetLatestRateAsync(sourceCurrency, targetCurrency, cancellationToken);
        }

        if (rate == null)
            return null;

        return amount * rate.Rate;
    }

    public async Task<IEnumerable<ExchangeRateDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        var rates = await _exchangeRateRepository.GetByDateRangeAsync(startDate, endDate, cancellationToken);
        return ExchangeRateMapper.ToDtoList(rates);
    }

    public async Task<ExchangeRateDto> CreateAsync(CreateExchangeRateDto dto, CancellationToken cancellationToken = default)
    {
        // Check if rate already exists for this date
        var existing = await _exchangeRateRepository.GetRateAsync(dto.SourceCurrency, dto.TargetCurrency, dto.Date, cancellationToken);
        if (existing != null)
        {
            throw new InvalidOperationException($"Exchange rate from {dto.SourceCurrency} to {dto.TargetCurrency} already exists for date {dto.Date:yyyy-MM-dd}");
        }

        var exchangeRate = new ExchangeRate
        {
            Id = Guid.NewGuid(),
            SourceCurrency = dto.SourceCurrency.ToUpper(),
            TargetCurrency = dto.TargetCurrency.ToUpper(),
            Rate = dto.Rate,
            Date = dto.Date.Date, // Ensure we only store the date part
            Source = dto.Source ?? "Manual input",
            Notes = dto.Notes
        };

        var created = await _exchangeRateRepository.CreateAsync(exchangeRate, cancellationToken);
        return ExchangeRateMapper.ToDto(created);
    }

    public async Task<ExchangeRateDto> UpdateAsync(Guid id, UpdateExchangeRateDto dto, CancellationToken cancellationToken = default)
    {
        var exchangeRate = await _exchangeRateRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Exchange rate with ID {id} not found");

        if (dto.Rate.HasValue) exchangeRate.Rate = dto.Rate.Value;
        if (dto.Source != null) exchangeRate.Source = dto.Source;
        if (dto.Notes != null) exchangeRate.Notes = dto.Notes;

        await _exchangeRateRepository.UpdateAsync(exchangeRate, cancellationToken);
        return ExchangeRateMapper.ToDto(exchangeRate);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await _exchangeRateRepository.DeleteAsync(id, cancellationToken);
    }
}
