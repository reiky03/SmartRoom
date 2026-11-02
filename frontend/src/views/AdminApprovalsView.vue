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
  Calendar
} from 'lucide-vue-next';

const bookings = ref<Booking[]>([]);
const rooms = ref<Room[]>([]);
const isLoading = ref(true);

const filterRoomId = ref<number | ''>('');
const filterStatus = ref<number | ''>(0); // Default to Pending (0) so admin immediately sees what needs action!
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
    await api.updateBookingStatus(booking.id, BookingStatus.Approved);
    booking.status = BookingStatus.Approved;
    addToast(`Peminjaman ${booking.bookingReference} (${booking.borrowerName}) berhasil disetujui!`, 'success');
  } catch (e: any) {
    addToast('Gagal menyetujui peminjaman: ' + e.message, 'error');
  }
}

function openRejectModal(booking: Booking) {
  activeBookingForReject.value = booking;
  isRejectModalOpen.value = true;
}

async function handleConfirmReject(notes: string) {
  if (!activeBookingForReject.value) return;
  const b = activeBookingForReject.value;

  try {
    await api.updateBookingStatus(b.id, BookingStatus.Rejected, notes);
    b.status = BookingStatus.Rejected;
    b.adminNotes = notes;
    isRejectModalOpen.value = false;
    activeBookingForReject.value = null;
    addToast(`Peminjaman ${b.bookingReference} ditolak. Alasan tercatat di akun mahasiswa.`, 'info');
  } catch (e: any) {
    addToast('Gagal menolak peminjaman: ' + e.message, 'error');
  }
}
</script>

