using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantMS.Application.DTOs;
using RestaurantMS.Application.Interfaces;
using RestaurantMS.Domain.Entities;

namespace RestaurantMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReservationsController : ControllerBase
{
    private readonly IReservationRepository _repo;
    public ReservationsController(IReservationRepository repo)
        => _repo = repo;

    // ── Customer đặt bàn ──────────────────────────────────────
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create(
        [FromBody] CreateReservationRequest req)
    {
        var hour = req.ReservedAt.Hour;
        if (hour < 10 || hour >= 22)
            return BadRequest(new
            {
                message = "Nhà hàng phục vụ từ 10:00 đến 22:00"
            });

        if (req.ReservedAt < DateTime.Now)
            return BadRequest(new
            {
                message = "Không thể đặt bàn trong quá khứ"
            });

        var r = new Reservation
        {
            CustomerName = req.CustomerName,
            CustomerPhone = req.CustomerPhone,
            GuestCount = req.GuestCount,
            ReservedAt = req.ReservedAt,
            Note = req.Note,
        };

        var created = await _repo.CreateAsync(r);
        return Ok(ToDto(created));
    }

    // ── Admin/Staff xem tất cả — có filter ───────────────────
    [HttpGet]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> GetAll(
        [FromQuery] DateTime? date = null,
        [FromQuery] bool? isConfirmed = null)
    {
        var list = await _repo.GetAllAsync(date, isConfirmed);
        return Ok(list.Select(ToDto));
    }

    // ── Customer xem đặt bàn của mình (theo SĐT) ─────────────
    [HttpGet("mine")]
    [Authorize]
    public async Task<IActionResult> GetMine(
        [FromQuery] string? phone = null)
    {
        // Nếu không truyền phone thì lấy tất cả (tạm thời)
        IEnumerable<Reservation> list;
        if (!string.IsNullOrEmpty(phone))
            list = await _repo.GetMineAsync(phone);
        else
            list = await _repo.GetAllAsync();

        return Ok(list.Select(ToDto));
    }

    // ── Tìm theo mã booking — Staff check-in ─────────────────
    [HttpGet("by-code/{code}")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> GetByCode(string code)
    {
        var r = await _repo.GetByCodeAsync(code);
        if (r is null)
            return NotFound(new { message = "Không tìm thấy mã booking" });
        return Ok(ToDto(r));
    }

    // ── Admin/Staff xác nhận + gán bàn ───────────────────────
    [HttpPatch("{id}/confirm")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> Confirm(int id,
        [FromBody] ConfirmReservationRequest req)
    {
        try
        {
            await _repo.ConfirmAsync(id, req.TableId);
            return Ok(new { message = "Đã xác nhận đặt bàn" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    // ── Huỷ (Customer hoặc Admin/Staff) ──────────────────────
    [HttpPatch("{id}/cancel")]
    [Authorize]
    public async Task<IActionResult> Cancel(int id)
    {
        try
        {
            await _repo.CancelAsync(id);
            return Ok(new { message = "Đã huỷ đặt bàn" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    // ── Helper ────────────────────────────────────────────────
    private static ReservationDto ToDto(Reservation r) => new(
        r.Id,
        r.CustomerName,
        r.CustomerPhone,
        r.GuestCount,
        r.ReservedAt,
        r.Note,
        r.BookingCode,
        r.IsConfirmed,
        r.TableId,
        r.Table?.Name
    );
}