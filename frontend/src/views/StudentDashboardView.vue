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
  DoorOpen, 
  Users, 
  MapPin, 
  Clock, 
  Copy, 
  Check, 
  Sparkles,
  AlertOctagon,
  Calendar
} from 'lucide-vue-next';

const emit = defineEmits<{
  (e: 'open-booking', roomId?: number): void;
}>();

const myBookings = ref<Booking[]>([]);
const rooms = ref<Room[]>([]);
const isLoading = ref(true);
const activeTab = ref<'my-bookings' | 'rooms-catalog'>('my-bookings');
const copiedRef = ref<string | null>(null);

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
    const [bookingsData, roomsData] = await Promise.all([
      api.getBookings({ borrowerEmail: userEmail }),
      api.getRooms(true), // active rooms only for students
    ]);
    myBookings.value = bookingsData;
    rooms.value = roomsData;
  } catch (e: any) {
    addToast('Gagal memuat data mahasiswa: ' + e.message, 'error');
  } finally {
    isLoading.value = false;
  }
}

// Personal KPI counts
const pendingCount = computed(() => myBookings.value.filter(b => b.status === BookingStatus.Pending).length);
const approvedCount = computed(() => myBookings.value.filter(b => b.status === BookingStatus.Approved).length);
const rejectedCount = computed(() => myBookings.value.filter(b => b.status === BookingStatus.Rejected).length);

function copyRef(refCode: string) {
  navigator.clipboard.writeText(refCode);
  copiedRef.value = refCode;
  addToast(`Nomor referensi ${refCode} disalin!`, 'info');
  setTimeout(() => {
    if (copiedRef.value === refCode) copiedRef.value = null;
  }, 2000);
}

defineExpose({ loadData });
</script>

