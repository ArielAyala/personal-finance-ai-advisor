using Financial.Application.DTOs;
using Financial.Application.Interfaces;
using Financial.Domain.Entities;

namespace Financial.Application.Services;

public class CategoryService
{
    private readonly ICategoryRepository _categoryRepository;

    public CategoryService(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<IEnumerable<CategoryDto>> GetUserCategoriesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var categories = await _categoryRepository.GetByUserIdAsync(userId, cancellationToken);
        return categories.Select(MapToDto);
    }

    public async Task<CategoryDto?> GetCategoryByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var category = await _categoryRepository.GetByIdAsync(id, cancellationToken);
        return category != null ? MapToDto(category) : null;
    }

    public async Task<CategoryDto> CreateCategoryAsync(Guid userId, CreateCategoryDto dto, CancellationToken cancellationToken = default)
    {
        var category = new Category
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = dto.Name,
            Description = dto.Description,
            Type = Enum.Parse<CategoryType>(dto.Type),
            Color = dto.Color,
            Icon = dto.Icon
        };

        var created = await _categoryRepository.CreateAsync(category, cancellationToken);
        return MapToDto(created);
    }

    public async Task<CategoryDto> UpdateCategoryAsync(Guid id, UpdateCategoryDto dto, CancellationToken cancellationToken = default)
    {
        var category = await _categoryRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Category with ID {id} not found");

        if (dto.Name != null) category.Name = dto.Name;
        if (dto.Description != null) category.Description = dto.Description;
        if (dto.Color != null) category.Color = dto.Color;
        if (dto.Icon != null) category.Icon = dto.Icon;

        await _categoryRepository.UpdateAsync(category, cancellationToken);
        return MapToDto(category);
    }

    public async Task DeleteCategoryAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await _categoryRepository.DeleteAsync(id, cancellationToken);
    }

    private static CategoryDto MapToDto(Category category)
    {
        return new CategoryDto(
            category.Id,
            category.Name,
            category.Description,
            category.Type.ToString(),
            category.Color,
            category.Icon,
            category.CreatedAt
        );
    }
}
