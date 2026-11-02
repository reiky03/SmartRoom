using Microsoft.AspNetCore.Mvc;
using SmartRoom.Models;
using SmartRoom.Services;

namespace SmartRoom.Controllers;

public class RoomsController : Controller
{
    private readonly IRoomService _roomService;

    public RoomsController(IRoomService roomService)
    {
        _roomService = roomService;
    }

    // GET: /Rooms
    public async Task<IActionResult> Index(bool? isActive)
    {
        ViewBag.CurrentFilter = isActive;
        var rooms = await _roomService.GetAllRoomsAsync(isActive);
        return View(rooms);
    }

    // GET: /Rooms/Create
    public IActionResult Create()
    {
        return View(new Room { IsActive = true, Capacity = 20 });
    }

    // POST: /Rooms/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Room room)
    {
        if (!ModelState.IsValid)
        {
            return View(room);
        }

        try
        {
            await _roomService.CreateRoomAsync(room);
            TempData["SuccessMessage"] = $"Ruangan '{room.Name}' ({room.RoomCode}) berhasil ditambahkan!";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError("RoomCode", ex.Message);
            return View(room);
        }
    }

    // GET: /Rooms/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var room = await _roomService.GetRoomByIdAsync(id);
        if (room == null)
        {
            TempData["ErrorMessage"] = "Ruangan tidak ditemukan.";
            return RedirectToAction(nameof(Index));
        }

        return View(room);
    }

    // POST: /Rooms/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Room room)
    {
        if (id != room.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View(room);
        }

        try
        {
            var updated = await _roomService.UpdateRoomAsync(id, room);
            if (updated == null)
            {
                TempData["ErrorMessage"] = "Ruangan tidak ditemukan.";
                return RedirectToAction(nameof(Index));
            }

            TempData["SuccessMessage"] = $"Data ruangan '{updated.Name}' berhasil diperbarui.";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError("RoomCode", ex.Message);
            return View(room);
        }
    }

    // POST: /Rooms/ToggleStatus/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleStatus(int id)
    {
        var success = await _roomService.ToggleRoomStatusAsync(id);
        if (success)
        {
            TempData["SuccessMessage"] = "Status aktif ruangan berhasil diubah.";
        }
        else
        {
            TempData["ErrorMessage"] = "Ruangan tidak ditemukan.";
        }

        return RedirectToAction(nameof(Index));
    }

    // POST: /Rooms/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var success = await _roomService.DeleteRoomAsync(id);
            if (success)
            {
                TempData["SuccessMessage"] = "Ruangan berhasil dihapus.";
            }
            else
            {
                TempData["ErrorMessage"] = "Ruangan tidak ditemukan.";
            }
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }
}
