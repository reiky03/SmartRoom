<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import { api } from '../api/client';
import type { Booking, Room } from '../types';
import { BookingStatus } from '../types';
import { addToast } from '../stores/auth';
import RejectModal from '../components/RejectModal.vue';
import {
  ShieldCheck,
  Check,
  X,
  Clock,
  DoorOpen,
  Users,
  Search,
  CheckCircle2,
  AlertOctagon,
  Calendar,
  ArrowLeft
} from 'lucide-vue-next';

const bookings = ref<Booking[]>([]);
const rooms = ref<Room[]>([]);
const isLoading = ref(true);

const filterRoomId = ref<number | ''>('');
const filterStatus = ref<number | ''>(0); // Default to Pending (0)
const searchQuery = ref('');

// Reject modal state
const isRejectModalOpen = ref(false);
const activeBookingForReject = ref<Booking | null>(null);

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

    bookings.value = await api.getBookings(params);
  } catch (e: any) {
    addToast('Gagal memuat antrean approval: ' + e.message, 'error');
  } finally {
    isLoading.value = false;
  }
}

const pendingCount = computed(() => bookings.value.filter(b => b.status === BookingStatus.Pending).length);

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

async function handleApprove(booking: Booking) {
  try {
    await api.updateBookingStatus(
      booking.id,
      BookingStatus.Approved,
      'Disetujui oleh Administrator Fasilitas Kampus.',
    );
    addToast(`Peminjaman ${booking.bookingReference} (${booking.borrowerName}) berhasil disetujui!`, 'success');
    window.dispatchEvent(new CustomEvent('booking-created'));
    await loadBookings();
  } catch (e: any) {
    addToast('Gagal menyetujui peminjaman: ' + e.message, 'error');
  }
}

function openRejectModal(booking: Booking) {
  activeBookingForReject.value = booking;
  isRejectModalOpen.value = true;
}

