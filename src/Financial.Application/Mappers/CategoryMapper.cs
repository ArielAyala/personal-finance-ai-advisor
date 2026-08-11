using Financial.Application.DTOs;
using Financial.Domain.Entities;
using Riok.Mapperly.Abstractions;

namespace Financial.Application.Mappers;

[Mapper]
public static partial class CategoryMapper
{
    [MapperIgnoreSource(nameof(Category.UserId))]
    [MapperIgnoreSource(nameof(Category.User))]
    [MapperIgnoreSource(nameof(Category.Transactions))]
    [MapperIgnoreSource(nameof(Category.Bills))]
    public static partial CategoryDto ToDto(Category category);

    private static string MapEnum(CategoryType type) => type.ToString();
}
