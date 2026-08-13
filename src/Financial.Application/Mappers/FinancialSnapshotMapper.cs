using Financial.Application.DTOs;
using Financial.Domain.Entities;
using Riok.Mapperly.Abstractions;

namespace Financial.Application.Mappers;

[Mapper]
public static partial class FinancialSnapshotMapper
{
    [MapperIgnoreSource(nameof(FinancialSnapshot.UserId))]
    [MapperIgnoreSource(nameof(FinancialSnapshot.User))]
    [MapProperty(nameof(FinancialSnapshot.SnapshotAccounts), nameof(FinancialSnapshotDto.Accounts))]
    public static partial FinancialSnapshotDto ToDto(FinancialSnapshot snapshot);

    public static FinancialSnapshotSummaryDto ToSummaryDto(FinancialSnapshot snapshot)
    {
        return new FinancialSnapshotSummaryDto(
            Id: snapshot.Id,
            Date: snapshot.Date,
            BaseCurrency: snapshot.BaseCurrency,
            TotalAmount: snapshot.TotalAmount,
            Status: snapshot.Status.ToString(),
            AccountCount: snapshot.SnapshotAccounts?.Count ?? 0,
            CreatedAt: snapshot.CreatedAt
        );
    }

    public static IEnumerable<FinancialSnapshotDto> ToDtoList(IEnumerable<FinancialSnapshot> snapshots)
        => snapshots.Select(ToDto);

    public static IEnumerable<FinancialSnapshotSummaryDto> ToSummaryDtoList(IEnumerable<FinancialSnapshot> snapshots)
        => snapshots.Select(ToSummaryDto);

    private static string MapEnum(SnapshotStatus status) => status.ToString();
}
