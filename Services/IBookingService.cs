using SmartRoom.DTOs;
using SmartRoom.Models;
using SmartRoom.Models.Enums;

namespace SmartRoom.Services;

public interface IBookingService
{
    Task<IEnumerable<Booking>> GetAllBookingsAsync(int? roomId = null, BookingStatus? status = null, DateOnly? date = null, string? borrowerEmail = null);
    Task<Booking?> GetBookingByIdAsync(int id);
    Task<Booking?> GetBookingByReferenceAsync(string reference);
    Task<Booking> CreateBookingAsync(CreateBookingDto dto);
    Task<Booking?> UpdateBookingStatusAsync(int id, UpdateBookingStatusDto dto);
    Task<(bool HasConflict, string? ConflictReason)> CheckScheduleConflictAsync(int roomId, DateTime startTime, DateTime endTime, int? excludeBookingId = null);
    Task<DashboardStatsDto> GetDashboardStatsAsync();
}
