<script setup lang="ts">
import { ref, onMounted, onUnmounted, computed } from 'vue';
import { api } from '../api/client';
import type { Room, Booking } from '../types';
import { BookingStatus } from '../types';
import { currentUser, addToast } from '../stores/auth';
import TimelineMatrix from '../components/TimelineMatrix.vue';
import { 
  DoorOpen, 
  Calendar, 
  Hourglass, 
  CheckCheck, 
  ShieldCheck, 
  Plus, 
  ArrowRight, 
  Clock, 
  ChevronRight,
  Sparkles,
  AlertOctagon
} from 'lucide-vue-next';

const emit = defineEmits<{
  (e: 'open-booking', roomId?: number, hour?: number): void;
}>();

const rooms = ref<Room[]>([]);
const bookings = ref<Booking[]>([]);
const isLoading = ref(true);

const todayStr = new Date().toISOString().split('T')[0];
const formattedDate = new Intl.DateTimeFormat('id-ID', { dateStyle: 'full' }).format(new Date());

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
          <span class="inline-flex items-center gap-1.5 px-2.5 py-0.5 rounded-full text-[11px] font-mono font-medium bg-rose-50 text-rose-700 border border-rose-200">
            <span class="w-1.5 h-1.5 rounded-full bg-rose-600 animate-pulse"></span>
            PUSAT PENGENDALI FASILITAS KAMPUS
          </span>
          <span class="text-zinc-300">&bull;</span>
          <span class="text-xs font-mono text-zinc-500 capitalize">{{ formattedDate }}</span>
        </div>
        <h1 class="text-2xl sm:text-3xl font-extrabold tracking-tight text-zinc-950">
          Dashboard Operasional Administrator
        </h1>
        <p class="text-xs sm:text-sm text-zinc-500 mt-1 max-w-2xl">
          Monitoring seluruh sarana prasarana kampus, verifikasi alur persetujuan, dan pengawasan okupansi jadwal harian.
        </p>
      </div>

      <div class="flex items-center gap-2.5 flex-shrink-0">
        <router-link 
          to="/admin/approvals" 
          class="inline-flex items-center gap-2 px-4 py-2.5 rounded-xl bg-zinc-950 hover:bg-zinc-800 text-white text-xs font-medium shadow-xs transition-all active:scale-[0.98]"
        >
          <ShieldCheck class="w-4 h-4 text-rose-400" />
          <span>Antrean Approval ({{ pendingCount }})</span>
        </router-link>

        <router-link 
          to="/admin/rooms" 
          class="inline-flex items-center gap-2 px-4 py-2.5 rounded-xl bg-white hover:bg-zinc-50 text-zinc-800 text-xs font-medium border border-zinc-200/80 shadow-xs transition-all"
        >
          <DoorOpen class="w-4 h-4 text-zinc-500" />
          <span>Kelola Ruangan</span>
        </router-link>
      </div>
    </div>

    <!-- Priority Action Callout Banner if Pending requests exist -->
    <div v-if="pendingCount > 0" class="p-4 rounded-2xl bg-[#FEF9C3]/70 border border-[#FEF08A] flex flex-col sm:flex-row sm:items-center justify-between gap-3 text-xs text-[#854D0E] shadow-xs">
      <div class="flex items-center gap-3">
        <div class="w-8 h-8 rounded-xl bg-[#FEF9C3] border border-[#FEF08A] flex items-center justify-center flex-shrink-0">
          <Hourglass class="w-4 h-4 text-[#854D0E] animate-pulse" />
        </div>
        <div>
          <div class="font-bold text-zinc-950 text-xs">Ada {{ pendingCount }} Pengajuan Peminjaman Menunggu Tindakan Anda</div>
          <div class="text-[11px] text-[#854D0E]/90 mt-0.5">Segera verifikasi atau berikan catatan penolakan untuk mahasiswa/staff pemohon.</div>
        </div>
      </div>
      <router-link 
        to="/admin/approvals" 
        class="inline-flex items-center gap-1.5 px-3.5 py-2 rounded-xl bg-[#854D0E] hover:bg-[#713F12] text-white text-xs font-semibold shadow-xs transition-all flex-shrink-0"
      >
        <span>Proses Antrean Sekarang</span>
        <ArrowRight class="w-3.5 h-3.5" />
      </router-link>
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
            <span class="w-1.5 h-1.5 rounded-full bg-[#166534]"></span> Terverifikasi
          </span>
          <router-link to="/admin/rooms" class="text-zinc-500 hover:text-zinc-950 font-mono text-[11px] flex items-center gap-0.5 transition-colors">
            <span>Kelola</span>
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
            to="/admin/approvals" 
            class="text-zinc-950 hover:text-rose-700 font-semibold flex items-center gap-1 font-mono text-[11px] transition-colors"
          >
            <span>Buka Antrean</span>
            <ArrowRight class="w-3 h-3" />
          </router-link>
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

    <!-- Campus-Wide Gantt Timeline (08:00 - 18:00) -->
    <TimelineMatrix 
      :rooms="rooms.filter(r => r.isActive)" 
      :bookings="bookings" 
      :selected-date="todayStr"
      @select-slot="handleSlotSelect"
    />

    <!-- Today's Schedule Quick Table -->
    <div class="bg-white border border-zinc-200/80 rounded-2xl overflow-hidden shadow-xs">
      <div class="p-5 border-b border-zinc-100 flex items-center justify-between">
        <div>
          <h2 class="text-sm font-bold text-zinc-950 tracking-tight">Rangkuman Peminjaman Hari Ini</h2>
          <p class="text-xs text-zinc-500 mt-0.5">Daftar seluruh sesi peminjaman yang dijadwalkan pada hari ini.</p>
        </div>
        <router-link to="/admin/bookings" class="text-xs font-semibold text-zinc-600 hover:text-zinc-950 flex items-center gap-1 transition-colors">
          <span>Buka Audit Log Lengkap</span>
          <ChevronRight class="w-3.5 h-3.5" />
        </router-link>
      </div>

      <div v-if="bookings.length > 0" class="overflow-x-auto">
        <table class="w-full text-left border-collapse text-xs">
          <thead>
            <tr class="bg-zinc-50/75 border-b border-zinc-200/80 text-[11px] font-mono uppercase tracking-wider text-zinc-500">
              <th class="py-3 px-5">Referensi</th>
              <th class="py-3 px-5">Ruangan</th>
              <th class="py-3 px-5">Peminjam</th>
              <th class="py-3 px-5">Agenda</th>
              <th class="py-3 px-5">Waktu</th>
              <th class="py-3 px-5">Status</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-zinc-100 text-zinc-700">
            <tr v-for="b in bookings" :key="b.id" class="hover:bg-zinc-50/70 transition-colors">
              <td class="py-3 px-5 font-mono font-bold text-zinc-950">{{ b.bookingReference }}</td>
              <td class="py-3 px-5 font-medium text-zinc-900">{{ b.room?.name || 'Ruangan #' + b.roomId }}</td>
              <td class="py-3 px-5">
                <div class="font-medium text-zinc-950">{{ b.borrowerName }}</div>
                <div class="text-[10px] text-zinc-400 font-mono">{{ b.borrowerEmail }}</div>
              </td>
              <td class="py-3 px-5 max-w-xs truncate" :title="b.purpose">{{ b.purpose }}</td>
              <td class="py-3 px-5 font-mono">
                {{ new Date(b.startTime).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }) }} — 
                {{ new Date(b.endTime).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }) }}
              </td>
              <td class="py-3 px-5">
                <span 
                  v-if="b.status === BookingStatus.Approved"
                  class="inline-flex items-center gap-1 px-2 py-0.5 rounded-full text-[10px] font-mono font-medium bg-[#EDF3EC] text-[#166534] border border-[#BBF7D0]"
                >
                  Approved
                </span>
                <span 
                  v-else-if="b.status === BookingStatus.Pending"
                  class="inline-flex items-center gap-1 px-2 py-0.5 rounded-full text-[10px] font-mono font-medium bg-[#FEF9C3] text-[#854D0E] border border-[#FEF08A]"
                >
                  Pending
                </span>
                <span 
                  v-else-if="b.status === BookingStatus.Rejected"
                  class="inline-flex items-center gap-1 px-2 py-0.5 rounded-full text-[10px] font-mono font-medium bg-[#FEE2E2] text-[#991B1B] border border-[#FECACA]"
                >
                  Rejected
                </span>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  </div>
</template>
