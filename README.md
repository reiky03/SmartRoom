# 🏛️ SmartRoom — Campus Architectural Space & Facility Desk

[![.NET 8](https://img.shields.io/badge/.NET-8.0-512BD4?style=flat&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![Vue 3](https://img.shields.io/badge/Vue-3.5-4FC08D?style=flat&logo=vuedotjs&logoColor=white)](https://vuejs.org/)
[![Vite](https://img.shields.io/badge/Vite-5.0-646CFF?style=flat&logo=vite&logoColor=white)](https://vitejs.dev/)
[![Tailwind CSS](https://img.shields.io/badge/Tailwind-3.4-38B2AC?style=flat&logo=tailwind-css&logoColor=white)](https://tailwindcss.com/)
[![Entity Framework Core](https://img.shields.io/badge/EF%20Core-8.0-blue?style=flat)](https://learn.microsoft.com/ef/core/)
[![Swagger OpenAPI](https://img.shields.io/badge/Swagger-OpenAPI%203.0-85EA2D?style=flat&logo=swagger&logoColor=black)](http://localhost:5100/swagger)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

> **Enterprise Full-Stack Facility Reservation & Schedule Arbitration Platform**  
> Menggabungkan ketahanan arsitektur backend **ASP.NET Core 8 Web API & EF Core** dengan presisi estetika frontend **Vue 3 SPA** bertema *Quiet Campus Atelier* (warm editorial minimalism terinspirasi oleh Halden dan Linear). Dilengkapi algoritma pencegahan jadwal ganda (*Anti Double-Booking Algorithm*), kontrol akses berbasis peran (RBAC), serta tiket digital *E-Pass Akses* terverifikasi.

---

## 📸 Galeri Antarmuka Sistem

### 1. Portal Civitas Akademika (Katalog Ruangan Kampus)
Pencarian instan, filter kapasitas pill, status ketersediaan real-time (*Sedang Digunakan* / *Bebas Seharian*), serta lencana fasilitas ala macOS Finder Tag.
![Portal Civitas Akademika - Katalog Ruangan](docs/screenshots/01-portal-civitas-katalog.png)

### 2. Riwayat Peminjaman & E-Pass Akses Digital
Pantau status permohonan langsung (*Pending*, *Approved*, *Rejected*), catatan resmi penolakan dari admin fasilitas, dan akses tiket digital *E-Pass* lengkap dengan QR verifikasi.
![Riwayat Peminjaman & E-Pass Akses](docs/screenshots/02-portal-civitas-peminjaman-epass.png)

### 3. Meja Operasi Fasilitas & Matriks Okupansi Harian
Pusat kendali administrator dengan 4 kartu metrik utama, navigator tanggal interaktif, dan matriks okupansi harian (*Gantt Schedule Matrix*) dengan penanda jam operasional yang presisi.
![Meja Operasi Fasilitas - Matriks Okupansi](docs/screenshots/03-admin-fasilitas-matriks.png)

### 4. Matriks Jadwal Harian Penuh (08:00 – 18:00 WIB)
Garis ruler jam operasional terkalibrasi secara matematis dari jam buka (`08:00`) hingga jam tutup (`18:00`), dilengkapi dukungan penyesuaian jam operasional dinamis (*Full Day*, *Shift Malam*).
![Matriks Jadwal Penuh](docs/screenshots/04-admin-matriks-okupansi-full.png)

---

## 🌟 Sorotan Fitur Unggulan (Core Capabilities)

### 🛡️ 1. Algoritma Anti Double-Booking (Mathematical Interval Collision)
Sistem menggunakan aritmatika irisan interval waktu pada query LINQ level database untuk menjamin tidak ada jadwal yang bertabrakan:
$$\text{Overlap} \iff (\text{Start}_{\text{existing}} < \text{End}_{\text{new}}) \land (\text{End}_{\text{existing}} > \text{Start}_{\text{new}})$$

```csharp
// Cuplikan BookingService.cs
var hasConflict = await _context.Bookings.AnyAsync(b =>
    b.RoomId == dto.RoomId &&
    b.Status != BookingStatus.Rejected &&
    b.Status != BookingStatus.Cancelled &&
    b.StartTime < dto.EndTime &&
    b.EndTime > dto.StartTime);

if (hasConflict)
    throw new InvalidOperationException("Ruangan telah dipesan pada rentang waktu tersebut.");
```

### 👥 2. Pemisahan Peran Ketat (Role-Based Access Control)
- **Portal Civitas (Mahasiswa / Dosen / Staff):**
  - Akses katalog sarana kampus terverifikasi.
  - Form peminjaman interaktif berbentuk *slide-over drawer* dengan pendeteksi slot bentrok secara real-time.
  - Tiket digital **E-Pass Akses** (modal QR Code dengan nomor referensi unik `BK-YYYYMMDD-XXX`).
  - Proteksi rute otomatis: dilarang mengakses modul persetujuan dan pengelolaan ruangan admin.
- **Meja Operasi Fasilitas (Administrator):**
  - Kartu ringkasan KPI (Ruangan Aktif, Agenda Hari Ini, Menunggu Review, Terkonfirmasi).
  - Matriks okupansi ruangan interaktif (*Timeline Matrix Runway*) dengan visualisasi kapsul jadwal *Approved* (emerald) dan *Pending* (amber).
  - Pengaturan fleksibel jam operasional matriks (*Standard 08:00–18:00*, *Full Day 07:00–21:00*, *Shift Malam 08:00–22:00*) tersimpan di `localStorage`.
  - Antrean persetujuan satu-klik (*Approve*) atau penolakan dengan input alasan resmi (*Reject with Mandatory Reason*).
  - Manajemen inventaris sarana (CRUD Ruangan, upload URL gambar, toggle status aktif).

### 🎨 3. Desain "Quiet Campus Atelier" (Design Philosophy)
- **Palet Hangat Netral:** Kanvas `#F4F5F8` dan `#FBFAF7` berpadu dengan aksen emerald pekat `#214C38`, amber `#996515`, serta tepian halus `#D9D8D1`.
- **Hierarki Tipografi Editorial:** Perpaduan serif anggun (*Newsreader*) untuk tajuk utama dan sans-serif teknis (*Plus Jakarta Sans* / *SF Pro Text*) untuk form dan kontrol navigasi.
- **Monospace Metadata:** Jam, kode ruangan (`AULA-MINI-01`), dan kode booking diformat dengan font monospace bertanda baca presisi.
- **Zero-Layout-Shift Slide-Over Drawer:** Form pemesanan muncul anggun dari sisi kanan tanpa memindahkan konten dasar.

---

## 🏛️ Arsitektur Sistem & Struktur Proyek

```text
BookingApp/
├── Controllers/
│   ├── Api/
│   │   ├── AuthApiController.cs        # REST API: Login, Register, Current User, Logout
│   │   ├── BookingsApiController.cs    # REST API: CRUD Peminjaman, Cek Ketersediaan, Review
│   │   └── RoomsApiController.cs       # REST API: Inventaris Ruangan, Status Operasional
│   ├── BookingsController.cs           # Fallback MVC: Peminjaman & Persetujuan
│   ├── HomeController.cs               # Fallback MVC: Dashboard
│   └── RoomsController.cs              # Fallback MVC: Kelola Ruangan
├── Data/
│   ├── AppDbContext.cs                 # EF Core DbContext, Indexing & Entity Configurations
│   └── DbSeeder.cs                     # Otomasi Seeding Akun Demo, Ruangan, & Jadwal
├── DTOs/
│   ├── AuthDtos.cs                     # LoginRequest, RegisterRequest, AuthUserDto
│   ├── CreateBookingDto.cs             # Payload reservasi baru + validasi waktu
│   ├── UpdateBookingStatusDto.cs       # Payload persetujuan / penolakan
│   └── RoomAvailabilityDto.cs          # Pengecekan slot bentrok harian
├── Models/
│   ├── User.cs                         # Identitas, Hash Password (PBKDF2), Peran
│   ├── Room.cs                         # Informasi sarana, kapasitas, status
│   ├── Booking.cs                      # Data reservasi, kode referensi, status approval
│   └── Enums/BookingStatus.cs          # Pending (0), Approved (1), Rejected (2), Cancelled (3)
├── Services/
│   ├── IAuthService.cs & AuthService.cs
│   ├── IBookingService.cs & BookingService.cs
│   └── IRoomService.cs & RoomService.cs
├── frontend/                           # Vue 3 SPA Client (Modern Frontend)
│   ├── src/
│   │   ├── api/client.ts               # Axios HTTP Client dengan error interception
│   │   ├── components/
│   │   │   ├── Navbar.vue              # Floating Capsule Bar + Profile Dropdown
│   │   │   ├── TimelineMatrix.vue      # Matriks Okupansi Runway + Dynamic Hours Ruler
│   │   │   ├── BookingDrawer.vue       # Slide-Over Booking Form + Live Conflict Check
│   │   │   ├── EPassModal.vue          # Tiket E-Pass Digital + Canvas QR Code
│   │   │   └── RejectModal.vue         # Modal Penolakan dengan Alasan Resmi
│   │   ├── views/
│   │   │   ├── LoginView.vue           # Login Gate + 1-Click Quick Demo Switcher
│   │   │   ├── StudentDashboardView.vue# Portal Mahasiswa (Katalog & Peminjaman Saya)
│   │   │   ├── AdminDashboardView.vue  # Dashboard Operasional + Matriks Harian
│   │   │   ├── AdminApprovalsView.vue  # Antrean Persetujuan / Penolakan
│   │   │   ├── AdminRoomsView.vue      # Manajemen Inventaris Sarana Kampus
│   │   │   └── AdminAllBookingsView.vue# Log Arsip Peminjaman Menyeluruh
│   │   └── router/index.ts             # Vue Router dengan RBAC Navigation Guards
│   └── test-redesign-smoke.js          # Puppeteer E2E Testing Suite
├── docs/screenshots/                   # Dokumentasi Visual High-Resolution
└── wwwroot/                            # Static Output Production Bundle dari Vite
```

---

## ⚡ Panduan Instalasi & Menjalankan Proyek

### 1. Prasyarat Sistem
- **.NET 8 SDK** ([Unduh .NET 8](https://dotnet.microsoft.com/download/dotnet/8.0))
- **Node.js 18+** & npm ([Unduh Node.js](https://nodejs.org/))

### 2. Kloning Repositori
```bash
git clone https://github.com/reiky03/BookingApp.git
cd BookingApp
```

### 3. Konfigurasi Database (`appsettings.json`)
Aplikasi mendukung **SQLite** (siap pakai tanpa server) dan **PostgreSQL**:
```json
{
  "DatabaseProvider": "Sqlite",
  "ConnectionStrings": {
    "Sqlite": "Data Source=smartroom.db",
    "PostgreSql": "Host=localhost;Port=5432;Database=smartroom_db;Username=postgres;Password=postgres"
  }
}
```

### 4. Build & Jalankan Aplikasi
```bash
# 1. Kompilasi bundle frontend (opsional jika sudah ter-build di wwwroot)
cd frontend
npm install
npm run build
cd ..

# 2. Jalankan backend server .NET 8
dotnet run --urls "http://127.0.0.1:5100"
```

Akses aplikasi pada peramban web:
- **Aplikasi Web SmartRoom:** `http://localhost:5100`
- **Interactive Swagger OpenAPI:** `http://localhost:5100/swagger`

---

## 🔑 Kredensial Akun Uji Coba (Demo Quick Login)

Aplikasi telah dilengkapi fitur **1-Click Demo Login** pada halaman masuk untuk kemudahan evaluasi:

| Peran (Role) | Username | Password | Hak Akses Utama |
| :--- | :--- | :--- | :--- |
| **Administrator Fasilitas** | `admin` | `password123` | Matriks okupansi, antrean approval, kelola ruangan, log audit |
| **Mahasiswa / Civitas** | `student` | `password123` | Katalog ruangan, pengajuan peminjaman, tiket E-Pass akses |
| **Staff / Dosen** | `staff` | `password123` | Akses peminjaman prioritas kegiatan akademik |

---

## 🧪 Pengujian Otomatis (E2E Smoke Testing)

Sistem telah diverifikasi menggunakan suite pengujian otomatis headless browser via Puppeteer:
```bash
SMARTROOM_BASE_URL="http://127.0.0.1:5100" node frontend/test-redesign-smoke.js
```
**Cakupan Pengujian:**
- [x] Login authentication & credential rejection
- [x] Role-Based Route Guard (Mahasiswa diblokir dari `/admin/*`)
- [x] Zero-clipping responsive viewport audit (Mobile 390x844 s/d Desktop 1440x900)
- [x] Interaksi pembukaan slide-over booking drawer & seleksi ruangan via keyboard
- [x] Dynamic hours timeline ruler rendering dari jam buka hingga jam tutup

---

## 📄 Lisensi

Proyek ini dirilis di bawah lisensi [MIT License](LICENSE). Bebas digunakan untuk keperluan pembelajaran, portofolio, dan pengembangan lebih lanjut.

---

<p align="center">
  Dirancang dan dikembangkan dengan ❤️ oleh <strong>Reiky Aryanando Pratama</strong>
</p>
