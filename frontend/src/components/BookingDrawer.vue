<script setup lang="ts">
import { ref, watch, computed, onMounted, onUnmounted } from 'vue';
import { api } from '../api/client';
import { currentUser, addToast } from '../stores/auth';
import type { Room, CreateBookingDto } from '../types';
import {
  X,
  Send,
  CheckCircle2,
  AlertOctagon,
  Clock,
  Loader2,
  ChevronDown,
  ChevronUp,
  MapPin,
  Users,
  Calendar,
  Sparkles,
  Check,
  Zap,
  Info
} from 'lucide-vue-next';

const props = defineProps<{
  isOpen: boolean;
  initialRoomId?: number;
  initialHour?: number;
  initialDate?: string;
}>();

const emit = defineEmits<{
  (e: 'close'): void;
  (e: 'created'): void;
}>();

const rooms = ref<Room[]>([]);
const isSubmitting = ref(false);
const isCheckingAvailability = ref(false);
const serverError = ref('');
const bookedSlots = ref<any[]>([]);
const isRoomDropdownOpen = ref(false);

// Form fields
const roomId = ref<number | ''>('');
const borrowerName = ref('');
const borrowerEmail = ref('');
const purpose = ref('');
const startDate = ref('');
const startTimeHour = ref('09:00');
const endTimeHour = ref('11:00');

// Architectural room photography mapping
const roomImages: Record<number, string> = {
  1: 'https://images.unsplash.com/photo-1517502884422-41eaead166d4?auto=format&fit=crop&w=600&q=80',
  2: 'https://images.unsplash.com/photo-1581091226825-a6a2a5aee158?auto=format&fit=crop&w=600&q=80',
  3: 'https://images.unsplash.com/photo-1497366216548-37526070297c?auto=format&fit=crop&w=600&q=80',
  4: 'https://images.unsplash.com/photo-1541829070764-84a7d30dd3f3?auto=format&fit=crop&w=600&q=80',
};

function getRoomImg(id: number) {
  return roomImages[id] || 'https://images.unsplash.com/photo-1497366811353-6870744d04b2?auto=format&fit=crop&w=600&q=80';
}

onMounted(() => {
  window.addEventListener('keydown', handleKeyDown);
});

onUnmounted(() => {
  window.removeEventListener('keydown', handleKeyDown);
});

function handleKeyDown(e: KeyboardEvent) {
  if (e.key === 'Escape' && props.isOpen) {
    emit('close');
  }
}

const selectedRoom = computed(() => {
  return rooms.value.find(r => r.id === Number(roomId.value)) || null;
});

// Initialize with defaults or props
watch(() => props.isOpen, async (open) => {
  if (open) {
    serverError.value = '';
    isRoomDropdownOpen.value = false;

    // Load rooms
    try {
      rooms.value = await api.getRooms(true);
    } catch (e) {
      console.error(e);
    }

    // Default borrower info
    borrowerName.value = currentUser.value?.fullName || '';
    borrowerEmail.value = currentUser.value?.email || '';

    // Default dates: default to tomorrow for fresh requests
    const tomorrow = new Date();
    tomorrow.setDate(tomorrow.getDate() + 1);
    const tomorrowStr = tomorrow.toISOString().split('T')[0];
    const todayStr = new Date().toISOString().split('T')[0];

    const defaultDate = props.initialDate || (props.initialHour !== undefined ? todayStr : tomorrowStr);
    startDate.value = defaultDate;

    if (props.initialRoomId) {
      roomId.value = props.initialRoomId;
    } else if (rooms.value.length > 0 && !roomId.value) {
      roomId.value = rooms.value[0].id;
    }

    if (props.initialHour !== undefined) {
      const h = props.initialHour;
      startTimeHour.value = `${h.toString().padStart(2, '0')}:00`;
      endTimeHour.value = `${(h + 2).toString().padStart(2, '0')}:00`;
    }

    checkAvailability();
  }
});

// Watch room or date to verify live availability
watch([roomId, startDate], () => {
  if (props.isOpen && roomId.value && startDate.value) {
    checkAvailability();
  }
});

async function checkAvailability() {
  if (!roomId.value || !startDate.value) {
    bookedSlots.value = [];
    return;
  }

  isCheckingAvailability.value = true;
  try {
    const res = await api.getRoomAvailability(Number(roomId.value), startDate.value);
    bookedSlots.value = res.bookedSlots || [];
  } catch (e) {
    bookedSlots.value = [];
  } finally {
    isCheckingAvailability.value = false;
  }
}

