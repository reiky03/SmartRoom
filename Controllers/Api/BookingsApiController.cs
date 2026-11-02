using Microsoft.AspNetCore.Mvc;
using SmartRoom.DTOs;
using SmartRoom.Models;
using SmartRoom.Models.Enums;
using SmartRoom.Services;

namespace SmartRoom.Controllers.Api;

[ApiController]
[Route("api/bookings")]
[Produces("application/json")]
public class BookingsApiController : ControllerBase
{
    private readonly IBookingService _bookingService;

    public BookingsApiController(IBookingService bookingService)
    {
        _bookingService = bookingService;
    }

    /// <summary>
    /// Mengambil daftar peminjaman dengan filter opsional.
    /// </summary>
    /// <param name="roomId">Filter berdasarkan ID ruangan</param>
    /// <param name="status">Filter berdasarkan status (0: Pending, 1: Approved, 2: Rejected, 3: Cancelled)</param>
    /// <param name="date">Filter berdasarkan tanggal (Format: YYYY-MM-DD)</param>
    /// <param name="borrowerEmail">Filter berdasarkan email peminjam</param>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<Booking>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllBookings(
        [FromQuery] int? roomId,
        [FromQuery] BookingStatus? status,
        [FromQuery] string? date,
        [FromQuery] string? borrowerEmail)
    {
        DateOnly? targetDate = null;
        if (!string.IsNullOrWhiteSpace(date) && DateOnly.TryParse(date, out var parsedDate))
        {
            targetDate = parsedDate;
        }

        var bookings = await _bookingService.GetAllBookingsAsync(roomId, status, targetDate, borrowerEmail);
        return Ok(bookings);
    }

    /// <summary>
    /// Mengambil detail peminjaman berdasarkan ID.
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(Booking), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBookingById(int id)
    {
        var booking = await _bookingService.GetBookingByIdAsync(id);
        if (booking == null)
        {
            return NotFound(new { message = $"Peminjaman dengan ID {id} tidak ditemukan." });
        }
        return Ok(booking);
    }

    /// <summary>
    /// Mengambil detail peminjaman berdasarkan Nomor Referensi (contoh: BK-20261102-XXXX).
    /// </summary>
    [HttpGet("reference/{reference}")]
    [ProducesResponseType(typeof(Booking), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBookingByReference(string reference)
    {
        var booking = await _bookingService.GetBookingByReferenceAsync(reference);
        if (booking == null)
        {
            return NotFound(new { message = $"Peminjaman dengan nomor referensi '{reference}' tidak ditemukan." });
        }
        return Ok(booking);
    }

    /// <summary>
    /// Mengajukan peminjaman ruangan baru (dilengkapi pengecekan bentrok jadwal otomatis).
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(Booking), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateBooking([FromBody] CreateBookingDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var created = await _bookingService.CreateBookingAsync(dto);
            return CreatedAtAction(nameof(GetBookingById), new { id = created.Id }, created);
        }
        catch (BookingScheduleConflictException ex)
        {
            return Conflict(new
            {
                status = 409,
                error = "ScheduleConflict",
                message = ex.Message
            });
        }
        catch (BookingValidationException ex)
        {
            return BadRequest(new
            {
                status = 400,
                error = "ValidationError",
                message = ex.Message
            });
        }
    }

    /// <summary>
    /// Mengubah status peminjaman ruangan (Approved / Rejected / Cancelled) beserta catatan admin.
    /// </summary>
    [HttpPatch("{id:int}/status")]
    [ProducesResponseType(typeof(Booking), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateBookingStatus(int id, [FromBody] UpdateBookingStatusDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var updated = await _bookingService.UpdateBookingStatusAsync(id, dto);
            if (updated == null)
            {
                return NotFound(new { message = $"Peminjaman dengan ID {id} tidak ditemukan." });
            }
            return Ok(updated);
        }
        catch (BookingScheduleConflictException ex)
        {
            return Conflict(new
            {
                status = 409,
                error = "ScheduleConflict",
                message = ex.Message
            });
        }
    }
}
