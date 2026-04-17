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
    public ReservationsController(IReservationRepository repo) => _repo = repo;

    // POST /api/reservations — Customer đặt bàn
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create(
        [FromBody] CreateReservationRequest req)
    {
        // Validate giờ mở cửa 10:00 - 22:00
        var hour = req.ReservedAt.Hour;
        if (hour < 10 || hour >= 22)
            return BadRequest(new
            {
                message = "Nhà hàng phục vụ từ 10:00 đến 22:00"
            });

        // Không đặt trong quá khứ
        if (req.ReservedAt < DateTime.Now)
            return BadRequest(new
            {
                message = "Không thể đặt bàn trong quá khứ"
            });

        var reservation = new Reservation
        {
            CustomerName = req.CustomerName,
            CustomerPhone = req.CustomerPhone,
            GuestCount = req.GuestCount,
            ReservedAt = req.ReservedAt,
            Note = req.Note,
        };

        var created = await _repo.CreateAsync(reservation);
        return Ok(ToDto(created));
    }

    // GET /api/reservations/mine — Customer xem đặt bàn của mình
    [HttpGet("mine")]
    [Authorize]
    public async Task<IActionResult> GetMine()
    {
        // Lấy phone từ claim hoặc trả về theo userId
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        // Tạm dùng: trả về tất cả — sẽ filter theo userId ở sprint sau
        var list = await _repo.GetAllAsync();
        return Ok(list.Select(ToDto));
    }

    // GET /api/reservations — Admin xem tất cả
    [HttpGet]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> GetAll([FromQuery] DateTime? date = null)
    {
        var list = await _repo.GetAllAsync(date);
        return Ok(list.Select(ToDto));
    }

    // PATCH /api/reservations/5/cancel
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

    // PATCH /api/reservations/5/confirm — Admin xác nhận
    [HttpPatch("{id}/confirm")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> Confirm(int id)
    {
        try
        {
            await _repo.ConfirmAsync(id);
            return Ok(new { message = "Đã xác nhận đặt bàn" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    private static ReservationDto ToDto(Reservation r) => new(
        r.Id, r.CustomerName, r.CustomerPhone,
        r.GuestCount, r.ReservedAt, r.Note,
        r.BookingCode, r.IsConfirmed, r.TableId);
}