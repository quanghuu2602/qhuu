using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantMS.Application.DTOs;
using RestaurantMS.Application.Interfaces;
using RestaurantMS.Domain.Entities;

namespace RestaurantMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MenuController : ControllerBase
{
    private readonly IMenuRepository _repo;
    public MenuController(IMenuRepository repo) => _repo = repo;

    // GET /api/menu/categories — ai cũng xem được
    [HttpGet("categories")]
    [AllowAnonymous]
    public async Task<IActionResult> GetCategories()
    {
        var cats = await _repo.GetCategoriesAsync();
        var dto = cats.Select(c => new CategoryDto(
            c.Id, c.Name, c.SortOrder,
            c.MenuItems?.Count(m => m.IsAvailable) ?? 0));
        return Ok(dto);
    }

    // GET /api/menu?categoryId=1&isAvailable=true&search=bò
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll(
        [FromQuery] int? categoryId = null,
        [FromQuery] bool? isAvailable = null,
        [FromQuery] string? search = null)
    {
        var items = await _repo.GetAllAsync(categoryId, isAvailable, search);
        var dto = items.Select(m => ToDto(m));
        return Ok(dto);
    }

    // GET /api/menu/5
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(int id)
    {
        var item = await _repo.GetByIdAsync(id);
        if (item is null) return NotFound(new { message = "Không tìm thấy món" });
        return Ok(ToDto(item));
    }

    // POST /api/menu — chỉ Admin
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateMenuItemRequest req)
    {
        var item = new MenuItem
        {
            Name = req.Name,
            Price = req.Price,
            ImageUrl = req.ImageUrl,
            Description = req.Description,
            CategoryId = req.CategoryId,
            Tags = req.Tags ?? new List<string>()
        };
        var created = await _repo.CreateAsync(item);
        return CreatedAtAction(nameof(GetById),
            new { id = created.Id }, ToDto(created));
    }

    // PUT /api/menu/5 — chỉ Admin
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id,
        [FromBody] UpdateMenuItemRequest req)
    {
        var item = await _repo.GetByIdAsync(id);
        if (item is null)
            return NotFound(new { message = "Không tìm thấy món" });

        item.Name = req.Name;
        item.Price = req.Price;
        item.ImageUrl = req.ImageUrl;
        item.Description = req.Description;
        item.CategoryId = req.CategoryId;
        item.IsAvailable = req.IsAvailable;
        item.Tags = req.Tags ?? new List<string>();

        await _repo.UpdateAsync(item);
        return Ok(new { message = "Cập nhật thành công" });
    }

    // DELETE /api/menu/5 — chỉ Admin
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _repo.DeleteAsync(id);
            return Ok(new { message = "Đã xoá món" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    // PATCH /api/menu/5/toggle — Admin và Staff
    [HttpPatch("{id}/toggle")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> Toggle(int id)
    {
        try
        {
            await _repo.ToggleAvailabilityAsync(id);
            return Ok(new { message = "Đã cập nhật trạng thái" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    // ── Helper chuyển Entity → DTO ────────────────────────────
    private static MenuItemDto ToDto(MenuItem m) => new(
        m.Id, m.Name, m.Price, m.ImageUrl, m.Description,
        m.IsAvailable, m.CategoryId,
        m.Category?.Name ?? "",
        m.Tags);
}