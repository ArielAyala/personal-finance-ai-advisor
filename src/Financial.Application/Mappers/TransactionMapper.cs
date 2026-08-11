using Financial.Application.DTOs;
using Financial.Domain.Entities;
using Riok.Mapperly.Abstractions;

namespace Financial.Application.Mappers;

[Mapper]
public static partial class TransactionMapper
{
    [MapperIgnoreSource(nameof(Transaction.UserId))]
    [MapperIgnoreSource(nameof(Transaction.UpdatedAt))]
    [MapperIgnoreSource(nameof(Transaction.User))]
    [MapProperty(nameof(Transaction.Account) + "." + nameof(Account.Name), nameof(TransactionDto.AccountName))]
    [MapProperty(nameof(Transaction.Category) + "." + nameof(Category.Name), nameof(TransactionDto.CategoryName))]
    public static partial TransactionDto ToDto(Transaction transaction);

    private static string MapEnum(TransactionType type) => type.ToString();
}
