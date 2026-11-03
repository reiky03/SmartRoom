<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import { api } from '../api/client';
import type { Room } from '../types';
import { addToast } from '../stores/auth';
import {
  Plus,
  MapPin,
  Users,
  Power,
  Pencil,
  DoorClosed,
  Loader2,
  X,
  Search,
  CheckCircle2,
  AlertTriangle
} from 'lucide-vue-next';

const rooms = ref<Room[]>([]);
const isLoading = ref(true);
const filterActive = ref<'all' | 'active' | 'inactive'>('all');
const searchQuery = ref('');

// Modal state
const isModalOpen = ref(false);
const isEditing = ref(false);
const editingId = ref<number | null>(null);
const modalLoading = ref(false);

// Confirmation toggle state
const confirmToggleRoom = ref<Room | null>(null);
const isConfirmToggleOpen = ref(false);
const toggleLoading = ref(false);

const formRoomCode = ref('');
const formName = ref('');
const formBuilding = ref('');
const formCapacity = ref(40);
const formFacilities = ref('');
const formIsActive = ref(true);

const roomImages: Record<number, string> = {
  1: 'https://images.unsplash.com/photo-1517502884422-41eaead166d4?auto=format&fit=crop&w=800&q=80',
  2: 'https://images.unsplash.com/photo-1581091226825-a6a2a5aee158?auto=format&fit=crop&w=800&q=80',
  3: 'https://images.unsplash.com/photo-1497366216548-37526070297c?auto=format&fit=crop&w=800&q=80',
  4: 'https://images.unsplash.com/photo-1541829070764-84a7d30dd3f3?auto=format&fit=crop&w=800&q=80',
};

function getRoomImg(id: number) {
  return roomImages[id] || 'https://images.unsplash.com/photo-1497366811353-6870744d04b2?auto=format&fit=crop&w=800&q=80';
}

onMounted(async () => {
  await loadRooms();
});

async function loadRooms() {
  isLoading.value = true;
  try {
    rooms.value = await api.getRooms();
  } catch (e: any) {
    addToast('Gagal memuat inventaris ruangan: ' + e.message, 'error');
  } finally {
    isLoading.value = false;
  }
}

const filteredRooms = computed(() => {
  return rooms.value.filter(r => {
    if (filterActive.value === 'active' && !r.isActive) return false;
    if (filterActive.value === 'inactive' && r.isActive) return false;

    if (searchQuery.value) {
      const q = searchQuery.value.toLowerCase();
      const matchCode = r.roomCode.toLowerCase().includes(q);
      const matchName = r.name.toLowerCase().includes(q);
      const matchBuilding = r.building.toLowerCase().includes(q);
      const matchFac = (r.facilities || '').toLowerCase().includes(q);
      return matchCode || matchName || matchBuilding || matchFac;
    }

    return true;
  });
});

const activeCount = computed(() => rooms.value.filter(r => r.isActive).length);
const inactiveCount = computed(() => rooms.value.filter(r => !r.isActive).length);

function openCreateModal() {
  isEditing.value = false;
  editingId.value = null;
  formRoomCode.value = 'R-' + Math.floor(100 + Math.random() * 900);
  formName.value = '';
  formBuilding.value = '';
  formCapacity.value = 40;
  formFacilities.value = 'AC, Proyektor, Sound System';
  formIsActive.value = true;
  isModalOpen.value = true;
}

function openEditModal(room: Room) {
  isEditing.value = true;
  editingId.value = room.id;
  formRoomCode.value = room.roomCode;
  formName.value = room.name;
  formBuilding.value = room.building;
  formCapacity.value = room.capacity;
  formFacilities.value = room.facilities || '';
  formIsActive.value = room.isActive;
  isModalOpen.value = true;
}

function promptToggleStatus(room: Room) {
  confirmToggleRoom.value = room;
  isConfirmToggleOpen.value = true;
}

