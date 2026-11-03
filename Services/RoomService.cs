using Microsoft.EntityFrameworkCore;
using SmartRoom.Data;
using SmartRoom.DTOs;
using SmartRoom.Models;
using SmartRoom.Models.Enums;

namespace SmartRoom.Services;

public class RoomService : IRoomService
{
    private readonly AppDbContext _context;

    public RoomService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Room>> GetAllRoomsAsync(bool? isActive = null)
    {
        var query = _context.Rooms.AsQueryable();

        if (isActive.HasValue)
        {
            query = query.Where(r => r.IsActive == isActive.Value);
        }

        return await query.OrderBy(r => r.Name).ToListAsync();
    }

    public async Task<Room?> GetRoomByIdAsync(int id)
    {
        return await _context.Rooms.FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<Room?> GetRoomByCodeAsync(string roomCode)
    {
        return await _context.Rooms.FirstOrDefaultAsync(r => r.RoomCode.ToLower() == roomCode.ToLower());
    }

    public async Task<Room> CreateRoomAsync(Room room)
    {
        // Enforce code uppercase
        room.RoomCode = room.RoomCode.Trim().ToUpperInvariant();
        room.Name = room.Name.Trim();
        room.Building = room.Building.Trim();
        room.CreatedAt = DateTime.UtcNow;

        var existingCode = await _context.Rooms.AnyAsync(r => r.RoomCode == room.RoomCode);
        if (existingCode)
        {
            throw new InvalidOperationException($"Kode ruangan '{room.RoomCode}' sudah terdaftar.");
        }

        await _context.Rooms.AddAsync(room);
        await _context.SaveChangesAsync();
        return room;
    }

    public async Task<Room?> UpdateRoomAsync(int id, Room room)
    {
        var existing = await _context.Rooms.FirstOrDefaultAsync(r => r.Id == id);
        if (existing == null) return null;

        var normalizedCode = room.RoomCode.Trim().ToUpperInvariant();
        var codeConflict = await _context.Rooms.AnyAsync(r => r.Id != id && r.RoomCode == normalizedCode);
        if (codeConflict)
        {
            throw new InvalidOperationException($"Kode ruangan '{normalizedCode}' sudah digunakan oleh ruangan lain.");
        }

        existing.RoomCode = normalizedCode;
        existing.Name = room.Name.Trim();
        existing.Building = room.Building.Trim();
        existing.Capacity = room.Capacity;
        existing.Facilities = room.Facilities?.Trim();
        existing.ImageUrl = room.ImageUrl?.Trim();
        existing.IsActive = room.IsActive;

        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> ToggleRoomStatusAsync(int id)
    {
        var room = await _context.Rooms.FirstOrDefaultAsync(r => r.Id == id);
        if (room == null) return false;

        room.IsActive = !room.IsActive;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteRoomAsync(int id)
    {
        var room = await _context.Rooms
            .Include(r => r.Bookings)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (room == null) return false;

        // Check if there are active bookings
        var hasActiveBookings = room.Bookings.Any(b => b.Status == BookingStatus.Pending || b.Status == BookingStatus.Approved);
        if (hasActiveBookings)
        {
            throw new InvalidOperationException("Ruangan tidak dapat dihapus karena masih memiliki riwayat jadwal aktif (Pending/Approved). Anda dapat menonaktifkan ruangan ini.");
        }

        _context.Rooms.Remove(room);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<RoomAvailabilityDto?> GetRoomAvailabilityAsync(int roomId, DateOnly date)
    {
        var room = await _context.Rooms.FirstOrDefaultAsync(r => r.Id == roomId);
        if (room == null) return null;

        var startOfDay = date.ToDateTime(TimeOnly.MinValue);
        var endOfDay = date.ToDateTime(TimeOnly.MaxValue);

        var bookings = await _context.Bookings
            .Where(b => b.RoomId == roomId &&
                        (b.Status == BookingStatus.Pending || b.Status == BookingStatus.Approved) &&
                        b.StartTime < endOfDay && b.EndTime > startOfDay)
            .OrderBy(b => b.StartTime)
            .ToListAsync();

        return new RoomAvailabilityDto
        {
            RoomId = room.Id,
            RoomCode = room.RoomCode,
            RoomName = room.Name,
            Date = date,
            BookedSlots = bookings.Select(b => new BookedSlotDto
            {
                BookingReference = b.BookingReference,
                BorrowerName = b.BorrowerName,
                Purpose = b.Purpose,
                StartTime = b.StartTime,
                EndTime = b.EndTime,
                Status = b.Status.ToString()
            }).ToList()
        };
    }
}
