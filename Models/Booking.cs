using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SmartRoom.Models.Enums;

namespace SmartRoom.Models;

[Table("Bookings")]
public class Booking
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    [Display(Name = "No. Referensi")]
    public string BookingReference { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ruangan wajib dipilih")]
    [Display(Name = "Ruangan")]
    public int RoomId { get; set; }

    [ForeignKey(nameof(RoomId))]
    public Room? Room { get; set; }

    public int? UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public User? User { get; set; }

    [Required(ErrorMessage = "Nama peminjam wajib diisi")]
    [StringLength(100, ErrorMessage = "Nama peminjam maksimal 100 karakter")]
    [Display(Name = "Nama Peminjam")]
    public string BorrowerName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email peminjam wajib diisi")]
    [EmailAddress(ErrorMessage = "Format email tidak valid")]
    [StringLength(100, ErrorMessage = "Email maksimal 100 karakter")]
    [Display(Name = "Email Peminjam")]
    public string BorrowerEmail { get; set; } = string.Empty;

    [Required(ErrorMessage = "Tujuan peminjaman wajib diisi")]
    [StringLength(255, ErrorMessage = "Tujuan peminjaman maksimal 255 karakter")]
    [Display(Name = "Tujuan Peminjaman")]
    public string Purpose { get; set; } = string.Empty;

    [Required(ErrorMessage = "Waktu mulai wajib diisi")]
    [Display(Name = "Waktu Mulai")]
    public DateTime StartTime { get; set; }

    [Required(ErrorMessage = "Waktu selesai wajib diisi")]
    [Display(Name = "Waktu Selesai")]
    public DateTime EndTime { get; set; }

    [Display(Name = "Status")]
    public BookingStatus Status { get; set; } = BookingStatus.Pending;

    [StringLength(255, ErrorMessage = "Catatan admin maksimal 255 karakter")]
    [Display(Name = "Catatan Admin")]
    public string? AdminNotes { get; set; }

    [Display(Name = "Tanggal Pengajuan")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
