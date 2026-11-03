<script setup lang="ts">
import { ref, onMounted, onUnmounted, computed, watch } from 'vue';
import { useRouter } from 'vue-router';
import { api } from '../api/client';
import type { Room, Booking } from '../types';
import { BookingStatus } from '../types';
import TimelineMatrix from '../components/TimelineMatrix.vue';
import { addToast } from '../stores/auth';
import {
  CalendarCheck,
  Hourglass,
  CheckCircle2,
  DoorOpen,
  ArrowRight,
  ShieldCheck,
  Activity,
  ChevronLeft,
  ChevronRight,
  Calendar,
  RotateCw,
  Pencil,
  Clock,
  X
} from 'lucide-vue-next';

const router = useRouter();

const rooms = ref<Room[]>([]);
const bookings = ref<Booking[]>([]);
const isLoading = ref(true);

// Configurable Operating Hours
const defaultStartHour = 8;
const defaultEndHour = 18;

const storedHours = localStorage.getItem('smartroom_operating_hours');
let initialStart = defaultStartHour;
let initialEnd = defaultEndHour;
if (storedHours) {
  try {
    const parsed = JSON.parse(storedHours);
    if (parsed.start >= 6 && parsed.end <= 24 && parsed.end > parsed.start) {
      initialStart = parsed.start;
      initialEnd = parsed.end;
    }
  } catch {}
}

const timelineStartHour = ref<number>(initialStart);
const timelineEndHour = ref<number>(initialEnd);

// Operating Hours Modal State
const isHoursModalOpen = ref(false);
const editStartHour = ref<number>(initialStart);
const editEndHour = ref<number>(initialEnd);

function openHoursModal() {
  editStartHour.value = timelineStartHour.value;
  editEndHour.value = timelineEndHour.value;
  isHoursModalOpen.value = true;
}

function closeHoursModal() {
  isHoursModalOpen.value = false;
}

function applyOperatingHours() {
  if (editEndHour.value <= editStartHour.value) {
    addToast('Jam selesai operasional harus lebih besar dari jam mulai.', 'error');
    return;
  }
  timelineStartHour.value = editStartHour.value;
  timelineEndHour.value = editEndHour.value;
  localStorage.setItem('smartroom_operating_hours', JSON.stringify({
    start: editStartHour.value,
    end: editEndHour.value,
  }));
  addToast(`Rentang jam operasional disetel ke ${editStartHour.value.toString().padStart(2, '0')}:00 – ${editEndHour.value.toString().padStart(2, '0')}:00 WIB`, 'success');
  isHoursModalOpen.value = false;
}

function setPresetHours(start: number, end: number) {
  editStartHour.value = start;
  editEndHour.value = end;
}

// Interactive Date Navigation
const todayIso = new Date().toISOString().split('T')[0];
const selectedDate = ref<string>(todayIso);

const isToday = computed(() => selectedDate.value === todayIso);

const formattedDateDisplay = computed(() => {
  const d = new Date(selectedDate.value);
  return new Intl.DateTimeFormat('id-ID', {
    weekday: 'long',
    day: 'numeric',
    month: 'long',
    year: 'numeric'
  }).format(d);
});

function goToPrevDay() {
  const d = new Date(selectedDate.value);
  d.setDate(d.getDate() - 1);
  selectedDate.value = d.toISOString().split('T')[0];
}

function goToNextDay() {
  const d = new Date(selectedDate.value);
  d.setDate(d.getDate() + 1);
  selectedDate.value = d.toISOString().split('T')[0];
}

function goToToday() {
  selectedDate.value = todayIso;
}

watch(selectedDate, () => {
  loadData();
});

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
      api.getBookings({ date: selectedDate.value }),
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
const activeRoomsCount = computed(() => rooms.value.filter(r => r.isActive).length);
const todayBookingsCount = computed(() => bookings.value.length);
const pendingBookingsCount = computed(() => bookings.value.filter(b => b.status === BookingStatus.Pending).length);
const approvedBookingsCount = computed(() => bookings.value.filter(b => b.status === BookingStatus.Approved).length);

function handleBookingClick(b: Booking) {
  if (b.status === BookingStatus.Pending) {
    router.push('/admin/approvals');
  } else {
    router.push('/admin/all-bookings');
  }
}

function handleRoomClick(_roomId: number) {
  router.push('/admin/rooms');
}
</script>

