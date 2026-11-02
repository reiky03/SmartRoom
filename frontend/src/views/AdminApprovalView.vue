<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import { api } from '../api/client';
import type { Booking, Room } from '../types';
import { BookingStatus } from '../types';
import { isAdmin, switchRole, addToast } from '../stores/auth';
import RejectModal from '../components/RejectModal.vue';
import { 
  ShieldCheck, 
  Calendar, 
  Check, 
  X, 
  Copy, 
  Clock, 
  ShieldAlert,
  Loader2 
} from 'lucide-vue-next';

const bookings = ref<Booking[]>([]);
const rooms = ref<Room[]>([]);
const isLoading = ref(true);

const filterRoomId = ref<number | ''>('');
const filterStatus = ref<number | ''>('');

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

async function handleApprove(booking: Booking) {
  try {
    await api.updateBookingStatus(booking.id, BookingStatus.Approved);
    booking.status = BookingStatus.Approved;
    addToast(`Peminjaman ${booking.bookingReference} berhasil disetujui!`, 'success');
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
    addToast(`Peminjaman ${b.bookingReference} telah ditolak dengan catatan.`, 'info');
  } catch (e: any) {
    addToast('Gagal menolak peminjaman: ' + e.message, 'error');
  }
}
</script>

<template>
  <div class="space-y-6">

    <!-- Non-Admin Guard View -->
    <div v-if="!isAdmin" class="max-w-md mx-auto my-12 text-center">
      <div class="p-8 rounded-2xl bg-white border border-zinc-200/80 shadow-xs">
        <div class="w-12 h-12 rounded-xl bg-rose-50 text-rose-600 border border-rose-200 flex items-center justify-center mx-auto mb-4">
          <ShieldAlert class="w-6 h-6" />
        </div>
        <div class="text-[10px] font-mono uppercase text-zinc-400 font-bold tracking-wider mb-1">
          ROLE RESTRICTED &bull; KHUSUS ADMINISTRATOR
        </div>
        <h2 class="text-lg font-bold text-zinc-950 tracking-tight mb-2">Halaman Terbatas Administrator</h2>
        <p class="text-xs text-zinc-500 mb-6 leading-relaxed">
          Akun Anda saat ini tidak memiliki izin untuk mengakses antrean verifikasi permohonan ruangan.
        </p>

        <div class="p-4 rounded-xl bg-zinc-50 border border-zinc-200 text-left mb-6 text-xs text-zinc-600">
          <div class="font-semibold text-zinc-900 mb-1">Ingin Menguji Akses Approval?</div>
          <p class="text-[11px] text-zinc-500 mb-3">Klik tombol cepat di bawah untuk beralih ke peran Administrator tanpa reload:</p>
          <button 
            type="button" 
            @click="switchRole('Admin')"
            class="w-full py-2 px-3 rounded-lg bg-zinc-900 hover:bg-zinc-800 text-white font-mono text-xs font-semibold shadow-xs transition-all flex items-center justify-center gap-2"
          >
            <span>👑 Beralih ke Admin Sekarang</span>
          </button>
        </div>

        <router-link to="/" class="text-xs font-medium text-zinc-600 hover:text-zinc-950 underline">
          Kembali ke Dashboard Utama
        </router-link>
      </div>
    </div>

    <!-- Admin Approval Workspace View -->
    <template v-else>
      <!-- Header Section -->
      <div class="flex flex-col sm:flex-row sm:items-end justify-between gap-4 pb-6 border-b border-zinc-200/80">
        <div>
          <div class="flex items-center gap-2 mb-1.5">
            <span class="inline-flex items-center gap-1.5 px-2.5 py-0.5 rounded-full text-[11px] font-mono font-medium bg-rose-50 text-rose-700 border border-rose-200">
              <span class="w-1.5 h-1.5 rounded-full bg-rose-600"></span>
              ROLE ADMINISTRATOR
            </span>
            <span class="text-zinc-300">&bull;</span>
            <span class="text-xs font-mono text-zinc-500">{{ pendingCount }} Menunggu Verifikasi</span>
          </div>
          <h1 class="text-2xl sm:text-3xl font-extrabold tracking-tight text-zinc-950">
            Panel Persetujuan Peminjaman
          </h1>
          <p class="text-xs sm:text-sm text-zinc-500 mt-1 max-w-xl">
            Tinjau pengajuan jadwal ruangan, verifikasi keperluan civitas, dan tentukan status persetujuan atau penolakan disertai catatan.
          </p>
        </div>

        <router-link 
          to="/bookings" 
          class="inline-flex items-center gap-2 px-3.5 py-2 rounded-xl border border-zinc-200 bg-white hover:bg-zinc-50 text-xs font-medium text-zinc-700 shadow-xs transition-colors flex-shrink-0"
        >
          <Calendar class="w-4 h-4 text-zinc-500" />
          <span>Semua Jadwal</span>
        </router-link>
      </div>

      <!-- Filter Card -->
      <div class="bg-white border border-zinc-200/80 rounded-2xl p-5 shadow-xs">
        <div class="grid grid-cols-1 sm:grid-cols-2 gap-3.5">
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

          <div>
            <label class="block text-[11px] font-mono uppercase tracking-wider font-semibold text-zinc-600 mb-1.5">Status Peminjaman</label>
            <select 
              v-model="filterStatus" 
              @change="loadBookings"
              class="w-full px-3 py-2 rounded-xl bg-zinc-50 border border-zinc-200 text-xs text-zinc-800 focus:bg-white focus:border-zinc-900 focus:outline-none transition-all"
            >
              <option value="">— Semua Status —</option>
              <option :value="BookingStatus.Pending">Menunggu Persetujuan (Pending)</option>
              <option :value="BookingStatus.Approved">Disetujui (Approved)</option>
              <option :value="BookingStatus.Rejected">Ditolak (Rejected)</option>
            </select>
          </div>
        </div>
      </div>

      <!-- Queue Table -->
      <div class="bg-white border border-zinc-200/80 rounded-2xl overflow-hidden shadow-xs">
        <div v-if="bookings.length > 0" class="overflow-x-auto">
          <table class="w-full text-left border-collapse">
            <thead>
              <tr class="bg-zinc-50/75 border-b border-zinc-200/80 text-[11px] font-mono uppercase tracking-wider text-zinc-500">
                <th class="py-3 px-5">Referensi</th>
                <th class="py-3 px-5">Ruangan</th>
                <th class="py-3 px-5">Pemohon</th>
                <th class="py-3 px-5">Agenda &amp; Jadwal</th>
                <th class="py-3 px-5">Status</th>
                <th class="py-3 px-5 text-right">Tindakan Admin</th>
              </tr>
            </thead>
            <tbody class="divide-y divide-zinc-100 text-xs text-zinc-700">
              <tr 
                v-for="b in bookings" 
                :key="b.id" 
                class="hover:bg-zinc-50/70 transition-colors"
                :class="{ 'bg-amber-50/20': b.status === BookingStatus.Pending }"
              >
                <!-- Reference -->
                <td class="py-3.5 px-5 font-mono">
                  <div class="font-bold text-zinc-950">{{ b.bookingReference }}</div>
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

                <!-- Purpose & Timeslot -->
                <td class="py-3.5 px-5">
                  <div class="font-medium text-zinc-950 max-w-xs truncate" :title="b.purpose">{{ b.purpose }}</div>
                  <div class="font-mono text-zinc-500 text-[11px] mt-0.5 flex items-center gap-1">
                    <Clock class="w-3 h-3 text-zinc-400" />
                    <span>
                      {{ new Date(b.startTime).toLocaleDateString('id-ID', { day: '2-digit', month: 'short' }) }} &bull; 
                      {{ new Date(b.startTime).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }) }} — 
                      {{ new Date(b.endTime).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }) }}
                    </span>
                  </div>
                  <div v-if="b.adminNotes" class="mt-1 p-1 px-2 rounded bg-zinc-100 text-zinc-600 text-[10px] font-mono border border-zinc-200/80">
                    Catatan: {{ b.adminNotes }}
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
                </td>

                <!-- Actions -->
                <td class="py-3.5 px-5 text-right font-mono">
                  <div class="flex items-center justify-end gap-1.5">
                    <button 
                      v-if="b.status !== BookingStatus.Approved"
                      type="button" 
                      @click="handleApprove(b)"
                      class="px-2.5 py-1.5 rounded-lg bg-[#EDF3EC] hover:bg-[#DCFCE7] text-[#166534] border border-[#BBF7D0] text-[11px] font-semibold transition-colors flex items-center gap-1 shadow-xs"
                    >
                      <Check class="w-3.5 h-3.5" />
                      <span>Setujui</span>
                    </button>

                    <button 
                      v-if="b.status !== BookingStatus.Rejected"
                      type="button" 
                      @click="openRejectModal(b)"
                      class="px-2.5 py-1.5 rounded-lg bg-[#FEE2E2] hover:bg-[#FECACA] text-[#991B1B] border border-[#FECACA] text-[11px] font-semibold transition-colors flex items-center gap-1 shadow-xs"
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
          <div class="text-xs font-semibold text-zinc-700">Semua Permohonan Selesai Diproses</div>
          <div class="text-[11px] text-zinc-400 mt-0.5">Tidak ada antrean yang membutuhkan persetujuan saat ini.</div>
        </div>
      </div>
    </template>

    <!-- Reject Confirmation Modal -->
    <RejectModal 
      :is-open="isRejectModalOpen" 
      :booking="activeBookingForReject"
      @close="isRejectModalOpen = false"
      @confirm="handleConfirmReject"
    />
  </div>
</template>
