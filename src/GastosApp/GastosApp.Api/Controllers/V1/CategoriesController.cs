using GastosApp.Api.Extensions;
using GastosApp.Application.Categories;
using GastosApp.Application.Common;
using GastosApp.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GastosApp.Api.Controllers.V1;

[ApiController]
[Route("api/v1/categories")]
[Authorize]
public sealed class CategoriesController : ControllerBase
{
    private readonly IApplicationDbContext _db;

    public CategoriesController(IApplicationDbContext db) => _db = db;

    /// <summary>Lista las categorias del sistema y las personales del usuario autenticado.</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CategoryDto>>> GetAll(CancellationToken ct)
    {
        var userId = User.GetUserId();

        var categories = await _db.Categories
            .Where(c => c.OwnerUserId == null || c.OwnerUserId == userId)
            .OrderBy(c => c.Name)
            .Select(c => new CategoryDto(c.Id, c.Name, c.Icon, c.Color, c.IsSystem, c.OwnerUserId))
            .ToListAsync(ct);

        return Ok(categories);
    }

    /// <summary>Crea una categoria personal para el usuario autenticado.</summary>
    [HttpPost]
    public async Task<ActionResult<CategoryDto>> Create(
        [FromBody] CreateCategoryRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var userId = User.GetUserId();
        var category = Category.CreatePersonal(Guid.NewGuid(), userId, request.Name, request.Icon, request.Color);

        await _db.Categories.AddAsync(category, ct);
        await _db.SaveChangesAsync(ct);

        var dto = ToDto(category);
        return CreatedAtAction(nameof(GetById), new { id = category.Id }, dto);
    }

    /// <summary>Obtiene una categoria por Id.</summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CategoryDto>> GetById(Guid id, CancellationToken ct)
    {
        var userId = User.GetUserId();
        var category = await _db.Categories
            .FirstOrDefaultAsync(c => c.Id == id &&
                (c.OwnerUserId == null || c.OwnerUserId == userId), ct);

        return category is null ? NotFound() : Ok(ToDto(category));
    }

    /// <summary>Actualiza nombre, icono o color de una categoria personal.</summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id, [FromBody] UpdateCategoryRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var userId = User.GetUserId();
        var category = await _db.Categories
            .FirstOrDefaultAsync(c => c.Id == id && c.OwnerUserId == userId, ct);

        if (category is null) return NotFound();

        category.Rename(request.Name);
        await _db.SaveChangesAsync(ct);
        return Ok(ToDto(category));
    }

    /// <summary>Elimina logicamente una categoria personal.</summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var userId = User.GetUserId();
        var category = await _db.Categories
            .FirstOrDefaultAsync(c => c.Id == id && c.OwnerUserId == userId, ct);

        if (category is null) return NotFound();

        category.Delete();
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    private static CategoryDto ToDto(Category c) =>
        new(c.Id, c.Name, c.Icon, c.Color, c.IsSystem, c.OwnerUserId);
}
