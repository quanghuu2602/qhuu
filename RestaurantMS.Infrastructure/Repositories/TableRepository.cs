using Microsoft.EntityFrameworkCore;
using RestaurantMS.Application.Interfaces;
using RestaurantMS.Domain.Entities;
using RestaurantMS.Domain.Enums;
using RestaurantMS.Infrastructure.Data;

namespace RestaurantMS.Infrastructure.Repositories;

public class TableRepository : ITableRepository
{
    private readonly ApplicationDbContext _db;
    public TableRepository(ApplicationDbContext db) => _db = db;

    public async Task<IEnumerable<Area>> GetAreasWithTablesAsync()
        => await _db.Areas
                    .Include(a => a.Tables)
                    .OrderBy(a => a.Name)
                    .ToListAsync();

    public async Task<IEnumerable<Table>> GetAllTablesAsync(int? areaId = null)
    {
        var q = _db.Tables.Include(t => t.Area).AsQueryable();
        if (areaId.HasValue)
            q = q.Where(t => t.AreaId == areaId.Value);
        return await q.OrderBy(t => t.Name).ToListAsync();
    }

    public async Task<Table?> GetByIdAsync(int id)
        => await _db.Tables.Include(t => t.Area)
                            .FirstOrDefaultAsync(t => t.Id == id);

    public async Task<Table> CreateAsync(Table table)
    {
        _db.Tables.Add(table);
        await _db.SaveChangesAsync();
        return table;
    }

    public async Task UpdateStatusAsync(int id, TableStatus status)
    {
        var table = await _db.Tables.FindAsync(id)
            ?? throw new KeyNotFoundException($"Không tìm thấy bàn #{id}");
        table.Status = status;
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var table = await _db.Tables.FindAsync(id)
            ?? throw new KeyNotFoundException($"Không tìm thấy bàn #{id}");
        _db.Tables.Remove(table);
        await _db.SaveChangesAsync();
    }
}

public class ReservationRepository : IReservationRepository
{
    private readonly ApplicationDbContext _db;
    public ReservationRepository(ApplicationDbContext db) => _db = db;

    public async Task<Reservation> CreateAsync(Reservation r)
    {
        // Tạo mã booking ngẫu nhiên 6 ký tự
        r.BookingCode = GenerateCode();
        _db.Reservations.Add(r);
        await _db.SaveChangesAsync();
        return r;
    }

    public async Task<IEnumerable<Reservation>> GetByPhoneAsync(string phone)
        => await _db.Reservations
                    .Where(r => r.CustomerPhone == phone)
                    .OrderByDescending(r => r.ReservedAt)
                    .ToListAsync();

    public async Task<IEnumerable<Reservation>> GetAllAsync(DateTime? date = null)
    {
        var q = _db.Reservations.AsQueryable();
        if (date.HasValue)
            q = q.Where(r => r.ReservedAt.Date == date.Value.Date);
        return await q.OrderBy(r => r.ReservedAt).ToListAsync();
    }

    public async Task<Reservation?> GetByCodeAsync(string code)
        => await _db.Reservations
                    .FirstOrDefaultAsync(r => r.BookingCode == code);

    public async Task CancelAsync(int id)
    {
        var r = await _db.Reservations.FindAsync(id)
            ?? throw new KeyNotFoundException("Không tìm thấy đặt bàn");
        _db.Reservations.Remove(r);
        await _db.SaveChangesAsync();
    }

    public async Task ConfirmAsync(int id)
    {
        var r = await _db.Reservations.FindAsync(id)
            ?? throw new KeyNotFoundException("Không tìm thấy đặt bàn");
        r.IsConfirmed = true;
        await _db.SaveChangesAsync();
    }

    private static string GenerateCode()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var rng = new Random();
        return new string(Enumerable.Repeat(chars, 6)
            .Select(s => s[rng.Next(s.Length)]).ToArray());
    }
}