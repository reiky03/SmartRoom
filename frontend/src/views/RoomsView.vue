<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import { api } from '../api/client';
import type { Room } from '../types';
import { isAdmin, addToast } from '../stores/auth';
import { 
  Plus, 
  MapPin, 
  Users, 
  Power, 
  Pencil, 
  CalendarPlus, 
  DoorClosed,
  Loader2,
  X
} from 'lucide-vue-next';

const emit = defineEmits<{
  (e: 'open-booking', roomId?: number): void;
}>();

const rooms = ref<Room[]>([]);
const isLoading = ref(true);
const filterActive = ref<'all' | 'active' | 'inactive'>('all');

// Create/Edit Room Modal state
const isModalOpen = ref(false);
const isEditing = ref(false);
const editingId = ref<number | null>(null);
const modalLoading = ref(false);

const formRoomCode = ref('');
const formName = ref('');
const formBuilding = ref('');
const formCapacity = ref(30);
const formFacilities = ref('');
const formIsActive = ref(true);

onMounted(async () => {
  await loadRooms();
});

async function loadRooms() {
  isLoading.value = true;
  try {
    rooms.value = await api.getRooms();
  } catch (e: any) {
    addToast('Gagal memuat katalog ruangan: ' + e.message, 'error');
  } finally {
    isLoading.value = false;
  }
}

const filteredRooms = computed(() => {
  if (filterActive.value === 'active') return rooms.value.filter(r => r.isActive);
  if (filterActive.value === 'inactive') return rooms.value.filter(r => !r.isActive);
  return rooms.value;
});

async function handleToggleStatus(room: Room) {
  try {
    await api.toggleRoomStatus(room.id);
    room.isActive = !room.isActive;
    addToast(`Status ${room.name} diubah menjadi ${room.isActive ? 'Aktif' : 'Nonaktif'}.`, 'success');
  } catch (e: any) {
    addToast('Gagal mengubah status: ' + e.message, 'error');
  }
}

