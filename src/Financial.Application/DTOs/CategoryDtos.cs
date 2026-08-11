namespace Financial.Application.DTOs;

public record CategoryDto(
    Guid Id,
    string Name,
    string? Description,
    string Type,
    string? Color,
    string? Icon,
    DateTime CreatedAt
);

public record CreateCategoryDto(
    string Name,
    string? Description,
    string Type,
    string? Color,
    string? Icon
);

public record UpdateCategoryDto(
    string? Name,
    string? Description,
    string? Color,
    string? Icon
);
