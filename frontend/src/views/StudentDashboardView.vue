<script setup lang="ts">
import { ref, onMounted, onUnmounted, computed } from 'vue';
import { api } from '../api/client';
import type { Booking, Room } from '../types';
import { BookingStatus } from '../types';
import { currentUser, addToast } from '../stores/auth';
import {
  CalendarPlus,
  Hourglass,
  CheckCircle2,
  XCircle,
  Users,
  MapPin,
  Clock,
  Copy,
  Check,
  Calendar,
  ArrowUpRight,
  Search,
  Filter,
  QrCode,
  Printer,
  X,
  FileCheck2,
  Sparkles,
  SlidersHorizontal,
  ShieldCheck
} from 'lucide-vue-next';

const emit = defineEmits<{
  (e: 'open-booking', roomId?: number): void;
}>();

const myBookings = ref<Booking[]>([]);
const rooms = ref<Room[]>([]);
const todayBookings = ref<Booking[]>([]);
const isLoading = ref(true);
const copiedRef = ref<string | null>(null);

// Filters
const selectedCategory = ref<string>('all');
const selectedCapacity = ref<'all' | 'small' | 'medium' | 'large'>('all');
const searchQuery = ref('');
const myBookingsFilter = ref<'all' | 'pending' | 'approved' | 'rejected'>('all');

// E-Pass modal
const selectedPassBooking = ref<Booking | null>(null);

// Curated Architectural Room Helpers
function getRoomImage(roomCode: string): string {
  switch (roomCode) {
    case 'AULA-MINI-01':
      return 'https://images.unsplash.com/photo-1517502884422-41eaead166d4?auto=format&fit=crop&w=800&q=80';
    case 'LAB-KOMP-A':
      return 'https://images.unsplash.com/photo-1581091226825-a6a2a5aee158?auto=format&fit=crop&w=800&q=80';
    case 'R-RAPAT-DEK':
      return 'https://images.unsplash.com/photo-1497366216548-37526070297c?auto=format&fit=crop&w=800&q=80';
    case 'R-SIDANG-01':
    default:
      return 'https://images.unsplash.com/photo-1541829070764-84a7d30dd3f3?auto=format&fit=crop&w=800&q=80';
  }
}

function getFacilities(facilitiesStr?: string): string[] {
  if (!facilitiesStr) return ['Proyektor', 'AC Central', 'Whiteboard'];
  return facilitiesStr.split(',').map(s => s.trim()).filter(Boolean).slice(0, 3);
}

function getCategory(roomCode: string): string {
  if (roomCode.includes('AULA') || roomCode.includes('MINI')) return 'auditorium';
  if (roomCode.includes('LAB')) return 'lab';
  return 'meeting';
}

onMounted(async () => {
  await loadData();
  window.addEventListener('booking-created', loadData);
});

onUnmounted(() => {
  window.removeEventListener('booking-created', loadData);
});

async function loadData() {
  isLoading.value = true;
  try {
    const userEmail = currentUser.value?.email || 'student@smartroom.ac.id';
    const todayStr = new Date().toISOString().split('T')[0];
    const [bookingsData, roomsData, todayBookingsData] = await Promise.all([
      api.getBookings({ borrowerEmail: userEmail }),
      api.getRooms(true),
      api.getBookings({ date: todayStr }),
    ]);
    myBookings.value = bookingsData;
    rooms.value = roomsData;
    todayBookings.value = todayBookingsData;
  } catch (e: any) {
    addToast('Gagal memuat data mahasiswa: ' + e.message, 'error');
  } finally {
    isLoading.value = false;
  }
}

// Live real-time occupancy badge for each room
function getRoomLiveStatus(roomId: number) {
  const roomBookings = todayBookings.value.filter(
    b => b.roomId === roomId && (b.status === BookingStatus.Approved || b.status === BookingStatus.Pending)
  );
  if (!roomBookings.length) {
    return { type: 'free', label: 'Bebas Seharian', tone: 'text-[#214C38] bg-[#E4EEE8] border-[#C3D9CD]' };
  }
  
  const now = new Date();
  const currentMinutes = now.getHours() * 60 + now.getMinutes();

  for (const b of roomBookings) {
    const start = new Date(b.startTime);
    const end = new Date(b.endTime);
    const startMin = start.getHours() * 60 + start.getMinutes();
    const endMin = end.getHours() * 60 + end.getMinutes();

    if (currentMinutes >= startMin && currentMinutes < endMin) {
      return { 
        type: 'occupied', 
        label: `Sedang Digunakan (${formatTime(b.startTime)}–${formatTime(b.endTime)})`,
        tone: 'text-[#A43D3D] bg-[#F7E8E6] border-[#E9C3C1]' 
      };
    }
  }

  // Next booking today
  const upcoming = roomBookings
    .map(b => ({ booking: b, start: new Date(b.startTime) }))
    .filter(item => (item.start.getHours() * 60 + item.start.getMinutes()) > currentMinutes)
    .sort((a, b) => a.start.getTime() - b.start.getTime())[0];

  if (upcoming) {
    return {
      type: 'upcoming',
      label: `Terjadwal ${formatTime(upcoming.booking.startTime)}–${formatTime(upcoming.booking.endTime)}`,
      tone: 'text-[#996515] bg-[#F8EFD7] border-[#E8D6AC]'
    };
  }

  return { type: 'free', label: 'Siap Digunakan', tone: 'text-[#214C38] bg-[#E4EEE8] border-[#C3D9CD]' };
}

