using Microsoft.AspNetCore.Mvc;
using SmartRoom.DTOs;
using SmartRoom.Models;
using SmartRoom.Services;

namespace SmartRoom.Controllers.Api;

[ApiController]
[Route("api/rooms")]
[Produces("application/json")]
public class RoomsApiController : ControllerBase
{
    private readonly IRoomService _roomService;

    public RoomsApiController(IRoomService roomService)
    {
        _roomService = roomService;
    }

    /// <summary>
    /// Mengambil daftar seluruh ruangan.
    /// </summary>
    /// <param name="isActive">Filter status aktif ruangan (opsional)</param>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<Room>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllRooms([FromQuery] bool? isActive)
    {
        var rooms = await _roomService.GetAllRoomsAsync(isActive);
        return Ok(rooms);
    }

    /// <summary>
    /// Mengambil detail satu ruangan berdasarkan ID.
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(Room), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRoomById(int id)
    {
        var room = await _roomService.GetRoomByIdAsync(id);
        if (room == null)
        {
            return NotFound(new { message = $"Ruangan dengan ID {id} tidak ditemukan." });
        }
        return Ok(room);
    }

    /// <summary>
    /// Mengecek ketersediaan dan slot jam yang sudah terisi pada ruangan dan tanggal tertentu.
    /// </summary>
    [HttpGet("{id:int}/availability")]
    [ProducesResponseType(typeof(RoomAvailabilityDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAvailability(int id, [FromQuery] string? date)
    {
        var targetDate = DateOnly.FromDateTime(DateTime.Today);
        if (!string.IsNullOrWhiteSpace(date) && DateOnly.TryParse(date, out var parsedDate))
        {
            targetDate = parsedDate;
        }

        var availability = await _roomService.GetRoomAvailabilityAsync(id, targetDate);
        if (availability == null)
        {
            return NotFound(new { message = $"Ruangan dengan ID {id} tidak ditemukan." });
        }

        return Ok(availability);
    }

    /// <summary>
    /// Membuat ruangan baru.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(Room), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateRoom([FromBody] Room room)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var created = await _roomService.CreateRoomAsync(room);
            return CreatedAtAction(nameof(GetRoomById), new { id = created.Id }, created);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Mengubah data ruangan.
    /// </summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(Room), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateRoom(int id, [FromBody] Room room)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var updated = await _roomService.UpdateRoomAsync(id, room);
            if (updated == null)
            {
                return NotFound(new { message = $"Ruangan dengan ID {id} tidak ditemukan." });
            }
            return Ok(updated);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Mengaktifkan atau menonaktifkan status ruangan.
    /// </summary>
    [HttpPatch("{id:int}/toggle")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ToggleStatus(int id)
    {
        var success = await _roomService.ToggleRoomStatusAsync(id);
        if (!success)
        {
            return NotFound(new { message = $"Ruangan dengan ID {id} tidak ditemukan." });
        }
        return Ok(new { message = "Status ruangan berhasil diperbarui." });
    }

    /// <summary>
    /// Menghapus ruangan secara permanen jika tidak ada jadwal aktif.
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteRoom(int id)
    {
        try
        {
            var success = await _roomService.DeleteRoomAsync(id);
            if (!success)
            {
                return NotFound(new { message = $"Ruangan dengan ID {id} tidak ditemukan." });
            }
            return Ok(new { message = "Ruangan berhasil dihapus secara permanen." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Mengunggah gambar ruangan baru.
    /// </summary>
    [HttpPost("upload-image")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadImage([FromForm] IFormFile? file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { message = "Berkas gambar tidak boleh kosong." });
        }

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(ext))
        {
            return BadRequest(new { message = "Format gambar tidak didukung. Harap unggah berkas JPG, PNG, atau WebP." });
        }

        if (file.Length > 5 * 1024 * 1024)
        {
            return BadRequest(new { message = "Ukuran gambar terlalu besar. Maksimal 5MB." });
        }

        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "rooms");
        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
        }

        var fileName = $"room_{DateTime.UtcNow:yyyyMMddHHmmss}_{Guid.NewGuid().ToString("N")[..8]}{ext}";
        var filePath = Path.Combine(uploadsFolder, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var relativePath = $"/uploads/rooms/{fileName}";
        return Ok(new { imageUrl = relativePath });
    }
}
