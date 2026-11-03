using System.ComponentModel.DataAnnotations;
using SmartRoom.Models.Enums;

namespace SmartRoom.DTOs;

public class UserAccountDto
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string AvatarInitials { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class CreateUserDto
{
    [Required(ErrorMessage = "Username wajib diisi")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Username minimal 3 dan maksimal 50 karakter")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Kata sandi wajib diisi")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Kata sandi minimal 6 karakter")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Nama lengkap wajib diisi")]
    [StringLength(100, ErrorMessage = "Nama lengkap maksimal 100 karakter")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email wajib diisi")]
    [EmailAddress(ErrorMessage = "Format email tidak valid")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Peran / Role wajib ditentukan")]
    public UserRole Role { get; set; } = UserRole.Student;

    [StringLength(100, ErrorMessage = "Departemen/Prodi maksimal 100 karakter")]
    public string Department { get; set; } = string.Empty;
}

public class UpdateUserDto
{
    [Required(ErrorMessage = "Nama lengkap wajib diisi")]
    [StringLength(100, ErrorMessage = "Nama lengkap maksimal 100 karakter")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email wajib diisi")]
    [EmailAddress(ErrorMessage = "Format email tidak valid")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Peran / Role wajib ditentukan")]
    public UserRole Role { get; set; } = UserRole.Student;

    [StringLength(100, ErrorMessage = "Departemen/Prodi maksimal 100 karakter")]
    public string Department { get; set; } = string.Empty;

    [StringLength(100, MinimumLength = 6, ErrorMessage = "Kata sandi baru minimal 6 karakter")]
    public string? NewPassword { get; set; }
}

public class LoginApiRequest
{
    [Required(ErrorMessage = "Username atau email wajib diisi")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Kata sandi wajib diisi")]
    public string Password { get; set; } = string.Empty;
}
