<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import { api } from '../api/client';
import type { Room, Booking } from '../types';
import { BookingStatus } from '../types';
import { isAdmin, currentUser, addToast } from '../stores/auth';
import TimelineMatrix from '../components/TimelineMatrix.vue';
import { 
  DoorOpen, 
  Calendar, 
  Hourglass, 
  CheckCheck, 
  ShieldCheck, 
  Plus, 
  ArrowRight, 
  Search, 
  Copy, 
  Check, 
  ChevronRight,
  Sparkles,
  Loader2
} from 'lucide-vue-next';

const emit = defineEmits<{
  (e: 'open-booking', roomId?: number, hour?: number): void;
}>();

const rooms = ref<Room[]>([]);
const bookings = ref<Booking[]>([]);
const isLoading = ref(true);

const tableFilter = ref<'all' | 'Approved' | 'Pending' | 'Rejected'>('all');
const searchQuery = ref('');
const copiedRef = ref<string | null>(null);

const todayStr = new Date().toISOString().split('T')[0];
const formattedDate = new Intl.DateTimeFormat('id-ID', { dateStyle: 'full' }).format(new Date());

onMounted(async () => {
  await loadData();
});

async function loadData() {
  isLoading.value = true;
  try {
    const [roomsData, bookingsData] = await Promise.all([
      api.getRooms(),
      api.getBookings({ date: todayStr }),
    ]);
    rooms.value = roomsData;
    bookings.value = bookingsData;
  } catch (e: any) {
    addToast('Gagal memuat data dashboard: ' + e.message, 'error');
  } finally {
    isLoading.value = false;
  }
}

// KPI Computations
const totalActiveRooms = computed(() => rooms.value.filter(r => r.isActive).length);
const todayBookingsCount = computed(() => bookings.value.length);
const pendingCount = computed(() => bookings.value.filter(b => b.status === BookingStatus.Pending).length);
const approvedCount = computed(() => bookings.value.filter(b => b.status === BookingStatus.Approved).length);

// Filtered Bookings Table
const filteredBookings = computed(() => {
  return bookings.value.filter(b => {
    // Filter by status tab
    if (tableFilter.value === 'Approved' && b.status !== BookingStatus.Approved) return false;
    if (tableFilter.value === 'Pending' && b.status !== BookingStatus.Pending) return false;
    if (tableFilter.value === 'Rejected' && b.status !== BookingStatus.Rejected) return false;

    // Filter by search query
    if (searchQuery.value) {
      const q = searchQuery.value.toLowerCase();
      const matchRef = b.bookingReference.toLowerCase().includes(q);
      const matchBorrower = b.borrowerName.toLowerCase().includes(q);
      const matchRoom = (b.room?.name || '').toLowerCase().includes(q);
      const matchPurpose = b.purpose.toLowerCase().includes(q);
      return matchRef || matchBorrower || matchRoom || matchPurpose;
    }

    return true;
  });
});

function copyRef(refCode: string) {
  navigator.clipboard.writeText(refCode);
  copiedRef.value = refCode;
  addToast(`Nomor referensi ${refCode} disalin ke clipboard!`, 'info');
  setTimeout(() => {
    if (copiedRef.value === refCode) copiedRef.value = null;
  }, 2000);
}

function handleSlotSelect(roomId: number, hour: number) {
  emit('open-booking', roomId, hour);
}

defineExpose({ loadData });
</script>

