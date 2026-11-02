<script setup lang="ts">
import { ref, onMounted, watch } from 'vue';
import { api } from '../api/client';
import type { Booking, Room } from '../types';
import { BookingStatus } from '../types';
import { addToast } from '../stores/auth';
import { 
  Plus, 
  Search, 
  Calendar, 
  Filter, 
  RotateCcw, 
  Copy, 
  Check, 
  CalendarX,
  Loader2 
} from 'lucide-vue-next';

const emit = defineEmits<{
  (e: 'open-booking'): void;
}>();

const bookings = ref<Booking[]>([]);
const rooms = ref<Room[]>([]);
const isLoading = ref(true);

const filterRoomId = ref<number | ''>('');
const filterStatus = ref<number | ''>('');
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
    addToast('Gagal memuat jadwal peminjaman: ' + e.message, 'error');
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
}

function copyRef(refCode: string) {
  navigator.clipboard.writeText(refCode);
  copiedRef.value = refCode;
  addToast(`Nomor referensi ${refCode} disalin!`, 'info');
  setTimeout(() => {
    if (copiedRef.value === refCode) copiedRef.value = null;
  }, 2000);
}
</script>

<template>
  <div class="space-y-6">

    <!-- Header Section -->
    <div class="flex flex-col sm:flex-row sm:items-end justify-between gap-4 pb-6 border-b border-zinc-200/80">
      <div>
        <div class="flex items-center gap-2 mb-1.5">
          <span class="text-[11px] font-mono text-zinc-400 uppercase tracking-wider font-semibold">Reservasi &amp; Agenda Terjadwal</span>
          <span class="text-zinc-300">&bull;</span>
          <span class="text-xs font-mono text-zinc-500">{{ bookings.length }} Sesi Terdaftar</span>
        </div>
        <h1 class="text-2xl sm:text-3xl font-extrabold tracking-tight text-zinc-950">
          Daftar Jadwal Peminjaman
        </h1>
        <p class="text-xs sm:text-sm text-zinc-500 mt-1 max-w-xl">
          Pantau seluruh agenda peminjaman ruangan yang telah diajukan, status verifikasi admin, dan jadwal pelaksanaan.
        </p>
      </div>

      <button 
        type="button" 
        @click="emit('open-booking')"
        class="inline-flex items-center gap-2 px-4 py-2.5 rounded-xl bg-zinc-900 hover:bg-zinc-800 text-white text-xs font-medium shadow-xs transition-all active:scale-[0.98] flex-shrink-0"
      >
        <Plus class="w-4 h-4" />
        <span>Ajukan Peminjaman</span>
      </button>
    </div>

    <!-- Filter Bar Card -->
    <div class="bg-white border border-zinc-200/80 rounded-2xl p-5 shadow-xs">
      <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-3.5 items-end">
        <div>
          <label class="block text-[11px] font-mono uppercase tracking-wider font-semibold text-zinc-600 mb-1.5">Filter Ruangan</label>
          <select 
            v-model="filterRoomId" 
            class="w-full px-3 py-2 rounded-xl bg-zinc-50 border border-zinc-200 text-xs text-zinc-800 focus:bg-white focus:border-zinc-900 focus:outline-none transition-all"
          >
            <option value="">— Semua Ruangan —</option>
            <option v-for="r in rooms" :key="r.id" :value="r.id">
              {{ r.roomCode }} - {{ r.name }}
            </option>
          </select>
        </div>

        <div>
          <label class="block text-[11px] font-mono uppercase tracking-wider font-semibold text-zinc-600 mb-1.5">Status Peminjaman</label>
          <select 
            v-model="filterStatus" 
            class="w-full px-3 py-2 rounded-xl bg-zinc-50 border border-zinc-200 text-xs text-zinc-800 focus:bg-white focus:border-zinc-900 focus:outline-none transition-all"
          >
            <option value="">— Semua Status —</option>
            <option :value="BookingStatus.Pending">Menunggu (Pending)</option>
            <option :value="BookingStatus.Approved">Disetujui (Approved)</option>
            <option :value="BookingStatus.Rejected">Ditolak (Rejected)</option>
            <option :value="BookingStatus.Cancelled">Dibatalkan (Cancelled)</option>
          </select>
        </div>

        <div>
          <label class="block text-[11px] font-mono uppercase tracking-wider font-semibold text-zinc-600 mb-1.5">Tanggal Pelaksanaan</label>
          <input 
            type="date" 
            v-model="filterDate" 
            class="w-full px-3 py-2 rounded-xl bg-zinc-50 border border-zinc-200 text-xs text-zinc-800 focus:bg-white focus:border-zinc-900 focus:outline-none transition-all font-mono"
          />
        </div>

        <div class="flex items-center gap-2">
          <button 
            type="button" 
            @click="loadBookings"
            class="flex-1 py-2 px-3 rounded-xl bg-zinc-900 hover:bg-zinc-800 text-white text-xs font-medium transition-all active:scale-[0.98] flex items-center justify-center gap-1.5"
          >
            <Filter class="w-3.5 h-3.5" />
            <span>Terapkan</span>
          </button>
          <button 
            type="button" 
            @click="resetFilter" 
            class="p-2 rounded-xl border border-zinc-200 hover:bg-zinc-50 text-zinc-600 transition-colors" 
            title="Reset Filter"
          >
            <RotateCcw class="w-3.5 h-3.5" />
          </button>
        </div>
      </div>
    </div>

    <!-- Bookings Table Card -->
    <div class="bg-white border border-zinc-200/80 rounded-2xl overflow-hidden shadow-xs">
      <div v-if="bookings.length > 0" class="overflow-x-auto">
        <table class="w-full text-left border-collapse">
          <thead>
            <tr class="bg-zinc-50/75 border-b border-zinc-200/80 text-[11px] font-mono uppercase tracking-wider text-zinc-500">
              <th class="py-3 px-5">Referensi</th>
              <th class="py-3 px-5">Ruangan &amp; Gedung</th>
              <th class="py-3 px-5">Pemohon</th>
              <th class="py-3 px-5">Agenda / Keperluan</th>
              <th class="py-3 px-5">Jadwal Penggunaan</th>
              <th class="py-3 px-5">Status</th>
              <th class="py-3 px-5">Catatan Admin</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-zinc-100 text-xs text-zinc-700">
            <tr v-for="b in bookings" :key="b.id" class="hover:bg-zinc-50/70 transition-colors">
              <!-- Reference -->
              <td class="py-3.5 px-5 font-mono">
                <div class="flex items-center gap-1.5">
                  <span class="font-bold text-zinc-950">{{ b.bookingReference }}</span>
                  <button 
                    type="button" 
                    @click="copyRef(b.bookingReference)"
                    class="text-zinc-400 hover:text-zinc-950 p-0.5 rounded transition-colors"
                  >
                    <Check v-if="copiedRef === b.bookingReference" class="w-3 h-3 text-[#166534]" />
                    <Copy v-else class="w-3 h-3" />
                  </button>
                </div>
                <div class="text-[10px] text-zinc-400 font-mono">
                  {{ new Date(b.createdAt).toLocaleDateString('id-ID') }}
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

              <!-- Purpose -->
              <td class="py-3.5 px-5 max-w-xs truncate" :title="b.purpose">
                <span class="text-zinc-800">{{ b.purpose }}</span>
              </td>

              <!-- Schedule -->
              <td class="py-3.5 px-5 font-mono">
                <div class="font-semibold text-zinc-950">
                  {{ new Date(b.startTime).toLocaleDateString('id-ID', { day: '2-digit', month: 'short', year: 'numeric' }) }}
                </div>
                <div class="text-[11px] text-zinc-500 mt-0.5">
                  {{ new Date(b.startTime).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }) }} — 
                  {{ new Date(b.endTime).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }) }}
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
                  <span class="w-1.5 h-1.5 rounded-full bg-[#854D0E]"></span> Pending
                </span>
                <span 
                  v-else-if="b.status === BookingStatus.Rejected"
                  class="inline-flex items-center gap-1.5 px-2.5 py-0.5 rounded-full text-[11px] font-mono font-medium bg-[#FEE2E2] text-[#991B1B] border border-[#FECACA]"
                >
                  <span class="w-1.5 h-1.5 rounded-full bg-[#991B1B]"></span> Rejected
                </span>
                <span 
                  v-else
                  class="inline-flex items-center gap-1.5 px-2.5 py-0.5 rounded-full text-[11px] font-mono font-medium bg-zinc-100 text-zinc-600 border border-zinc-200"
                >
                  Cancelled
                </span>
              </td>

              <!-- Admin Notes -->
              <td class="py-3.5 px-5">
                <span v-if="b.adminNotes" class="px-2 py-0.5 rounded bg-zinc-100 text-zinc-600 font-mono text-[11px] border border-zinc-200/80">
                  {{ b.adminNotes }}
                </span>
                <span v-else class="text-zinc-300 font-mono">—</span>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <div v-else-if="!isLoading" class="py-16 text-center text-zinc-400">
        <CalendarX class="w-10 h-10 mx-auto mb-2 opacity-30" />
        <div class="text-sm font-medium text-zinc-700">Tidak Ada Peminjaman Ditemukan</div>
        <div class="text-xs text-zinc-400 mt-0.5">Coba ubah kriteria filter ruangan atau status di atas.</div>
      </div>
    </div>
  </div>
</template>
