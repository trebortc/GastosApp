using System.ComponentModel.DataAnnotations;

namespace GastosApp.Application.Categories;

public sealed record CreateCategoryRequest(
    [Required, MaxLength(100)] string Name,
    [MaxLength(50)] string? Icon = null,
    [MaxLength(20)] string? Color = null);

public sealed record UpdateCategoryRequest(
    [Required, MaxLength(100)] string Name,
    [MaxLength(50)] string? Icon = null,
    [MaxLength(20)] string? Color = null);

public sealed record CategoryDto(
    Guid Id,
    string Name,
    string? Icon,
    string? Color,
    bool IsSystem,
    Guid? OwnerUserId);
