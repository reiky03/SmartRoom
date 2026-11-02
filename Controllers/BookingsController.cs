using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SmartRoom.DTOs;
using SmartRoom.Models.Enums;
using SmartRoom.Services;

namespace SmartRoom.Controllers;

public class BookingsController : Controller
{
    private readonly IBookingService _bookingService;
    private readonly IRoomService _roomService;

    public BookingsController(IBookingService bookingService, IRoomService roomService)
    {
        _bookingService = bookingService;
        _roomService = roomService;
    }

    // GET: /Bookings
    public async Task<IActionResult> Index(int? roomId, BookingStatus? status, string? date)
    {
        DateOnly? parsedDate = null;
        if (!string.IsNullOrWhiteSpace(date) && DateOnly.TryParse(date, out var d))
        {
            parsedDate = d;
        }

        var bookings = await _bookingService.GetAllBookingsAsync(roomId, status, parsedDate);
        var rooms = await _roomService.GetAllRoomsAsync();

        ViewBag.Rooms = new SelectList(rooms, "Id", "Name", roomId);
        ViewBag.CurrentRoomId = roomId;
        ViewBag.CurrentStatus = status;
        ViewBag.CurrentDate = date;

        return View(bookings);
    }

    // GET: /Bookings/Create
    public async Task<IActionResult> Create(int? roomId)
    {
        var activeRooms = await _roomService.GetAllRoomsAsync(isActive: true);
        ViewBag.Rooms = new SelectList(activeRooms, "Id", "Name", roomId);

        var today = DateTime.Today;
        var defaultStart = today.AddHours(9);
        if (DateTime.Now.TimeOfDay > TimeSpan.FromHours(8))
        {
            defaultStart = DateTime.Now.AddHours(1);
            // round to nearest 30 mins
            defaultStart = new DateTime(defaultStart.Year, defaultStart.Month, defaultStart.Day, defaultStart.Hour, defaultStart.Minute >= 30 ? 30 : 0, 0);
        }

        var dto = new CreateBookingDto
        {
            RoomId = roomId ?? 0,
            StartTime = defaultStart,
            EndTime = defaultStart.AddHours(2)
        };

        return View(dto);
    }

    // POST: /Bookings/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateBookingDto dto)
    {
        if (!ModelState.IsValid)
        {
            var activeRooms = await _roomService.GetAllRoomsAsync(isActive: true);
            ViewBag.Rooms = new SelectList(activeRooms, "Id", "Name", dto.RoomId);
            return View(dto);
        }

        try
        {
            var created = await _bookingService.CreateBookingAsync(dto);
            TempData["SuccessMessage"] = $"Peminjaman berhasil diajukan dengan Nomor Referensi: {created.BookingReference}. Menunggu verifikasi admin.";
            return RedirectToAction(nameof(Index));
        }
        catch (BookingScheduleConflictException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            ViewBag.ConflictAlert = ex.Message;
        }
        catch (BookingValidationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
        }

        var rooms = await _roomService.GetAllRoomsAsync(isActive: true);
        ViewBag.Rooms = new SelectList(rooms, "Id", "Name", dto.RoomId);
        return View(dto);
    }

    // GET: /Bookings/Manage (Approval Admin Panel)
    public async Task<IActionResult> Manage(int? roomId, BookingStatus? status)
    {
        var bookings = await _bookingService.GetAllBookingsAsync(roomId, status);
        var rooms = await _roomService.GetAllRoomsAsync();

        ViewBag.Rooms = new SelectList(rooms, "Id", "Name", roomId);
        ViewBag.CurrentRoomId = roomId;
        ViewBag.CurrentStatus = status;

        return View(bookings);
    }

    // POST: /Bookings/UpdateStatus
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, BookingStatus status, string? adminNotes, string? returnUrl)
    {
        try
        {
            var dto = new UpdateBookingStatusDto
            {
                Status = status,
                AdminNotes = adminNotes
            };

            var updated = await _bookingService.UpdateBookingStatusAsync(id, dto);
            if (updated == null)
            {
                TempData["ErrorMessage"] = "Peminjaman tidak ditemukan.";
            }
            else
            {
                var actionLabel = status == BookingStatus.Approved ? "disetujui" : (status == BookingStatus.Rejected ? "ditolak" : "diperbarui");
                TempData["SuccessMessage"] = $"Peminjaman {updated.BookingReference} berhasil {actionLabel}.";
            }
        }
        catch (BookingScheduleConflictException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction(nameof(Manage));
    }
}
