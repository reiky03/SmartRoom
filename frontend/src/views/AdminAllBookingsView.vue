<script setup lang="ts">
import { ref, onMounted, watch, computed } from 'vue';
import { api } from '../api/client';
import type { Booking, Room } from '../types';
import { BookingStatus } from '../types';
import { addToast } from '../stores/auth';
import {
  Search,
  RotateCcw,
  Copy,
  Check,
  ArrowLeft,
  FileSpreadsheet
} from 'lucide-vue-next';

const bookings = ref<Booking[]>([]);
const rooms = ref<Room[]>([]);
const isLoading = ref(true);

const filterRoomId = ref<number | ''>('');
const filterStatus = ref<number | ''>(0); // Default to Pending (0) or all
const filterDate = ref('');
const searchQuery = ref('');
const copiedRef = ref<string | null>(null);

onMounted(async () => {
  await Promise.all([loadRooms(), loadBookings()]);
});

async function loadRooms() {
  try {
    rooms.value = await api.getRooms();
  } catch (e) {
    console.error(e);
  }
}

async function loadBookings() {
  isLoading.value = true;
  try {
    const params: any = {};
    if (filterRoomId.value !== '') params.roomId = Number(filterRoomId.value);
    if (filterStatus.value !== '') params.status = Number(filterStatus.value);
    if (filterDate.value) params.date = filterDate.value;

    bookings.value = await api.getBookings(params);
  } catch (e: any) {
    addToast('Gagal memuat log peminjaman: ' + e.message, 'error');
  } finally {
    isLoading.value = false;
  }
}

watch([filterRoomId, filterStatus, filterDate], () => {
  loadBookings();
});

function resetFilter() {
  filterRoomId.value = '';
  filterStatus.value = '';
  filterDate.value = '';
  searchQuery.value = '';
  loadBookings();
}

