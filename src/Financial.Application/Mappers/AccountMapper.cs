using Financial.Application.DTOs;
using Financial.Domain.Entities;
using Riok.Mapperly.Abstractions;

namespace Financial.Application.Mappers;

[Mapper]
public static partial class AccountMapper
{
    [MapperIgnoreSource(nameof(Account.UserId))]
    [MapperIgnoreSource(nameof(Account.UpdatedAt))]
    [MapperIgnoreSource(nameof(Account.User))]
    [MapperIgnoreSource(nameof(Account.Transactions))]
    [MapperIgnoreSource(nameof(Account.SnapshotAccounts))]
    public static partial AccountDto ToDto(Account account);

    private static string MapEnum(AccountType type) => type.ToString();
}