// Filtered rooms
const filteredRooms = computed(() => {
  return rooms.value.filter(r => {
    const cat = getCategory(r.roomCode);
    const matchesCat = selectedCategory.value === 'all' || cat === selectedCategory.value;
    const matchesSearch = !searchQuery.value ||
      r.name.toLowerCase().includes(searchQuery.value.toLowerCase()) ||
      r.roomCode.toLowerCase().includes(searchQuery.value.toLowerCase()) ||
      r.building.toLowerCase().includes(searchQuery.value.toLowerCase()) ||
      (r.facilities && r.facilities.toLowerCase().includes(searchQuery.value.toLowerCase()));

    let matchesCapacity = true;
    if (selectedCapacity.value === 'small') matchesCapacity = r.capacity <= 25;
    else if (selectedCapacity.value === 'medium') matchesCapacity = r.capacity > 25 && r.capacity <= 60;
    else if (selectedCapacity.value === 'large') matchesCapacity = r.capacity > 60;

    return matchesCat && matchesSearch && matchesCapacity;
  });
});

// Personal KPI counts
const pendingCount = computed(() => myBookings.value.filter(b => b.status === BookingStatus.Pending).length);
const approvedCount = computed(() => myBookings.value.filter(b => b.status === BookingStatus.Approved).length);
const rejectedCount = computed(() => myBookings.value.filter(b => b.status === BookingStatus.Rejected).length);

// Filtered personal bookings
const filteredMyBookings = computed(() => {
  if (myBookingsFilter.value === 'pending') {
    return myBookings.value.filter(b => b.status === BookingStatus.Pending);
  }
  if (myBookingsFilter.value === 'approved') {
    return myBookings.value.filter(b => b.status === BookingStatus.Approved);
  }
  if (myBookingsFilter.value === 'rejected') {
    return myBookings.value.filter(b => b.status === BookingStatus.Rejected);
  }
  return myBookings.value;
});

function copyRef(refCode: string) {
  navigator.clipboard.writeText(refCode);
  copiedRef.value = refCode;
  addToast(`Nomor referensi ${refCode} disalin!`, 'info');
  setTimeout(() => {
    if (copiedRef.value === refCode) copiedRef.value = null;
  }, 2000);
}

function formatDate(iso: string) {
  return new Intl.DateTimeFormat('id-ID', {
    day: '2-digit',
    month: 'short',
    year: 'numeric'
  }).format(new Date(iso));
}

function formatTime(iso: string) {
  return new Intl.DateTimeFormat('id-ID', {
    hour: '2-digit',
    minute: '2-digit',
    hour12: false
  }).format(new Date(iso));
}

function openPass(booking: Booking) {
  selectedPassBooking.value = booking;
}

function closePass() {
  selectedPassBooking.value = null;
}

function printPass() {
  window.print();
}
</script>

