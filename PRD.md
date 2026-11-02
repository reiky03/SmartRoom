# Product Requirements Document (PRD): SmartRoom – Campus & Office Room Booking System

## 1. Project Overview
SmartRoom adalah aplikasi manajemen peminjaman ruangan berbasis web (Full-Stack menggunakan ASP.NET Core 8 MVC) yang dilengkapi dengan RESTful API terintegrasi. Sistem ini dirancang untuk mencegah bentrok jadwal (double booking) peminjaman ruangan serta menyediakan alur persetujuan (approval workflow) yang transparan.

**Tujuan Portofolio:**
- Menampilkan penguasaan arsitektur .NET 8 (C#), Entity Framework Core, LINQ, Service Layer Pattern, dan DTOs.
- Menyediakan antarmuka visual (Razor Views + Bootstrap 5) untuk demonstrasi langsung di GitHub sekaligus dokumentasi Swagger OpenAPI untuk pengujian endpoint Backend via Postman.

---

## 2. Tech Stack & Architecture
- **Framework:** ASP.NET Core 8.0 (Hybrid: MVC untuk UI + Web API Controllers untuk REST API).
- **Language:** C# 12.
- **Database & ORM:** MySQL (via Pomelo.EntityFrameworkCore.MySql) / SQLite (Development Fallback) & Entity Framework Core 8 (Code-First Approach).
- **Frontend UI:** Razor Views (.cshtml), Bootstrap 5, Bootstrap Icons.
- **API Documentation:** Swashbuckle / Swagger UI (`/swagger`).
- **Architecture Pattern:** Layered Architecture (Controllers → Services → Data/DbContext) dengan pemisahan Models (Entities) dan DTOs / ViewModels.

---

## 3. Database Schema (Entities)

### 3.1. Room (Tabel: `Rooms`)
- `Id` (int, Primary Key, Auto Increment)
- `RoomCode` (string, Required, MaxLength: 20, Unique) — Contoh: "R-SIDANG-01"
- `Name` (string, Required, MaxLength: 100) — Contoh: "Ruang Sidang Utama Lt. 2"
- `Building` (string, Required, MaxLength: 100) — Contoh: "Gedung Fakultas TI"
- `Capacity` (int, Required)
- `Facilities` (string, MaxLength: 255) — Contoh: "Proyektor, AC, Smart TV, Papan Tulis"
- `IsActive` (bool, Default: true)
- `CreatedAt` (DateTime, Default: DateTime.UtcNow)

### 3.2. Booking (Tabel: `Bookings`)
- `Id` (int, Primary Key, Auto Increment)
- `BookingReference` (string, Required, Unique) — Auto-generated, contoh: "BK-20261102-XXXX"
- `RoomId` (int, Foreign Key ke Rooms.Id, Required)
- `BorrowerName` (string, Required, MaxLength: 100)
- `BorrowerEmail` (string, Required, EmailAddress)
- `Purpose` (string, Required, MaxLength: 255)
- `StartTime` (DateTime, Required)
- `EndTime` (DateTime, Required)
- `Status` (Enum BookingStatus: `Pending = 0`, `Approved = 1`, `Rejected = 2`, `Cancelled = 3`)
- `AdminNotes` (string, Nullable, MaxLength: 255) — Alasan penolakan atau catatan admin
- `CreatedAt` (DateTime, Default: DateTime.UtcNow)

---

## 4. Core Business Rules & Validations (Backend Focus)

### Validasi Rentang Waktu:
- `StartTime` harus lebih kecil dari `EndTime` (`StartTime < EndTime`).
- `StartTime` tidak boleh di masa lalu (`StartTime >= DateTime.Now`).
- Durasi peminjaman minimal 30 menit dan maksimal 8 jam dalam satu pengajuan.

### Logika Anti-Bentrok Jadwal (Double-Booking Prevention):
- Ruangan tidak dapat dipesan jika pada `RoomId` yang sama terdapat jadwal dengan status `Pending` atau `Approved` yang beririsan waktu.
- Rumus irisan waktu di LINQ:
  ```csharp
  existing.RoomId == request.RoomId &&
  (existing.Status == BookingStatus.Pending || existing.Status == BookingStatus.Approved) &&
  request.StartTime < existing.EndTime &&
  request.EndTime > existing.StartTime
  ```

### Ketersediaan Ruangan:
- Ruangan dengan `IsActive == false` tidak dapat dipilih atau dipesan.

### Shared Business Logic (DRY Principle):
- Semua logika bisnis wajib ditempatkan di dalam `RoomService` dan `BookingService` (`Services/`), sehingga baik MVC Controller (Web UI) maupun API Controller (`/api/...`) memanggil fungsi Service yang sama.

---

## 5. Functional Requirements

### A. Web UI Modules (ASP.NET Core MVC - Razor Views)
- **Dashboard (`/`)**
  - Menampilkan 4 kartu statistik ringkas: Total Ruangan Aktif, Peminjaman Hari Ini, Menunggu Persetujuan (Pending), dan Total Disetujui.
  - Menampilkan tabel "Jadwal Peminjaman Hari Ini" beserta badge status berwarna (Pending = Kuning, Approved = Hijau, Rejected = Merah).
- **Manajemen Ruangan (`/Rooms`)**
  - Daftar ruangan dalam bentuk tabel/kartu, lengkap dengan fitur tambah ruangan baru, edit, dan toggle status aktif/nonaktif.
- **Form & Daftar Peminjaman (`/Bookings`)**
  - Halaman katalog pengajuan peminjaman baru dengan validasi real-time (menampilkan pesan alert merah yang jelas jika jadwal bentrok).
  - Fitur filter daftar peminjaman berdasarkan Ruangan, Tanggal, dan Status.
- **Panel Approval Admin (`/Bookings/Manage`)**
  - Tombol aksi cepat Approve dan Reject (disertai modal/input alasan AdminNotes) untuk mengubah status peminjaman.

### B. REST API Endpoints (Untuk Postman & Swagger)
Semua endpoint API harus mengembalikan format JSON standar dan terdokumentasi di `/swagger`:
- `GET /api/rooms` — Mengambil daftar seluruh ruangan (mendukung query `?isActive=true`).
- `GET /api/rooms/{id}/availability?date=YYYY-MM-DD` — Mengecek slot jam yang sudah terisi pada ruangan dan tanggal tertentu.
- `GET /api/bookings` — Mengambil daftar peminjaman dengan filter opsional (`roomId`, `status`, `date`).
- `POST /api/bookings` — Membuat peminjaman baru menggunakan `CreateBookingDto` (mengembalikan HTTP 201 Created jika sukses, atau HTTP 409 Conflict jika jadwal bentrok).
- `PATCH /api/bookings/{id}/status` — Mengubah status peminjaman (Approved / Rejected) beserta AdminNotes.

---

## 6. Target Project Structure
```text
SmartRoom/
├── Controllers/
│   ├── HomeController.cs            # MVC: Dashboard
│   ├── RoomsController.cs           # MVC: UI Manajemen Ruangan
│   ├── BookingsController.cs        # MVC: UI Peminjaman & Approval
│   └── Api/                         # REST API Controllers (JSON Only)
│       ├── RoomsApiController.cs    # Route: /api/rooms
│       └── BookingsApiController.cs # Route: /api/bookings
├── Data/
│   ├── AppDbContext.cs              # EF Core DbContext
│   └── DbSeeder.cs                  # Auto-seed 4 Ruangan & sampel Booking saat pertama jalan
├── DTOs/
│   ├── CreateBookingDto.cs
│   ├── UpdateBookingStatusDto.cs
│   └── DashboardStatsDto.cs
├── Models/
│   ├── Room.cs
│   ├── Booking.cs
│   └── Enums/
│       └── BookingStatus.cs
├── Services/
│   ├── IRoomService.cs
│   ├── RoomService.cs
│   ├── IBookingService.cs
│   └── BookingService.cs
├── Views/
│   ├── Home/
│   ├── Rooms/
│   ├── Bookings/
│   └── Shared/
│       └── _Layout.cshtml           # Modern Navbar (Dashboard, Rooms, Bookings, Link ke Swagger UI)
├── appsettings.json
└── Program.cs
```

---

## 7. Initial Data Seeding (`DbSeeder.cs`)
Saat aplikasi dijalankan pertama kali (`dotnet run`), sistem harus otomatis mengisi (seed) data awal jika tabel masih kosong:
- 4 Ruangan Contoh: Ruang Sidang Utama, Lab Komputer A, Ruang Rapat Dekanat, Aula Mini.
- 4 Data Booking Contoh: Kombinasi status Approved, Pending, dan Rejected pada hari ini agar Dashboard langsung terlihat hidup saat pertama kali dibuka atau di-screenshot.
