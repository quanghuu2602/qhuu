using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantMS.Application.DTOs;
using RestaurantMS.Application.Interfaces;
using RestaurantMS.Domain.Entities;
using RestaurantMS.Domain.Enums;

namespace RestaurantMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TablesController : ControllerBase
{
    private readonly ITableRepository _repo;
    public TablesController(ITableRepository repo) => _repo = repo;

    // GET /api/tables — Staff + Admin xem sơ đồ bàn
    [HttpGet]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> GetAll([FromQuery] int? areaId = null)
    {
        var tables = await _repo.GetAllTablesAsync(areaId);
        var dto = tables.Select(t => new TableDto(
            t.Id, t.Name, t.Capacity,
            t.Status.ToString(),
            StatusColor(t.Status),
            t.AreaId,
            t.Area?.Name ?? ""));
        return Ok(dto);
    }

    // GET /api/tables/areas — lấy theo khu vực
    [HttpGet("areas")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> GetAreas()
    {
        var areas = await _repo.GetAreasWithTablesAsync();
        var dto = areas.Select(a => new AreaDto(
            a.Id, a.Name,
            a.Tables.Select(t => new TableDto(
                t.Id, t.Name, t.Capacity,
                t.Status.ToString(),
                StatusColor(t.Status),
                t.AreaId, a.Name)).ToList()
        ));
        return Ok(dto);
    }

    // GET /api/tables/available — Customer chọn bàn khi đặt
    [HttpGet("available")]
    [AllowAnonymous]
    public async Task<IActionResult> GetAvailable()
    {
        var tables = await _repo.GetAllTablesAsync();
        var available = tables.Where(t => t.Status == TableStatus.Available)
                              .Select(t => new TableDto(
                                  t.Id, t.Name, t.Capacity,
                                  "Available", "green",
                                  t.AreaId, t.Area?.Name ?? ""));
        return Ok(available);
    }

    // PATCH /api/tables/5/status — Staff cập nhật trạng thái
    [HttpPatch("{id}/status")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> UpdateStatus(int id,
        [FromBody] UpdateTableStatusRequest req)
    {
        if (!Enum.TryParse<TableStatus>(req.Status, out var status))
            return BadRequest(new { message = "Trạng thái không hợp lệ" });

        try
        {
            await _repo.UpdateStatusAsync(id, status);
            return Ok(new { message = "Đã cập nhật trạng thái bàn" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    // POST /api/tables — Admin thêm bàn mới
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateTableRequest req)
    {
        var table = new Table
        {
            Name = req.Name,
            Capacity = req.Capacity,
            AreaId = req.AreaId
        };
        var created = await _repo.CreateAsync(table);
        return Ok(new { message = "Đã thêm bàn", id = created.Id });
    }

    // DELETE /api/tables/5 — Admin xoá bàn
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _repo.DeleteAsync(id);
            return Ok(new { message = "Đã xoá bàn" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    private static string StatusColor(TableStatus s) => s switch
    {
        TableStatus.Available => "green",
        TableStatus.Occupied => "red",
        TableStatus.Reserved => "yellow",
        _ => "gray"
    };
}