<template>
  <div class="space-y-8">
    
    <!-- Hero Header / Operations Ticker -->
    <div class="flex flex-col md:flex-row md:items-end justify-between gap-4 pb-6 border-b border-zinc-200/80">
      <div>
        <div class="flex items-center gap-2 mb-2">
          <span class="inline-flex items-center gap-1.5 px-2.5 py-0.5 rounded-full text-[11px] font-mono font-medium bg-[#EDF3EC] text-[#166534] border border-[#BBF7D0]">
            <span class="w-1.5 h-1.5 rounded-full bg-[#166534] animate-pulse"></span>
            SISTEM OPERASIONAL AKTIF (VUE 3 SPA)
          </span>
          <span class="text-zinc-300">&bull;</span>
          <span class="text-xs font-mono text-zinc-500 capitalize">{{ formattedDate }}</span>
        </div>
        <h1 class="text-3xl font-extrabold tracking-tight text-zinc-950 sm:text-4xl">
          SmartRoom Operations
        </h1>
        <p class="text-sm text-zinc-500 mt-1 max-w-2xl">
          Platform reservasi fasilitas kampus terintegrasi dengan validasi pencegahan jadwal tumpang tindih (anti-conflict) dan alur persetujuan bertingkat.
        </p>
      </div>

      <div class="flex items-center gap-2.5 flex-shrink-0">
        <button 
          type="button" 
          @click="emit('open-booking')"
          class="inline-flex items-center gap-2 px-4 py-2.5 rounded-xl bg-zinc-900 hover:bg-zinc-800 text-white text-xs font-medium shadow-xs transition-all active:scale-[0.98]"
        >
          <Plus class="w-4 h-4" />
          <span>Ajukan Peminjaman</span>
        </button>

        <router-link 
          v-if="isAdmin" 
          to="/admin/approvals" 
          class="inline-flex items-center gap-2 px-4 py-2.5 rounded-xl bg-white hover:bg-zinc-50 text-zinc-800 text-xs font-medium border border-zinc-200/80 shadow-xs transition-all"
        >
          <ShieldCheck class="w-4 h-4 text-rose-600" />
          <span>Antrean ({{ pendingCount }})</span>
        </router-link>
      </div>
    </div>

    <!-- Bento Grid KPI Cards -->
    <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
      
      <!-- Card 1: Active Rooms -->
      <div class="bg-white border border-zinc-200/80 rounded-2xl p-5 hover:border-zinc-300 hover:shadow-subtle transition-all duration-200 flex flex-col justify-between">
        <div>
          <div class="flex items-center justify-between text-zinc-400 mb-3">
            <span class="text-[11px] font-mono uppercase tracking-wider text-zinc-500 font-semibold">Ruangan Aktif</span>
            <div class="w-7 h-7 rounded-lg bg-zinc-100 text-zinc-700 flex items-center justify-center">
              <DoorOpen class="w-3.5 h-3.5" />
            </div>
          </div>
          <div class="flex items-baseline gap-2">
            <span class="text-3xl font-extrabold text-zinc-950 font-mono tracking-tight">{{ totalActiveRooms }}</span>
            <span class="text-xs text-zinc-400 font-mono">Unit Siap Pakai</span>
          </div>
        </div>
        <div class="mt-4 pt-3 border-t border-zinc-100 flex items-center justify-between text-xs">
          <span class="inline-flex items-center gap-1 text-[#166534] font-medium text-[11px]">
            <span class="w-1.5 h-1.5 rounded-full bg-[#166534]"></span> 100% Terverifikasi
          </span>
          <router-link to="/rooms" class="text-zinc-500 hover:text-zinc-900 font-mono text-[11px] flex items-center gap-0.5 transition-colors">
            <span>Katalog</span>
            <ChevronRight class="w-3 h-3" />
          </router-link>
        </div>
      </div>

      <!-- Card 2: Today's Bookings -->
      <div class="bg-white border border-zinc-200/80 rounded-2xl p-5 hover:border-zinc-300 hover:shadow-subtle transition-all duration-200 flex flex-col justify-between">
        <div>
          <div class="flex items-center justify-between text-zinc-400 mb-3">
            <span class="text-[11px] font-mono uppercase tracking-wider text-zinc-500 font-semibold">Agenda Hari Ini</span>
            <div class="w-7 h-7 rounded-lg bg-zinc-100 text-zinc-700 flex items-center justify-center">
              <Calendar class="w-3.5 h-3.5" />
            </div>
          </div>
          <div class="flex items-baseline gap-2">
            <span class="text-3xl font-extrabold text-zinc-950 font-mono tracking-tight">{{ todayBookingsCount }}</span>
            <span class="text-xs text-zinc-400 font-mono">Sesi Terdaftar</span>
          </div>
        </div>
        <div class="mt-4 pt-3 border-t border-zinc-100 flex items-center justify-between text-xs text-zinc-500">
          <span class="font-mono text-[11px]">Rentang 08:00 - 18:00</span>
          <Sparkles class="w-3.5 h-3.5 text-zinc-400" />
        </div>
      </div>

      <!-- Card 3: Pending Approvals -->
      <div class="bg-white border border-zinc-200/80 rounded-2xl p-5 hover:border-zinc-300 hover:shadow-subtle transition-all duration-200 flex flex-col justify-between">
        <div>
          <div class="flex items-center justify-between text-zinc-400 mb-3">
            <span class="text-[11px] font-mono uppercase tracking-wider text-zinc-500 font-semibold">Menunggu Review</span>
            <div class="w-7 h-7 rounded-lg bg-[#FEF9C3] text-[#854D0E] flex items-center justify-center">
              <Hourglass class="w-3.5 h-3.5" />
            </div>
          </div>
          <div class="flex items-baseline gap-2">
            <span class="text-3xl font-extrabold text-[#854D0E] font-mono tracking-tight">{{ pendingCount }}</span>
            <span class="text-xs px-2 py-0.5 rounded-full bg-[#FEF9C3] text-[#854D0E] font-mono text-[11px] font-semibold">Pending</span>
          </div>
        </div>
        <div class="mt-4 pt-3 border-t border-zinc-100 flex items-center justify-between text-xs">
          <router-link 
            v-if="isAdmin" 
            to="/admin/approvals" 
            class="text-zinc-950 hover:text-rose-700 font-semibold flex items-center gap-1 font-mono text-[11px] transition-colors"
          >
            <span>Review Antrean</span>
            <ArrowRight class="w-3 h-3" />
          </router-link>
          <span v-else class="text-zinc-400 font-mono text-[11px]">Oleh Admin Kampus</span>
        </div>
      </div>

      <!-- Card 4: Confirmed / Approved -->
      <div class="bg-white border border-zinc-200/80 rounded-2xl p-5 hover:border-zinc-300 hover:shadow-subtle transition-all duration-200 flex flex-col justify-between">
        <div>
          <div class="flex items-center justify-between text-zinc-400 mb-3">
            <span class="text-[11px] font-mono uppercase tracking-wider text-zinc-500 font-semibold">Terkonfirmasi</span>
            <div class="w-7 h-7 rounded-lg bg-[#EDF3EC] text-[#166534] flex items-center justify-center">
              <CheckCheck class="w-3.5 h-3.5" />
            </div>
          </div>
          <div class="flex items-baseline gap-2">
            <span class="text-3xl font-extrabold text-[#166534] font-mono tracking-tight">{{ approvedCount }}</span>
            <span class="text-xs px-2 py-0.5 rounded-full bg-[#EDF3EC] text-[#166534] font-mono text-[11px] font-semibold">Approved</span>
          </div>
        </div>
        <div class="mt-4 pt-3 border-t border-zinc-100 flex items-center justify-between text-xs text-zinc-400 font-mono text-[11px]">
          <span>Anti-Bentrok Validated</span>
          <ShieldCheck class="w-3.5 h-3.5 text-[#166534]" />
        </div>
      </div>
    </div>

    <!-- Interactive Visual Timeline (08:00 - 18:00) -->
    <TimelineMatrix 
      :rooms="rooms.filter(r => r.isActive)" 
      :bookings="bookings" 
      :selected-date="todayStr"
      @select-slot="handleSlotSelect"
    />

    <!-- Filterable Schedule Table -->
    <div class="bg-white border border-zinc-200/80 rounded-2xl overflow-hidden shadow-xs">
      
      <!-- Toolbar -->
      <div class="p-5 border-b border-zinc-200/80 flex flex-col md:flex-row md:items-center justify-between gap-4">
        
        <!-- Segmented Tabs -->
        <div class="flex items-center gap-1 bg-zinc-100 p-1 rounded-xl border border-zinc-200/60 text-xs font-medium text-zinc-600 w-fit">
          <button 
            type="button" 
            @click="tableFilter = 'all'" 
            :class="tableFilter === 'all' ? 'bg-white text-zinc-950 shadow-xs font-semibold' : 'hover:text-zinc-950'"
            class="px-3 py-1.5 rounded-lg transition-all"
          >
            Semua ({{ bookings.length }})
          </button>
          <button 
            type="button" 
            @click="tableFilter = 'Approved'" 
            :class="tableFilter === 'Approved' ? 'bg-white text-zinc-950 shadow-xs font-semibold' : 'hover:text-zinc-950'"
            class="px-3 py-1.5 rounded-lg transition-all flex items-center gap-1.5"
          >
            <span class="w-1.5 h-1.5 rounded-full bg-[#166534]"></span>
            Disetujui ({{ approvedCount }})
          </button>
          <button 
            type="button" 
            @click="tableFilter = 'Pending'" 
            :class="tableFilter === 'Pending' ? 'bg-white text-zinc-950 shadow-xs font-semibold' : 'hover:text-zinc-950'"
            class="px-3 py-1.5 rounded-lg transition-all flex items-center gap-1.5"
          >
            <span class="w-1.5 h-1.5 rounded-full bg-[#854D0E]"></span>
            Pending ({{ pendingCount }})
          </button>
          <button 
            type="button" 
            @click="tableFilter = 'Rejected'" 
            :class="tableFilter === 'Rejected' ? 'bg-white text-zinc-950 shadow-xs font-semibold' : 'hover:text-zinc-950'"
            class="px-3 py-1.5 rounded-lg transition-all flex items-center gap-1.5"
          >
            <span class="w-1.5 h-1.5 rounded-full bg-[#991B1B]"></span>
            Ditolak ({{ bookings.filter(b => b.status === BookingStatus.Rejected).length }})
          </button>
        </div>

        <!-- Search Input -->
        <div class="relative w-full md:w-64">
          <Search class="w-4 h-4 text-zinc-400 absolute left-3 top-1/2 -translate-y-1/2" />
          <input 
            type="text" 
            v-model="searchQuery" 
            placeholder="Cari peminjam, kode, agenda..." 
            class="w-full pl-9 pr-3 py-1.5 text-xs rounded-xl bg-zinc-50 border border-zinc-200/80 text-zinc-800 placeholder-zinc-400 focus:outline-none focus:bg-white focus:border-zinc-900 transition-all font-mono"
          />
        </div>
      </div>

      <!-- Table Body -->
      <div v-if="filteredBookings.length > 0" class="overflow-x-auto">
        <table class="w-full text-left border-collapse">
          <thead>
            <tr class="bg-zinc-50/75 border-b border-zinc-200/80 text-[11px] font-mono uppercase tracking-wider text-zinc-500">
              <th class="py-3 px-5">Referensi</th>
              <th class="py-3 px-5">Ruangan</th>
              <th class="py-3 px-5">Pemohon</th>
              <th class="py-3 px-5">Agenda / Keperluan</th>
              <th class="py-3 px-5">Jadwal Sesi</th>
              <th class="py-3 px-5">Status</th>
              <th class="py-3 px-5 text-right">Tindakan</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-zinc-100 text-xs text-zinc-700">
            <tr 
              v-for="b in filteredBookings" 
              :key="b.id"
              class="hover:bg-zinc-50/70 transition-colors"
            >
              <!-- Reference -->
              <td class="py-3.5 px-5 font-mono">
                <div class="flex items-center gap-1.5">
                  <span class="font-bold text-zinc-950">{{ b.bookingReference }}</span>
                  <button 
                    type="button" 
                    @click="copyRef(b.bookingReference)"
                    class="text-zinc-400 hover:text-zinc-950 p-0.5 rounded transition-colors" 
                    title="Salin ID Referensi"
                  >
                    <Check v-if="copiedRef === b.bookingReference" class="w-3 h-3 text-[#166534]" />
                    <Copy v-else class="w-3 h-3" />
                  </button>
                </div>
                <div class="text-[10px] text-zinc-400 font-mono">
                  {{ new Date(b.createdAt).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }) }} WIB
                </div>
              </td>

              <!-- Room -->
              <td class="py-3.5 px-5">
                <div class="font-semibold text-zinc-950">{{ b.room?.name || 'Ruangan #' + b.roomId }}</div>
                <div class="text-[11px] font-mono text-zinc-400 flex items-center gap-1">
                  <span>{{ b.room?.roomCode || '-' }}</span>
                  <span>&bull;</span>
                  <span>{{ b.room?.building || '-' }}</span>
                </div>
              </td>

              <!-- Borrower -->
              <td class="py-3.5 px-5">
                <div class="flex items-center gap-2">
                  <div class="w-6 h-6 rounded-md bg-zinc-100 text-zinc-700 font-mono text-[10px] font-bold flex items-center justify-center border border-zinc-200">
                    {{ b.borrowerName.slice(0, 2).toUpperCase() }}
                  </div>
                  <div>
                    <div class="font-medium text-zinc-950">{{ b.borrowerName }}</div>
                    <div class="text-[11px] text-zinc-400 font-mono">{{ b.borrowerEmail }}</div>
                  </div>
                </div>
              </td>

              <!-- Purpose -->
              <td class="py-3.5 px-5 max-w-xs truncate" :title="b.purpose">
                <span class="text-zinc-800">{{ b.purpose }}</span>
              </td>

              <!-- Schedule -->
              <td class="py-3.5 px-5 font-mono">
                <div class="font-semibold text-zinc-950">
                  {{ new Date(b.startTime).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }) }} — 
                  {{ new Date(b.endTime).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }) }}
                </div>
                <div class="text-[11px] text-zinc-400">
                  {{ ((new Date(b.endTime).getTime() - new Date(b.startTime).getTime()) / (1000 * 60 * 60)).toFixed(1) }} Jam
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

              <!-- Action -->
              <td class="py-3.5 px-5 text-right font-mono">
                <router-link 
                  v-if="b.status === BookingStatus.Pending && isAdmin" 
                  to="/admin/approvals" 
                  class="inline-flex items-center gap-1 px-2.5 py-1 rounded-lg bg-zinc-900 hover:bg-zinc-800 text-white text-[11px] font-medium transition-colors"
                >
                  <span>Review</span>
                  <ArrowRight class="w-3 h-3" />
                </router-link>
                <router-link 
                  v-else 
                  to="/bookings" 
                  class="inline-flex items-center gap-1 text-zinc-400 hover:text-zinc-900 text-[11px] transition-colors"
                >
                  <span>Detail</span>
                  <ChevronRight class="w-3 h-3" />
                </router-link>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <div v-else-if="!isLoading" class="py-12 text-center text-zinc-400">
        <Calendar class="w-8 h-8 mx-auto mb-2 opacity-30" />
        <div class="text-xs font-medium text-zinc-600">Tidak ada peminjaman sesuai filter saat ini</div>
        <div class="text-[11px] text-zinc-400 mt-0.5">Semua ruangan bebas dipesan.</div>
      </div>
    </div>
  </div>
</template>