function selectRoom(r: Room) {
  roomId.value = r.id;
  isRoomDropdownOpen.value = false;
}

// Duration and conflict computation
const computedStartTime = computed(() => `${startDate.value}T${startTimeHour.value}:00`);
const computedEndTime = computed(() => `${startDate.value}T${endTimeHour.value}:00`);

const durationHours = computed(() => {
  if (!startTimeHour.value || !endTimeHour.value) return 0;
  const [sh, sm] = startTimeHour.value.split(':').map(Number);
  const [eh, em] = endTimeHour.value.split(':').map(Number);
  const diff = (eh * 60 + em) - (sh * 60 + sm);
  return diff / 60;
});

// Quick duration setter
function setDuration(hours: number) {
  if (!startTimeHour.value) return;
  const [sh, sm] = startTimeHour.value.split(':').map(Number);
  const totalMin = sh * 60 + sm + hours * 60;
  const endH = Math.min(20, Math.floor(totalMin / 60));
  const endM = totalMin % 60;
  endTimeHour.value = `${endH.toString().padStart(2, '0')}:${endM.toString().padStart(2, '0')}`;
  addToast(`Durasi disetel ke ${hours} jam (${startTimeHour.value} – ${endTimeHour.value} WIB)`, 'info');
}

// Active booked slots on the selected date (excluding Rejected/Cancelled)
const activeBookedSlots = computed(() => {
  return bookedSlots.value.filter(slot => slot.status !== 'Rejected' && slot.status !== 'Cancelled');
});

const isConflict = computed(() => {
  if (!activeBookedSlots.value.length) return false;
  const reqStart = new Date(computedStartTime.value).getTime();
  const reqEnd = new Date(computedEndTime.value).getTime();

  return activeBookedSlots.value.some(slot => {
    const sStart = new Date(slot.startTime).getTime();
    const sEnd = new Date(slot.endTime).getTime();
    // Overlap: reqStart < sEnd && reqEnd > sStart
    return reqStart < sEnd && reqEnd > sStart;
  });
});

// Mini-timeline position calculation
function getTimelinePos(startIso: string, endIso: string) {
  const start = new Date(startIso);
  const end = new Date(endIso);
  const startH = start.getHours() + start.getMinutes() / 60;
  const endH = end.getHours() + end.getMinutes() / 60;
  const left = Math.max(0, (startH - 8) / 10) * 100;
  const width = Math.min(100 - left, ((endH - startH) / 10) * 100);
  return { left: `${left.toFixed(1)}%`, width: `${width.toFixed(1)}%` };
}

const currentSelectionTimelinePos = computed(() => {
  if (!startTimeHour.value || !endTimeHour.value) return null;
  const [sh, sm] = startTimeHour.value.split(':').map(Number);
  const [eh, em] = endTimeHour.value.split(':').map(Number);
  const startH = sh + sm / 60;
  const endH = eh + em / 60;
  if (endH <= startH) return null;
  const left = Math.max(0, (startH - 8) / 10) * 100;
  const width = Math.min(100 - left, ((endH - startH) / 10) * 100);
  return { left: `${left.toFixed(1)}%`, width: `${width.toFixed(1)}%` };
});

// Smart slot suggestions based on free intervals
const availableSuggestions = computed(() => {
  const occupied = activeBookedSlots.value.map(s => {
    const start = new Date(s.startTime);
    const end = new Date(s.endTime);
    return {
      startH: start.getHours() + start.getMinutes() / 60,
      endH: end.getHours() + end.getMinutes() / 60,
    };
  });

  const candidates = [
    { start: '08:00', end: '10:00', startH: 8, endH: 10, label: '08:00 – 10:00' },
    { start: '10:00', end: '12:00', startH: 10, endH: 12, label: '10:00 – 12:00' },
    { start: '13:00', end: '15:00', startH: 13, endH: 15, label: '13:00 – 15:00' },
    { start: '15:00', end: '17:00', startH: 15, endH: 17, label: '15:00 – 17:00' },
    { start: '09:00', end: '11:00', startH: 9, endH: 11, label: '09:00 – 11:00' },
    { start: '14:00', end: '16:00', startH: 14, endH: 16, label: '14:00 – 16:00' },
  ];

  const free: { start: string; end: string; label: string }[] = [];
  for (const cand of candidates) {
    const hasCollision = occupied.some(occ => cand.startH < occ.endH && cand.endH > occ.startH);
    if (!hasCollision) {
      free.push({ start: cand.start, end: cand.end, label: cand.label });
    }
  }

  return free.slice(0, 3);
});