async function executeToggleStatus() {
  if (!confirmToggleRoom.value) return;
  const room = confirmToggleRoom.value;
  toggleLoading.value = true;
  try {
    const nextIsActive = !room.isActive;
    await api.toggleRoomStatus(room.id);
    room.isActive = nextIsActive;
    const msg = nextIsActive ? `Ruangan ${room.roomCode} berhasil diaktifkan kembali!` : `Ruangan ${room.roomCode} telah dinonaktifkan dari katalog.`;
    addToast(msg, nextIsActive ? 'success' : 'info');
    isConfirmToggleOpen.value = false;
    confirmToggleRoom.value = null;
  } catch (e: any) {
    addToast('Gagal mengubah status: ' + e.message, 'error');
  } finally {
    toggleLoading.value = false;
  }
}

async function handleSaveRoom() {
  if (!formName.value.trim() || !formRoomCode.value.trim() || !formBuilding.value.trim()) {
    addToast('Silakan lengkapi seluruh kolom wajib.', 'error');
    return;
  }

  modalLoading.value = true;
  try {
    const payload = {
      roomCode: formRoomCode.value.trim(),
      name: formName.value.trim(),
      building: formBuilding.value.trim(),
      capacity: Number(formCapacity.value),
      facilities: formFacilities.value.trim(),
      isActive: formIsActive.value,
    };

    if (isEditing.value && editingId.value) {
      await api.updateRoom(editingId.value, payload);
      addToast(`Ruangan ${payload.roomCode} berhasil diperbarui!`, 'success');
    } else {
      await api.createRoom(payload);
      addToast(`Ruangan baru ${payload.roomCode} berhasil ditambahkan!`, 'success');
    }

    isModalOpen.value = false;
    await loadRooms();
  } catch (e: any) {
    addToast('Gagal menyimpan ruangan: ' + e.message, 'error');
  } finally {
    modalLoading.value = false;
  }
}
</script>

