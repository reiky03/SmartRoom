using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SmartRoom.Models;
using SmartRoom.Models.Enums;

namespace SmartRoom.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        // Ensure database is created
        await context.Database.EnsureCreatedAsync();

        var hasher = new PasswordHasher<User>();

        // Seed Users if empty
        if (!await context.Users.AnyAsync())
        {
            var adminUser = new User
            {
                Username = "admin",
                FullName = "Dr. Hendra Wijaya, M.Sc.",
                Email = "admin@smartroom.ac.id",
                Role = UserRole.Admin,
                Department = "Biro Pengelolaan Fasilitas & Aset Kampus",
                AvatarInitials = "HW",
                CreatedAt = DateTime.UtcNow.AddMonths(-2)
            };
            adminUser.PasswordHash = hasher.HashPassword(adminUser, "admin123");

            var staffUser = new User
            {
                Username = "staff",
                FullName = "Siti Nurhaliza, M.Kom.",
                Email = "staff@smartroom.ac.id",
                Role = UserRole.Staff,
                Department = "Koordinator Laboratorium Fakultas TI",
                AvatarInitials = "SN",
                CreatedAt = DateTime.UtcNow.AddMonths(-2)
            };
            staffUser.PasswordHash = hasher.HashPassword(staffUser, "staff123");

            var studentUser = new User
            {
                Username = "student",
                FullName = "Rizky Pratama",
                Email = "student@smartroom.ac.id",
                Role = UserRole.Student,
                Department = "Badan Eksekutif Mahasiswa (BEM) TI",
                AvatarInitials = "RP",
                CreatedAt = DateTime.UtcNow.AddMonths(-2)
            };
            studentUser.PasswordHash = hasher.HashPassword(studentUser, "student123");

            await context.Users.AddRangeAsync(adminUser, staffUser, studentUser);
            await context.SaveChangesAsync();
        }

        if (await context.Rooms.AnyAsync())
        {
            return; // Rooms already seeded
        }

        var today = DateTime.Today;

        var rooms = new List<Room>
        {
            new()
            {
                RoomCode = "R-SIDANG-01",
                Name = "Ruang Sidang Utama Lt. 2",
                Building = "Gedung Fakultas TI",
                Capacity = 50,
                Facilities = "Proyektor 4K Laser, Dual Smart TV 75 Inch, Audio Conference 8-Mic, AC Central, Podium",
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-10)
            },
            new()
            {
                RoomCode = "LAB-KOMP-A",
                Name = "Laboratorium Rekayasa Perangkat Lunak",
                Building = "Gedung Cyber Lt. 3",
                Capacity = 40,
                Facilities = "40 PC Intel Core i9 32GB RAM, Gigabit Switch, AC Central, Interactive Whiteboard",
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-8)
            },
            new()
            {
                RoomCode = "R-RAPAT-DEK",
                Name = "Executive Boardroom Dekanat",
                Building = "Gedung Rektorat Lt. 1",
                Capacity = 20,
                Facilities = "Cisco Webex Room Bar, Meja Konferensi Mahoni, Soundproofing, Smart Dimming Light",
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-5)
            },
            new()
            {
                RoomCode = "AULA-MINI-01",
                Name = "Auditorium Mini Sivitas",
                Building = "Pusat Kegiatan Mahasiswa Lt. 1",
                Capacity = 120,
                Facilities = "Stage Panggung, Wireless Line Array Sound, Dual Laser Projector, Lighting Truss",
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-3)
            }
        };

        await context.Rooms.AddRangeAsync(rooms);
        await context.SaveChangesAsync();

        var admin = await context.Users.FirstOrDefaultAsync(u => u.Username == "admin");
        var staff = await context.Users.FirstOrDefaultAsync(u => u.Username == "staff");
        var student = await context.Users.FirstOrDefaultAsync(u => u.Username == "student");

        var bookings = new List<Booking>
        {
            new()
            {
                BookingReference = $"BK-{today:yyyyMMdd}-001",
                RoomId = rooms[0].Id,
                UserId = staff?.Id,
                BorrowerName = staff?.FullName ?? "Dr. Ir. Budi Santoso, M.T.",
                BorrowerEmail = staff?.Email ?? "staff@smartroom.ac.id",
                Purpose = "Sidang Terbuka Tugas Akhir & Skripsi Mahasiswa TI",
                StartTime = today.AddHours(8).AddMinutes(30),
                EndTime = today.AddHours(11).AddMinutes(30),
                Status = BookingStatus.Approved,
                AdminNotes = "Disetujui. Ruangan sudah disiapkan oleh bagian perlengkapan.",
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            },
            new()
            {
                BookingReference = $"BK-{today:yyyyMMdd}-002",
                RoomId = rooms[1].Id,
                UserId = staff?.Id,
                BorrowerName = staff?.FullName ?? "Siti Nurhaliza, M.Kom.",
                BorrowerEmail = staff?.Email ?? "staff@smartroom.ac.id",
                Purpose = "Workshop Hands-on: Modern Cloud Architecture & Distributed Systems",
                StartTime = today.AddHours(13).AddMinutes(0),
                EndTime = today.AddHours(16).AddMinutes(0),
                Status = BookingStatus.Approved,
                AdminNotes = "Disetujui. Harap menjaga kebersihan dan perangkat lab.",
                CreatedAt = DateTime.UtcNow.AddHours(-12)
            },
            new()
            {
                BookingReference = $"BK-{today:yyyyMMdd}-003",
                RoomId = rooms[2].Id,
                UserId = admin?.Id,
                BorrowerName = "Dr. Hendra Wijaya, M.Sc.",
                BorrowerEmail = "admin@smartroom.ac.id",
                Purpose = "Rapat Koordinasi Anggaran Pengadaan Server & Jaringan",
                StartTime = today.AddHours(10).AddMinutes(0),
                EndTime = today.AddHours(12).AddMinutes(0),
                Status = BookingStatus.Pending,
                AdminNotes = null,
                CreatedAt = DateTime.UtcNow.AddHours(-3)
            },
            new()
            {
                BookingReference = $"BK-{today:yyyyMMdd}-004",
                RoomId = rooms[3].Id,
                UserId = student?.Id,
                BorrowerName = student?.FullName ?? "Rizky Pratama",
                BorrowerEmail = student?.Email ?? "student@smartroom.ac.id",
                Purpose = "Konser Akustik & Temu Akrab Anggota BEM Baru",
                StartTime = today.AddHours(14).AddMinutes(0),
                EndTime = today.AddHours(17).AddMinutes(0),
                Status = BookingStatus.Rejected,
                AdminNotes = "Ditolak: Ruangan dialokasikan untuk gladi bersih wisuda universitas.",
                CreatedAt = DateTime.UtcNow.AddHours(-6)
            }
        };

        await context.Bookings.AddRangeAsync(bookings);
        await context.SaveChangesAsync();
    }
}