function applySlotSuggestion(slot: { start: string; end: string }) {
  startTimeHour.value = slot.start;
  endTimeHour.value = slot.end;
  addToast(`Jam disetel ke ${slot.start} – ${slot.end} WIB`, 'info');
}

function formatSlotTime(iso: string) {
  return new Intl.DateTimeFormat('id-ID', {
    hour: '2-digit',
    minute: '2-digit',
    hour12: false
  }).format(new Date(iso));
}

function formatSelectedDate(dateStr: string) {
  if (!dateStr) return '';
  return new Intl.DateTimeFormat('id-ID', {
    weekday: 'long',
    day: 'numeric',
    month: 'long',
    year: 'numeric'
  }).format(new Date(dateStr));
}

async function handleSubmit() {
  serverError.value = '';

  if (!roomId.value) {
    serverError.value = 'Silakan pilih ruangan.';
    return;
  }

  if (durationHours.value < 0.5) {
    serverError.value = 'Durasi peminjaman minimal adalah 30 menit.';
    return;
  }

  if (durationHours.value > 8) {
    serverError.value = 'Durasi peminjaman maksimal adalah 8 jam.';
    return;
  }

  if (isConflict.value) {
    serverError.value = 'Jadwal yang Anda pilih bentrok dengan sesi peminjaman lain pada ruangan ini.';
    return;
  }

  isSubmitting.value = true;

  try {
    const dto: CreateBookingDto = {
      roomId: Number(roomId.value),
      borrowerName: borrowerName.value.trim(),
      borrowerEmail: borrowerEmail.value.trim(),
      purpose: purpose.value.trim(),
      startTime: computedStartTime.value,
      endTime: computedEndTime.value,
    };

    const created = await api.createBooking(dto);
    addToast(`Peminjaman ${created.bookingReference} berhasil diajukan! Menunggu persetujuan admin.`, 'success');
    emit('created');
    emit('close');
  } catch (err: any) {
    serverError.value = err.message || 'Gagal mengajukan peminjaman. Silakan coba lagi.';
    addToast(serverError.value, 'error');
  } finally {
    isSubmitting.value = false;
  }
}
</script>