async function handleRejectConfirm(notes: string) {
  if (!activeBookingForReject.value) return;
  try {
    await api.updateBookingStatus(
      activeBookingForReject.value.id,
      BookingStatus.Rejected,
      notes,
    );
    addToast(`Peminjaman ${activeBookingForReject.value.bookingReference} telah ditolak. Catatan tersimpan.`, 'info');
    isRejectModalOpen.value = false;
    activeBookingForReject.value = null;
    window.dispatchEvent(new CustomEvent('booking-created'));
    await loadBookings();
  } catch (e: any) {
    addToast('Gagal menolak peminjaman: ' + e.message, 'error');
  }
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

    <!-- Top Header (Halden Architectural Card) -->
    <div class="page-hero flex flex-col justify-between gap-6 p-6 sm:p-9 md:flex-row md:items-end lg:p-10">
      <div class="space-y-2">
        <div class="page-kicker">
          <span>Meja persetujuan</span>
          <span class="text-[#596159] font-medium">{{ pendingCount }} menunggu keputusan</span>
        </div>
        <h1 class="max-w-[17ch] text-4xl font-medium leading-[0.98] text-[#182019] sm:text-5xl lg:text-6xl">
          Keputusan tepat, konteks lengkap
        </h1>
        <p class="max-w-2xl text-sm leading-6 text-[#5F675F] sm:text-base">
          Tinjau ruang, pemohon, agenda, dan irisan waktu sebelum menyetujui atau mencatat alasan penolakan.
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

    <!-- Filters & Search Toolbar -->
    <div class="surface-card grid grid-cols-1 items-end gap-4 p-5 sm:grid-cols-3">

      <!-- Status Filter -->
      <div>
        <label for="approval-status" class="mb-2 block text-sm font-semibold text-[#3E473F]">
          Status Pengajuan
        </label>
        <select
          id="approval-status"
          name="approval-status"
          v-model="filterStatus"
          @change="loadBookings"
          class="halden-input w-full px-3.5 py-2.5 text-sm"
        >
          <option :value="''">Semua Status</option>
          <option :value="0">Menunggu Persetujuan Saja (Pending)</option>
          <option :value="1">Disetujui Saja (Approved)</option>
          <option :value="2">Ditolak Saja (Rejected)</option>
        </select>
      </div>

      <!-- Room Filter -->
      <div>
        <label for="approval-room" class="mb-2 block text-sm font-semibold text-[#3E473F]">
          Filter Ruangan
        </label>
        <select
          id="approval-room"
          name="approval-room"
          v-model="filterRoomId"
          @change="loadBookings"
          class="halden-input w-full px-3.5 py-2.5 text-sm"
        >
          <option :value="''">&mdash; Semua Ruangan &mdash;</option>
          <option v-for="r in rooms" :key="r.id" :value="r.id">
            {{ r.roomCode }} &bull; {{ r.name }}
          </option>
        </select>
      </div>

      <!-- Quick Search -->
      <div>
        <label for="approval-search" class="mb-2 block text-sm font-semibold text-[#3E473F]">
          Pencarian Cepat
        </label>
        <div class="relative">
          <input
            id="approval-search"
            name="approval-search"
            type="text"
            v-model="searchQuery"
            placeholder="Cari pemohon, no. ref..."
            class="halden-input w-full py-2.5 pl-10 pr-3.5 text-sm placeholder:text-[#8A908A]"
          />
          <Search class="w-4 h-4 text-[#78716C] absolute left-3 top-3" />
        </div>
      </div>
    </div>

    <!-- Approvals Table Container -->
    <div class="surface-card overflow-hidden">

      <!-- Loading indicator -->
      <div v-if="isLoading" class="p-12 text-center text-xs text-[#78716C] font-mono">
        Memuat antrean peminjaman...
      </div>

      <!-- Empty state -->
      <div v-else-if="filteredBookings.length === 0" class="p-12 text-center text-[#57534E] space-y-2">
        <CheckCircle2 class="w-10 h-10 text-[#15803D] mx-auto" />
        <div class="text-base font-serif font-bold text-[#1C1917]">Tidak Ada Permohonan yang Perlu Ditindak</div>
        <div class="text-xs text-[#78716C] max-w-sm mx-auto">
          Semua permohonan dengan kriteria filter yang dipilih sudah diproses atau belum ada pengajuan baru.
        </div>
      </div>

      <!-- Table -->
      <div v-else class="overflow-x-auto">
        <table class="w-full text-left border-collapse text-xs">
          <thead>
            <tr class="border-b border-[#E5DFD5] bg-[#FAF8F5] text-[10px] font-mono uppercase tracking-wider text-[#78716C]">
              <th class="py-3.5 px-5">No. Referensi</th>
              <th class="py-3.5 px-5">Ruangan</th>
              <th class="py-3.5 px-5">Pemohon</th>
              <th class="py-3.5 px-5">Agenda &amp; Jadwal Sesi</th>
              <th class="py-3.5 px-5">Status</th>
              <th class="py-3.5 px-5 text-right">Keputusan Admin</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-[#E5DFD5]">
            <tr
              v-for="b in filteredBookings"
              :key="b.id"
              class="hover:bg-[#FAF8F5]/80 transition-colors"
            >
              <!-- Reference -->
              <td class="py-4 px-5 font-mono">
                <div class="font-bold text-[#1C1917]">{{ b.bookingReference }}</div>
                <div class="text-[10px] text-[#78716C]">Diajukan: {{ formatDate(b.createdAt) }}</div>
              </td>

              <!-- Room -->
              <td class="py-4 px-5">
                <div class="font-serif font-bold text-sm text-[#1C1917]">{{ b.room?.name }}</div>
                <div class="text-[10px] font-mono text-[#78716C]">{{ b.room?.roomCode }} &bull; {{ b.room?.building }}</div>
              </td>

              <!-- Borrower -->
              <td class="py-4 px-5">
                <div class="font-semibold text-[#1C1917]">{{ b.borrowerName }}</div>
                <div class="text-[11px] font-mono text-[#78716C]">{{ b.borrowerEmail }}</div>
              </td>

              <!-- Agenda & Schedule -->
              <td class="py-4 px-5 max-w-xs">
                <div class="text-[#1C1917] font-medium truncate" :title="b.purpose">{{ b.purpose }}</div>
                <div class="flex items-center gap-1.5 text-[11px] font-mono text-[#57534E] mt-1">
                  <Calendar class="w-3 h-3 text-[#78716C]" />
                  <span>{{ formatDate(b.startTime) }} &bull; {{ formatTime(b.startTime) }} &ndash; {{ formatTime(b.endTime) }} WIB</span>
                </div>
              </td>

              <!-- Status Badge -->
              <td class="py-4 px-5">
                <span
                  v-if="b.status === BookingStatus.Pending"
                  class="inline-flex items-center gap-1 px-2.5 py-0.5 rounded text-[10px] font-mono font-semibold bg-[#FEF3C7] text-[#92400E] border border-[#FDE68A]"
                >
                  <span class="w-1.5 h-1.5 rounded-full bg-[#B45309]"></span>
                  Pending
                </span>
                <span
                  v-else-if="b.status === BookingStatus.Approved"
                  class="inline-flex items-center gap-1 px-2.5 py-0.5 rounded text-[10px] font-mono font-semibold bg-[#DCFCE7] text-[#166534] border border-[#BBF7D0]"
                >
                  <span class="w-1.5 h-1.5 rounded-full bg-[#15803D]"></span>
                  Approved
                </span>
                <span
                  v-else-if="b.status === BookingStatus.Rejected"
                  class="inline-flex items-center gap-1 px-2.5 py-0.5 rounded text-[10px] font-mono font-semibold bg-[#FEE2E2] text-[#991B1B] border border-[#FECACA]"
                >
                  <span class="w-1.5 h-1.5 rounded-full bg-[#B91C1C]"></span>
                  Rejected
                </span>
              </td>

              <!-- Actions -->
              <td class="py-4 px-5 text-right">
                <div v-if="b.status === BookingStatus.Pending" class="inline-flex items-center gap-2">
                  <button
                    type="button"
                    @click="handleApprove(b)"
                    class="inline-flex items-center gap-1 px-3 py-1.5 rounded-md bg-[#166534] hover:bg-[#14532D] text-white text-[11px] font-mono font-semibold transition-all active:scale-95 shadow-xs cursor-pointer"
                  >
                    <Check class="w-3.5 h-3.5" />
                    <span>Setujui</span>
                  </button>
                  <button
                    type="button"
                    @click="openRejectModal(b)"
                    class="inline-flex items-center gap-1 px-3 py-1.5 rounded-md bg-[#991B1B] hover:bg-[#7F1D1D] text-white text-[11px] font-mono font-semibold transition-all active:scale-95 shadow-xs cursor-pointer"
                  >
                    <X class="w-3.5 h-3.5" />
                    <span>Tolak</span>
                  </button>
                </div>
                <div v-else class="text-[11px] font-mono text-[#78716C] italic">
                  Selesai Diproses
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- Reject Modal -->
    <RejectModal
      :is-open="isRejectModalOpen"
      :booking="activeBookingForReject"
      @close="isRejectModalOpen = false"
      @confirm="handleRejectConfirm"
    />
  </div>
</template>
