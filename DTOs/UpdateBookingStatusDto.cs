using System.ComponentModel.DataAnnotations;
using SmartRoom.Models.Enums;

namespace SmartRoom.DTOs;

public class UpdateBookingStatusDto
{
    [Required(ErrorMessage = "Status baru wajib ditentukan")]
    public BookingStatus Status { get; set; }

    [StringLength(255, ErrorMessage = "Catatan admin maksimal 255 karakter")]
    public string? AdminNotes { get; set; }
}
