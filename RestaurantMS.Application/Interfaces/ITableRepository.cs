using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RestaurantMS.Domain.Entities;
using RestaurantMS.Domain.Enums;

namespace RestaurantMS.Application.Interfaces;

public interface ITableRepository
{
    Task<IEnumerable<Area>> GetAreasWithTablesAsync();
    Task<IEnumerable<Table>> GetAllTablesAsync(int? areaId = null);
    Task<Table?> GetByIdAsync(int id);
    Task<Table> CreateAsync(Table table);
    Task UpdateStatusAsync(int id, TableStatus status);
    Task DeleteAsync(int id);
}

public interface IReservationRepository
{
    Task<Reservation> CreateAsync(Reservation reservation);
    Task<IEnumerable<Reservation>> GetByPhoneAsync(string phone);
    Task<IEnumerable<Reservation>> GetAllAsync(DateTime? date = null);
    Task<Reservation?> GetByCodeAsync(string code);
    Task CancelAsync(int id);
    Task ConfirmAsync(int id);
}