using Financial.Application.DTOs;
using Financial.Domain.Entities;
using Riok.Mapperly.Abstractions;

namespace Financial.Application.Mappers;

[Mapper]
public static partial class UserMapper
{
    [MapperIgnoreSource(nameof(User.PasswordHash))]
    [MapperIgnoreSource(nameof(User.UpdatedAt))]
    [MapperIgnoreSource(nameof(User.Accounts))]
    [MapperIgnoreSource(nameof(User.Transactions))]
    [MapperIgnoreSource(nameof(User.Categories))]
    [MapperIgnoreSource(nameof(User.Bills))]
    [MapperIgnoreSource(nameof(User.FinancialSnapshots))]
    [MapperIgnoreSource(nameof(User.Goals))]
    [MapperIgnoreSource(nameof(User.Documents))]
    [MapperIgnoreSource(nameof(User.RefreshTokens))]
    public static partial UserDto ToDto(User user);
}