<template>
  <div class="space-y-8">
    
    <!-- Welcome Header for Student -->
    <div class="flex flex-col sm:flex-row sm:items-end justify-between gap-4 pb-6 border-b border-zinc-200/80">
      <div>
        <div class="flex items-center gap-2 mb-2">
          <span class="inline-flex items-center gap-1.5 px-2.5 py-0.5 rounded-full text-[11px] font-mono font-medium bg-emerald-50 text-emerald-800 border border-emerald-200">
            <span class="w-1.5 h-1.5 rounded-full bg-emerald-600"></span>
            PORTAL MAHASISWA &amp; CIVITAS
          </span>
          <span class="text-zinc-300">&bull;</span>
          <span class="text-xs font-mono text-zinc-500">{{ currentUser?.department }}</span>
        </div>
        <h1 class="text-2xl sm:text-3xl font-extrabold tracking-tight text-zinc-950">
          Selamat Datang, {{ currentUser?.fullName }}
        </h1>
        <p class="text-xs sm:text-sm text-zinc-500 mt-1 max-w-xl">
          Kelola permohonan reservasi ruangan kuliah, sidang, atau kegiatan organisasi kampus Anda di sini.
        </p>
      </div>

      <button 
        type="button" 
        id="student-dashboard-booking-btn"
        @click="emit('open-booking')"
        class="inline-flex items-center gap-2 px-4 py-2.5 rounded-xl bg-zinc-950 hover:bg-zinc-800 text-white text-xs font-medium shadow-xs transition-all active:scale-[0.98] flex-shrink-0"
      >
        <CalendarPlus class="w-4 h-4" />
        <span>+ Ajukan Peminjaman Baru</span>
      </button>
    </div>

    <!-- 3 Personal KPI Status Cards -->
    <div class="grid grid-cols-1 sm:grid-cols-3 gap-4">
      
      <!-- Pending Card -->
      <div class="bg-white border border-zinc-200/80 rounded-2xl p-5 shadow-xs flex flex-col justify-between">
        <div class="flex items-center justify-between text-zinc-400 mb-2">
          <span class="text-[11px] font-mono uppercase tracking-wider text-[#854D0E] font-semibold">Menunggu Review</span>
          <div class="w-7 h-7 rounded-lg bg-[#FEF9C3] text-[#854D0E] flex items-center justify-center">
            <Hourglass class="w-3.5 h-3.5" />
          </div>
        </div>
        <div class="flex items-baseline gap-2">
          <span class="text-3xl font-extrabold text-[#854D0E] font-mono tracking-tight">{{ pendingCount }}</span>
          <span class="text-xs text-zinc-400 font-mono">Pengajuan Aktif</span>
        </div>
        <div class="mt-4 pt-3 border-t border-zinc-100 text-[11px] text-zinc-400">
          Sedang ditinjau oleh Biro Sarana Admin
        </div>
      </div>

      <!-- Approved Card -->
      <div class="bg-white border border-zinc-200/80 rounded-2xl p-5 shadow-xs flex flex-col justify-between">
        <div class="flex items-center justify-between text-zinc-400 mb-2">
          <span class="text-[11px] font-mono uppercase tracking-wider text-[#166534] font-semibold">Disetujui (Siap Pakai)</span>
          <div class="w-7 h-7 rounded-lg bg-[#EDF3EC] text-[#166534] flex items-center justify-center">
            <CheckCircle2 class="w-3.5 h-3.5" />
          </div>
        </div>
        <div class="flex items-baseline gap-2">
          <span class="text-3xl font-extrabold text-[#166534] font-mono tracking-tight">{{ approvedCount }}</span>
          <span class="text-xs text-zinc-400 font-mono">Ruangan Terkonfirmasi</span>
        </div>
        <div class="mt-4 pt-3 border-t border-zinc-100 text-[11px] text-zinc-400">
          Jadwal resmi tercatat di sistem kampus
        </div>
      </div>

      <!-- Rejected Card -->
      <div class="bg-white border border-zinc-200/80 rounded-2xl p-5 shadow-xs flex flex-col justify-between">
        <div class="flex items-center justify-between text-zinc-400 mb-2">
          <span class="text-[11px] font-mono uppercase tracking-wider text-[#991B1B] font-semibold">Permohonan Ditolak</span>
          <div class="w-7 h-7 rounded-lg bg-[#FEE2E2] text-[#991B1B] flex items-center justify-center">
            <XCircle class="w-3.5 h-3.5" />
          </div>
        </div>
        <div class="flex items-baseline gap-2">
          <span class="text-3xl font-extrabold text-[#991B1B] font-mono tracking-tight">{{ rejectedCount }}</span>
          <span class="text-xs text-zinc-400 font-mono">Catatan Disertakan</span>
        </div>
        <div class="mt-4 pt-3 border-t border-zinc-100 text-[11px] text-zinc-400">
          Periksa alasan penolakan admin pada tabel
        </div>
      </div>
    </div>

    <!-- Segmented Navigation Tabs for Student Portal -->
    <div class="flex items-center gap-1 bg-zinc-100 p-1 rounded-xl border border-zinc-200/60 w-fit text-xs font-medium text-zinc-600">
      <button 
        type="button" 
        @click="activeTab = 'my-bookings'"
        class="px-4 py-2 rounded-lg transition-all"
        :class="activeTab === 'my-bookings' ? 'bg-white text-zinc-950 shadow-xs font-semibold' : 'hover:text-zinc-950'"
      >
        Daftar Peminjaman Saya ({{ myBookings.length }})
      </button>
      <button 
        type="button" 
        @click="activeTab = 'rooms-catalog'"
        class="px-4 py-2 rounded-lg transition-all"
        :class="activeTab === 'rooms-catalog' ? 'bg-white text-zinc-950 shadow-xs font-semibold' : 'hover:text-zinc-950'"
      >
        Katalog Ruangan Kampus ({{ rooms.length }})
      </button>
    </div>

    <!-- TAB 1: Peminjaman Saya -->
    <div v-if="activeTab === 'my-bookings'" class="bg-white border border-zinc-200/80 rounded-2xl overflow-hidden shadow-xs">
      <div class="p-5 border-b border-zinc-100 flex items-center justify-between">
        <div>
          <h2 class="text-sm font-bold text-zinc-950 tracking-tight">Riwayat Pengajuan Peminjaman Akun Anda</h2>
          <p class="text-xs text-zinc-500 mt-0.5">Pantau status persetujuan, jadwal penggunaan, serta catatan resmi dari administrator fasilitas.</p>
        </div>
        <span class="text-[11px] font-mono text-zinc-400">{{ currentUser?.email }}</span>
      </div>

      <div v-if="myBookings.length > 0" class="divide-y divide-zinc-100">
        <div 
          v-for="b in myBookings" 
          :key="b.id"
          class="p-5 hover:bg-zinc-50/70 transition-colors space-y-3"
        >
          <!-- Top Row: Reference, Room, Status Badge -->
          <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-2">
            <div class="flex items-center gap-2">
              <span class="font-mono font-bold text-zinc-950 text-xs">{{ b.bookingReference }}</span>
              <button 
                type="button" 
                @click="copyRef(b.bookingReference)"
                class="text-zinc-400 hover:text-zinc-950 p-0.5 rounded transition-colors"
                title="Salin ID"
              >
                <Check v-if="copiedRef === b.bookingReference" class="w-3 h-3 text-[#166534]" />
                <Copy v-else class="w-3 h-3" />
              </button>
              <span class="text-zinc-300">&bull;</span>
              <span class="text-xs font-semibold text-zinc-900">{{ b.room?.name || 'Ruangan #' + b.roomId }}</span>
              <span class="text-[11px] font-mono text-zinc-400">({{ b.room?.building }})</span>
            </div>

            <!-- Status Pill -->
            <div>
              <span 
                v-if="b.status === BookingStatus.Approved"
                class="inline-flex items-center gap-1.5 px-3 py-1 rounded-full text-xs font-mono font-medium bg-[#EDF3EC] text-[#166534] border border-[#BBF7D0]"
              >
                <span class="w-1.5 h-1.5 rounded-full bg-[#166534]"></span> Disetujui (Approved)
              </span>
              <span 
                v-else-if="b.status === BookingStatus.Pending"
                class="inline-flex items-center gap-1.5 px-3 py-1 rounded-full text-xs font-mono font-medium bg-[#FEF9C3] text-[#854D0E] border border-[#FEF08A]"
              >
                <span class="w-1.5 h-1.5 rounded-full bg-[#854D0E] animate-pulse"></span> Menunggu Persetujuan (Pending)
              </span>
              <span 
                v-else-if="b.status === BookingStatus.Rejected"
                class="inline-flex items-center gap-1.5 px-3 py-1 rounded-full text-xs font-mono font-medium bg-[#FEE2E2] text-[#991B1B] border border-[#FECACA]"
              >
                <span class="w-1.5 h-1.5 rounded-full bg-[#991B1B]"></span> Ditolak (Rejected)
              </span>
            </div>
          </div>

          <!-- Middle Row: Purpose and Timeslot -->
          <div class="grid grid-cols-1 sm:grid-cols-2 gap-3 text-xs text-zinc-600">
            <div>
              <div class="text-[10px] font-mono uppercase text-zinc-400 font-semibold mb-0.5">Keperluan / Agenda:</div>
              <div class="text-zinc-900 font-medium">{{ b.purpose }}</div>
            </div>

            <div class="font-mono text-xs">
              <div class="text-[10px] font-mono uppercase text-zinc-400 font-semibold mb-0.5">Jadwal Penggunaan:</div>
              <div class="text-zinc-900 font-semibold flex items-center gap-1.5">
                <Clock class="w-3.5 h-3.5 text-zinc-400" />
                <span>
                  {{ new Date(b.startTime).toLocaleDateString('id-ID', { day: '2-digit', month: 'short', year: 'numeric' }) }} &bull;
                  {{ new Date(b.startTime).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }) }} — 
                  {{ new Date(b.endTime).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }) }} WIB
                </span>
              </div>
            </div>
          </div>

          <!-- Bottom Row: Rejection Reason Alert if Rejected -->
          <div v-if="b.status === BookingStatus.Rejected && b.adminNotes" class="p-3 rounded-xl bg-[#FEE2E2]/60 border border-[#FECACA] text-xs text-[#991B1B] flex items-start gap-2">
            <AlertOctagon class="w-4 h-4 flex-shrink-0 mt-0.5" />
            <div>
              <strong class="font-mono uppercase text-[10px] tracking-wider">Alasan Penolakan dari Admin:</strong>
              <div class="mt-0.5">{{ b.adminNotes }}</div>
            </div>
          </div>

          <div v-else-if="b.status === BookingStatus.Approved && b.adminNotes" class="p-2.5 rounded-xl bg-[#EDF3EC]/60 border border-[#BBF7D0] text-xs text-[#166534] flex items-center gap-2">
            <CheckCircle2 class="w-3.5 h-3.5 flex-shrink-0" />
            <div class="text-[11px] font-mono">Catatan Admin: {{ b.adminNotes }}</div>
          </div>
        </div>
      </div>

      <!-- Empty State for Personal Bookings -->
      <div v-else-if="!isLoading" class="py-16 text-center text-zinc-400">
        <Calendar class="w-10 h-10 mx-auto mb-2 opacity-30" />
        <div class="text-sm font-semibold text-zinc-800">Belum Ada Pengajuan Peminjaman</div>
        <p class="text-xs text-zinc-400 mt-1 max-w-sm mx-auto">
          Anda belum memiliki riwayat peminjaman ruangan. Klik tombol di bawah untuk membuat pengajuan baru.
        </p>
        <button 
          type="button" 
          @click="emit('open-booking')"
          class="mt-4 inline-flex items-center gap-1.5 px-4 py-2 rounded-xl bg-zinc-950 text-white text-xs font-medium hover:bg-zinc-800 shadow-xs"
        >
          <CalendarPlus class="w-3.5 h-3.5" />
          <span>Ajukan Peminjaman Sekarang</span>
        </button>
      </div>
    </div>

    <!-- TAB 2: Katalog Ruangan Kampus -->
    <div v-else class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-5">
      <div 
        v-for="room in rooms" 
        :key="room.id"
        class="bg-white border border-zinc-200/80 rounded-2xl p-6 shadow-xs hover:border-zinc-300 hover:shadow-subtle transition-all duration-200 flex flex-col justify-between"
      >
        <div>
          <div class="flex items-center justify-between mb-3.5">
            <span class="px-2.5 py-1 rounded-md bg-zinc-100 text-zinc-800 font-mono text-[11px] font-bold border border-zinc-200/80">
              {{ room.roomCode }}
            </span>
            <span class="inline-flex items-center gap-1.5 px-2.5 py-0.5 rounded-full text-[11px] font-mono font-medium bg-[#EDF3EC] text-[#166534] border border-[#BBF7D0]">
              <span class="w-1.5 h-1.5 rounded-full bg-[#166534]"></span> Siap Dipesan
            </span>
          </div>

          <h3 class="text-base font-bold text-zinc-950 tracking-tight mb-1">{{ room.name }}</h3>
          <div class="flex items-center gap-1.5 text-xs text-zinc-500 mb-4">
            <MapPin class="w-3.5 h-3.5 text-zinc-400" />
            <span>{{ room.building }}</span>
          </div>

          <div class="p-3 rounded-xl bg-zinc-50 border border-zinc-200/60 flex items-center justify-between text-xs mb-4">
            <span class="text-zinc-500 font-medium">Kapasitas Maksimal</span>
            <span class="font-mono font-bold text-zinc-950 flex items-center gap-1.5">
              <Users class="w-3.5 h-3.5 text-zinc-600" />
              {{ room.capacity }} Orang
            </span>
          </div>

          <div class="mb-4">
            <div class="text-[10px] font-mono uppercase tracking-wider text-zinc-400 font-semibold mb-2">Fasilitas Resmi</div>
            <div class="flex flex-wrap gap-1.5">
              <template v-if="room.facilities">
                <span 
                  v-for="(f, idx) in room.facilities.split(',')" 
                  :key="idx"
                  class="px-2.5 py-0.5 rounded-md bg-zinc-100 text-zinc-700 text-[11px] border border-zinc-200/60 font-medium"
                >
                  {{ f.trim() }}
                </span>
              </template>
            </div>
          </div>
        </div>

        <div class="pt-4 border-t border-zinc-100 mt-auto">
          <button 
            type="button" 
            @click="emit('open-booking', room.id)"
            class="w-full py-2.5 px-3 rounded-xl text-center text-xs font-medium bg-zinc-950 hover:bg-zinc-800 text-white shadow-xs transition-all active:scale-[0.98] flex items-center justify-center gap-2"
          >
            <CalendarPlus class="w-3.5 h-3.5" />
            <span>Pesan Ruangan Ini</span>
          </button>
        </div>
      </div>
    </div>
  </div>
</template>
