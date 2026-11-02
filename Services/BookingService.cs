using Microsoft.EntityFrameworkCore;
using SmartRoom.Data;
using SmartRoom.DTOs;
using SmartRoom.Models;
using SmartRoom.Models.Enums;

namespace SmartRoom.Services;

public class BookingScheduleConflictException : Exception
{
    public BookingScheduleConflictException(string message) : base(message) { }
}

public class BookingValidationException : Exception
{
    public BookingValidationException(string message) : base(message) { }
}

public class BookingService : IBookingService
{
    private readonly AppDbContext _context;

    public BookingService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Booking>> GetAllBookingsAsync(int? roomId = null, BookingStatus? status = null, DateOnly? date = null, string? borrowerEmail = null)
    {
        var query = _context.Bookings
            .Include(b => b.Room)
            .AsQueryable();

        if (roomId.HasValue && roomId.Value > 0)
        {
            query = query.Where(b => b.RoomId == roomId.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(b => b.Status == status.Value);
        }

        if (date.HasValue)
        {
            var startOfDay = date.Value.ToDateTime(TimeOnly.MinValue);
            var endOfDay = date.Value.ToDateTime(TimeOnly.MaxValue);
            query = query.Where(b => b.StartTime >= startOfDay && b.StartTime <= endOfDay);
        }

        if (!string.IsNullOrWhiteSpace(borrowerEmail))
        {
            var normalizedEmail = borrowerEmail.Trim().ToLowerInvariant();
            query = query.Where(b => b.BorrowerEmail.ToLower() == normalizedEmail);
        }

        return await query.OrderByDescending(b => b.StartTime).ToListAsync();
    }

    public async Task<Booking?> GetBookingByIdAsync(int id)
    {
        return await _context.Bookings
            .Include(b => b.Room)
            .FirstOrDefaultAsync(b => b.Id == id);
    }

    public async Task<Booking?> GetBookingByReferenceAsync(string reference)
    {
        return await _context.Bookings
            .Include(b => b.Room)
            .FirstOrDefaultAsync(b => b.BookingReference == reference);
    }

    public async Task<(bool HasConflict, string? ConflictReason)> CheckScheduleConflictAsync(
        int roomId, DateTime startTime, DateTime endTime, int? excludeBookingId = null)
    {
        var conflictBooking = await _context.Bookings
            .Include(b => b.Room)
            .Where(existing =>
                existing.RoomId == roomId &&
                (excludeBookingId == null || existing.Id != excludeBookingId) &&
                (existing.Status == BookingStatus.Pending || existing.Status == BookingStatus.Approved) &&
                startTime < existing.EndTime &&
                endTime > existing.StartTime)
            .OrderBy(b => b.StartTime)
            .FirstOrDefaultAsync();

        if (conflictBooking != null)
        {
            var statusLabel = conflictBooking.Status == BookingStatus.Approved ? "Disetujui" : "Menunggu Persetujuan";
            return (true, $"Jadwal bentrok dengan peminjaman '{conflictBooking.BookingReference}' ({conflictBooking.BorrowerName}) pada {conflictBooking.StartTime:dd MMM yyyy HH:mm} - {conflictBooking.EndTime:HH:mm} [Status: {statusLabel}].");
        }

        return (false, null);
    }

    public async Task<Booking> CreateBookingAsync(CreateBookingDto dto)
    {
        // 1. Validate Time Range Rules
        if (dto.StartTime >= dto.EndTime)
        {
            throw new BookingValidationException("Waktu mulai harus lebih kecil dari waktu selesai.");
        }

        // Allow 2 minutes grace period for form submission delays
        if (dto.StartTime < DateTime.Now.AddMinutes(-2))
        {
            throw new BookingValidationException("Waktu mulai tidak boleh berada di masa lalu.");
        }

        var duration = dto.EndTime - dto.StartTime;
        if (duration < TimeSpan.FromMinutes(30))
        {
            throw new BookingValidationException("Durasi peminjaman minimal adalah 30 menit.");
        }

        if (duration > TimeSpan.FromHours(8))
        {
            throw new BookingValidationException("Durasi peminjaman maksimal adalah 8 jam dalam satu pengajuan.");
        }

        // 2. Validate Room Availability
        var room = await _context.Rooms.FirstOrDefaultAsync(r => r.Id == dto.RoomId);
        if (room == null)
        {
            throw new BookingValidationException("Ruangan yang dipilih tidak ditemukan.");
        }

        if (!room.IsActive)
        {
            throw new BookingValidationException($"Ruangan '{room.Name}' sedang nonaktif dan tidak dapat dipesan.");
        }

        // 3. Double-Booking Prevention Rule
        var (hasConflict, conflictReason) = await CheckScheduleConflictAsync(dto.RoomId, dto.StartTime, dto.EndTime);
        if (hasConflict)
        {
            throw new BookingScheduleConflictException(conflictReason ?? "Jadwal yang diajukan bentrok dengan peminjaman lain.");
        }

        // 4. Generate Unique Reference Code: BK-YYYYMMDD-XXXX
        var randomCode = Guid.NewGuid().ToString("N")[..4].ToUpperInvariant();
        var reference = $"BK-{DateTime.UtcNow:yyyyMMdd}-{randomCode}";

        var booking = new Booking
        {
            BookingReference = reference,
            RoomId = dto.RoomId,
            UserId = dto.UserId,
            BorrowerName = dto.BorrowerName.Trim(),
            BorrowerEmail = dto.BorrowerEmail.Trim().ToLowerInvariant(),
            Purpose = dto.Purpose.Trim(),
            StartTime = dto.StartTime,
            EndTime = dto.EndTime,
            Status = BookingStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        await _context.Bookings.AddAsync(booking);
        await _context.SaveChangesAsync();

        // Load navigation property
        await _context.Entry(booking).Reference(b => b.Room).LoadAsync();

        return booking;
    }

    public async Task<Booking?> UpdateBookingStatusAsync(int id, UpdateBookingStatusDto dto)
    {
        var booking = await _context.Bookings
            .Include(b => b.Room)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (booking == null) return null;

        // If approving, re-check for conflicts to prevent race condition approvals
        if (dto.Status == BookingStatus.Approved && booking.Status != BookingStatus.Approved)
        {
            var (hasConflict, conflictReason) = await CheckScheduleConflictAsync(booking.RoomId, booking.StartTime, booking.EndTime, booking.Id);
            if (hasConflict)
            {
                throw new BookingScheduleConflictException($"Tidak dapat menyetujui peminjaman: {conflictReason}");
            }
        }

        booking.Status = dto.Status;
        if (!string.IsNullOrWhiteSpace(dto.AdminNotes))
        {
            booking.AdminNotes = dto.AdminNotes.Trim();
        }

        await _context.SaveChangesAsync();
        return booking;
    }

    public async Task<DashboardStatsDto> GetDashboardStatsAsync()
    {
        var startOfDay = DateTime.Today;
        var endOfDay = startOfDay.AddDays(1).AddTicks(-1);

        var totalActiveRooms = await _context.Rooms.CountAsync(r => r.IsActive);

        var todayBookings = await _context.Bookings
            .Include(b => b.Room)
            .Where(b => b.StartTime >= startOfDay && b.StartTime <= endOfDay)
            .OrderBy(b => b.StartTime)
            .ToListAsync();

        var pendingCount = await _context.Bookings.CountAsync(b => b.Status == BookingStatus.Pending);
        var approvedCount = await _context.Bookings.CountAsync(b => b.Status == BookingStatus.Approved);

        return new DashboardStatsDto
        {
            TotalActiveRooms = totalActiveRooms,
            TodayBookingsCount = todayBookings.Count(),
            PendingBookingsCount = pendingCount,
            ApprovedBookingsCount = approvedCount,
            TodayBookings = todayBookings
        };
    }
}
