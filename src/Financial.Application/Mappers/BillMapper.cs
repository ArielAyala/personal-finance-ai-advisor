using Financial.Application.DTOs;
using Financial.Domain.Entities;
using Riok.Mapperly.Abstractions;

namespace Financial.Application.Mappers;

[Mapper]
public static partial class BillMapper
{
    [MapperIgnoreSource(nameof(Bill.UserId))]
    [MapperIgnoreSource(nameof(Bill.UpdatedAt))]
    [MapperIgnoreSource(nameof(Bill.User))]
    [MapperIgnoreSource(nameof(Bill.Category))]
    [MapperIgnoreSource(nameof(Bill.Account))]
    public static partial BillDto ToDto(Bill bill);

    private static string MapEnum(BillPeriodicity periodicity) => periodicity.ToString();
    private static string MapEnum(BillStatus status) => status.ToString();
}
