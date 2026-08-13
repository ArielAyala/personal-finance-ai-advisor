using Financial.Application.DTOs;
using Financial.Domain.Entities;
using Riok.Mapperly.Abstractions;

namespace Financial.Application.Mappers;

[Mapper]
public static partial class ExchangeRateMapper
{
    public static partial ExchangeRateDto ToDto(ExchangeRate exchangeRate);

    public static IEnumerable<ExchangeRateDto> ToDtoList(IEnumerable<ExchangeRate> exchangeRates)
        => exchangeRates.Select(ToDto);
}
