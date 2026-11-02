<script setup lang="ts">
import { ref, watch, computed } from 'vue';
import { api } from '../api/client';
import { currentUser, addToast } from '../stores/auth';
import type { Room, CreateBookingDto } from '../types';
import { X, Send, Activity, CheckCircle2, AlertOctagon, Clock, Loader2 } from 'lucide-vue-next';

const props = defineProps<{
  isOpen: boolean;
  initialRoomId?: number;
  initialDate?: string;
  initialHour?: number;
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

// Form fields
const roomId = ref<number | ''>('');
const borrowerName = ref('');
const borrowerEmail = ref('');
const purpose = ref('');
const startDate = ref('');
const startTimeHour = ref('09:00');
const endTimeHour = ref('11:00');

// Initialize with defaults or props
watch(() => props.isOpen, async (open) => {
  if (open) {
    serverError.value = '';
    // Load rooms
    try {
      rooms.value = await api.getRooms(true);
    } catch (e) {
      console.error(e);
    }

    // Default borrower info
    borrowerName.value = currentUser.value.fullName;
    borrowerEmail.value = currentUser.value.email;

    // Default dates
    const today = props.initialDate || new Date().toISOString().split('T')[0];
    startDate.value = today;

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

const isConflict = computed(() => {
  if (!bookedSlots.value.length) return false;
  const reqStart = new Date(computedStartTime.value).getTime();
  const reqEnd = new Date(computedEndTime.value).getTime();

  return bookedSlots.value.some(slot => {
    if (slot.status === 'Rejected' || slot.status === 'Cancelled') return false;
    const sStart = new Date(slot.startTime).getTime();
    const sEnd = new Date(slot.endTime).getTime();
    // Overlap: reqStart < sEnd && reqEnd > sStart
    return reqStart < sEnd && reqEnd > sStart;
  });
});

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
  <div v-if="isOpen" class="fixed inset-0 z-50 overflow-hidden">
    <!-- Backdrop blur -->
    <div 
      class="absolute inset-0 bg-zinc-950/40 backdrop-blur-xs transition-opacity"
      @click="emit('close')"
    ></div>

    <div class="fixed inset-y-0 right-0 max-w-full flex pl-10">
      <transition name="drawer" appear>
        <div class="w-screen max-w-md bg-white border-l border-zinc-200/80 shadow-2xl flex flex-col justify-between">
          
          <!-- Drawer Header -->
          <div class="p-6 border-b border-zinc-100 flex items-center justify-between">
            <div>
              <div class="text-[10px] font-mono text-zinc-400 uppercase tracking-wider font-semibold">Reservasi Baru</div>
              <h2 class="text-lg font-bold text-zinc-950 tracking-tight">Ajukan Peminjaman Ruangan</h2>
            </div>
            <button 
              @click="emit('close')" 
              class="p-1.5 rounded-lg text-zinc-400 hover:text-zinc-700 hover:bg-zinc-100 transition-colors"
            >
              <X class="w-5 h-5" />
            </button>
          </div>

          <!-- Drawer Body (Scrollable) -->
          <div class="p-6 space-y-5 overflow-y-auto flex-1 text-xs">
            
            <!-- Server Error Alert -->
            <div v-if="serverError" class="p-3.5 rounded-xl bg-[#FEE2E2] border border-[#FECACA] text-[#991B1B] flex items-start gap-2.5">
              <AlertOctagon class="w-4 h-4 flex-shrink-0 mt-0.5 text-[#991B1B]" />
              <div class="text-[11px] leading-tight font-medium">{{ serverError }}</div>
            </div>

            <!-- Room Selector -->
            <div>
              <label class="block text-[11px] font-mono uppercase tracking-wider font-semibold text-zinc-700 mb-1.5">
                Ruangan Target <span class="text-rose-500">*</span>
              </label>
              <select 
                v-model="roomId" 
                class="w-full px-3.5 py-2.5 rounded-xl bg-zinc-50 border border-zinc-200 text-zinc-900 text-xs focus:bg-white focus:border-zinc-900 focus:outline-none transition-all"
              >
                <option value="" disabled>— Pilih Ruangan —</option>
                <option v-for="r in rooms" :key="r.id" :value="r.id">
                  {{ r.roomCode }} &bull; {{ r.name }} ({{ r.capacity }} Kursi)
                </option>
              </select>
            </div>

            <!-- Borrower Info -->
            <div class="space-y-3">
              <div>
                <label class="block text-[11px] font-mono uppercase tracking-wider font-semibold text-zinc-700 mb-1.5">
                  Nama Pemohon <span class="text-rose-500">*</span>
                </label>
                <input 
                  type="text" 
                  v-model="borrowerName" 
                  required
                  placeholder="Nama Lengkap Pemohon / Organisasi" 
                  class="w-full px-3.5 py-2.5 rounded-xl bg-zinc-50 border border-zinc-200 text-zinc-900 text-xs focus:bg-white focus:border-zinc-900 focus:outline-none transition-all"
                />
              </div>

              <div>
                <label class="block text-[11px] font-mono uppercase tracking-wider font-semibold text-zinc-700 mb-1.5">
                  Email Kontak <span class="text-rose-500">*</span>
                </label>
                <input 
                  type="email" 
                  v-model="borrowerEmail" 
                  required
                  placeholder="nama@kampus.ac.id" 
                  class="w-full px-3.5 py-2.5 rounded-xl bg-zinc-50 border border-zinc-200 text-zinc-900 text-xs font-mono focus:bg-white focus:border-zinc-900 focus:outline-none transition-all"
                />
              </div>
            </div>

            <!-- Purpose -->
            <div>
              <label class="block text-[11px] font-mono uppercase tracking-wider font-semibold text-zinc-700 mb-1.5">
                Agenda / Keperluan <span class="text-rose-500">*</span>
              </label>
              <textarea 
                v-model="purpose" 
                rows="2" 
                required
                placeholder="Contoh: Sidang Yudisium / Workshop Pemrograman Web"
                class="w-full px-3.5 py-2.5 rounded-xl bg-zinc-50 border border-zinc-200 text-zinc-900 text-xs focus:bg-white focus:border-zinc-900 focus:outline-none transition-all"
              ></textarea>
            </div>

            <!-- Date and Timeslots -->
            <div class="space-y-3 pt-2 border-t border-zinc-100">
              <div>
                <label class="block text-[11px] font-mono uppercase tracking-wider font-semibold text-zinc-700 mb-1.5">
                  Tanggal Penggunaan <span class="text-rose-500">*</span>
                </label>
                <input 
                  type="date" 
                  v-model="startDate" 
                  required
                  class="w-full px-3.5 py-2.5 rounded-xl bg-zinc-50 border border-zinc-200 text-zinc-900 text-xs font-mono focus:bg-white focus:border-zinc-900 focus:outline-none transition-all"
                />
              </div>

              <div class="grid grid-cols-2 gap-3">
                <div>
                  <label class="block text-[11px] font-mono uppercase tracking-wider font-semibold text-zinc-700 mb-1.5">
                    Mulai <span class="text-rose-500">*</span>
                  </label>
                  <input 
                    type="time" 
                    v-model="startTimeHour" 
                    required
                    class="w-full px-3.5 py-2.5 rounded-xl bg-zinc-50 border border-zinc-200 text-zinc-900 text-xs font-mono focus:bg-white focus:border-zinc-900 focus:outline-none transition-all"
                  />
                </div>

                <div>
                  <label class="block text-[11px] font-mono uppercase tracking-wider font-semibold text-zinc-700 mb-1.5">
                    Selesai <span class="text-rose-500">*</span>
                  </label>
                  <input 
                    type="time" 
                    v-model="endTimeHour" 
                    required
                    class="w-full px-3.5 py-2.5 rounded-xl bg-zinc-50 border border-zinc-200 text-zinc-900 text-xs font-mono focus:bg-white focus:border-zinc-900 focus:outline-none transition-all"
                  />
                </div>
              </div>

              <!-- Duration Calculation Indicator -->
              <div class="flex items-center justify-between p-2.5 rounded-xl bg-zinc-50 border border-zinc-200/80 font-mono text-[11px]">
                <span class="text-zinc-500 flex items-center gap-1.5">
                  <Clock class="w-3.5 h-3.5 text-zinc-400" />
                  Durasi Sesi:
                </span>
                <span :class="durationHours >= 0.5 && durationHours <= 8 ? 'text-zinc-950 font-bold' : 'text-rose-600 font-bold'">
                  {{ durationHours > 0 ? durationHours.toFixed(1) + ' Jam' : 'Waktu tidak valid' }}
                </span>
              </div>
            </div>

            <!-- Live Anti-Conflict Inspector Card -->
            <div class="p-3.5 rounded-xl border text-xs" :class="isConflict ? 'bg-[#FEE2E2]/60 border-[#FECACA]' : 'bg-[#EDF3EC]/70 border-[#BBF7D0]'">
              <div class="flex items-center justify-between mb-1">
                <span class="font-bold flex items-center gap-1.5" :class="isConflict ? 'text-[#991B1B]' : 'text-[#166534]'">
                  <AlertOctagon v-if="isConflict" class="w-3.5 h-3.5" />
                  <CheckCircle2 v-else class="w-3.5 h-3.5" />
                  <span>{{ isConflict ? 'Jadwal Bentrok Terdeteksi!' : 'Slot Jam Tersedia' }}</span>
                </span>
                <Loader2 v-if="isCheckingAvailability" class="w-3.5 h-3.5 animate-spin text-zinc-500" />
              </div>
              <p class="text-[11px] leading-tight" :class="isConflict ? 'text-[#991B1B]/80' : 'text-[#166534]/80'">
                {{ isConflict 
                  ? 'Jam yang Anda pilih bertubrukan dengan peminjaman lain. Silakan ubah jam mulai atau selesai.' 
                  : 'Ruangan ini bebas dari peminjaman lain pada jam tersebut.' }}
              </p>
            </div>
          </div>

          <!-- Drawer Footer -->
          <div class="p-5 border-t border-zinc-100 flex items-center justify-end gap-3 bg-zinc-50/50">
            <button 
              type="button" 
              @click="emit('close')"
              class="px-4 py-2.5 rounded-xl border border-zinc-200 text-xs font-medium text-zinc-700 hover:bg-zinc-100 transition-colors"
            >
              Batal
            </button>
            <button 
              type="button" 
              @click="handleSubmit"
              :disabled="isSubmitting || isConflict || durationHours < 0.5 || durationHours > 8"
              class="px-5 py-2.5 rounded-xl bg-zinc-900 hover:bg-zinc-800 disabled:bg-zinc-300 text-white text-xs font-medium shadow-xs transition-all active:scale-[0.98] flex items-center gap-1.5"
            >
              <Loader2 v-if="isSubmitting" class="w-3.5 h-3.5 animate-spin" />
              <Send v-else class="w-3.5 h-3.5" />
              <span>{{ isSubmitting ? 'Mengirim...' : 'Kirim Pengajuan' }}</span>
            </button>
          </div>
        </div>
      </transition>
    </div>
  </div>
</template>
