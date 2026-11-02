# 🏢 SmartRoom – Campus & Office Room Booking System

> **Full-Stack Hybrid ASP.NET Core 8 (.NET 8 MVC + RESTful API Controllers + Swagger OpenAPI)**  
> Sistem manajemen peminjaman ruangan cerdas dengan pencegahan jadwal ganda (*Anti Double-Booking Algorithm*) dan alur persetujuan (*Admin Approval Workflow*) terintegrasi.

---

## 🌟 Fitur Utama

- **Hybrid Web Architecture:**
  - **Web UI:** Antarmuka visual responsif berbasis **ASP.NET Core Razor Views**, **Bootstrap 5**, dan **Bootstrap Icons**.
  - **REST API & Swagger:** Endpoints JSON terstandarisasi terdokumentasi lengkap di `/swagger` via Swashbuckle OpenAPI.
- **Logika Anti-Bentrok (Double-Booking Prevention):**
  - Mencegah jadwal beririsan untuk ruangan yang sama (memeriksa status `Pending` dan `Approved`).
  - Aturan validasi: waktu mulai tidak boleh di masa lalu, durasi minimal 30 menit, dan maksimal 8 jam.
- **Service Layer Pattern (DRY Principle):**
  - Seluruh logika bisnis dipusatkan di `RoomService` dan `BookingService`, digunakan bersama oleh MVC Controller dan REST API Controller.
- **Dual Database Flexibility:**
  - **PostgreSQL** (`Npgsql.EntityFrameworkCore.PostgreSQL`) sebagai database production/staging utama.
  - **SQLite** (`Microsoft.EntityFrameworkCore.Sqlite`) untuk testing cepat lokal tanpa perlu menyalakan database server. Cukup ubah `"DatabaseProvider": "Sqlite"` atau `"PostgreSql"` di `appsettings.json`.
- **Automatic Data Seeding:**
  - Otomatis melakukan seeding 4 ruangan dan 4 data peminjaman sampel (*Approved*, *Pending*, *Rejected*) pada hari berjalan saat pertama kali aplikasi dijalankan.

---

## 🏗️ Struktur Proyek

```text
BookingApp/
├── Controllers/
│   ├── HomeController.cs            # MVC: Dashboard Ringkasan & Agenda Hari Ini
│   ├── RoomsController.cs           # MVC: UI Manajemen Ruangan (CRUD & Toggle)
│   ├── BookingsController.cs        # MVC: UI Pengajuan Peminjaman & Panel Approval
│   └── Api/                         # REST API Controllers (JSON Only)
│       ├── RoomsApiController.cs    # Endpoint: /api/rooms
│       └── BookingsApiController.cs # Endpoint: /api/bookings
├── Data/
│   ├── AppDbContext.cs              # EF Core DbContext (Fluent API & Indexes)
│   └── DbSeeder.cs                  # Auto-seeder 4 Ruangan & Sampel Booking Hari Ini
├── DTOs/
│   ├── CreateBookingDto.cs          # Validasi payload pengajuan
│   ├── UpdateBookingStatusDto.cs    # Payload update status & alasan penolakan
│   ├── DashboardStatsDto.cs         # Statistik agregat dashboard
│   └── RoomAvailabilityDto.cs       # Pengecekan slot terisi per tanggal
├── Models/
│   ├── Room.cs                      # Entitas Ruangan
│   ├── Booking.cs                   # Entitas Peminjaman
│   └── Enums/
│       └── BookingStatus.cs         # Pending (0), Approved (1), Rejected (2), Cancelled (3)
├── Services/
│   ├── IRoomService.cs & RoomService.cs
│   └── IBookingService.cs & BookingService.cs
├── Views/
│   ├── Home/Index.cshtml            # 4 Kartu Statistik + Tabel Agenda Hari Ini
│   ├── Rooms/                       # Katalog & Form Tambah/Edit Ruangan
│   ├── Bookings/                    # Form Peminjaman + Panel Approval Admin
│   └── Shared/_Layout.cshtml        # Layout Modern dengan tautan langsung Swagger
├── appsettings.json                 # Konfigurasi Provider (PostgreSql / Sqlite)
└── Program.cs                       # Pipeline Middleware, Swagger, DI & DB Init
```

---

## ⚡ Panduan Menjalankan Aplikasi

### 1. Prasyarat
- **.NET 8 SDK** (Sudah terpasang di sistem).
- **PostgreSQL** (opsional jika menggunakan SQLite).

### 2. Konfigurasi Database (`appsettings.json`)
Pilih provider database yang diinginkan pada file `appsettings.json`:

```json
{
  "DatabaseProvider": "PostgreSql",
  "ConnectionStrings": {
    "PostgreSql": "Host=localhost;Port=5432;Database=smartroom_db;Username=postgres;Password=postgres",
    "Sqlite": "Data Source=smartroom.db"
  }
}
```
*Catatan: Jika PostgreSQL lokal belum berjalan, Anda cukup mengubah `"DatabaseProvider": "Sqlite"` untuk langsung mencoba seluruh fitur.*

### 3. Build & Run
```bash
# Menjalankan build
dotnet build

# Menjalankan aplikasi
dotnet run
```

Buka browser Anda di:
- **Web UI Dashboard:** `http://localhost:5000` atau URL yang ditampilkan di terminal.
- **Interactive Swagger UI:** `http://localhost:5000/swagger`

---

## 📡 Dokumentasi REST API

| Method | Endpoint | Deskripsi |
| :--- | :--- | :--- |
| `GET` | `/api/rooms` | Mengambil seluruh ruangan (query `?isActive=true`) |
| `GET` | `/api/rooms/{id}` | Mengambil detail spesifik ruangan |
| `GET` | `/api/rooms/{id}/availability?date=YYYY-MM-DD` | Mengecek slot waktu yang sudah terisi |
| `POST` | `/api/rooms` | Mendaftarkan ruangan baru |
| `PUT` | `/api/rooms/{id}` | Mengubah informasi ruangan |
| `PATCH` | `/api/rooms/{id}/toggle` | Mengaktifkan/menonaktifkan status ruangan |
| `GET` | `/api/bookings` | Mengambil daftar peminjaman (`?roomId=..&status=..&date=..`) |
| `GET` | `/api/bookings/{id}` | Mengambil detail peminjaman |
| `POST` | `/api/bookings` | Mengajukan peminjaman (**201 Created** / **409 Conflict**) |
| `PATCH` | `/api/bookings/{id}/status` | Mengubah status peminjaman (`Approved` / `Rejected`) |

---

## 🛡️ Logika Pencegahan Bentrok Jadwal

```csharp
existing.RoomId == request.RoomId &&
(existing.Status == BookingStatus.Pending || existing.Status == BookingStatus.Approved) &&
request.StartTime < existing.EndTime &&
request.EndTime > existing.StartTime
```
Ketika terjadi irisan waktu, sistem otomatis mengembalikan status HTTP **409 Conflict** pada REST API dan alert merah informatif pada Razor Web UI.
