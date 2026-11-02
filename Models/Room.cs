using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace SmartRoom.Models;

[Table("Rooms")]
public class Room
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required(ErrorMessage = "Kode ruangan wajib diisi")]
    [StringLength(20, ErrorMessage = "Kode ruangan maksimal 20 karakter")]
    [Display(Name = "Kode Ruangan")]
    public string RoomCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "Nama ruangan wajib diisi")]
    [StringLength(100, ErrorMessage = "Nama ruangan maksimal 100 karakter")]
    [Display(Name = "Nama Ruangan")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Gedung wajib diisi")]
    [StringLength(100, ErrorMessage = "Nama gedung maksimal 100 karakter")]
    [Display(Name = "Gedung")]
    public string Building { get; set; } = string.Empty;

    [Required(ErrorMessage = "Kapasitas wajib diisi")]
    [Range(1, 1000, ErrorMessage = "Kapasitas harus antara 1 sampai 1000 orang")]
    [Display(Name = "Kapasitas (Orang)")]
    public int Capacity { get; set; }

    [StringLength(255, ErrorMessage = "Fasilitas maksimal 255 karakter")]
    [Display(Name = "Fasilitas")]
    public string? Facilities { get; set; }

    [Display(Name = "Status Aktif")]
    public bool IsActive { get; set; } = true;

    [Display(Name = "Dibuat Pada")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [JsonIgnore]
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