<template>
  <div class="space-y-8 lg:space-y-10">

    <!-- Hero Header / Operations Ticker (Quiet Campus Atelier Banner) -->
    <div class="page-hero flex flex-col justify-between gap-7 p-6 sm:p-9 md:flex-row md:items-end lg:p-11">
      <div class="space-y-2">
        <div class="page-kicker">
          <span>Meja operasi fasilitas</span>
          <span class="text-[#596159] font-medium">{{ formattedDateDisplay }}</span>
        </div>
        <h1 class="max-w-[18ch] text-4xl font-medium leading-[0.98] text-[#182019] sm:text-5xl lg:text-6xl">
          Kendali ruang dalam satu pandangan
        </h1>
        <p class="max-w-2xl text-sm leading-6 text-[#5F675F] sm:text-base">
          Pantau ketersediaan sarana, tuntaskan antrean persetujuan, dan inspeksi jadwal harian tanpa hambatan.
        </p>
      </div>

      <div class="flex items-center gap-2.5 flex-shrink-0">
        <router-link
          to="/admin/approvals"
          class="halden-btn-primary relative z-10 inline-flex items-center gap-2 px-5 text-sm active:scale-95 transition-all"
        >
          <ShieldCheck class="w-4 h-4" />
          <span>Antrean Persetujuan ({{ pendingBookingsCount }})</span>
        </router-link>
        <router-link
          to="/admin/rooms"
          class="halden-btn-secondary relative z-10 inline-flex items-center gap-2 px-5 text-sm active:scale-95 transition-all"
        >
          <DoorOpen class="w-4 h-4 text-[#6E766F]" />
          <span>Kelola Ruangan</span>
        </router-link>
      </div>
    </div>

    <!-- Active Pending Notification Banner -->
    <div
      v-if="pendingBookingsCount > 0"
      class="flex flex-col justify-between gap-4 rounded-[18px] border border-[#E4CD95] bg-[#F8EFD7] p-4 sm:flex-row sm:items-center sm:p-5"
    >
      <div class="flex items-center gap-3.5">
        <div class="flex h-10 w-10 flex-shrink-0 items-center justify-center rounded-xl bg-white text-[#996515] border border-[#E4CD95]">
          <Hourglass class="w-5 h-5" />
        </div>
        <div>
          <div class="text-sm font-bold text-[#65440F]">{{ pendingBookingsCount }} pengajuan menunggu keputusan</div>
          <div class="mt-0.5 text-xs text-[#76500F]">Tinjau detail agenda lalu setujui atau berikan alasan penolakan resmi.</div>
        </div>
      </div>
      <router-link
        to="/admin/approvals"
        class="inline-flex min-h-11 items-center gap-2 self-start rounded-xl bg-[#996515] hover:bg-[#76500F] px-4 text-sm font-bold text-white transition-colors active:scale-95 sm:self-auto shadow-xs"
      >
        <span>Proses Sekarang</span>
        <ArrowRight class="w-3.5 h-3.5" />
      </router-link>
    </div>

    <!-- 4 Bento Grid KPI Cards -->
    <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">

      <!-- Card 1: Active Rooms -->
      <div class="metric-card flex flex-col justify-between p-5 border border-[#D9D8D1]">
        <div>
          <div class="flex items-center justify-between text-[#6E766F] mb-2">
            <span class="text-xs font-mono uppercase tracking-wider font-semibold text-[#5F675F]">Ruangan aktif</span>
            <div class="w-8 h-8 rounded-lg bg-[#FBFAF7] text-[#182019] flex items-center justify-center border border-[#D9D8D1]">
              <DoorOpen class="w-4 h-4" />
            </div>
          </div>
          <div class="flex items-baseline gap-2">
            <span class="text-4xl font-serif font-bold text-[#182019] tracking-tight">{{ activeRoomsCount }}</span>
            <span class="text-xs text-[#6E766F]">unit siap pakai</span>
          </div>
        </div>
        <div class="mt-4 pt-3 border-t border-[#D9D8D1] flex items-center justify-between text-[11px] text-[#5F675F]">
          <span class="text-[#214C38] flex items-center gap-1 font-mono font-semibold">
            <span class="w-1.5 h-1.5 rounded-full bg-[#214C38]"></span> Terverifikasi
          </span>
          <router-link to="/admin/rooms" class="hover:text-[#182019] font-mono text-[11px] transition-colors font-semibold">Kelola &rarr;</router-link>
        </div>
      </div>

      <!-- Card 2: Today Bookings -->
      <div class="metric-card flex flex-col justify-between p-5 border border-[#D9D8D1]">
        <div>
          <div class="flex items-center justify-between text-[#6E766F] mb-2">
            <span class="text-xs font-mono uppercase tracking-wider font-semibold text-[#5F675F]">Agenda {{ isToday ? 'Hari Ini' : 'Terpilih' }}</span>
            <div class="w-8 h-8 rounded-lg bg-[#FBFAF7] text-[#182019] flex items-center justify-center border border-[#D9D8D1]">
              <CalendarCheck class="w-4 h-4" />
            </div>
          </div>
          <div class="flex items-baseline gap-2">
            <span class="text-4xl font-serif font-bold text-[#182019] tracking-tight">{{ todayBookingsCount }}</span>
            <span class="text-xs text-[#6E766F]">sesi terdaftar</span>
          </div>
        </div>
        <div class="mt-4 pt-3 border-t border-[#D9D8D1] flex items-center justify-between text-[11px] font-mono text-[#5F675F]">
          <span>08:00 &mdash; 18:00 WIB</span>
          <Activity class="w-3.5 h-3.5 text-[#6E766F]" />
        </div>
      </div>

      <!-- Card 3: Pending Approvals -->
      <div class="metric-card flex flex-col justify-between p-5 border border-[#D9D8D1]">
        <div>
          <div class="flex items-center justify-between text-[#6E766F] mb-2">
            <span class="text-xs font-mono uppercase tracking-wider font-semibold text-[#996515]">Menunggu review</span>
            <div class="w-8 h-8 rounded-lg bg-[#FEF3C7] text-[#92400E] flex items-center justify-center border border-[#FDE68A]">
              <Hourglass class="w-4 h-4" />
            </div>
          </div>
          <div class="flex items-baseline gap-2">
            <span class="text-4xl font-serif font-bold text-[#182019] tracking-tight">{{ pendingBookingsCount }}</span>
            <span class="text-xs text-[#92400E] font-mono bg-[#FEF3C7] px-2 py-0.5 rounded border border-[#FDE68A] font-semibold">Pending</span>
          </div>
        </div>
        <div class="mt-4 pt-3 border-t border-[#D9D8D1] flex items-center justify-between text-[11px] text-[#5F675F]">
          <router-link to="/admin/approvals" class="hover:text-[#182019] font-mono text-[11px] transition-colors font-semibold">Buka Antrean &rarr;</router-link>
        </div>
      </div>

      <!-- Card 4: Approved Bookings -->
      <div class="metric-card flex flex-col justify-between p-5 border border-[#D9D8D1]">
        <div>
          <div class="flex items-center justify-between text-[#6E766F] mb-2">
            <span class="text-xs font-mono uppercase tracking-wider font-semibold text-[#214C38]">Terkonfirmasi</span>
            <div class="w-8 h-8 rounded-lg bg-[#E4EEE8] text-[#214C38] flex items-center justify-center border border-[#C3D9CD]">
              <CheckCircle2 class="w-4 h-4" />
            </div>
          </div>
          <div class="flex items-baseline gap-2">
            <span class="text-4xl font-serif font-bold text-[#182019] tracking-tight">{{ approvedBookingsCount }}</span>
            <span class="text-xs text-[#214C38] font-mono bg-[#E4EEE8] px-2 py-0.5 rounded border border-[#C3D9CD] font-semibold">Approved</span>
          </div>
        </div>
        <div class="mt-4 pt-3 border-t border-[#D9D8D1] flex items-center justify-between text-[11px] text-[#5F675F]">
          <span>LINQ Overlap Valid</span>
          <ShieldCheck class="w-3.5 h-3.5 text-[#214C38]" />
        </div>
      </div>
    </div>

    <!-- Interactive Gantt Matrix Section (With Dynamic Date Navigator) -->
    <div class="surface-card space-y-5 p-5 sm:p-7 lg:p-8 rounded-[26px] border border-[#D9D8D1]">

      <!-- Header & Interactive Date Navigator -->
      <div class="flex flex-col lg:flex-row lg:items-center justify-between gap-4 pb-4 border-b border-[#D9D8D1]">
        <div>
          <div class="flex items-center gap-2.5">
            <h2 class="text-xl font-serif font-bold text-[#182019] tracking-tight">Matriks Okupansi Ruangan Harian</h2>
            <button
              id="edit-timeline-hours-btn"
              type="button"
              @click="openHoursModal"
              class="group inline-flex items-center gap-1.5 px-2.5 py-1 rounded-lg text-[10px] font-mono bg-[#FBFAF7] hover:bg-[#F1EFE9] text-[#182019] border border-[#D9D8D1] hover:border-[#214C38] transition-all cursor-pointer shadow-xs active:scale-95"
              title="Klik untuk ubah rentang jam operasional matriks"
            >
              <span>{{ timelineStartHour.toString().padStart(2, '0') }}:00 &mdash; {{ timelineEndHour.toString().padStart(2, '0') }}:00 WIB</span>
              <Pencil class="w-3 h-3 text-[#6E766F] group-hover:text-[#214C38] transition-colors" />
            </button>
          </div>
          <p class="text-xs text-[#5F675F] mt-1 font-sans">
            Menampilkan pemetaan jadwal ruangan untuk tanggal <strong>{{ formattedDateDisplay }}</strong>.
          </p>
        </div>

        <!-- Date Navigator Controls -->
        <div class="flex items-center gap-2 self-start lg:self-auto">
          <!-- Prev Day -->
          <button
            type="button"
            @click="goToPrevDay"
            title="Hari Sebelumnya"
            class="h-9 w-9 rounded-xl bg-white border border-[#D9D8D1] hover:bg-[#F1EFE9] flex items-center justify-center text-[#5F675F] transition-all active:scale-95"
          >
            <ChevronLeft class="w-4 h-4" />
          </button>

          <!-- Today Button -->
          <button
            type="button"
            @click="goToToday"
            class="h-9 px-3 rounded-xl border text-xs font-mono font-semibold transition-all active:scale-95"
            :class="isToday ? 'bg-[#214C38] text-white border-[#214C38]' : 'bg-white text-[#5F675F] border-[#D9D8D1] hover:border-[#182019]'"
          >
            Hari Ini
          </button>

          <!-- Date Picker Input -->
          <div class="relative">
            <input
              type="date"
              v-model="selectedDate"
              class="h-9 px-3 rounded-xl bg-white border border-[#D9D8D1] text-xs font-mono text-[#182019] focus:outline-none focus:border-[#214C38] cursor-pointer"
            />
          </div>

          <!-- Next Day -->
          <button
            type="button"
            @click="goToNextDay"
            title="Hari Berikutnya"
            class="h-9 w-9 rounded-xl bg-white border border-[#D9D8D1] hover:bg-[#F1EFE9] flex items-center justify-center text-[#5F675F] transition-all active:scale-95"
          >
            <ChevronRight class="w-4 h-4" />
          </button>

          <!-- Refresh Button -->
          <button
            type="button"
            @click="loadData"
            title="Perbarui Data"
            class="h-9 w-9 rounded-xl bg-white border border-[#D9D8D1] hover:bg-[#F1EFE9] flex items-center justify-center text-[#5F675F] transition-all active:scale-95 ml-1"
          >
            <RotateCw class="w-3.5 h-3.5" :class="isLoading ? 'animate-spin text-[#214C38]' : ''" />
          </button>
        </div>
      </div>

      <!-- Legend -->
      <div class="flex items-center justify-between text-xs font-mono text-[#5F675F]">
        <div class="flex items-center gap-3">
          <span class="flex items-center gap-1.5"><span class="w-2.5 h-2.5 rounded-full bg-[#214C38]"></span> Disetujui</span>
          <span class="flex items-center gap-1.5"><span class="w-2.5 h-2.5 rounded-full bg-[#996515]"></span> Pending</span>
          <span class="flex items-center gap-1.5"><span class="w-2.5 h-2.5 rounded-full bg-white border border-[#D9D8D1]"></span> Kosong</span>
        </div>
        <span class="text-[10px] text-[#8A908A] hidden sm:inline">Arahkan kursor ke balok jadwal untuk rincian pemohon</span>
      </div>

      <TimelineMatrix
        :rooms="rooms"
        :bookings="bookings"
        :is-admin="true"
        :selected-date="selectedDate"
        :start-hour="timelineStartHour"
        :end-hour="timelineEndHour"
        @select-booking="handleBookingClick"
        @select-room="handleRoomClick"
      />
    </div>

    <!-- MODAL PENGATURAN JAM OPERASIONAL MATRIKS -->
    <Teleport to="body">
      <div
        v-if="isHoursModalOpen"
        id="hours-config-modal"
        class="fixed inset-0 z-50 overflow-y-auto bg-[#182019]/40 backdrop-blur-sm flex items-center justify-center p-4"
        role="dialog"
        aria-modal="true"
      >
        <div class="relative w-full max-w-md rounded-[24px] bg-[#FBFAF7] border border-[#D9D8D1] shadow-2xl p-6 sm:p-7 space-y-5 text-[#182019]">

          <!-- Header -->
          <div class="flex items-center justify-between border-b border-[#D9D8D1] pb-3.5">
            <div class="flex items-center gap-2.5">
              <div class="w-9 h-9 rounded-xl bg-[#214C38] text-white flex items-center justify-center">
                <Clock class="w-5 h-5" />
              </div>
              <div>
                <p class="text-[10px] font-mono uppercase tracking-widest text-[#A45F35] font-bold">Kebijakan Fasilitas</p>
                <h3 class="font-serif text-lg font-bold text-[#182019]">Rentang Jam Operasional</h3>
              </div>
            </div>
            <button
              type="button"
              @click="closeHoursModal"
              class="w-8 h-8 rounded-lg bg-white border border-[#D9D8D1] hover:bg-[#F1EFE9] flex items-center justify-center text-[#5F675F] transition-colors"
            >
              <X class="w-4 h-4" />
            </button>
          </div>

          <p class="text-xs text-[#5F675F] leading-relaxed">
            Sesuaikan rentang waktu tampilan matriks harian sesuai arahan pimpinan atau kebutuhan shift operasional kampus.
          </p>

          <!-- Preset Buttons -->
          <div class="space-y-1.5">
            <span class="text-[10px] font-mono uppercase tracking-wider text-[#6E766F] font-semibold">Preset Cepat:</span>
            <div class="flex flex-wrap gap-1.5 text-xs font-mono">
              <button
                type="button"
                @click="setPresetHours(8, 18)"
                class="px-2.5 py-1 rounded-lg border transition-all cursor-pointer"
                :class="editStartHour === 8 && editEndHour === 18 ? 'bg-[#214C38] text-white border-[#214C38] font-bold' : 'bg-white text-[#5F675F] border-[#D9D8D1] hover:border-[#182019]'"
              >
                Standar (08:00–18:00)
              </button>
              <button
                type="button"
                @click="setPresetHours(7, 21)"
                class="px-2.5 py-1 rounded-lg border transition-all cursor-pointer"
                :class="editStartHour === 7 && editEndHour === 21 ? 'bg-[#214C38] text-white border-[#214C38] font-bold' : 'bg-white text-[#5F675F] border-[#D9D8D1] hover:border-[#182019]'"
              >
                Full Day (07:00–21:00)
              </button>
              <button
                type="button"
                @click="setPresetHours(8, 22)"
                class="px-2.5 py-1 rounded-lg border transition-all cursor-pointer"
                :class="editStartHour === 8 && editEndHour === 22 ? 'bg-[#214C38] text-white border-[#214C38] font-bold' : 'bg-white text-[#5F675F] border-[#D9D8D1] hover:border-[#182019]'"
              >
                Shift Malam (08:00–22:00)
              </button>
            </div>
          </div>

          <!-- Select Inputs -->
          <div class="grid grid-cols-2 gap-3.5 p-4 rounded-xl bg-white border border-[#D9D8D1]">
            <div>
              <label class="block text-xs font-semibold text-[#3E473F] mb-1.5 font-sans">
                Jam Mulai Operasional
              </label>
              <select
                v-model.number="editStartHour"
                class="w-full px-3 py-2 halden-input text-xs font-mono rounded-lg border border-[#D9D8D1]"
              >
                <option v-for="h in [6, 7, 8, 9, 10]" :key="h" :value="h">
                  {{ h.toString().padStart(2, '0') }}:00 WIB
                </option>
              </select>
            </div>

            <div>
              <label class="block text-xs font-semibold text-[#3E473F] mb-1.5 font-sans">
                Jam Selesai Operasional
              </label>
              <select
                v-model.number="editEndHour"
                class="w-full px-3 py-2 halden-input text-xs font-mono rounded-lg border border-[#D9D8D1]"
              >
                <option v-for="h in [16, 17, 18, 19, 20, 21, 22, 23]" :key="h" :value="h">
                  {{ h.toString().padStart(2, '0') }}:00 WIB
                </option>
              </select>
            </div>
          </div>

          <!-- Total Span Preview -->
          <div class="flex items-center justify-between text-xs font-mono text-[#6E766F] px-1">
            <span>Rentang Ditampilkan:</span>
            <span class="font-bold text-[#182019]">
              {{ Math.max(0, editEndHour - editStartHour) }} Jam Kerja
            </span>
          </div>

          <!-- Actions -->
          <div class="flex items-center justify-end gap-2.5 pt-2 border-t border-[#D9D8D1]">
            <button
              type="button"
              @click="closeHoursModal"
              class="halden-btn-secondary px-4 text-xs font-semibold"
            >
              Batal
            </button>
            <button
              id="save-hours-config-btn"
              type="button"
              @click="applyOperatingHours"
              class="halden-btn-primary px-5 text-xs font-semibold active:scale-95 transition-all"
            >
              Terapkan Jam Operasional
            </button>
          </div>
        </div>
      </div>
    </Teleport>
  </div>
</template>