function openCreateModal() {
  isEditing.value = false;
  editingId.value = null;
  formRoomCode.value = '';
  formName.value = '';
  formBuilding.value = '';
  formCapacity.value = 40;
  formFacilities.value = 'Proyektor 4K, Smart TV, AC Central';
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

async function handleSaveRoom() {
  if (!formRoomCode.value || !formName.value || !formBuilding.value) {
    addToast('Lengkapi kode, nama, dan gedung ruangan.', 'error');
    return;
  }

  modalLoading.value = true;
  try {
    const payload: Partial<Room> = {
      roomCode: formRoomCode.value.trim().toUpperCase(),
      name: formName.value.trim(),
      building: formBuilding.value.trim(),
      capacity: Number(formCapacity.value),
      facilities: formFacilities.value.trim(),
      isActive: formIsActive.value,
    };

    if (isEditing.value && editingId.value) {
      await api.updateRoom(editingId.value, payload);
      addToast('Ruangan berhasil diperbarui!', 'success');
    } else {
      await api.createRoom(payload);
      addToast('Ruangan baru berhasil ditambahkan ke inventaris!', 'success');
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
  <div class="space-y-6">

    <!-- Header Section -->
    <div class="flex flex-col sm:flex-row sm:items-end justify-between gap-4 pb-6 border-b border-zinc-200/80">
      <div>
        <div class="flex items-center gap-2 mb-1.5">
          <span class="text-[11px] font-mono text-zinc-400 uppercase tracking-wider font-semibold">Sarana &amp; Prasarana Kampus</span>
          <span class="text-zinc-300">&bull;</span>
          <span class="text-xs font-mono text-zinc-500">{{ rooms.length }} Ruangan Terdaftar</span>
        </div>
        <h1 class="text-2xl sm:text-3xl font-extrabold tracking-tight text-zinc-950">
          Katalog &amp; Manajemen Ruangan
        </h1>
        <p class="text-xs sm:text-sm text-zinc-500 mt-1 max-w-xl">
          Daftar ruang sidang, laboratorium, dan auditorium lengkap dengan kapasitas, fasilitas resmi, dan status ketersediaan.
        </p>
      </div>

      <button 
        v-if="isAdmin"
        type="button"
        @click="openCreateModal"
        class="inline-flex items-center gap-2 px-4 py-2.5 rounded-xl bg-zinc-900 hover:bg-zinc-800 text-white text-xs font-medium shadow-xs transition-all active:scale-[0.98] flex-shrink-0"
      >
        <Plus class="w-4 h-4" />
        <span>Tambah Ruangan Baru</span>
      </button>
    </div>

    <!-- Filter Pills (Modulify Segmented Control) -->
    <div class="flex items-center gap-1 bg-zinc-100 p-1 rounded-xl border border-zinc-200/60 w-fit text-xs font-medium text-zinc-600">
      <button 
        type="button" 
        @click="filterActive = 'all'" 
        class="px-3.5 py-1.5 rounded-lg transition-all"
        :class="filterActive === 'all' ? 'bg-white text-zinc-950 shadow-xs font-semibold' : 'hover:text-zinc-950'"
      >
        Semua ({{ rooms.length }})
      </button>
      <button 
        type="button" 
        @click="filterActive = 'active'" 
        class="px-3.5 py-1.5 rounded-lg transition-all flex items-center gap-1.5"
        :class="filterActive === 'active' ? 'bg-white text-zinc-950 shadow-xs font-semibold' : 'hover:text-zinc-950'"
      >
        <span class="w-1.5 h-1.5 rounded-full bg-[#166534]"></span>
        Aktif Saja ({{ rooms.filter(r => r.isActive).length }})
      </button>
      <button 
        type="button" 
        @click="filterActive = 'inactive'" 
        class="px-3.5 py-1.5 rounded-lg transition-all flex items-center gap-1.5"
        :class="filterActive === 'inactive' ? 'bg-white text-zinc-950 shadow-xs font-semibold' : 'hover:text-zinc-950'"
      >
        <span class="w-1.5 h-1.5 rounded-full bg-zinc-400"></span>
        Nonaktif ({{ rooms.filter(r => !r.isActive).length }})
      </button>
    </div>

    <!-- Rooms Bento Grid -->
    <div v-if="filteredRooms.length > 0" class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-5">
      <div 
        v-for="room in filteredRooms" 
        :key="room.id"
        class="bg-white border border-zinc-200/80 rounded-2xl p-6 shadow-xs hover:border-zinc-300 hover:shadow-subtle transition-all duration-200 flex flex-col justify-between"
        :class="{ 'opacity-75 bg-zinc-50/50': !room.isActive }"
      >
        <div>
          <!-- Code Tag & Status Pill -->
          <div class="flex items-center justify-between mb-3.5">
            <span class="px-2.5 py-1 rounded-md bg-zinc-100 text-zinc-800 font-mono text-[11px] font-bold border border-zinc-200/80">
              {{ room.roomCode }}
            </span>

            <span 
              v-if="room.isActive"
              class="inline-flex items-center gap-1.5 px-2.5 py-0.5 rounded-full text-[11px] font-mono font-medium bg-[#EDF3EC] text-[#166534] border border-[#BBF7D0]"
            >
              <span class="w-1.5 h-1.5 rounded-full bg-[#166534]"></span> Aktif
            </span>
            <span 
              v-else
              class="inline-flex items-center gap-1.5 px-2.5 py-0.5 rounded-full text-[11px] font-mono font-medium bg-zinc-100 text-zinc-500 border border-zinc-200"
            >
              <span class="w-1.5 h-1.5 rounded-full bg-zinc-400"></span> Nonaktif
            </span>
          </div>

          <!-- Room Name & Building -->
          <h3 class="text-base font-bold text-zinc-950 tracking-tight mb-1">{{ room.name }}</h3>
          <div class="flex items-center gap-1.5 text-xs text-zinc-500 mb-4">
            <MapPin class="w-3.5 h-3.5 text-zinc-400" />
            <span>{{ room.building }}</span>
          </div>

          <!-- Capacity Block -->
          <div class="p-3 rounded-xl bg-zinc-50 border border-zinc-200/60 flex items-center justify-between text-xs mb-4">
            <span class="text-zinc-500 font-medium">Kapasitas Maksimal</span>
            <span class="font-mono font-bold text-zinc-950 flex items-center gap-1.5">
              <Users class="w-3.5 h-3.5 text-zinc-600" />
              {{ room.capacity }} Orang
            </span>
          </div>

          <!-- Facilities Cloud -->
          <div class="mb-4">
            <div class="text-[10px] font-mono uppercase tracking-wider text-zinc-400 font-semibold mb-2">Fasilitas Ruangan</div>
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
              <span v-else class="text-xs text-zinc-400 italic">Standar perkuliahan / pertemuan</span>
            </div>
          </div>
        </div>

        <!-- Footer Actions (Separated by Role) -->
        <div class="pt-4 border-t border-zinc-100 mt-auto">
          <!-- Administrator Controls -->
          <div v-if="isAdmin" class="flex items-center justify-between gap-2">
            <div class="flex items-center gap-1.5">
              <button 
                type="button" 
                @click="openEditModal(room)"
                class="px-3 py-1.5 rounded-lg border border-zinc-200 bg-white hover:bg-zinc-50 text-xs font-medium text-zinc-700 transition-colors inline-flex items-center gap-1.5"
              >
                <Pencil class="w-3 h-3 text-zinc-500" />
                <span>Edit</span>
              </button>
              <button 
                type="button" 
                @click="handleToggleStatus(room)"
                class="px-3 py-1.5 rounded-lg border border-zinc-200 bg-white hover:bg-zinc-50 text-xs font-medium transition-colors inline-flex items-center gap-1.5"
                :class="room.isActive ? 'text-amber-700' : 'text-[#166534]'"
              >
                <Power class="w-3 h-3" />
                <span>{{ room.isActive ? 'Nonaktifkan' : 'Aktifkan' }}</span>
              </button>
            </div>
          </div>

          <!-- Civitas / Student / Staff Action -->
          <div v-else>
            <button 
              v-if="room.isActive"
              type="button"
              @click="emit('open-booking', room.id)"
              class="w-full py-2 px-3 rounded-lg text-center text-xs font-medium bg-zinc-900 hover:bg-zinc-800 text-white shadow-xs transition-all active:scale-[0.98] flex items-center justify-center gap-2"
            >
              <CalendarPlus class="w-3.5 h-3.5" />
              <span>Pinjam Ruangan Ini</span>
            </button>
            <div v-else class="w-full py-2 px-3 rounded-lg text-center text-xs font-medium bg-zinc-100 text-zinc-400 cursor-not-allowed">
              Ruangan Sedang Nonaktif
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Empty State -->
    <div v-else-if="!isLoading" class="py-16 text-center bg-white rounded-2xl border border-zinc-200/80 p-8 shadow-xs">
      <DoorClosed class="w-10 h-10 text-zinc-300 mx-auto mb-3" />
      <h3 class="text-sm font-bold text-zinc-800 mb-1">Tidak Ada Ruangan</h3>
      <p class="text-xs text-zinc-500 mb-4">Belum ada inventaris ruangan dengan filter yang dipilih.</p>
      <button 
        v-if="isAdmin" 
        @click="openCreateModal" 
        class="inline-flex items-center gap-2 px-4 py-2 rounded-xl bg-zinc-900 text-white text-xs font-medium hover:bg-zinc-800 shadow-xs"
      >
        <Plus class="w-3.5 h-3.5" />
        <span>Tambah Ruangan Baru</span>
      </button>
    </div>

    <!-- Create / Edit Room Modal -->
    <div v-if="isModalOpen" class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-zinc-950/50 backdrop-blur-xs">
      <transition name="modal-fade" appear>
        <div class="bg-white rounded-2xl max-w-lg w-full p-6 sm:p-8 shadow-2xl border border-zinc-200 space-y-4 text-xs">
          
          <div class="flex items-center justify-between pb-3 border-b border-zinc-100">
            <div>
              <div class="text-[10px] font-mono text-zinc-400 uppercase tracking-wider font-semibold">Administrator Desk</div>
              <h2 class="text-base font-bold text-zinc-950">{{ isEditing ? 'Edit Ruangan' : 'Tambah Ruangan Baru' }}</h2>
            </div>
            <button @click="isModalOpen = false" class="p-1 rounded-lg text-zinc-400 hover:text-zinc-700">
              <X class="w-5 h-5" />
            </button>
          </div>

          <form @submit.prevent="handleSaveRoom" class="space-y-4">
            <div class="grid grid-cols-2 gap-3">
              <div>
                <label class="block text-[11px] font-mono uppercase tracking-wider font-semibold text-zinc-700 mb-1">
                  Kode Ruangan *
                </label>
                <input 
                  type="text" 
                  v-model="formRoomCode" 
                  required 
                  placeholder="R-SIDANG-01" 
                  class="w-full px-3 py-2 rounded-xl bg-zinc-50 border border-zinc-200 text-zinc-900 font-mono text-xs focus:bg-white focus:border-zinc-900 focus:outline-none"
                />
              </div>

              <div>
                <label class="block text-[11px] font-mono uppercase tracking-wider font-semibold text-zinc-700 mb-1">
                  Kapasitas Kursi *
                </label>
                <input 
                  type="number" 
                  v-model="formCapacity" 
                  min="1" 
                  max="1000" 
                  required 
                  class="w-full px-3 py-2 rounded-xl bg-zinc-50 border border-zinc-200 text-zinc-900 font-mono text-xs focus:bg-white focus:border-zinc-900 focus:outline-none"
                />
              </div>
            </div>

            <div>
              <label class="block text-[11px] font-mono uppercase tracking-wider font-semibold text-zinc-700 mb-1">
                Nama Ruangan *
              </label>
              <input 
                type="text" 
                v-model="formName" 
                required 
                placeholder="Ruang Sidang Utama Lt. 2" 
                class="w-full px-3 py-2 rounded-xl bg-zinc-50 border border-zinc-200 text-zinc-900 text-xs focus:bg-white focus:border-zinc-900 focus:outline-none"
              />
            </div>

            <div>
              <label class="block text-[11px] font-mono uppercase tracking-wider font-semibold text-zinc-700 mb-1">
                Gedung / Lokasi *
              </label>
              <input 
                type="text" 
                v-model="formBuilding" 
                required 
                placeholder="Gedung Fakultas TI" 
                class="w-full px-3 py-2 rounded-xl bg-zinc-50 border border-zinc-200 text-zinc-900 text-xs focus:bg-white focus:border-zinc-900 focus:outline-none"
              />
            </div>

            <div>
              <label class="block text-[11px] font-mono uppercase tracking-wider font-semibold text-zinc-700 mb-1">
                Fasilitas (Pisahkan koma)
              </label>
              <textarea 
                v-model="formFacilities" 
                rows="2" 
                placeholder="Proyektor 4K, Smart TV, AC Central, Sound System" 
                class="w-full px-3 py-2 rounded-xl bg-zinc-50 border border-zinc-200 text-zinc-900 text-xs focus:bg-white focus:border-zinc-900 focus:outline-none"
              ></textarea>
            </div>

            <div class="p-3 rounded-xl bg-zinc-50 border border-zinc-200 flex items-center justify-between">
              <div>
                <div class="font-semibold text-zinc-900">Status Operasional Aktif</div>
                <div class="text-[11px] text-zinc-500">Ruangan dapat dipilih pada formulir peminjaman jika aktif.</div>
              </div>
              <input type="checkbox" v-model="formIsActive" class="w-4 h-4 rounded border-zinc-300 text-zinc-900 focus:ring-zinc-900" />
            </div>

            <div class="flex items-center justify-end gap-2.5 pt-3 border-t border-zinc-100">
              <button 
                type="button" 
                @click="isModalOpen = false" 
                class="px-4 py-2 rounded-xl border border-zinc-200 text-zinc-600 hover:bg-zinc-50 font-medium"
              >
                Batal
              </button>
              <button 
                type="submit" 
                :disabled="modalLoading" 
                class="px-4 py-2 rounded-xl bg-zinc-900 hover:bg-zinc-800 text-white font-medium flex items-center gap-1.5 shadow-xs"
              >
                <Loader2 v-if="modalLoading" class="w-3.5 h-3.5 animate-spin" />
                <span>{{ isEditing ? 'Simpan Perubahan' : 'Tambah Ruangan' }}</span>
              </button>
            </div>
          </form>
        </div>
      </transition>
    </div>
  </div>
</template>
