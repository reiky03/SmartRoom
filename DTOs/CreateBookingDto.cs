using System.ComponentModel.DataAnnotations;

namespace SmartRoom.DTOs;

public class CreateBookingDto
{
    [Required(ErrorMessage = "Ruangan wajib dipilih")]
    public int RoomId { get; set; }

    public int? UserId { get; set; }

    [Required(ErrorMessage = "Nama peminjam wajib diisi")]
    [StringLength(100, ErrorMessage = "Nama peminjam maksimal 100 karakter")]
    public string BorrowerName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email peminjam wajib diisi")]
    [EmailAddress(ErrorMessage = "Format email tidak valid")]
    [StringLength(100, ErrorMessage = "Email maksimal 100 karakter")]
    public string BorrowerEmail { get; set; } = string.Empty;

    [Required(ErrorMessage = "Tujuan peminjaman wajib diisi")]
    [StringLength(255, ErrorMessage = "Tujuan peminjaman maksimal 255 karakter")]
    public string Purpose { get; set; } = string.Empty;

    [Required(ErrorMessage = "Waktu mulai wajib diisi")]
    public DateTime StartTime { get; set; }

    [Required(ErrorMessage = "Waktu selesai wajib diisi")]
    public DateTime EndTime { get; set; }
}
