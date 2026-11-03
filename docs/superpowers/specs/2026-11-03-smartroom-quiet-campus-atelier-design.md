# SmartRoom Quiet Campus Atelier Design

## Intent

SmartRoom harus terasa seperti perangkat operasional fasilitas kampus yang tenang, presisi, dan berkelas. Antarmuka tetap cepat dipakai oleh mahasiswa dan administrator, tetapi visualnya mengambil material dari arsitektur kampus: batu kapur, kertas arsip, kayu gelap, logam bronze, dan tanaman hijau tua.

## Goals

- Menyatukan seluruh Vue SPA dalam satu sistem visual yang konsisten.
- Mempertahankan alur RBAC, quick login, peminjaman, approval, inventaris, dan audit.
- Mengurangi label kapital, nomor dekoratif, border berulang, dan penggunaan monospace berlebihan.
- Memberikan hierarchy yang jelas pada desktop dan mobile tanpa mengurangi kepadatan informasi.
- Menambahkan motion yang memberi orientasi dan feedback, bukan dekorasi.

## Non-goals

- Tidak mengubah kontrak API, routing, DTO, atau aturan overlap jadwal.
- Tidak menambah framework UI atau library animasi baru.
- Tidak mengubah data seed dan tidak membuat klaim performa baru.

## Visual system

- Canvas `#F1EFE9`, paper `#FBFAF7`, elevated white `#FFFFFF`.
- Ink `#182019`, secondary `#5F675F`, quiet text `#7A817A`.
- Forest `#214C38` untuk aksi/keberhasilan; bronze `#A45F35` untuk perhatian terpilih.
- Newsreader untuk display; Plus Jakarta Sans untuk UI; JetBrains Mono hanya untuk kode, referensi, dan jam.
- Radius memiliki fungsi: 12px kontrol, 18px kartu, 26px hero/drawer.
- Bayangan hanya untuk lapisan yang benar-benar terangkat; permukaan diam dipisahkan oleh tone dan border lembut.

## Motion

- Kontrol sering dipakai: 160ms, transform/opacity saja.
- Menu, toast, dan pergantian status: 220–280ms dengan kurva `cubic-bezier(0.22, 1, 0.36, 1)`.
- Drawer: 360ms dengan origin dari kanan; overlay 220ms.
- Tidak ada pulse atau loop perhatian.
- `prefers-reduced-motion` menghasilkan perubahan state instan tanpa menghilangkan informasi.

## Accessibility and responsive behavior

- Semua kontrol minimum 44px pada mobile dan memiliki focus ring yang terlihat.
- Navigasi desktop berubah menjadi panel mobile tanpa horizontal overflow.
- Teks normal minimal 14px desktop dan 16px pada form mobile.
- Menu, drawer, modal, filter, dan status tidak bergantung pada warna saja.
- ID yang dipakai E2E tetap dipertahankan.