<template>
  <div v-if="isOpen" class="fixed inset-0 z-50 overflow-hidden" role="dialog" aria-modal="true" aria-labelledby="booking-drawer-title">

    <!-- Blur Backdrop -->
    <div
      class="absolute inset-0 bg-[#182019]/40 backdrop-blur-sm transition-opacity duration-200"
      @click="emit('close')"
    ></div>

    <div class="fixed inset-y-0 right-0 flex max-w-full pl-3 sm:p-3 sm:pl-10">
      <transition name="drawer" appear>
        <!-- Quiet Campus Atelier Drawer Container -->
        <div id="booking-drawer-container" class="floating-surface relative flex w-screen max-w-lg flex-col justify-between overflow-hidden rounded-l-[26px] text-[#182019] sm:rounded-[26px] bg-[#FBFAF7] border-l border-[#D9D8D1] shadow-2xl">

          <!-- Header -->
          <div class="flex items-center justify-between border-b border-[#D9D8D1] bg-white p-5 sm:p-6">
            <div>
              <p class="text-xs font-mono uppercase tracking-widest text-[#A45F35] font-bold">Reservasi fasilitas</p>
              <h2 id="booking-drawer-title" class="mt-1 font-serif text-3xl font-bold leading-tight text-[#182019]">
                Ajukan Peminjaman
              </h2>
            </div>
            <button
              id="close-booking-drawer-btn"
              @click="emit('close')"
              class="flex h-10 w-10 items-center justify-center rounded-xl bg-[#F1EFE9] text-[#5F675F] transition-colors hover:bg-[#E8E5DC] hover:text-[#182019] active:scale-95"
              aria-label="Tutup panel peminjaman"
            >
              <X class="w-4 h-4" />
            </button>
          </div>

          <!-- Scrollable Body -->
          <div class="flex-1 space-y-5 overflow-y-auto p-5 text-sm sm:p-6">

            <!-- Server Error Alert -->
            <div v-if="serverError" class="status-danger flex items-start gap-2.5 rounded-xl border p-3.5" role="alert">
              <AlertOctagon class="w-4 h-4 flex-shrink-0 mt-0.5 text-[#A43D3D]" />
              <div class="text-[11px] leading-tight font-medium">{{ serverError }}</div>
            </div>

            <!-- 1. VISUAL ROOM SELECTOR (WITH THUMBNAIL, NAME & CAPACITY) -->
            <div class="space-y-2">
              <div class="flex items-center justify-between">
                <label class="block text-[11px] font-mono uppercase tracking-wider font-semibold text-[#5F675F]">
                  Ruangan Pilihan <span class="text-[#A43D3D]">*</span>
                </label>
                <span class="text-[10px] font-mono text-[#8A908A]">
                  {{ rooms.length }} Ruangan Terdaftar
                </span>
              </div>

              <!-- Selected Room Display Card -->
              <div
                v-if="selectedRoom"
                id="booking-room-selector"
                @click="isRoomDropdownOpen = !isRoomDropdownOpen"
                class="group flex cursor-pointer items-center justify-between gap-3 rounded-[18px] border border-[#D9D8D1] bg-white p-3.5 transition-all duration-200 hover:border-[#214C38] hover:shadow-xs active:scale-[0.99]"
                role="button"
                tabindex="0"
                :aria-expanded="isRoomDropdownOpen"
                aria-controls="booking-room-options"
                aria-haspopup="true"
                @keydown.enter="isRoomDropdownOpen = !isRoomDropdownOpen"
                @keydown.space.prevent="isRoomDropdownOpen = !isRoomDropdownOpen"
              >
                <div class="flex items-center gap-3.5">
                  <!-- Thumbnail photo -->
                  <div class="w-16 h-16 rounded-xl overflow-hidden bg-[#F1EFE9] border border-[#D9D8D1] flex-shrink-0 relative">
                    <img
                      :src="getRoomImg(selectedRoom.id)"
                      :alt="selectedRoom.name"
                      class="w-full h-full object-cover group-hover:scale-105 transition-transform duration-300"
                    />
                  </div>
                  <div class="space-y-1">
                    <div class="flex items-center gap-2">
                      <span class="font-serif font-bold text-base text-[#182019] group-hover:text-[#214C38] transition-colors">
                        {{ selectedRoom.name }}
                      </span>
                    </div>
                    <div class="text-[11px] text-[#5F675F] flex items-center gap-1.5 font-sans">
                      <MapPin class="w-3.5 h-3.5 text-[#8A908A]" />
                      <span>{{ selectedRoom.building }}</span>
                    </div>
                    <div class="flex items-center gap-2 pt-0.5 font-mono text-[10px]">
                      <span class="bg-[#F1EFE9] text-[#182019] px-2 py-0.5 rounded-md border border-[#D9D8D1] font-semibold">
                        {{ selectedRoom.roomCode }}
                      </span>
                      <span class="text-[#214C38] bg-[#E4EEE8] px-2 py-0.5 rounded-md border border-[#C3D9CD] font-semibold flex items-center gap-1">
                        <Users class="w-3 h-3" />
                        <span>{{ selectedRoom.capacity }} Kursi</span>
                      </span>
                    </div>
                  </div>
                </div>

                <div class="flex items-center gap-1 text-[11px] font-mono text-[#8A908A] group-hover:text-[#182019] pr-2">
                  <span>{{ isRoomDropdownOpen ? 'Tutup' : 'Ganti' }}</span>
                  <ChevronUp v-if="isRoomDropdownOpen" class="w-4 h-4" />
                  <ChevronDown v-else class="w-4 h-4" />
                </div>
              </div>

              <!-- Collapsible Visual Room Picker -->
              <transition name="modal-fade">
                <div
                  v-if="isRoomDropdownOpen"
                  id="booking-room-options"
                  aria-label="Pilih ruangan"
                  class="mt-2 p-2.5 rounded-2xl bg-white border border-[#D9D8D1] shadow-xl space-y-2 max-h-64 overflow-y-auto"
                >
                  <button
                    v-for="r in rooms"
                    :key="r.id"
                    type="button"
                    @click="selectRoom(r)"
                    class="w-full p-2.5 rounded-xl border transition-all cursor-pointer flex items-center justify-between gap-3 text-left"
                    :class="r.id === Number(roomId)
                      ? 'bg-[#E4EEE8] border-[#214C38] shadow-xs'
                      : 'bg-white hover:bg-[#FBFAF7] border-[#D9D8D1] hover:border-[#182019]'"
                  >
                    <div class="flex items-center gap-3">
                      <img
                        :src="getRoomImg(r.id)"
                        :alt="r.name"
                        class="w-12 h-12 rounded-lg object-cover border border-[#D9D8D1] flex-shrink-0"
                      />
                      <div>
                        <div class="font-serif font-bold text-sm text-[#182019]">
                          {{ r.name }}
                        </div>
                        <div class="text-[10px] text-[#6E766F] font-mono">
                          {{ r.roomCode }} &bull; {{ r.building }}
                        </div>
                      </div>
                    </div>

                    <div class="flex items-center gap-2 font-mono text-[10px]">
                      <span class="px-2 py-0.5 rounded bg-[#F1EFE9] border border-[#D9D8D1] text-[#182019] font-semibold">
                        {{ r.capacity }} Kursi
                      </span>
                      <Check v-if="r.id === Number(roomId)" class="w-4 h-4 text-[#214C38]" />
                    </div>
                  </button>
                </div>
              </transition>
            </div>

            <!-- 2. BORROWER DETAILS -->
            <div class="surface-card space-y-3.5 p-4 rounded-2xl border border-[#D9D8D1] bg-white">
              <div class="border-b border-[#D9D8D1] pb-2 text-xs font-mono uppercase tracking-wider font-bold text-[#182019]">
                Informasi Pemohon
              </div>

              <div>
                <label for="booking-borrower-name" class="mb-1.5 block text-xs font-semibold text-[#3E473F]">
                  Nama Lengkap / Organisasi <span class="text-[#A43D3D]">*</span>
                </label>
                <input
                  id="booking-borrower-name"
                  name="booking-borrower-name"
                  type="text"
                  v-model="borrowerName"
                  required
                  placeholder="Nama Pemohon"
                  class="halden-input w-full px-3.5 py-2 text-xs"
                />
              </div>

              <div>
                <label for="booking-borrower-email" class="mb-1.5 block text-xs font-semibold text-[#3E473F]">
                  Email Kontak <span class="text-[#A43D3D]">*</span>
                </label>
                <input
                  id="booking-borrower-email"
                  name="booking-borrower-email"
                  type="email"
                  v-model="borrowerEmail"
                  required
                  placeholder="email@kampus.ac.id"
                  class="halden-input w-full px-3.5 py-2 text-xs font-mono"
                />
              </div>

              <div>
                <label for="booking-purpose" class="mb-1.5 block text-xs font-semibold text-[#3E473F]">
                  Agenda / Keperluan Acara <span class="text-[#A43D3D]">*</span>
                </label>
                <textarea
                  id="booking-purpose"
                  name="booking-purpose"
                  v-model="purpose"
                  rows="2"
                  required
                  placeholder="Contoh: Sidang Terbuka Skripsi / Rapat Koordinasi Tim Peneliti"
                  class="halden-input w-full resize-none px-3.5 py-2 text-xs"
                ></textarea>
              </div>
            </div>

            <!-- 3. DATE & LIVE REAL-TIME SCHEDULE AVAILABILITY -->
            <div class="surface-card space-y-4 p-4 rounded-2xl border border-[#D9D8D1] bg-white">

              <div class="flex items-center justify-between border-b border-[#D9D8D1] pb-2">
                <span class="text-[10px] font-mono uppercase tracking-widest text-[#6E766F] font-semibold">
                  Jadwal &amp; Pengecekan Okupansi Real-Time
                </span>
                <span v-if="isCheckingAvailability" class="inline-flex items-center gap-1 text-[10px] font-mono text-[#6E766F]">
                  <Loader2 class="w-3 h-3 animate-spin text-[#214C38]" />
                  <span>Memeriksa...</span>
                </span>
              </div>

              <!-- Date Input Field -->
              <div>
                <label for="booking-date" class="mb-1.5 block text-xs font-semibold text-[#3E473F]">
                  Tanggal Penggunaan <span class="text-[#A43D3D]">*</span>
                </label>
                <input
                  id="booking-date"
                  name="booking-date"
                  type="date"
                  v-model="startDate"
                  required
                  class="w-full px-3.5 py-2 halden-input text-xs font-mono"
                />
                <div v-if="startDate" class="text-[10px] font-mono text-[#6E766F] mt-1 capitalize">
                  {{ formatSelectedDate(startDate) }}
                </div>
              </div>

              <!-- INTERACTIVE MINI-TIMELINE STRIP (08:00 TO 18:00) -->
              <div class="space-y-1.5 pt-1">
                <div class="flex items-center justify-between text-[10px] font-mono text-[#6E766F]">
                  <span>Peta Okupansi (08:00 &ndash; 18:00 WIB):</span>
                  <span class="flex items-center gap-2">
                    <span class="inline-flex items-center gap-1"><span class="w-2 h-2 rounded bg-[#214C38]"></span> Pilihan Anda</span>
                    <span class="inline-flex items-center gap-1"><span class="w-2 h-2 rounded bg-[#F8EFD7] border border-[#E4CD95]"></span> Terisi</span>
                  </span>
                </div>

                <!-- 10-hour horizontal track -->
                <div class="relative h-7 w-full rounded-lg bg-[#F1EFE9] border border-[#D9D8D1] overflow-hidden flex">
                  <!-- Hour guidelines -->
                  <div v-for="i in 10" :key="i" class="flex-1 border-r border-[#D9D8D1]/70 h-full"></div>

                  <!-- Occupied Slots on the track -->
                  <div
                    v-for="(slot, i) in activeBookedSlots"
                    :key="i"
                    class="absolute inset-y-0.5 rounded bg-[#F8EFD7] border border-[#E4CD95] text-[9px] font-mono text-[#996515] flex items-center justify-center font-bold overflow-hidden"
                    :style="getTimelinePos(slot.startTime, slot.endTime)"
                    :title="`${formatSlotTime(slot.startTime)} - ${formatSlotTime(slot.endTime)} (${slot.borrowerName})`"
                  >
                    <span class="truncate px-1">{{ formatSlotTime(slot.startTime) }}</span>
                  </div>

                  <!-- Current User Selection on the track -->
                  <div
                    v-if="currentSelectionTimelinePos"
                    class="absolute inset-y-0.5 rounded border border-[#214C38] flex items-center justify-center text-[9px] font-mono text-white font-bold transition-all shadow-xs"
                    :class="isConflict ? 'bg-[#A43D3D]/90 border-[#A43D3D]' : 'bg-[#214C38]/90'"
                    :style="currentSelectionTimelinePos"
                  >
                    <span class="truncate px-1">{{ startTimeHour }} &ndash; {{ endTimeHour }}</span>
                  </div>
                </div>
              </div>

              <!-- LIVE OCCUPIED HOURS FEEDBACK -->
              <div class="pt-2 border-t border-[#D9D8D1] space-y-2.5">

                <!-- IF OCCUPIED BOOKINGS EXIST ON THIS DATE -->
                <div v-if="activeBookedSlots.length > 0" class="space-y-2">
                  <div class="flex items-center justify-between text-[11px] font-mono">
                    <span class="font-bold text-[#996515] flex items-center gap-1.5">
                      <Clock class="w-3.5 h-3.5 text-[#996515]" />
                      <span>Jadwal Sudah Terisi di Tanggal Ini:</span>
                    </span>
                    <span class="text-[10px] px-2 py-0.5 rounded-md bg-[#F8EFD7] border border-[#E4CD95] text-[#996515] font-semibold">
                      {{ activeBookedSlots.length }} Sesi Terjadwal
                    </span>
                  </div>

                  <!-- Slot Badges List -->
                  <div class="space-y-1.5">
                    <div
                      v-for="(slot, i) in activeBookedSlots"
                      :key="i"
                      class="p-2.5 rounded-xl bg-[#F8EFD7]/60 border border-[#E4CD95] flex items-center justify-between gap-2"
                    >
                      <div class="space-y-0.5">
                        <div class="font-mono text-xs font-bold text-[#996515] flex items-center gap-2">
                          <span>{{ formatSlotTime(slot.startTime) }} &ndash; {{ formatSlotTime(slot.endTime) }} WIB</span>
                          <span class="text-[9px] font-sans px-1.5 py-0.2 rounded bg-[#E4CD95] text-[#65440F] font-semibold">
                            {{ slot.status }}
                          </span>
                        </div>
                        <div class="text-[10px] text-[#5F675F] truncate max-w-xs font-sans">
                          Pemohon: <strong>{{ slot.borrowerName }}</strong> &bull; {{ slot.purpose }}
                        </div>
                      </div>
                      <span class="text-[10px] font-mono text-[#996515] bg-white px-2 py-0.5 rounded border border-[#E4CD95] flex-shrink-0 font-semibold">
                        Terisi
                      </span>
                    </div>
                  </div>

                  <!-- Quick suggestions of free hours -->
                  <div v-if="availableSuggestions.length > 0" class="pt-2">
                    <div class="text-[10px] font-mono text-[#5F675F] font-semibold mb-1.5 flex items-center gap-1">
                      <Sparkles class="w-3 h-3 text-[#214C38]" />
                      <span>Slot Jam Kosong yang Siap (Klik untuk langsung setel):</span>
                    </div>
                    <div class="flex flex-wrap gap-1.5 font-mono text-[11px]">
                      <button
                        v-for="(sug, idx) in availableSuggestions"
                        :key="idx"
                        type="button"
                        @click="applySlotSuggestion(sug)"
                        class="px-2.5 py-1 rounded-lg bg-[#E4EEE8] hover:bg-[#214C38] hover:text-white border border-[#C3D9CD] hover:border-[#214C38] text-[#214C38] shadow-xs transition-all active:scale-95 flex items-center gap-1 cursor-pointer"
                      >
                        <span>{{ sug.label }}</span>
                      </button>
                    </div>
                  </div>
                </div>

                <!-- IF ENTIRE DAY IS READY / ZERO CONFLICTS -->
                <div v-else-if="!isCheckingAvailability" class="p-3 rounded-xl bg-[#E4EEE8]/70 border border-[#C3D9CD] text-[#214C38] space-y-1.5">
                  <div class="flex items-center gap-2 font-bold text-xs font-mono">
                    <CheckCircle2 class="w-4 h-4 text-[#214C38]" />
                    <span>Ruangan Bebas Seharian (08:00 &ndash; 18:00 WIB)</span>
                  </div>
                  <p class="text-[11px] text-[#214C38]/90 leading-tight font-sans">
                    Belum ada yang meminjam ruangan ini pada tanggal yang dipilih. Seluruh slot jam operasional tersedia!
                  </p>

                  <!-- Quick preset buttons -->
                  <div class="flex flex-wrap gap-1.5 pt-1 font-mono text-[10px]">
                    <button
                      type="button"
                      @click="applySlotSuggestion({ start: '09:00', end: '11:00' })"
                      class="px-2.5 py-1 rounded-lg bg-white hover:bg-[#214C38] hover:text-white border border-[#C3D9CD] transition-all cursor-pointer font-semibold shadow-xs"
                    >
                      Pagi: 09:00&ndash;11:00
                    </button>
                    <button
                      type="button"
                      @click="applySlotSuggestion({ start: '13:00', end: '15:00' })"
                      class="px-2.5 py-1 rounded-lg bg-white hover:bg-[#214C38] hover:text-white border border-[#C3D9CD] transition-all cursor-pointer font-semibold shadow-xs"
                    >
                      Siang: 13:00&ndash;15:00
                    </button>
                    <button
                      type="button"
                      @click="applySlotSuggestion({ start: '15:00', end: '17:00' })"
                      class="px-2.5 py-1 rounded-lg bg-white hover:bg-[#214C38] hover:text-white border border-[#C3D9CD] transition-all cursor-pointer font-semibold shadow-xs"
                    >
                      Sore: 15:00&ndash;17:00
                    </button>
                  </div>
                </div>
              </div>

              <!-- Time Inputs: Mulai & Selesai -->
              <div class="pt-2 border-t border-[#D9D8D1] space-y-3">
                <div class="grid grid-cols-2 gap-3">
                  <div>
                    <label for="booking-start-time" class="mb-1.5 block text-xs font-semibold text-[#3E473F]">
                      Jam Mulai <span class="text-[#A43D3D]">*</span>
                    </label>
                    <select
                      id="booking-start-time"
                      name="booking-start-time"
                      v-model="startTimeHour"
                      class="w-full px-3 py-2 halden-input text-xs font-mono"
                    >
                      <option v-for="h in 10" :key="h" :value="`${(h + 7).toString().padStart(2, '0')}:00`">
                        {{ (h + 7).toString().padStart(2, '0') }}:00 WIB
                      </option>
                    </select>
                  </div>

                  <div>
                    <label for="booking-end-time" class="mb-1.5 block text-xs font-semibold text-[#3E473F]">
                      Jam Selesai <span class="text-[#A43D3D]">*</span>
                    </label>
                    <select
                      id="booking-end-time"
                      name="booking-end-time"
                      v-model="endTimeHour"
                      class="w-full px-3 py-2 halden-input text-xs font-mono"
                    >
                      <option v-for="h in 10" :key="h" :value="`${(h + 8).toString().padStart(2, '0')}:00`">
                        {{ (h + 8).toString().padStart(2, '0') }}:00 WIB
                      </option>
                    </select>
                  </div>
                </div>

                <!-- Quick Duration Presets (+1h, +2h, +3h) -->
                <div class="flex items-center gap-1.5 text-[10px] font-mono">
                  <span class="text-[#6E766F] font-bold">Durasi Cepat:</span>
                  <button
                    type="button"
                    @click="setDuration(1)"
                    class="px-2 py-0.5 rounded-md bg-[#F1EFE9] hover:bg-[#214C38] hover:text-white border border-[#D9D8D1] transition-all cursor-pointer"
                  >
                    +1 Jam
                  </button>
                  <button
                    type="button"
                    @click="setDuration(2)"
                    class="px-2 py-0.5 rounded-md bg-[#F1EFE9] hover:bg-[#214C38] hover:text-white border border-[#D9D8D1] transition-all cursor-pointer"
                  >
                    +2 Jam
                  </button>
                  <button
                    type="button"
                    @click="setDuration(3)"
                    class="px-2 py-0.5 rounded-md bg-[#F1EFE9] hover:bg-[#214C38] hover:text-white border border-[#D9D8D1] transition-all cursor-pointer"
                  >
                    +3 Jam
                  </button>
                  <button
                    type="button"
                    @click="setDuration(4)"
                    class="px-2 py-0.5 rounded-md bg-[#F1EFE9] hover:bg-[#214C38] hover:text-white border border-[#D9D8D1] transition-all cursor-pointer"
                  >
                    +4 Jam
                  </button>
                </div>

                <!-- Total Duration Badge & Conflict Live Alert -->
                <div class="flex items-center justify-between text-xs font-mono text-[#6E766F] pt-1">
                  <span class="flex items-center gap-1">
                    <Clock class="w-3.5 h-3.5 text-[#8A908A]" />
                    <span>Total Durasi:</span>
                  </span>
                  <span :class="durationHours < 0.5 || durationHours > 8 ? 'text-[#A43D3D] font-bold' : 'text-[#182019] font-bold'">
                    {{ durationHours > 0 ? durationHours.toFixed(1) + ' Jam' : 'Waktu Tidak Valid' }}
                  </span>
                </div>

                <!-- LIVE CONFLICT OR READY ALERT BOX -->
                <div
                  v-if="isConflict"
                  class="p-3.5 rounded-xl status-danger border text-xs space-y-1"
                >
                  <div class="flex items-center gap-2 font-bold text-xs text-[#A43D3D]">
                    <AlertOctagon class="w-4 h-4 flex-shrink-0" />
                    <span>Jadwal Bentrok Terdeteksi!</span>
                  </div>
                  <p class="text-[11px] text-[#A43D3D] leading-tight font-sans">
                    Jam <strong>{{ startTimeHour }} – {{ endTimeHour }} WIB</strong> berbenturan dengan sesi lain yang sudah terisi di atas. Silakan pilih jam lain yang bertanda hijau.
                  </p>
                </div>

                <div
                  v-else-if="durationHours >= 0.5 && durationHours <= 8 && !isCheckingAvailability"
                  class="p-3 rounded-xl status-success border text-xs flex items-center gap-2"
                >
                  <CheckCircle2 class="w-4 h-4 text-[#214C38] flex-shrink-0" />
                  <span class="text-[11px] font-medium leading-tight">
                    Jadwal Tersedia! Ruangan bebas digunakan pukul <strong>{{ startTimeHour }} – {{ endTimeHour }} WIB</strong>.
                  </span>
                </div>
              </div>
            </div>
          </div>

          <!-- Drawer Footer Actions -->
          <div class="flex items-center justify-end gap-3 border-t border-[#D9D8D1] bg-white p-4 sm:p-5">
            <button
              type="button"
              @click="emit('close')"
              class="halden-btn-secondary px-4 text-xs font-semibold"
            >
              Batal
            </button>
            <button
              type="button"
              id="submit-booking-drawer-btn"
              @click="handleSubmit"
              :disabled="isSubmitting || isConflict || durationHours < 0.5 || durationHours > 8"
              class="halden-btn-primary flex items-center gap-2 px-6 text-xs font-semibold disabled:cursor-not-allowed disabled:opacity-40 active:scale-95 transition-all"
            >
              <Loader2 v-if="isSubmitting" class="w-3.5 h-3.5 animate-spin" />
              <Send v-else class="w-3.5 h-3.5" />
              <span>{{ isSubmitting ? 'Mengirim...' : 'Kirim Pengajuan Reservasi' }}</span>
            </button>
          </div>
        </div>
      </transition>
    </div>
  </div>
</template>
