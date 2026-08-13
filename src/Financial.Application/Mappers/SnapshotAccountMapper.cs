using Financial.Application.DTOs;
using Financial.Domain.Entities;
using Riok.Mapperly.Abstractions;

namespace Financial.Application.Mappers;

[Mapper]
public static partial class SnapshotAccountMapper
{
    [MapperIgnoreSource(nameof(SnapshotAccount.Snapshot))]
    [MapperIgnoreSource(nameof(SnapshotAccount.Account))]
    public static partial SnapshotAccountDto ToDto(SnapshotAccount snapshotAccount);

    public static IEnumerable<SnapshotAccountDto> ToDtoList(IEnumerable<SnapshotAccount> snapshotAccounts)
        => snapshotAccounts.Select(ToDto);
}