<template>
  <div class="space-y-10 lg:space-y-14">

    <!-- Hero Banner (Quiet Campus Atelier Banner) -->
    <div class="page-hero p-6 sm:p-9 lg:p-11">
      <div class="grid grid-cols-1 lg:grid-cols-12 gap-8 items-end">
        <div class="lg:col-span-8 space-y-3.5">
          <div class="page-kicker">
            <span>Portal civitas</span>
            <span class="text-[#596159] font-medium">{{ currentUser?.department || 'Badan Eksekutif Mahasiswa (BEM) TI' }}</span>
          </div>
          <h1 class="max-w-[16ch] text-4xl font-medium leading-[0.98] text-[#182019] sm:text-5xl lg:text-6xl">
            Ruang untuk agenda berikutnya
          </h1>
          <p class="max-w-2xl text-sm leading-6 text-[#5F675F] sm:text-base">
            Periksa ketersediaan real-time setiap ruangan, bandingkan kapasitas dan fasilitas, lalu ajukan peminjaman dengan jaminan anti-bentrok jadwal.
          </p>
        </div>

        <div class="lg:col-span-4 flex flex-col sm:flex-row lg:flex-col items-start lg:items-end justify-end gap-3">
          <button
            type="button"
            id="student-dashboard-booking-btn"
            @click="emit('open-booking')"
            class="halden-btn-primary relative z-10 inline-flex items-center gap-2 px-6 text-sm active:scale-[0.98] transition-transform"
          >
            <CalendarPlus class="w-4 h-4 text-[#FAF8F5]" />
            <span>Ajukan Peminjaman</span>
          </button>

          <div class="relative z-10 flex items-center gap-2 rounded-xl bg-white px-3.5 py-2 text-xs text-[#5F675F] border border-[#D9D8D1]/70 shadow-xs">
            <span class="w-2 h-2 rounded-full bg-[#214C38] animate-pulse"></span>
            <span>Sesi aktif &bull; {{ currentUser?.fullName }}</span>
          </div>
        </div>
      </div>
    </div>

    <!-- ROOM CATALOG SECTION -->
    <div class="space-y-6">

      <!-- Catalog Controls: Title & Category Tabs -->
      <div class="flex flex-col justify-between gap-4 md:flex-row md:items-end">
        <div>
          <h2 class="text-3xl font-medium text-[#182019] sm:text-4xl">
            Katalog Ruangan Kampus
          </h2>
          <p class="text-xs text-[#6E766F] mt-1">
            Pilih ruangan untuk memeriksa jadwal operasional atau langsung melakukan peminjaman.
          </p>
        </div>

        <!-- Filter Category Tabs -->
        <div class="flex max-w-full items-center gap-1 overflow-x-auto rounded-xl bg-[#E8E5DC] p-1 text-xs">
          <button
            type="button"
            @click="selectedCategory = 'all'"
            class="min-h-10 whitespace-nowrap rounded-lg px-3.5 font-semibold transition-all active:scale-[0.98]"
            :class="selectedCategory === 'all' ? 'bg-white text-[#214C38] shadow-xs' : 'text-[#5F675F] hover:text-[#182019]'"
          >
            Semua ({{ rooms.length }})
          </button>
          <button
            type="button"
            @click="selectedCategory = 'auditorium'"
            class="min-h-10 whitespace-nowrap rounded-lg px-3.5 font-semibold transition-all active:scale-[0.98]"
            :class="selectedCategory === 'auditorium' ? 'bg-white text-[#214C38] shadow-xs' : 'text-[#5F675F] hover:text-[#182019]'"
          >
            Auditorium
          </button>
          <button
            type="button"
            @click="selectedCategory = 'meeting'"
            class="min-h-10 whitespace-nowrap rounded-lg px-3.5 font-semibold transition-all active:scale-[0.98]"
            :class="selectedCategory === 'meeting' ? 'bg-white text-[#214C38] shadow-xs' : 'text-[#5F675F] hover:text-[#182019]'"
          >
            Ruang Rapat
          </button>
          <button
            type="button"
            @click="selectedCategory = 'lab'"
            class="min-h-10 whitespace-nowrap rounded-lg px-3.5 font-semibold transition-all active:scale-[0.98]"
            :class="selectedCategory === 'lab' ? 'bg-white text-[#214C38] shadow-xs' : 'text-[#5F675F] hover:text-[#182019]'"
          >
            Laboratorium
          </button>
        </div>
      </div>

      <!-- Live Search & Capacity Filter Bar -->
      <div class="grid grid-cols-1 sm:grid-cols-12 gap-3 p-3 rounded-2xl bg-[#FBFAF7] border border-[#D9D8D1]/80">
        <!-- Search Input -->
        <div class="sm:col-span-7 relative">
          <Search class="w-4 h-4 text-[#8A908A] absolute left-3.5 top-1/2 -translate-y-1/2" />
          <input
            type="text"
            v-model="searchQuery"
            placeholder="Cari nama ruangan, gedung, atau fasilitas..."
            class="w-full pl-10 pr-4 py-2 text-xs rounded-xl bg-white border border-[#D9D8D1] text-[#182019] placeholder:text-[#8A908A] focus:outline-none focus:border-[#214C38] focus:ring-2 focus:ring-[#214C38]/20 transition-all"
          />
        </div>

        <!-- Capacity Chips Filter -->
        <div class="sm:col-span-5 flex items-center justify-start sm:justify-end gap-1.5 overflow-x-auto text-[11px] font-mono">
          <span class="text-[#6E766F] text-[10px] uppercase font-bold mr-1 hidden lg:inline">Kapasitas:</span>
          <button
            type="button"
            @click="selectedCapacity = 'all'"
            class="px-2.5 py-1.5 rounded-lg border transition-all"
            :class="selectedCapacity === 'all' ? 'bg-[#214C38] text-white border-[#214C38] font-bold' : 'bg-white text-[#5F675F] border-[#D9D8D1] hover:border-[#182019]'"
          >
            Semua
          </button>
          <button
            type="button"
            @click="selectedCapacity = 'small'"
            class="px-2.5 py-1.5 rounded-lg border transition-all"
            :class="selectedCapacity === 'small' ? 'bg-[#214C38] text-white border-[#214C38] font-bold' : 'bg-white text-[#5F675F] border-[#D9D8D1] hover:border-[#182019]'"
          >
            &le; 25
          </button>
          <button
            type="button"
            @click="selectedCapacity = 'medium'"
            class="px-2.5 py-1.5 rounded-lg border transition-all"
            :class="selectedCapacity === 'medium' ? 'bg-[#214C38] text-white border-[#214C38] font-bold' : 'bg-white text-[#5F675F] border-[#D9D8D1] hover:border-[#182019]'"
          >
            26&ndash;50
          </button>
          <button
            type="button"
            @click="selectedCapacity = 'large'"
            class="px-2.5 py-1.5 rounded-lg border transition-all"
            :class="selectedCapacity === 'large' ? 'bg-[#214C38] text-white border-[#214C38] font-bold' : 'bg-white text-[#5F675F] border-[#D9D8D1] hover:border-[#182019]'"
          >
            &gt; 50
          </button>
        </div>
      </div>

      <!-- Room Cards Grid -->
      <div v-if="isLoading" class="grid grid-cols-1 md:grid-cols-2 gap-6">
        <div v-for="i in 4" :key="i" class="h-80 rounded-2xl bg-white border border-[#D9D8D1] animate-pulse"></div>
      </div>

      <div v-else-if="filteredRooms.length === 0" class="p-12 text-center rounded-2xl bg-white border border-[#D9D8D1]">
        <Filter class="w-8 h-8 text-[#8A908A] mx-auto mb-2" />
        <p class="font-serif text-lg text-[#182019]">Tidak ada ruangan yang cocok</p>
        <p class="text-xs text-[#6E766F] mt-1">Coba sesuaikan kata kunci pencarian atau ubah filter kapasitas.</p>
      </div>

      <div v-else class="grid grid-cols-1 md:grid-cols-2 gap-6">
        <div
          v-for="r in filteredRooms"
          :key="r.id"
          class="surface-card group flex flex-col justify-between overflow-hidden border border-[#D9D8D1] hover:border-[#214C38] transition-all duration-300"
        >
          <!-- Top Media Showcase -->
          <div class="relative h-64 w-full overflow-hidden bg-[#F1EFE9] lg:h-72">
            <img
              :src="getRoomImage(r.roomCode)"
              :alt="r.name"
              loading="lazy"
              class="h-full w-full object-cover transition-transform duration-500 group-hover:scale-[1.035]"
            />

            <!-- Top Badges Overlay -->
            <div class="absolute top-3 inset-x-3 flex items-center justify-between">
              <span class="px-2.5 py-1 rounded-lg bg-[#182019]/85 backdrop-blur-sm text-[#FAF8F5] font-mono text-[11px] font-semibold">
                {{ r.roomCode }}
              </span>
              <span class="px-2.5 py-1 rounded-lg bg-white/95 backdrop-blur-sm text-[#182019] font-mono text-[11px] font-bold border border-[#D9D8D1] shadow-xs flex items-center gap-1.5">
                <Users class="w-3.5 h-3.5 text-[#214C38]" />
                <span>{{ r.capacity }} Kursi</span>
              </span>
            </div>

            <!-- Hover Action Quick Pill -->
            <div class="absolute inset-0 bg-[#182019]/40 opacity-0 group-hover:opacity-100 transition-opacity duration-200 flex items-center justify-center p-4">
              <button
                type="button"
                @click="emit('open-booking', r.id)"
                class="flex min-h-11 scale-95 items-center gap-2 rounded-xl bg-white px-5 text-xs font-semibold text-[#182019] shadow-lg transition-all duration-200 group-hover:scale-100 hover:bg-[#FBFAF7] active:scale-95"
              >
                <CalendarPlus class="w-4 h-4 text-[#214C38]" />
                <span>Pilih &amp; Reservasi Ruangan Ini</span>
              </button>
            </div>
          </div>

          <!-- Bottom Card Body -->
          <div class="p-5 space-y-3.5 bg-white border-t border-[#D9D8D1] flex-1 flex flex-col justify-between">
            <div class="space-y-1.5">
              <div class="flex items-baseline justify-between gap-2">
                <h3 class="font-serif text-xl font-bold text-[#182019] leading-snug group-hover:text-[#214C38] transition-colors">
                  {{ r.name }}
                </h3>
              </div>

              <div class="text-xs text-[#5F675F] flex items-center gap-1.5 font-sans">
                <MapPin class="w-3.5 h-3.5 text-[#8A908A] flex-shrink-0" />
                <span class="truncate">{{ r.building }}</span>
              </div>
            </div>

            <!-- Facilities tags -->
            <div class="pt-2 border-t border-[#D9D8D1]/70 flex flex-wrap gap-1.5">
              <span
                v-for="(f, fi) in getFacilities(r.facilities)"
                :key="fi"
                class="px-2 py-0.5 rounded-md text-[10px] font-mono text-[#5F675F] bg-[#FBFAF7] border border-[#D9D8D1]"
              >
                {{ f }}
              </span>
            </div>

            <!-- Action & Live Availability Bar -->
            <div class="pt-3 border-t border-[#D9D8D1] flex items-center justify-between">
              <!-- Real-time Status Badge -->
              <span
                class="text-[11px] font-mono px-2.5 py-1 rounded-lg border font-semibold flex items-center gap-1.5"
                :class="getRoomLiveStatus(r.id).tone"
              >
                <span class="w-1.5 h-1.5 rounded-full" :class="getRoomLiveStatus(r.id).type === 'free' ? 'bg-[#214C38]' : getRoomLiveStatus(r.id).type === 'occupied' ? 'bg-[#A43D3D] animate-ping' : 'bg-[#996515]'"></span>
                <span>{{ getRoomLiveStatus(r.id).label }}</span>
              </span>

              <button
                type="button"
                @click="emit('open-booking', r.id)"
                class="inline-flex items-center gap-1 text-xs font-semibold text-[#182019] hover:text-[#214C38] transition-colors cursor-pointer active:scale-95"
              >
                <span>Ajukan Peminjaman</span>
                <ArrowUpRight class="w-3.5 h-3.5" />
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- 3 PERSONAL KPI STATUS BENTO CARDS (INTERACTIVE FILTER) -->
    <div class="grid grid-cols-1 sm:grid-cols-3 gap-5 pt-2">

      <!-- Pending Card -->
      <button
        type="button"
        @click="myBookingsFilter = myBookingsFilter === 'pending' ? 'all' : 'pending'"
        class="metric-card flex flex-col justify-between p-6 text-left transition-all active:scale-[0.99] cursor-pointer"
        :class="myBookingsFilter === 'pending' ? 'ring-2 ring-[#996515] border-[#996515]' : ''"
      >
        <div class="flex items-center justify-between text-[#78716C] mb-2">
          <span class="status-warning rounded-lg border px-2.5 py-1 text-xs font-bold">Menunggu review</span>
          <div class="w-8 h-8 rounded-lg bg-[#FEF3C7] text-[#92400E] flex items-center justify-center border border-[#FDE68A]">
            <Hourglass class="w-4 h-4" />
          </div>
        </div>
        <div class="flex items-baseline gap-2">
          <span class="text-4xl font-serif font-bold text-[#182019] tracking-tight">{{ pendingCount }}</span>
          <span class="text-xs text-[#6E766F] font-mono">Pengajuan Aktif</span>
        </div>
        <div class="mt-4 pt-3 border-t border-[#D9D8D1] text-[11px] text-[#5F675F] font-sans flex items-center justify-between">
          <span>Sedang diverifikasi oleh Tim Fasilitas</span>
          <span class="font-mono text-[10px] text-[#996515] font-semibold">{{ myBookingsFilter === 'pending' ? 'Filter Aktif' : 'Klik Filter' }}</span>
        </div>
      </button>

      <!-- Approved Card -->
      <button
        type="button"
        @click="myBookingsFilter = myBookingsFilter === 'approved' ? 'all' : 'approved'"
        class="metric-card flex flex-col justify-between p-6 text-left transition-all active:scale-[0.99] cursor-pointer"
        :class="myBookingsFilter === 'approved' ? 'ring-2 ring-[#214C38] border-[#214C38]' : ''"
      >
        <div class="flex items-center justify-between text-[#78716C] mb-2">
          <span class="status-success rounded-lg border px-2.5 py-1 text-xs font-bold">Disetujui &bull; siap pakai</span>
          <div class="w-8 h-8 rounded-lg bg-[#E4EEE8] text-[#214C38] flex items-center justify-center border border-[#C3D9CD]">
            <CheckCircle2 class="w-4 h-4" />
          </div>
        </div>
        <div class="flex items-baseline gap-2">
          <span class="text-4xl font-serif font-bold text-[#182019] tracking-tight">{{ approvedCount }}</span>
          <span class="text-xs text-[#6E766F] font-mono">Terkonfirmasi</span>
        </div>
        <div class="mt-4 pt-3 border-t border-[#D9D8D1] text-[11px] text-[#5F675F] font-sans flex items-center justify-between">
          <span>Jadwal peminjaman sah &bull; E-Pass aktif</span>
          <span class="font-mono text-[10px] text-[#214C38] font-semibold">{{ myBookingsFilter === 'approved' ? 'Filter Aktif' : 'Klik Filter' }}</span>
        </div>
      </button>

      <!-- Rejected Card -->
      <button
        type="button"
        @click="myBookingsFilter = myBookingsFilter === 'rejected' ? 'all' : 'rejected'"
        class="metric-card flex flex-col justify-between p-6 text-left transition-all active:scale-[0.99] cursor-pointer"
        :class="myBookingsFilter === 'rejected' ? 'ring-2 ring-[#A43D3D] border-[#A43D3D]' : ''"
      >
        <div class="flex items-center justify-between text-[#78716C] mb-2">
          <span class="status-danger rounded-lg border px-2.5 py-1 text-xs font-bold">Permohonan ditolak</span>
          <div class="w-8 h-8 rounded-lg bg-[#F7E8E6] text-[#A43D3D] flex items-center justify-center border border-[#E9C3C1]">
            <XCircle class="w-4 h-4" />
          </div>
        </div>
        <div class="flex items-baseline gap-2">
          <span class="text-4xl font-serif font-bold text-[#182019] tracking-tight">{{ rejectedCount }}</span>
          <span class="text-xs text-[#6E766F] font-mono">Catatan Disertakan</span>
        </div>
        <div class="mt-4 pt-3 border-t border-[#D9D8D1] text-[11px] text-[#5F675F] font-sans flex items-center justify-between">
          <span>Periksa alasan resmi dari admin fasilitas</span>
          <span class="font-mono text-[10px] text-[#A43D3D] font-semibold">{{ myBookingsFilter === 'rejected' ? 'Filter Aktif' : 'Klik Filter' }}</span>
        </div>
      </button>
    </div>

    <!-- MY BOOKINGS LIST SECTION (ANCHOR FOR NAVBAR LINK) -->
    <div id="my-bookings-section" class="space-y-4 pt-4 scroll-mt-24">
      <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-3 pb-3 border-b border-[#D9D8D1]">
        <div>
          <h2 class="font-serif text-2xl font-bold text-[#182019] tracking-tight">
            Daftar Peminjaman Saya ({{ filteredMyBookings.length }})
          </h2>
          <p class="text-xs text-[#6E766F] mt-0.5">
            Status pengajuan ruangan Anda yang terhubung langsung dengan sistem verifikasi admin.
          </p>
        </div>

        <!-- Filter Sub-tabs -->
        <div class="flex items-center gap-1.5 text-xs font-mono">
          <button
            type="button"
            @click="myBookingsFilter = 'all'"
            class="px-2.5 py-1 rounded-lg border transition-all"
            :class="myBookingsFilter === 'all' ? 'bg-[#182019] text-white border-[#182019] font-bold' : 'bg-white text-[#5F675F] border-[#D9D8D1] hover:border-[#182019]'"
          >
            Semua ({{ myBookings.length }})
          </button>
          <button
            type="button"
            @click="myBookingsFilter = 'pending'"
            class="px-2.5 py-1 rounded-lg border transition-all"
            :class="myBookingsFilter === 'pending' ? 'bg-[#996515] text-white border-[#996515] font-bold' : 'bg-white text-[#5F675F] border-[#D9D8D1] hover:border-[#182019]'"
          >
            Pending ({{ pendingCount }})
          </button>
          <button
            type="button"
            @click="myBookingsFilter = 'approved'"
            class="px-2.5 py-1 rounded-lg border transition-all"
            :class="myBookingsFilter === 'approved' ? 'bg-[#214C38] text-white border-[#214C38] font-bold' : 'bg-white text-[#5F675F] border-[#D9D8D1] hover:border-[#182019]'"
          >
            Approved ({{ approvedCount }})
          </button>
          <button
            type="button"
            @click="myBookingsFilter = 'rejected'"
            class="px-2.5 py-1 rounded-lg border transition-all"
            :class="myBookingsFilter === 'rejected' ? 'bg-[#A43D3D] text-white border-[#A43D3D] font-bold' : 'bg-white text-[#5F675F] border-[#D9D8D1] hover:border-[#182019]'"
          >
            Rejected ({{ rejectedCount }})
          </button>
        </div>
      </div>

      <!-- Empty State -->
      <div v-if="!isLoading && filteredMyBookings.length === 0" class="text-center py-12 rounded-2xl bg-white border border-[#D9D8D1] p-8 shadow-xs">
        <Calendar class="w-10 h-10 text-[#8A908A] mx-auto mb-3" />
        <h3 class="text-base font-serif font-bold text-[#182019]">Belum Ada Pengajuan Peminjaman</h3>
        <p class="text-xs text-[#5F675F] mt-1 max-w-sm mx-auto">
          Tidak ada data peminjaman untuk filter ini. Silakan pilih salah satu ruangan di katalog atas untuk mengajukan reservasi.
        </p>
        <button
          @click="emit('open-booking')"
          class="mt-4 inline-flex items-center gap-2 px-5 py-2.5 rounded-xl halden-btn-primary text-xs font-semibold cursor-pointer shadow-xs transition-all active:scale-95"
        >
          <CalendarPlus class="w-3.5 h-3.5 text-[#FAF8F5]" />
          <span>Ajukan Sekarang</span>
        </button>
      </div>

      <!-- Bookings List Cards -->
      <div v-else class="space-y-3.5">
        <div
          v-for="b in filteredMyBookings"
          :key="b.id"
          class="p-5 rounded-2xl bg-white border border-[#D9D8D1] hover:border-[#214C38] shadow-xs transition-all space-y-4"
        >
          <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-3">

            <!-- Reference & Room -->
            <div class="space-y-1">
              <div class="flex items-center gap-2">
                <span class="font-mono text-xs font-bold bg-[#FBFAF7] px-2 py-0.5 rounded border border-[#D9D8D1] text-[#182019]">
                  {{ b.bookingReference }}
                </span>
                <button
                  type="button"
                  @click="copyRef(b.bookingReference)"
                  title="Salin Nomor Referensi"
                  class="text-[#8A908A] hover:text-[#182019] transition-colors cursor-pointer"
                >
                  <Check v-if="copiedRef === b.bookingReference" class="w-3.5 h-3.5 text-[#214C38]" />
                  <Copy v-else class="w-3.5 h-3.5" />
                </button>
                <span class="text-[10px] text-[#6E766F] font-mono">
                  Diajukan: {{ formatDate(b.createdAt) }}
                </span>
              </div>
              <h3 class="font-serif text-xl font-bold text-[#182019]">
                {{ b.room?.name }}
              </h3>
              <div class="text-xs text-[#5F675F] flex items-center gap-1.5 font-sans">
                <MapPin class="w-3.5 h-3.5 text-[#8A908A]" />
                <span>{{ b.room?.building }} (Kode: {{ b.room?.roomCode }})</span>
              </div>
            </div>

            <!-- Status Pill & E-Pass Trigger -->
            <div class="flex items-center gap-2 self-start sm:self-center">
              <span
                v-if="b.status === BookingStatus.Pending"
                class="inline-flex items-center gap-1.5 px-3 py-1 rounded-full text-xs font-mono font-semibold status-warning border"
              >
                <Hourglass class="w-3.5 h-3.5" />
                <span>Menunggu Review</span>
              </span>

              <span
                v-else-if="b.status === BookingStatus.Approved"
                class="inline-flex items-center gap-1.5 px-3 py-1 rounded-full text-xs font-mono font-semibold status-success border"
              >
                <CheckCircle2 class="w-3.5 h-3.5" />
                <span>Disetujui Siap Pakai</span>
              </span>

              <span
                v-else-if="b.status === BookingStatus.Rejected"
                class="inline-flex items-center gap-1.5 px-3 py-1 rounded-full text-xs font-mono font-semibold status-danger border"
              >
                <XCircle class="w-3.5 h-3.5" />
                <span>Pengajuan Ditolak</span>
              </span>

              <!-- Digital E-Pass Button (Visible for Approved) -->
              <button
                v-if="b.status === BookingStatus.Approved"
                type="button"
                @click="openPass(b)"
                class="inline-flex items-center gap-1.5 px-3 py-1 rounded-lg bg-[#214C38] text-white hover:bg-[#173D2C] text-xs font-mono font-semibold transition-all active:scale-95 shadow-xs"
              >
                <QrCode class="w-3.5 h-3.5" />
                <span>E-Pass Akses</span>
              </button>
            </div>
          </div>

          <!-- Schedule & Purpose Details -->
          <div class="grid grid-cols-1 sm:grid-cols-2 gap-3 pt-3 border-t border-[#D9D8D1]/80 text-xs">
            <div class="space-y-1">
              <span class="text-[10px] font-mono uppercase tracking-wider text-[#6E766F] font-semibold">Jadwal Sesi</span>
              <div class="font-mono text-[#182019] font-semibold flex items-center gap-1.5">
                <Calendar class="w-3.5 h-3.5 text-[#8A908A]" />
                <span>{{ formatDate(b.startTime) }}</span>
                <span>&bull;</span>
                <span>{{ formatTime(b.startTime) }} &ndash; {{ formatTime(b.endTime) }} WIB</span>
              </div>
            </div>

            <div class="space-y-1">
              <span class="text-[10px] font-mono uppercase tracking-wider text-[#6E766F] font-semibold">Agenda Kegiatan</span>
              <div class="text-[#5F675F] font-medium leading-relaxed">
                {{ b.purpose }}
              </div>
            </div>
          </div>

          <!-- Rejection Admin Notes Alert -->
          <div
            v-if="b.status === BookingStatus.Rejected && b.adminNotes"
            class="p-3.5 rounded-xl status-danger border text-xs space-y-1"
          >
            <div class="font-mono font-bold text-[10px] uppercase tracking-wider flex items-center gap-1.5 text-[#A43D3D]">
              <XCircle class="w-3.5 h-3.5" />
              <span>Catatan Resmi Penolakan dari Admin Fasilitas:</span>
            </div>
            <div class="text-[11px] font-sans leading-relaxed text-[#5F675F] pl-5">
              "{{ b.adminNotes }}"
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- DIGITAL E-PASS / TICKET MODAL (SUPER POWERFUL PORTFOLIO FEATURE) -->
    <Teleport to="body">
      <div
        v-if="selectedPassBooking"
        class="fixed inset-0 z-50 overflow-y-auto bg-[#182019]/45 backdrop-blur-sm flex items-center justify-center p-4"
        role="dialog"
        aria-modal="true"
      >
        <div class="relative w-full max-w-md rounded-[26px] bg-[#FBFAF7] border border-[#D9D8D1] shadow-2xl p-6 sm:p-8 space-y-6 text-[#182019]">

          <!-- Header -->
          <div class="flex items-center justify-between border-b border-[#D9D8D1] pb-4">
            <div class="flex items-center gap-2.5">
              <div class="w-9 h-9 rounded-xl bg-[#214C38] text-white flex items-center justify-center">
                <ShieldCheck class="w-5 h-5" />
              </div>
              <div>
                <p class="text-[10px] font-mono uppercase tracking-widest text-[#6E766F] font-bold">SmartRoom Security</p>
                <h3 class="font-serif text-lg font-bold text-[#182019]">E-Pass Izin Penggunaan Ruang</h3>
              </div>
            </div>
            <button
              type="button"
              @click="closePass"
              class="w-8 h-8 rounded-lg bg-white border border-[#D9D8D1] hover:bg-[#F1EFE9] flex items-center justify-center text-[#5F675F] transition-colors"
            >
              <X class="w-4 h-4" />
            </button>
          </div>

          <!-- Pass Body (Architectural Boarding Pass Layout) -->
          <div class="space-y-4 rounded-2xl bg-white p-5 border border-[#D9D8D1] shadow-xs">

            <!-- Reference & Status -->
            <div class="flex items-center justify-between">
              <span class="font-mono text-xs font-bold px-2.5 py-1 rounded bg-[#E4EEE8] text-[#214C38] border border-[#C3D9CD]">
                {{ selectedPassBooking.bookingReference }}
              </span>
              <span class="text-[10px] font-mono uppercase tracking-wider text-[#214C38] font-bold flex items-center gap-1">
                <span class="w-1.5 h-1.5 rounded-full bg-[#214C38]"></span>
                <span>Terverifikasi Sah</span>
              </span>
            </div>

            <!-- Room Info -->
            <div class="pt-2 border-t border-[#D9D8D1]/70">
              <span class="text-[10px] font-mono text-[#6E766F] uppercase">Fasilitas / Ruangan</span>
              <div class="font-serif text-xl font-bold text-[#182019] mt-0.5">
                {{ selectedPassBooking.room?.name }}
              </div>
              <div class="text-xs text-[#5F675F] font-mono mt-0.5">
                {{ selectedPassBooking.room?.building }} &bull; Kode: {{ selectedPassBooking.room?.roomCode }}
              </div>
            </div>

            <!-- Schedule -->
            <div class="grid grid-cols-2 gap-3 pt-2 border-t border-[#D9D8D1]/70 text-xs">
              <div>
                <span class="text-[10px] font-mono text-[#6E766F] uppercase">Tanggal Sesi</span>
                <div class="font-mono font-bold text-[#182019] mt-0.5">
                  {{ formatDate(selectedPassBooking.startTime) }}
                </div>
              </div>
              <div>
                <span class="text-[10px] font-mono text-[#6E766F] uppercase">Jam Operasional</span>
                <div class="font-mono font-bold text-[#214C38] mt-0.5">
                  {{ formatTime(selectedPassBooking.startTime) }} &ndash; {{ formatTime(selectedPassBooking.endTime) }} WIB
                </div>
              </div>
            </div>

            <!-- Borrower Info -->
            <div class="pt-2 border-t border-[#D9D8D1]/70 text-xs">
              <span class="text-[10px] font-mono text-[#6E766F] uppercase">Penanggung Jawab</span>
              <div class="font-bold text-[#182019] mt-0.5">
                {{ selectedPassBooking.borrowerName }}
              </div>
              <div class="text-[11px] text-[#5F675F] font-mono">
                {{ selectedPassBooking.borrowerEmail }}
              </div>
            </div>

            <!-- Mockup QR Code & Security Stamp -->
            <div class="pt-3 border-t border-dashed border-[#D9D8D1] flex items-center justify-between">
              <div class="space-y-1">
                <div class="text-[9px] font-mono text-[#8A908A]">TOKEN VERIFIKASI QR</div>
                <div class="text-[10px] font-mono font-bold text-[#182019]">
                  SR-SEC-{{ selectedPassBooking.id }}-{{ new Date(selectedPassBooking.startTime).getTime().toString().slice(-6) }}
                </div>
                <div class="text-[9px] text-[#6E766F]">Tunjukkan bukti ini kepada satpam gedung.</div>
              </div>

              <!-- SVG Stylized QR Mockup -->
              <div class="w-16 h-16 rounded-xl bg-[#182019] p-2 flex items-center justify-center flex-shrink-0">
                <QrCode class="w-12 h-12 text-white" />
              </div>
            </div>
          </div>

          <!-- Modal Actions -->
          <div class="flex items-center justify-end gap-2.5 pt-2">
            <button
              type="button"
              @click="closePass"
              class="halden-btn-secondary px-4 text-xs"
            >
              Tutup
            </button>
            <button
              type="button"
              @click="printPass"
              class="halden-btn-primary inline-flex items-center gap-2 px-5 text-xs active:scale-95"
            >
              <Printer class="w-3.5 h-3.5" />
              <span>Cetak / Simpan PDF</span>
            </button>
          </div>
        </div>
      </div>
    </Teleport>

  </div>
</template>
