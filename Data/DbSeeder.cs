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

        // Seed Users if empty
        if (!await context.Users.AnyAsync())
        {
            var defaultUsers = new List<User>
            {
                new()
                {
                    Username = "admin",
                    FullName = "Dr. Hendra Wijaya, M.Sc.",
                    Email = "admin@smartroom.ac.id",
                    Role = UserRole.Admin,
                    Department = "Biro Sarana & Fasilitas Kampus",
                    AvatarInitials = "HW",
                    CreatedAt = DateTime.UtcNow.AddMonths(-1)
                },
                new()
                {
                    Username = "staff",
                    FullName = "Siti Nurhaliza, M.Kom.",
                    Email = "staff@smartroom.ac.id",
                    Role = UserRole.Staff,
                    Department = "Dosen & Koordinator Lab TI",
                    AvatarInitials = "SN",
                    CreatedAt = DateTime.UtcNow.AddMonths(-1)
                },
                new()
                {
                    Username = "student",
                    FullName = "Rizky Pratama",
                    Email = "student@smartroom.ac.id",
                    Role = UserRole.Student,
                    Department = "Mahasiswa / Ketua BEM TI",
                    AvatarInitials = "RP",
                    CreatedAt = DateTime.UtcNow.AddMonths(-1)
                }
            };
            await context.Users.AddRangeAsync(defaultUsers);
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
                Facilities = "Proyektor 4K, Sound System, AC, Smart TV 75 Inch, Papan Tulis Kaca",
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-10)
            },
            new()
            {
                RoomCode = "LAB-KOMP-A",
                Name = "Laboratorium Komputer A",
                Building = "Gedung Cyber Lt. 3",
                Capacity = 40,
                Facilities = "40 PC Workstation, AC Central, Gigabit LAN, Proyektor Laser",
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-8)
            },
            new()
            {
                RoomCode = "R-RAPAT-DEK",
                Name = "Ruang Rapat Dekanat",
                Building = "Gedung Rektorat Lt. 1",
                Capacity = 20,
                Facilities = "Meja Konferensi Bundar, Video Conference Zoom Rooms, AC, Dispenser",
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-5)
            },
            new()
            {
                RoomCode = "AULA-MINI-01",
                Name = "Aula Mini Serbaguna",
                Building = "Pusat Kegiatan Mahasiswa Lt. 1",
                Capacity = 120,
                Facilities = "Panggung Presentasi, Wireless Mic, Proyektor Ganda, AC Stand 5PK",
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-3)
            }
        };

        await context.Rooms.AddRangeAsync(rooms);
        await context.SaveChangesAsync();

        var bookings = new List<Booking>
        {
            new()
            {
                BookingReference = $"BK-{today:yyyyMMdd}-001",
                RoomId = rooms[0].Id,
                BorrowerName = "Dr. Ir. Budi Santoso, M.T.",
                BorrowerEmail = "budi.santoso@university.ac.id",
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
                BorrowerName = "Himpunan Mahasiswa Informatika",
                BorrowerEmail = "hmif@students.university.ac.id",
                Purpose = "Workshop Hands-on: Building Modern Web with .NET 8",
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
                BorrowerName = "Siti Rahmawati, S.Kom.",
                BorrowerEmail = "siti.rahmawati@staff.university.ac.id",
                Purpose = "Koordinasi Anggaran & Akreditasi Program Studi",
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
                BorrowerName = "Unit Kegiatan Mahasiswa Musik",
                BorrowerEmail = "ukm.musik@students.university.ac.id",
                Purpose = "Gladi Resik Konser Akustik Kampus",
                StartTime = today.AddHours(14).AddMinutes(0),
                EndTime = today.AddHours(17).AddMinutes(0),
                Status = BookingStatus.Rejected,
                AdminNotes = "Ditolak: Aula Mini sedang dalam perawatan sound system rutin.",
                CreatedAt = DateTime.UtcNow.AddHours(-6)
            }
        };

        await context.Bookings.AddRangeAsync(bookings);
        await context.SaveChangesAsync();
    }
}