const filteredBookings = computed(() => {
  if (!searchQuery.value) return bookings.value;
  const q = searchQuery.value.toLowerCase();
  return bookings.value.filter(b =>
    b.bookingReference.toLowerCase().includes(q) ||
    b.borrowerName.toLowerCase().includes(q) ||
    b.borrowerEmail.toLowerCase().includes(q) ||
    (b.room?.name || '').toLowerCase().includes(q) ||
    b.purpose.toLowerCase().includes(q)
  );
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
</script>

<template>
  <div class="space-y-8">

    <!-- Top Header -->
    <div class="page-hero flex flex-col justify-between gap-6 p-6 sm:p-9 md:flex-row md:items-end lg:p-10">
      <div class="space-y-2">
        <div class="page-kicker">
          <span>Riwayat peminjaman</span>
          <span class="text-[#596159] font-medium">{{ bookings.length }} sesi tercatat</span>
        </div>
        <h1 class="max-w-[17ch] text-4xl font-medium leading-[0.98] text-[#182019] sm:text-5xl lg:text-6xl">
          Riwayat setiap penggunaan ruang
        </h1>
        <p class="max-w-2xl text-sm leading-6 text-[#5F675F] sm:text-base">
          Log historis lengkap peminjaman seluruh ruangan kampus dari civitas akademika, disertai status verifikasi dan catatan resmi administrator.
        </p>
      </div>

      <router-link
        to="/admin/dashboard"
        class="halden-btn-secondary relative z-10 inline-flex items-center gap-2 self-start px-4 text-sm md:self-auto"
      >
        <ArrowLeft class="w-3.5 h-3.5" />
        <span>Kembali ke Overview</span>
      </router-link>
    </div>

    <!-- Filter Control Bar (Halden Architectural Surface) -->
    <div class="surface-card grid grid-cols-1 items-end gap-4 p-5 sm:grid-cols-2 lg:grid-cols-4">

      <!-- Room -->
      <div>
        <label for="audit-room" class="mb-2 block text-sm font-semibold text-[#3E473F]">
          Filter Ruangan
        </label>
        <select
          id="audit-room"
          name="audit-room"
          v-model="filterRoomId"
          class="halden-input w-full px-3.5 py-2.5 text-sm"
        >
          <option :value="''">&mdash; Semua Ruangan &mdash;</option>
          <option v-for="r in rooms" :key="r.id" :value="r.id">
            {{ r.roomCode }} &bull; {{ r.name }}
          </option>
        </select>
      </div>

      <!-- Status -->
      <div>
        <label for="audit-status" class="mb-2 block text-sm font-semibold text-[#3E473F]">
          Status Peminjaman
        </label>
        <select
          id="audit-status"
          name="audit-status"
          v-model="filterStatus"
          class="halden-input w-full px-3.5 py-2.5 text-sm"
        >
          <option :value="''">&mdash; Semua Status &mdash;</option>
          <option :value="0">Pending (Menunggu)</option>
          <option :value="1">Approved (Disetujui)</option>
          <option :value="2">Rejected (Ditolak)</option>
        </select>
      </div>

      <!-- Date -->
      <div>
        <label for="audit-date" class="mb-2 block text-sm font-semibold text-[#3E473F]">
          Tanggal Pelaksanaan
        </label>
        <input
          id="audit-date"
          name="audit-date"
          type="date"
          v-model="filterDate"
          class="halden-input w-full px-3.5 py-2.5 text-sm"
        />
      </div>

      <!-- Search & Reset -->
      <div class="flex items-center gap-2">
        <div class="relative flex-1">
          <input
            id="audit-search"
            name="audit-search"
            aria-label="Cari riwayat peminjaman"
            type="text"
            v-model="searchQuery"
            placeholder="Cari kata kunci..."
            class="halden-input w-full py-2.5 pl-9 pr-3 text-sm placeholder:text-[#8A908A]"
          />
          <Search class="w-3.5 h-3.5 text-stone-500 absolute left-2.5 top-2.5" aria-hidden="true" />
        </div>
        <button
          type="button"
          @click="resetFilter"
          title="Reset Filter"
          class="halden-btn-secondary flex h-11 w-11 items-center justify-center text-[#5F675F] hover:text-[#182019]"
        >
          <RotateCcw class="w-4 h-4" />
        </button>
      </div>
    </div>

    <!-- Table Container (Halden Architectural Card) -->
    <div class="surface-card overflow-hidden">

      <div v-if="isLoading" class="p-12 text-center text-xs text-stone-500 font-mono">
        Memuat arsip log peminjaman...
      </div>

      <div v-else-if="filteredBookings.length === 0" class="p-12 text-center text-stone-600 space-y-2">
        <div class="text-base font-serif font-bold text-[#1C1917]">Tidak Ada Catatan Peminjaman Ditemukan</div>
        <div class="text-xs text-stone-500">Coba ubah kriteria filter atau kata kunci pencarian Anda.</div>
      </div>

      <div v-else class="overflow-x-auto">
        <table class="w-full text-left border-collapse text-xs">
          <thead>
            <tr class="border-b border-[#E5DFD5] bg-[#FAF8F5] text-[10px] font-mono uppercase tracking-wider text-stone-500">
              <th class="py-3 px-4">No. Referensi</th>
              <th class="py-3 px-4">Ruangan &amp; Gedung</th>
              <th class="py-3 px-4">Pemohon</th>
              <th class="py-3 px-4">Agenda / Keperluan</th>
              <th class="py-3 px-4">Jadwal Penggunaan</th>
              <th class="py-3 px-4">Status</th>
              <th class="py-3 px-4">Catatan Admin</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-[#E5DFD5]">
            <tr
              v-for="b in filteredBookings"
              :key="b.id"
              class="hover:bg-[#FAF8F5]/80 transition-colors"
            >
              <!-- Reference -->
              <td class="py-3.5 px-4 font-mono">
                <div class="flex items-center gap-1.5 font-bold text-[#1C1917]">
                  <span class="bg-[#F5F2EB] text-[#1C1917] border border-[#E5DFD5] px-2 py-0.5 rounded text-[11px]">{{ b.bookingReference }}</span>
                  <button
                    type="button"
                    :aria-label="`Salin referensi ${b.bookingReference}`"
                    @click="copyRef(b.bookingReference)"
                    class="grid min-h-11 min-w-11 place-items-center rounded-lg text-stone-600 hover:bg-[#F1EFE9] hover:text-[#1C1917] transition-colors cursor-pointer"
                  >
                    <Check v-if="copiedRef === b.bookingReference" class="w-3 h-3 text-emerald-600" />
                    <Copy v-else class="w-3 h-3" />
                  </button>
                </div>
                <div class="text-[10px] text-stone-600 mt-0.5">{{ formatDate(b.createdAt) }}</div>
              </td>

              <!-- Room -->
              <td class="py-3.5 px-4">
                <div class="font-serif font-bold text-sm text-[#1C1917]">{{ b.room?.name }}</div>
                <div class="text-[10px] font-mono text-stone-500">{{ b.room?.roomCode }} &bull; {{ b.room?.building }}</div>
              </td>

              <!-- Borrower -->
              <td class="py-3.5 px-4">
                <div class="font-semibold text-[#1C1917]">{{ b.borrowerName }}</div>
                <div class="text-[10px] font-mono text-stone-500">{{ b.borrowerEmail }}</div>
              </td>

              <!-- Purpose -->
              <td class="py-3.5 px-4 max-w-xs text-stone-600">
                <div class="truncate" :title="b.purpose">{{ b.purpose }}</div>
              </td>

              <!-- Schedule -->
              <td class="py-3.5 px-4 font-mono text-[#1C1917]">
                <div class="font-semibold">{{ formatDate(b.startTime) }}</div>
                <div class="text-[10px] text-stone-500">{{ formatTime(b.startTime) }} &ndash; {{ formatTime(b.endTime) }} WIB</div>
              </td>

              <!-- Status -->
              <td class="py-3.5 px-4">
                <span
                  v-if="b.status === BookingStatus.Pending"
                  class="inline-flex items-center gap-1 px-2.5 py-0.5 rounded-full text-[10px] font-mono font-semibold bg-amber-50 text-amber-900 border border-amber-200/80"
                >
                  Pending
                </span>
                <span
                  v-else-if="b.status === BookingStatus.Approved"
                  class="inline-flex items-center gap-1 px-2.5 py-0.5 rounded-full text-[10px] font-mono font-semibold bg-emerald-50 text-emerald-900 border border-emerald-200/80"
                >
                  Approved
                </span>
                <span
                  v-else-if="b.status === BookingStatus.Rejected"
                  class="inline-flex items-center gap-1 px-2.5 py-0.5 rounded-full text-[10px] font-mono font-semibold bg-rose-50 text-rose-900 border border-rose-200/80"
                >
                  Rejected
                </span>
              </td>

              <!-- Admin Notes -->
              <td class="py-3.5 px-4 text-xs font-mono text-stone-600 max-w-xs">
                <div v-if="b.adminNotes" class="bg-[#FAF8F5] border border-[#E5DFD5] p-2 rounded text-[11px] truncate" :title="b.adminNotes">
                  {{ b.adminNotes }}
                </div>
                <span v-else class="text-stone-300">&mdash;</span>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  </div>
</template>