<template>
  <div class="space-y-8">

    <!-- Top Header -->
    <div class="page-hero flex flex-col justify-between gap-6 p-6 sm:p-9 md:flex-row md:items-end lg:p-10">
      <div class="space-y-2">
        <div class="page-kicker">
          <span>Inventaris fasilitas</span>
          <span class="text-[#596159] font-medium">{{ rooms.length }} ruang terdaftar</span>
        </div>
        <h1 class="max-w-[16ch] text-4xl font-medium leading-[0.98] text-[#182019] sm:text-5xl lg:text-6xl">
          Ruang siap untuk setiap agenda
        </h1>
        <p class="max-w-2xl text-sm leading-6 text-[#5F675F] sm:text-base">
          Atur kapasitas, sarana pendukung, dan status operasional ruangan. Ruangan nonaktif secara otomatis disaring dari katalog civitas.
        </p>
      </div>

      <button
        type="button"
        @click="openCreateModal"
        class="halden-btn-primary relative z-10 inline-flex items-center gap-2 self-start px-5 text-sm md:self-auto"
      >
        <Plus class="w-4 h-4" />
        <span>Tambah ruangan</span>
      </button>
    </div>

    <!-- Filters Bar -->
    <div class="flex flex-col sm:flex-row items-center justify-between gap-3">

      <!-- Segmented Buttons -->
      <div class="flex items-center gap-1 bg-[#FAF8F5] p-1 rounded-lg border border-[#E5DFD5] text-xs font-mono w-full sm:w-auto shadow-xs">
        <button
          @click="filterActive = 'all'"
          class="flex-1 sm:flex-none px-4 py-1.5 rounded transition-all cursor-pointer"
          :class="filterActive === 'all' ? 'bg-[#1C1917] text-[#FAF8F5] font-semibold shadow-xs' : 'text-[#78716C] hover:text-[#1C1917]'"
        >
          Semua ({{ rooms.length }})
        </button>
        <button
          @click="filterActive = 'active'"
          class="flex-1 sm:flex-none px-4 py-1.5 rounded transition-all flex items-center justify-center gap-1.5 cursor-pointer"
          :class="filterActive === 'active' ? 'bg-[#1C1917] text-[#FAF8F5] font-semibold shadow-xs' : 'text-[#78716C] hover:text-[#1C1917]'"
        >
          <span class="w-1.5 h-1.5 rounded-full bg-[#15803D]"></span>
          <span>Aktif ({{ activeCount }})</span>
        </button>
        <button
          @click="filterActive = 'inactive'"
          class="flex-1 sm:flex-none px-4 py-1.5 rounded transition-all flex items-center justify-center gap-1.5 cursor-pointer"
          :class="filterActive === 'inactive' ? 'bg-[#1C1917] text-[#FAF8F5] font-semibold shadow-xs' : 'text-[#78716C] hover:text-[#1C1917]'"
        >
          <span class="w-1.5 h-1.5 rounded-full bg-[#78716C]"></span>
          <span>Nonaktif ({{ inactiveCount }})</span>
        </button>
      </div>

      <!-- Search Input -->
      <div class="relative w-full sm:w-72">
        <input
          id="room-search"
          name="room-search"
          type="text"
          v-model="searchQuery"
          aria-label="Cari ruangan"
          placeholder="Cari nama, kode, gedung..."
          class="w-full pl-9 pr-3.5 py-2 halden-input text-xs placeholder:text-[#A8A29E] font-mono"
        />
        <Search class="w-4 h-4 text-[#78716C] absolute left-3 top-2.5" />
      </div>
    </div>

    <!-- Rooms Cards Grid (Halden Architectural Minimalist Cards) -->
    <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
      <div
        v-for="r in filteredRooms"
        :key="r.id"
        class="bg-white rounded-xl border border-[#E5DFD5] hover:border-[#1C1917] shadow-xs hover:shadow-md transition-all duration-300 overflow-hidden flex flex-col justify-between group"
        :class="!r.isActive ? 'opacity-75 bg-[#FEF2F2]/20 border-[#FECACA]' : ''"
      >
        <!-- Card Image & Header -->
        <div>
          <div class="p-3.5 border-b border-[#E5DFD5] flex items-center justify-between font-mono text-[10px] bg-[#FAF8F5]">
            <span class="font-bold text-[#1C1917] bg-[#EAE4D9] px-2.5 py-0.5 rounded border border-[#D8D1C5]">
              {{ r.roomCode }}
            </span>
            <span
              class="inline-flex items-center gap-1.5 px-2 py-0.5 rounded font-semibold border"
              :class="r.isActive
                ? 'bg-[#DCFCE7] text-[#166534] border-[#BBF7D0]'
                : 'bg-[#FEE2E2] text-[#991B1B] border-[#FECACA]'"
            >
              <span class="w-1.5 h-1.5 rounded-full" :class="r.isActive ? 'bg-[#15803D]' : 'bg-[#B91C1C]'"></span>
              {{ r.isActive ? 'Aktif' : 'Nonaktif' }}
            </span>
          </div>

          <!-- Room Photo -->
          <div class="relative aspect-[16/9] overflow-hidden bg-[#EAE4D9]">
            <img
              :src="getRoomImg(r.id)"
              :alt="r.name"
              class="w-full h-full object-cover group-hover:scale-105 transition-transform duration-500"
              loading="lazy"
            />
          </div>

          <!-- Room Info -->
          <div class="p-5 space-y-3">
            <div>
              <h3 class="font-serif text-lg font-bold text-[#1C1917] group-hover:text-black">
                {{ r.name }}
              </h3>
              <div class="flex items-center gap-1.5 text-xs text-[#57534E] mt-1 font-sans">
                <MapPin class="w-3.5 h-3.5 text-[#78716C]" />
                <span>{{ r.building }}</span>
              </div>
            </div>

            <!-- Capacity -->
            <div class="flex items-center justify-between py-2 border-y border-[#E5DFD5] text-xs font-mono text-[#57534E]">
              <span>Kapasitas Kursi</span>
              <span class="flex items-center gap-1.5">
                <Users class="w-3.5 h-3.5 text-[#1C1917]" />
                <strong class="text-[#1C1917] font-bold">{{ r.capacity }}</strong> Orang
              </span>
            </div>

            <!-- Facilities -->
            <div v-if="r.facilities" class="space-y-1">
              <div class="text-[10px] font-mono uppercase tracking-wider text-[#78716C] font-semibold">Fasilitas Resmi</div>
              <div class="flex flex-wrap gap-1">
                <span
                  v-for="(f, i) in r.facilities.split(',')"
                  :key="i"
                  class="px-2 py-0.5 rounded text-[10px] bg-[#FAF8F5] border border-[#E5DFD5] text-[#57534E] font-mono"
                >
                  {{ f.trim() }}
                </span>
              </div>
            </div>
          </div>
        </div>

        <!-- Card Footer Actions -->
        <div class="p-4 border-t border-[#E5DFD5] bg-[#FAF8F5] flex items-center justify-between gap-2">
          <button
            type="button"
            @click="openEditModal(r)"
            class="px-3.5 py-1.5 rounded-lg halden-btn-secondary text-xs flex items-center gap-1.5 cursor-pointer"
          >
            <Pencil class="w-3 h-3 text-[#78716C]" />
            <span>Edit</span>
          </button>

          <button
            :id="'toggle-room-' + r.id + '-btn'"
            type="button"
            @click="promptToggleStatus(r)"
            class="px-3.5 py-1.5 rounded-lg text-xs font-mono font-semibold border transition-all flex items-center gap-1.5 cursor-pointer active:scale-95"
            :class="r.isActive
              ? 'text-[#991B1B] bg-[#FEE2E2] border-[#FECACA] hover:bg-[#FCA5A5]'
              : 'text-[#166534] bg-[#DCFCE7] border-[#BBF7D0] hover:bg-[#86EFAC]'"
          >
            <Power class="w-3 h-3" />
            <span>{{ r.isActive ? 'Nonaktifkan' : 'Aktifkan' }}</span>
          </button>
        </div>
      </div>
    </div>

    <!-- Create/Edit Room Halden Modal Sheet (Refined Squircle with Zero Gaps) -->
    <Teleport to="body">
      <div
        v-if="isModalOpen"
        id="room-form-modal"
        class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-[#182019]/45 backdrop-blur-sm overflow-y-auto"
        role="dialog"
        aria-modal="true"
        aria-labelledby="room-modal-title"
      >
        <transition name="modal-fade" appear>
          <div
            class="relative w-full max-w-lg rounded-[24px] bg-[#FBFAF7] border border-[#D9D8D1] shadow-2xl p-6 sm:p-7 space-y-5 text-[#182019] my-auto"
          >
            <!-- Header with Squircle Icon Box -->
            <div class="flex items-center justify-between pb-4 border-b border-[#D9D8D1]">
              <div class="flex items-center gap-3">
                <div class="w-10 h-10 rounded-xl bg-[#214C38] text-white flex items-center justify-center shadow-xs">
                  <DoorClosed class="w-5 h-5" />
                </div>
                <div>
                  <span class="text-[10px] font-mono tracking-widest uppercase text-[#A45F35] font-bold">
                    Spesifikasi Fasilitas
                  </span>
                  <h3 id="room-modal-title" class="font-serif text-xl font-bold text-[#182019] mt-0.5">
                    {{ isEditing ? 'Edit Spesifikasi Ruangan' : 'Tambah Ruangan Baru' }}
                  </h3>
                </div>
              </div>
              <button
                type="button"
                aria-label="Tutup dialog"
                @click="isModalOpen = false"
                class="w-9 h-9 rounded-xl bg-white border border-[#D9D8D1] hover:bg-[#F1EFE9] flex items-center justify-center text-[#5F675F] hover:text-[#182019] transition-colors cursor-pointer"
              >
                <X class="w-4 h-4" />
              </button>
            </div>

            <!-- Form Content -->
            <div class="space-y-4 font-sans text-xs">
              <div class="grid grid-cols-2 gap-3.5">
                <div>
                  <label for="room-code" class="block font-semibold text-[#3E473F] mb-1.5">
                    Kode Ruangan <span class="text-[#991B1B]">*</span>
                  </label>
                  <input
                    id="room-code"
                    name="room-code"
                    type="text"
                    v-model="formRoomCode"
                    class="w-full px-3.5 py-2.5 rounded-xl border border-[#D9D8D1] bg-white text-xs font-mono focus:border-[#214C38] focus:ring-1 focus:ring-[#214C38] outline-hidden transition-all shadow-xs"
                  />
                </div>
                <div>
                  <label for="room-capacity" class="block font-semibold text-[#3E473F] mb-1.5">
                    Kapasitas (Kursi) <span class="text-[#991B1B]">*</span>
                  </label>
                  <input
                    id="room-capacity"
                    name="room-capacity"
                    type="number"
                    v-model="formCapacity"
                    class="w-full px-3.5 py-2.5 rounded-xl border border-[#D9D8D1] bg-white text-xs font-mono focus:border-[#214C38] focus:ring-1 focus:ring-[#214C38] outline-hidden transition-all shadow-xs"
                  />
                </div>
              </div>

              <div>
                <label for="room-name" class="block font-semibold text-[#3E473F] mb-1.5">
                  Nama Ruangan <span class="text-[#991B1B]">*</span>
                </label>
                <input
                  id="room-name"
                  name="room-name"
                  type="text"
                  v-model="formName"
                  placeholder="Contoh: Lab Komputer Riset A"
                  class="w-full px-3.5 py-2.5 rounded-xl border border-[#D9D8D1] bg-white text-xs placeholder:text-[#A8A29E] focus:border-[#214C38] focus:ring-1 focus:ring-[#214C38] outline-hidden transition-all shadow-xs"
                />
              </div>

              <div>
                <label for="room-building" class="block font-semibold text-[#3E473F] mb-1.5">
                  Lokasi / Gedung <span class="text-[#991B1B]">*</span>
                </label>
                <input
                  id="room-building"
                  name="room-building"
                  type="text"
                  v-model="formBuilding"
                  placeholder="Contoh: Gedung Fakultas TI Lt. 2"
                  class="w-full px-3.5 py-2.5 rounded-xl border border-[#D9D8D1] bg-white text-xs placeholder:text-[#A8A29E] focus:border-[#214C38] focus:ring-1 focus:ring-[#214C38] outline-hidden transition-all shadow-xs"
                />
              </div>

              <div>
                <label for="room-facilities" class="block font-semibold text-[#3E473F] mb-1.5">
                  Fasilitas (Pisahkan dengan koma)
                </label>
                <input
                  id="room-facilities"
                  name="room-facilities"
                  type="text"
                  v-model="formFacilities"
                  placeholder="Proyektor, AC, Smart TV, Whiteboard"
                  class="w-full px-3.5 py-2.5 rounded-xl border border-[#D9D8D1] bg-white text-xs placeholder:text-[#A8A29E] focus:border-[#214C38] focus:ring-1 focus:ring-[#214C38] outline-hidden transition-all shadow-xs"
                />
              </div>
            </div>

            <!-- Footer Buttons -->
            <div class="flex items-center justify-end gap-2.5 pt-4 border-t border-[#D9D8D1]">
              <button
                type="button"
                @click="isModalOpen = false"
                class="px-5 py-2.5 rounded-xl halden-btn-secondary text-xs font-semibold cursor-pointer"
              >
                Batal
              </button>
              <button
                type="button"
                @click="handleSaveRoom"
                :disabled="modalLoading"
                class="px-6 py-2.5 rounded-xl halden-btn-primary text-xs font-semibold flex items-center gap-1.5 cursor-pointer disabled:opacity-50 active:scale-95 transition-all"
              >
                <Loader2 v-if="modalLoading" class="w-3.5 h-3.5 animate-spin" />
                <span>{{ modalLoading ? 'Menyimpan...' : 'Simpan Ruangan' }}</span>
              </button>
            </div>
          </div>
        </transition>
      </div>
    </Teleport>

    <!-- Modal Konfirmasi Status Ruangan (Nonaktifkan / Aktifkan) -->
    <Teleport to="body">
      <div
        v-if="isConfirmToggleOpen && confirmToggleRoom"
        id="toggle-room-confirm-modal"
        class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-[#182019]/45 backdrop-blur-sm"
        role="dialog"
        aria-modal="true"
        aria-labelledby="toggle-dialog-title"
      >
        <transition name="modal-fade" appear>
          <div class="relative w-full max-w-md rounded-[24px] bg-[#FBFAF7] border border-[#D9D8D1] shadow-2xl p-6 sm:p-7 space-y-4 text-[#182019]">
            <div class="flex items-center gap-3">
              <div
                class="w-10 h-10 rounded-xl flex items-center justify-center shadow-xs border"
                :class="confirmToggleRoom.isActive
                  ? 'bg-[#FEE2E2] border-[#FECACA] text-[#991B1B]'
                  : 'bg-[#DCFCE7] border-[#BBF7D0] text-[#166534]'"
              >
                <AlertTriangle v-if="confirmToggleRoom.isActive" class="w-5 h-5" />
                <CheckCircle2 v-else class="w-5 h-5" />
              </div>
              <div>
                <p
                  class="text-[10px] font-mono uppercase tracking-widest font-bold"
                  :class="confirmToggleRoom.isActive ? 'text-[#991B1B]' : 'text-[#166534]'"
                >
                  {{ confirmToggleRoom.isActive ? 'Konfirmasi Nonaktifkan' : 'Konfirmasi Aktifkan' }}
                </p>
                <h3 id="toggle-dialog-title" class="font-serif text-lg font-bold text-[#182019]">
                  {{ confirmToggleRoom.isActive ? 'Nonaktifkan Ruangan?' : 'Aktifkan Ruangan Kembali?' }}
                </h3>
              </div>
            </div>

            <div class="p-3.5 rounded-xl bg-white border border-[#D9D8D1] text-xs space-y-1">
              <div class="font-bold text-[#182019] text-sm">{{ confirmToggleRoom.name }}</div>
              <div class="font-mono text-[#5F675F] text-[11px]">{{ confirmToggleRoom.roomCode }} &bull; {{ confirmToggleRoom.building }}</div>
            </div>

            <p class="text-xs text-[#5F675F] leading-relaxed">
              <template v-if="confirmToggleRoom.isActive">
                Ruangan ini akan <strong>disembunyikan dari katalog peminjaman civitas</strong>. Jadwal yang telah disetujui sebelumnya tetap aman dan tercatat di riwayat.
              </template>
              <template v-else>
                Ruangan ini akan <strong>kembali tersedia di katalog</strong> dan civitas akademika dapat mengajukan jadwal peminjaman baru.
              </template>
            </p>

            <div class="flex items-center justify-end gap-2.5 pt-3 border-t border-[#D9D8D1]">
              <button
                type="button"
                @click="isConfirmToggleOpen = false"
                :disabled="toggleLoading"
                class="px-4 py-2.5 rounded-xl halden-btn-secondary text-xs font-semibold cursor-pointer disabled:opacity-50"
              >
                Batal
              </button>
              <button
                id="execute-toggle-room-btn"
                type="button"
                @click="executeToggleStatus"
                :disabled="toggleLoading"
                class="px-5 py-2.5 rounded-xl text-xs font-semibold shadow-xs transition-all active:scale-95 cursor-pointer flex items-center gap-1.5 text-white disabled:opacity-50"
                :class="confirmToggleRoom.isActive
                  ? 'bg-[#991B1B] hover:bg-[#7F1D1D]'
                  : 'bg-[#214C38] hover:bg-[#18392A]'"
              >
                <Loader2 v-if="toggleLoading" class="w-3.5 h-3.5 animate-spin" />
                <span>
                  {{ toggleLoading
                    ? 'Memproses...'
                    : (confirmToggleRoom.isActive ? 'Ya, Nonaktifkan Ruangan' : 'Ya, Aktifkan Ruangan') }}
                </span>
              </button>
            </div>
          </div>
        </transition>
      </div>
    </Teleport>
  </div>
</template>