<template>
  <div class="space-y-6">

    <!-- Header Section -->
    <div class="flex flex-col sm:flex-row sm:items-end justify-between gap-4 pb-6 border-b border-zinc-200/80">
      <div>
        <div class="flex items-center gap-2 mb-1.5">
          <span class="inline-flex items-center gap-1.5 px-2.5 py-0.5 rounded-full text-[11px] font-mono font-medium bg-rose-50 text-rose-700 border border-rose-200">
            <span class="w-1.5 h-1.5 rounded-full bg-rose-600"></span>
            WORKFLOW PERSETUJUAN FASILITAS
          </span>
          <span class="text-zinc-300">&bull;</span>
          <span class="text-xs font-mono text-zinc-500">{{ pendingCount }} Pengajuan Pending</span>
        </div>
        <h1 class="text-2xl sm:text-3xl font-extrabold tracking-tight text-zinc-950">
          Antrean Persetujuan (Approval Queue)
        </h1>
        <p class="text-xs sm:text-sm text-zinc-500 mt-1 max-w-xl">
          Tinjau permohonan peminjaman ruangan dari civitas. Berikan persetujuan instan atau tolak dengan alasan resmi.
        </p>
      </div>

      <router-link 
        to="/admin/dashboard" 
        class="inline-flex items-center gap-2 px-3.5 py-2 rounded-xl border border-zinc-200 bg-white hover:bg-zinc-50 text-xs font-medium text-zinc-700 shadow-xs transition-colors flex-shrink-0"
      >
        <span>&larr; Kembali ke Dashboard</span>
      </router-link>
    </div>

    <!-- Filter Card -->
    <div class="bg-white border border-zinc-200/80 rounded-2xl p-5 shadow-xs flex flex-col md:flex-row md:items-end justify-between gap-3.5">
      <div class="grid grid-cols-1 sm:grid-cols-2 gap-3.5 flex-1">
        <div>
          <label class="block text-[11px] font-mono uppercase tracking-wider font-semibold text-zinc-600 mb-1.5">Status Pengajuan</label>
          <select 
            v-model="filterStatus" 
            @change="loadBookings"
            class="w-full px-3 py-2 rounded-xl bg-zinc-50 border border-zinc-200 text-xs text-zinc-800 focus:bg-white focus:border-zinc-900 focus:outline-none transition-all"
          >
            <option :value="0">Menunggu Persetujuan Saja (Pending)</option>
            <option value="">— Tampilkan Semua Status —</option>
            <option :value="1">Disetujui (Approved)</option>
            <option :value="2">Ditolak (Rejected)</option>
          </select>
        </div>

        <div>
          <label class="block text-[11px] font-mono uppercase tracking-wider font-semibold text-zinc-600 mb-1.5">Filter Ruangan</label>
          <select 
            v-model="filterRoomId" 
            @change="loadBookings"
            class="w-full px-3 py-2 rounded-xl bg-zinc-50 border border-zinc-200 text-xs text-zinc-800 focus:bg-white focus:border-zinc-900 focus:outline-none transition-all"
          >
            <option value="">— Semua Ruangan —</option>
            <option v-for="r in rooms" :key="r.id" :value="r.id">
              {{ r.roomCode }} - {{ r.name }}
            </option>
          </select>
        </div>
      </div>

      <div class="w-full md:w-64">
        <label class="block text-[11px] font-mono uppercase tracking-wider font-semibold text-zinc-600 mb-1.5">Pencarian Cepat</label>
        <div class="relative">
          <Search class="w-4 h-4 text-zinc-400 absolute left-3 top-1/2 -translate-y-1/2" />
          <input 
            type="text" 
            v-model="searchQuery" 
            placeholder="Cari pemohon, no. ref..." 
            class="w-full pl-9 pr-3 py-2 text-xs rounded-xl bg-zinc-50 border border-zinc-200 text-zinc-800 focus:bg-white focus:border-zinc-900 focus:outline-none transition-all font-mono"
          />
        </div>
      </div>
    </div>

    <!-- Review Queue Table -->
    <div class="bg-white border border-zinc-200/80 rounded-2xl overflow-hidden shadow-xs">
      <div v-if="filteredBookings.length > 0" class="overflow-x-auto">
        <table class="w-full text-left border-collapse text-xs">
          <thead>
            <tr class="bg-zinc-50/75 border-b border-zinc-200/80 text-[11px] font-mono uppercase tracking-wider text-zinc-500">
              <th class="py-3 px-5">Referensi</th>
              <th class="py-3 px-5">Ruangan</th>
              <th class="py-3 px-5">Pemohon</th>
              <th class="py-3 px-5">Agenda &amp; Jadwal Sesi</th>
              <th class="py-3 px-5">Status</th>
              <th class="py-3 px-5 text-right">Keputusan Admin</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-zinc-100 text-zinc-700">
            <tr 
              v-for="b in filteredBookings" 
              :key="b.id" 
              class="hover:bg-zinc-50/70 transition-colors"
              :class="{ 'bg-amber-50/20': b.status === BookingStatus.Pending }"
            >
              <!-- Reference -->
              <td class="py-3.5 px-5 font-mono">
                <div class="font-bold text-zinc-950">{{ b.bookingReference }}</div>
                <div class="text-[10px] text-zinc-400 font-mono">
                  Diajukan: {{ new Date(b.createdAt).toLocaleDateString('id-ID') }}
                </div>
              </td>

              <!-- Room -->
              <td class="py-3.5 px-5">
                <div class="font-semibold text-zinc-950">{{ b.room?.name || 'Ruangan #' + b.roomId }}</div>
                <div class="text-[11px] font-mono text-zinc-400">
                  {{ b.room?.roomCode }} &bull; {{ b.room?.building }}
                </div>
              </td>

              <!-- Borrower -->
              <td class="py-3.5 px-5">
                <div class="font-medium text-zinc-950">{{ b.borrowerName }}</div>
                <div class="text-[11px] text-zinc-400 font-mono">{{ b.borrowerEmail }}</div>
              </td>

              <!-- Purpose & Schedule -->
              <td class="py-3.5 px-5">
                <div class="font-medium text-zinc-950 max-w-xs truncate" :title="b.purpose">{{ b.purpose }}</div>
                <div class="font-mono text-zinc-500 text-[11px] mt-0.5 flex items-center gap-1">
                  <Clock class="w-3 h-3 text-zinc-400" />
                  <span>
                    {{ new Date(b.startTime).toLocaleDateString('id-ID', { day: '2-digit', month: 'short', year: 'numeric' }) }} &bull; 
                    {{ new Date(b.startTime).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }) }} — 
                    {{ new Date(b.endTime).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }) }}
                  </span>
                </div>
                <div v-if="b.adminNotes" class="mt-1 p-1 px-2 rounded bg-zinc-100 text-zinc-600 text-[10px] font-mono border border-zinc-200/80">
                  Alasan: {{ b.adminNotes }}
                </div>
              </td>

              <!-- Status -->
              <td class="py-3.5 px-5">
                <span 
                  v-if="b.status === BookingStatus.Approved"
                  class="inline-flex items-center gap-1.5 px-2.5 py-0.5 rounded-full text-[11px] font-mono font-medium bg-[#EDF3EC] text-[#166534] border border-[#BBF7D0]"
                >
                  <span class="w-1.5 h-1.5 rounded-full bg-[#166534]"></span> Approved
                </span>
                <span 
                  v-else-if="b.status === BookingStatus.Pending"
                  class="inline-flex items-center gap-1.5 px-2.5 py-0.5 rounded-full text-[11px] font-mono font-medium bg-[#FEF9C3] text-[#854D0E] border border-[#FEF08A]"
                >
                  <span class="w-1.5 h-1.5 rounded-full bg-[#854D0E] animate-pulse"></span> Pending
                </span>
                <span 
                  v-else-if="b.status === BookingStatus.Rejected"
                  class="inline-flex items-center gap-1.5 px-2.5 py-0.5 rounded-full text-[11px] font-mono font-medium bg-[#FEE2E2] text-[#991B1B] border border-[#FECACA]"
                >
                  <span class="w-1.5 h-1.5 rounded-full bg-[#991B1B]"></span> Rejected
                </span>
              </td>

              <!-- Actions -->
              <td class="py-3.5 px-5 text-right font-mono">
                <div class="flex items-center justify-end gap-1.5">
                  <button 
                    v-if="b.status !== BookingStatus.Approved"
                    type="button" 
                    @click="handleApprove(b)"
                    class="px-3 py-1.5 rounded-lg bg-[#EDF3EC] hover:bg-[#DCFCE7] text-[#166534] border border-[#BBF7D0] text-[11px] font-semibold transition-colors flex items-center gap-1 shadow-xs"
                  >
                    <Check class="w-3.5 h-3.5" />
                    <span>Setujui</span>
                  </button>

                  <button 
                    v-if="b.status !== BookingStatus.Rejected"
                    type="button" 
                    @click="openRejectModal(b)"
                    class="px-3 py-1.5 rounded-lg bg-[#FEE2E2] hover:bg-[#FECACA] text-[#991B1B] border border-[#FECACA] text-[11px] font-semibold transition-colors flex items-center gap-1 shadow-xs"
                  >
                    <X class="w-3.5 h-3.5" />
                    <span>Tolak</span>
                  </button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <div v-else-if="!isLoading" class="py-16 text-center text-zinc-400">
        <ShieldCheck class="w-10 h-10 mx-auto mb-2 text-[#166534] opacity-50" />
        <div class="text-xs font-semibold text-zinc-700">Tidak Ada Antrean Approval Pending</div>
        <div class="text-[11px] text-zinc-400 mt-0.5">Seluruh pengajuan telah berhasil ditindaklanjuti.</div>
      </div>
    </div>

    <!-- Reject Modal -->
    <RejectModal 
      :is-open="isRejectModalOpen" 
      :booking="activeBookingForReject"
      @close="isRejectModalOpen = false"
      @confirm="handleConfirmReject"
    />
  </div>
</template>
