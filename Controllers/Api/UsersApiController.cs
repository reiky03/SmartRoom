using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartRoom.Data;
using SmartRoom.DTOs;
using SmartRoom.Models;
using SmartRoom.Models.Enums;

namespace SmartRoom.Controllers.Api;

[ApiController]
[Route("api/users")]
[Produces("application/json")]
public class UsersApiController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly PasswordHasher<User> _hasher = new();

    public UsersApiController(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Mengambil seluruh data pengguna dan civitas kampus.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<UserAccountDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _context.Users
            .OrderByDescending(u => u.CreatedAt)
            .Select(u => new UserAccountDto
            {
                Id = u.Id,
                Username = u.Username,
                FullName = u.FullName,
                Email = u.Email,
                Role = u.Role.ToString(),
                Department = u.Department,
                AvatarInitials = u.AvatarInitials,
                CreatedAt = u.CreatedAt
            })
            .ToListAsync();

        return Ok(users);
    }

    /// <summary>
    /// Mengambil satu profil pengguna berdasarkan ID.
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(UserAccountDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserById(int id)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user == null)
        {
            return NotFound(new { message = $"Pengguna dengan ID {id} tidak ditemukan." });
        }

        return Ok(new UserAccountDto
        {
            Id = user.Id,
            Username = user.Username,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role.ToString(),
            Department = user.Department,
            AvatarInitials = user.AvatarInitials,
            CreatedAt = user.CreatedAt
        });
    }

    /// <summary>
    /// Membuat akun pengguna / civitas baru (Mahasiswa, Staff/Dosen, Admin).
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(UserAccountDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var normalizedUsername = dto.Username.Trim().ToLowerInvariant();
        var normalizedEmail = dto.Email.Trim().ToLowerInvariant();

        if (await _context.Users.AnyAsync(u => u.Username.ToLower() == normalizedUsername))
        {
            return BadRequest(new { message = $"Username '{dto.Username}' sudah digunakan oleh pengguna lain." });
        }

        if (await _context.Users.AnyAsync(u => u.Email.ToLower() == normalizedEmail))
        {
            return BadRequest(new { message = $"Alamat email '{dto.Email}' sudah terdaftar dalam sistem." });
        }

        // Calculate initials
        var parts = dto.FullName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var initials = parts.Length >= 2
            ? $"{parts[0][0]}{parts[1][0]}".ToUpperInvariant()
            : (dto.FullName.Trim().Length >= 2 ? dto.FullName.Trim()[..2].ToUpperInvariant() : "SR");

        var user = new User
        {
            Username = normalizedUsername,
            FullName = dto.FullName.Trim(),
            Email = normalizedEmail,
            Role = dto.Role,
            Department = dto.Department?.Trim() ?? string.Empty,
            AvatarInitials = initials,
            CreatedAt = DateTime.UtcNow
        };

        user.PasswordHash = _hasher.HashPassword(user, dto.Password);

        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var resultDto = new UserAccountDto
        {
            Id = user.Id,
            Username = user.Username,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role.ToString(),
            Department = user.Department,
            AvatarInitials = user.AvatarInitials,
            CreatedAt = user.CreatedAt
        };

        return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, resultDto);
    }

    /// <summary>
    /// Memperbarui informasi profil pengguna.
    /// </summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(UserAccountDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user == null)
        {
            return NotFound(new { message = $"Pengguna dengan ID {id} tidak ditemukan." });
        }

        var normalizedEmail = dto.Email.Trim().ToLowerInvariant();
        var emailConflict = await _context.Users.AnyAsync(u => u.Id != id && u.Email.ToLower() == normalizedEmail);
        if (emailConflict)
        {
            return BadRequest(new { message = $"Alamat email '{dto.Email}' sudah digunakan akun lain." });
        }

        // Prevent demoting the last admin
        if (user.Role == UserRole.Admin && dto.Role != UserRole.Admin)
        {
            var adminCount = await _context.Users.CountAsync(u => u.Role == UserRole.Admin);
            if (adminCount <= 1)
            {
                return BadRequest(new { message = "Tidak dapat mengubah peran admin terakhir. Sistem membutuhkan minimal satu Administrator." });
            }
        }

        user.FullName = dto.FullName.Trim();
        user.Email = normalizedEmail;
        user.Role = dto.Role;
        user.Department = dto.Department?.Trim() ?? string.Empty;

        // Recalculate initials
        var parts = user.FullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        user.AvatarInitials = parts.Length >= 2
            ? $"{parts[0][0]}{parts[1][0]}".ToUpperInvariant()
            : (user.FullName.Length >= 2 ? user.FullName[..2].ToUpperInvariant() : "SR");

        // Optional password reset
        if (!string.IsNullOrWhiteSpace(dto.NewPassword))
        {
            user.PasswordHash = _hasher.HashPassword(user, dto.NewPassword);
        }

        await _context.SaveChangesAsync();

        return Ok(new UserAccountDto
        {
            Id = user.Id,
            Username = user.Username,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role.ToString(),
            Department = user.Department,
            AvatarInitials = user.AvatarInitials,
            CreatedAt = user.CreatedAt
        });
    }

    /// <summary>
    /// Menghapus akun pengguna dari sistem.
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user == null)
        {
            return NotFound(new { message = $"Pengguna dengan ID {id} tidak ditemukan." });
        }

        // Prevent deleting the last admin
        if (user.Role == UserRole.Admin)
        {
            var adminCount = await _context.Users.CountAsync(u => u.Role == UserRole.Admin);
            if (adminCount <= 1)
            {
                return BadRequest(new { message = "Tidak dapat menghapus administrator utama/terakhir dalam sistem." });
            }
        }

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        return Ok(new { message = $"Pengguna '{user.FullName}' ({user.Username}) berhasil dihapus." });
    }

    /// <summary>
    /// Otentikasi login untuk civitas dan admin.
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(UserAccountDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginApiRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { message = "Username/email dan kata sandi wajib diisi." });
        }

        var normalized = request.Username.Trim().ToLowerInvariant();
        var user = await _context.Users.FirstOrDefaultAsync(u =>
            u.Username.ToLower() == normalized ||
            u.Email.ToLower() == normalized);

        if (user == null)
        {
            return Unauthorized(new { message = "Username atau email tidak terdaftar dalam sistem." });
        }

        var verifyResult = _hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (verifyResult == PasswordVerificationResult.Failed)
        {
            return Unauthorized(new { message = "Kata sandi yang Anda masukkan tidak sesuai." });
        }

        return Ok(new UserAccountDto
        {
            Id = user.Id,
            Username = user.Username,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role.ToString(),
            Department = user.Department,
            AvatarInitials = user.AvatarInitials,
            CreatedAt = user.CreatedAt
        });
    }
}
