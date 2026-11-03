<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import { api } from '../api/client';
import type { UserAccount, CreateUserPayload, UpdateUserPayload } from '../types';
import { currentUser, addToast } from '../stores/auth';
import {
  UserPlus,
  Users,
  ShieldCheck,
  GraduationCap,
  Briefcase,
  Search,
  Pencil,
  Trash2,
  X,
  Loader2,
  Lock,
  Mail,
  User,
  Building2,
  AlertTriangle
} from 'lucide-vue-next';

const users = ref<UserAccount[]>([]);
const isLoading = ref(true);
const filterRole = ref<'all' | 'Student' | 'Staff' | 'Admin'>('all');
const searchQuery = ref('');

// Create / Edit Modal State
const isModalOpen = ref(false);
const isEditing = ref(false);
const editingId = ref<number | null>(null);
const modalLoading = ref(false);

const formUsername = ref('');
const formPassword = ref('');
const formFullName = ref('');
const formEmail = ref('');
const formRole = ref<number>(2); // 0: Admin, 1: Staff, 2: Student
const formDepartment = ref('');

// Delete Confirmation State
const confirmDeleteUser = ref<UserAccount | null>(null);
const isConfirmDeleteOpen = ref(false);
const deleteLoading = ref(false);

onMounted(async () => {
  await loadUsers();
});

async function loadUsers() {
  isLoading.value = true;
  try {
    users.value = await api.getUsers();
  } catch (e: any) {
    addToast('Gagal memuat daftar pengguna: ' + e.message, 'error');
  } finally {
    isLoading.value = false;
  }
}

const filteredUsers = computed(() => {
  return users.value.filter(u => {
    if (filterRole.value !== 'all' && u.role !== filterRole.value) {
      return false;
    }

    if (searchQuery.value.trim()) {
      const q = searchQuery.value.trim().toLowerCase();
      const matchName = u.fullName.toLowerCase().includes(q);
      const matchUser = u.username.toLowerCase().includes(q);
      const matchEmail = u.email.toLowerCase().includes(q);
      const matchDept = (u.department || '').toLowerCase().includes(q);
      return matchName || matchUser || matchEmail || matchDept;
    }

    return true;
  });
});

const studentCount = computed(() => users.value.filter(u => u.role === 'Student').length);
const staffCount = computed(() => users.value.filter(u => u.role === 'Staff').length);
const adminCount = computed(() => users.value.filter(u => u.role === 'Admin').length);

function openCreateModal() {
  isEditing.value = false;
  editingId.value = null;
  formUsername.value = '';
  formPassword.value = '';
  formFullName.value = '';
  formEmail.value = '';
  formRole.value = 2; // Student by default
  formDepartment.value = '';
  isModalOpen.value = true;
}

function openEditModal(user: UserAccount) {
  isEditing.value = true;
  editingId.value = user.id;
  formUsername.value = user.username;
  formPassword.value = ''; // Empty means don't change
  formFullName.value = user.fullName;
  formEmail.value = user.email;
  formRole.value = user.role === 'Admin' ? 0 : (user.role === 'Staff' ? 1 : 2);
  formDepartment.value = user.department || '';
  isModalOpen.value = true;
}

function promptDeleteUser(user: UserAccount) {
  if (currentUser.value?.username === user.username) {
    addToast('Anda tidak dapat menghapus akun Anda sendiri saat sedang masuk.', 'error');
    return;
  }
  confirmDeleteUser.value = user;
  isConfirmDeleteOpen.value = true;
}

async function executeDeleteUser() {
  if (!confirmDeleteUser.value) return;
  const user = confirmDeleteUser.value;
  deleteLoading.value = true;
  try {
    await api.deleteUser(user.id);
    addToast(`Pengguna ${user.fullName} (@${user.username}) berhasil dihapus.`, 'success');
    isConfirmDeleteOpen.value = false;
    confirmDeleteUser.value = null;
    await loadUsers();
  } catch (e: any) {
    addToast('Gagal menghapus pengguna: ' + e.message, 'error');
  } finally {
    deleteLoading.value = false;
  }
}

async function handleSaveUser() {
  if (!formFullName.value.trim() || !formEmail.value.trim()) {
    addToast('Nama lengkap dan alamat email wajib diisi.', 'error');
    return;
  }

  if (!isEditing.value) {
    if (!formUsername.value.trim() || !formPassword.value) {
      addToast('Username dan kata sandi wajib diisi untuk akun baru.', 'error');
      return;
    }
  }

  modalLoading.value = true;
  try {
    if (isEditing.value && editingId.value) {
      const payload: UpdateUserPayload = {
        fullName: formFullName.value.trim(),
        email: formEmail.value.trim(),
        role: formRole.value,
        department: formDepartment.value.trim(),
        newPassword: formPassword.value.trim() || undefined,
      };
      await api.updateUser(editingId.value, payload);
      addToast(`Data pengguna ${payload.fullName} berhasil diperbarui!`, 'success');
    } else {
      const payload: CreateUserPayload = {
        username: formUsername.value.trim(),
        password: formPassword.value,
        fullName: formFullName.value.trim(),
        email: formEmail.value.trim(),
        role: formRole.value,
        department: formDepartment.value.trim(),
      };
      await api.createUser(payload);
      addToast(`Akun civitas/pengguna baru ${payload.fullName} berhasil dibuat!`, 'success');
    }

    isModalOpen.value = false;
    await loadUsers();
  } catch (e: any) {
    addToast('Gagal menyimpan pengguna: ' + e.message, 'error');
  } finally {
    modalLoading.value = false;
  }
}

function formatDate(iso: string) {
  if (!iso) return '-';
  try {
    return new Date(iso).toLocaleDateString('id-ID', {
      day: 'numeric',
      month: 'short',
      year: 'numeric'
    });
  } catch {
    return iso;
  }
}
</script>

<template>
  <div class="space-y-8">

    <!-- Top Header / Page Hero -->
    <div class="page-hero flex flex-col justify-between gap-6 p-6 sm:p-9 md:flex-row md:items-end lg:p-10">
      <div class="space-y-2">
        <div class="page-kicker">
          <span>Manajemen civitas & hak akses</span>
          <span class="text-[#596159] font-medium">{{ users.length }} akun aktif</span>
        </div>
        <h1 class="max-w-[16ch] text-4xl font-medium leading-[0.98] text-[#182019] sm:text-5xl lg:text-6xl">
          Kelola civitas & administrator
        </h1>
        <p class="max-w-2xl text-sm leading-6 text-[#5F675F] sm:text-base">
          Buat akun baru untuk mahasiswa, dosen & staf, maupun sesama administrator dengan hak akses (RBAC) yang terstruktur dan aman.
        </p>
      </div>

      <button
        id="add-user-btn"
        type="button"
        @click="openCreateModal"
        class="halden-btn-primary relative z-10 inline-flex items-center gap-2 self-start px-5 text-sm md:self-auto cursor-pointer"
      >
        <UserPlus class="w-4 h-4" />
        <span>Tambah pengguna</span>
      </button>
    </div>

    <!-- Filters & Search Bar -->
    <div class="flex flex-col sm:flex-row items-center justify-between gap-3">
      <!-- Segmented Buttons -->
      <div class="flex items-center gap-1 bg-[#FAF8F5] p-1 rounded-lg border border-[#E5DFD5] text-xs font-mono w-full sm:w-auto shadow-xs overflow-x-auto">
        <button
          @click="filterRole = 'all'"
          class="flex-1 sm:flex-none px-3.5 py-1.5 rounded transition-all cursor-pointer whitespace-nowrap"
          :class="filterRole === 'all' ? 'bg-[#1C1917] text-[#FAF8F5] font-semibold shadow-xs' : 'text-[#78716C] hover:text-[#1C1917]'"
        >
          Semua ({{ users.length }})
        </button>
        <button
          @click="filterRole = 'Student'"
          class="flex-1 sm:flex-none px-3.5 py-1.5 rounded transition-all flex items-center justify-center gap-1.5 cursor-pointer whitespace-nowrap"
          :class="filterRole === 'Student' ? 'bg-[#1C1917] text-[#FAF8F5] font-semibold shadow-xs' : 'text-[#78716C] hover:text-[#1C1917]'"
        >
          <GraduationCap class="w-3.5 h-3.5 text-[#A45F35]" />
          <span>Mahasiswa ({{ studentCount }})</span>
        </button>
        <button
          @click="filterRole = 'Staff'"
          class="flex-1 sm:flex-none px-3.5 py-1.5 rounded transition-all flex items-center justify-center gap-1.5 cursor-pointer whitespace-nowrap"
          :class="filterRole === 'Staff' ? 'bg-[#1C1917] text-[#FAF8F5] font-semibold shadow-xs' : 'text-[#78716C] hover:text-[#1C1917]'"
        >
          <Briefcase class="w-3.5 h-3.5 text-[#2563EB]" />
          <span>Dosen & Staf ({{ staffCount }})</span>
        </button>
        <button
          @click="filterRole = 'Admin'"
          class="flex-1 sm:flex-none px-3.5 py-1.5 rounded transition-all flex items-center justify-center gap-1.5 cursor-pointer whitespace-nowrap"
          :class="filterRole === 'Admin' ? 'bg-[#1C1917] text-[#FAF8F5] font-semibold shadow-xs' : 'text-[#78716C] hover:text-[#1C1917]'"
        >
          <ShieldCheck class="w-3.5 h-3.5 text-[#15803D]" />
          <span>Admin ({{ adminCount }})</span>
        </button>
      </div>

      <!-- Search Input -->
      <div class="relative w-full sm:w-72">
        <input
          id="user-search"
          name="user-search"
          type="text"
          v-model="searchQuery"
          aria-label="Cari pengguna"
          placeholder="Cari nama, username, email..."
          class="w-full pl-9 pr-3.5 py-2 halden-input text-xs placeholder:text-[#A8A29E] font-mono"
        />
        <Search class="w-4 h-4 text-[#78716C] absolute left-3 top-2.5" />
      </div>
    </div>

    <!-- Users Grid Cards -->
    <div v-if="isLoading" class="flex flex-col items-center justify-center py-20 text-[#78716C] space-y-3">
      <Loader2 class="w-8 h-8 animate-spin text-[#214C38]" />
      <p class="text-xs font-mono">Memuat basis data civitas...</p>
    </div>

    <div v-else-if="filteredUsers.length === 0" class="rounded-[24px] bg-[#FBFAF7] border border-[#D9D8D1] p-12 text-center space-y-3">
      <div class="w-12 h-12 rounded-xl bg-[#F1EFE9] text-[#5F675F] flex items-center justify-center mx-auto">
        <Users class="w-6 h-6" />
      </div>
      <h3 class="font-serif text-lg font-bold text-[#182019]">Tidak ada pengguna ditemukan</h3>
      <p class="text-xs text-[#5F675F] max-w-sm mx-auto">
        Tidak ditemukan akun yang sesuai dengan kriteria filter atau kata kunci pencarian Anda.
      </p>
    </div>

    <div v-else class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-5">
      <div
        v-for="u in filteredUsers"
        :key="u.id"
        class="bg-white rounded-2xl border border-[#E5DFD5] hover:border-[#1C1917] shadow-xs hover:shadow-md transition-all duration-300 p-5 flex flex-col justify-between group"
      >
        <div class="space-y-4">
          <!-- Top Badge & Role Indicator -->
          <div class="flex items-center justify-between pb-3 border-b border-[#F1EFE9]">
            <div class="flex items-center gap-2.5">
              <div
                class="w-10 h-10 rounded-xl flex items-center justify-center font-serif font-bold text-sm shadow-xs border"
                :class="u.role === 'Admin'
                  ? 'bg-[#214C38] text-white border-[#214C38]'
                  : (u.role === 'Staff'
                    ? 'bg-[#EFF6FF] text-[#1E40AF] border-[#DBEAFE]'
                    : 'bg-[#FFF7ED] text-[#9A3412] border-[#FFEDD5]')"
              >
                {{ u.avatarInitials }}
              </div>
              <div>
                <span class="text-xs font-mono font-bold text-[#182019] block">@{{ u.username }}</span>
                <span class="text-[10px] text-[#78716C] font-mono">ID: #{{ u.id }}</span>
              </div>
            </div>

            <!-- Role pill -->
            <span
              class="inline-flex items-center gap-1.5 px-2.5 py-1 rounded-lg text-[11px] font-semibold border"
              :class="u.role === 'Admin'
                ? 'bg-[#DCFCE7] text-[#166534] border-[#BBF7D0]'
                : (u.role === 'Staff'
                  ? 'bg-[#DBEAFE] text-[#1E40AF] border-[#BFDBFE]'
                  : 'bg-[#FFEDD5] text-[#9A3412] border-[#FED7AA]')"
            >
              <ShieldCheck v-if="u.role === 'Admin'" class="w-3.5 h-3.5" />
              <Briefcase v-else-if="u.role === 'Staff'" class="w-3.5 h-3.5" />
              <GraduationCap v-else class="w-3.5 h-3.5" />
              <span>{{ u.role === 'Admin' ? 'Administrator' : (u.role === 'Staff' ? 'Dosen / Staf' : 'Mahasiswa') }}</span>
            </span>
          </div>

          <!-- User Details -->
          <div class="space-y-2">
            <h3 class="font-serif text-lg font-bold text-[#182019] leading-snug group-hover:text-black">
              {{ u.fullName }}
            </h3>

            <div class="space-y-1.5 text-xs text-[#57534E]">
              <div class="flex items-center gap-2">
                <Mail class="w-3.5 h-3.5 text-[#78716C] shrink-0" />
                <span class="truncate font-mono text-[11px]">{{ u.email }}</span>
              </div>
              <div v-if="u.department" class="flex items-center gap-2">
                <Building2 class="w-3.5 h-3.5 text-[#78716C] shrink-0" />
                <span class="line-clamp-1">{{ u.department }}</span>
              </div>
            </div>
          </div>
        </div>

        <!-- Footer / Action Buttons -->
        <div class="mt-5 pt-3.5 border-t border-[#F1EFE9] flex items-center justify-between gap-2">
          <span class="text-[10px] font-mono text-[#78716C]">
            Terdaftar: {{ formatDate(u.createdAt) }}
          </span>

          <div class="flex items-center gap-1.5">
            <button
              :id="'edit-user-' + u.id + '-btn'"
              type="button"
              @click="openEditModal(u)"
              class="px-2.5 py-1.5 rounded-lg halden-btn-secondary text-xs flex items-center gap-1 cursor-pointer"
            >
              <Pencil class="w-3 h-3 text-[#78716C]" />
              <span>Edit</span>
            </button>

            <button
              :id="'delete-user-' + u.id + '-btn'"
              type="button"
              @click="promptDeleteUser(u)"
              class="px-2.5 py-1.5 rounded-lg text-xs font-mono border border-[#FECACA] text-[#991B1B] bg-white hover:bg-[#FEE2E2] transition-colors flex items-center gap-1 cursor-pointer active:scale-95"
              title="Hapus akun pengguna"
            >
              <Trash2 class="w-3.5 h-3.5" />
              <span class="text-[11px] font-medium hidden sm:inline">Hapus</span>
            </button>
          </div>
        </div>
      </div>
    </div>

    <!-- Create/Edit User Halden Modal Sheet -->
    <Teleport to="body">
      <div
        v-if="isModalOpen"
        id="user-form-modal"
        class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-[#182019]/45 backdrop-blur-sm overflow-y-auto"
        role="dialog"
        aria-modal="true"
        aria-labelledby="user-modal-title"
      >
        <transition name="modal-fade" appear>
          <div
            class="relative w-full max-w-lg rounded-[24px] bg-[#FBFAF7] border border-[#D9D8D1] shadow-2xl p-6 sm:p-7 space-y-5 text-[#182019] my-auto"
          >
            <!-- Header with Squircle Icon Box -->
            <div class="flex items-center justify-between pb-4 border-b border-[#D9D8D1]">
              <div class="flex items-center gap-3">
                <div class="w-10 h-10 rounded-xl bg-[#214C38] text-white flex items-center justify-center shadow-xs">
                  <UserPlus class="w-5 h-5" />
                </div>
                <div>
                  <span class="text-[10px] font-mono tracking-widest uppercase text-[#A45F35] font-bold">
                    Akses & Identitas
                  </span>
                  <h3 id="user-modal-title" class="font-serif text-xl font-bold text-[#182019] mt-0.5">
                    {{ isEditing ? 'Edit Profil Pengguna' : 'Tambah Pengguna Baru' }}
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
              <div class="grid grid-cols-1 sm:grid-cols-2 gap-3.5">
                <div>
                  <label for="user-username" class="block font-semibold text-[#3E473F] mb-1.5">
                    Username <span class="text-[#991B1B]">*</span>
                  </label>
                  <input
                    id="user-username"
                    name="user-username"
                    type="text"
                    v-model="formUsername"
                    :disabled="isEditing"
                    placeholder="Contoh: hendraw"
                    class="w-full px-3.5 py-2.5 rounded-xl border border-[#D9D8D1] bg-white text-xs font-mono focus:border-[#214C38] focus:ring-1 focus:ring-[#214C38] outline-hidden transition-all shadow-xs disabled:bg-[#F1EFE9] disabled:text-[#78716C]"
                  />
                </div>

                <div>
                  <label for="user-role" class="block font-semibold text-[#3E473F] mb-1.5">
                    Peran / Role <span class="text-[#991B1B]">*</span>
                  </label>
                  <select
                    id="user-role"
                    name="user-role"
                    v-model="formRole"
                    class="w-full px-3.5 py-2.5 rounded-xl border border-[#D9D8D1] bg-white text-xs font-sans font-medium focus:border-[#214C38] focus:ring-1 focus:ring-[#214C38] outline-hidden transition-all shadow-xs"
                  >
                    <option :value="2">Mahasiswa (Civitas)</option>
                    <option :value="1">Dosen & Staf (Civitas)</option>
                    <option :value="0">Administrator Fasilitas</option>
                  </select>
                </div>
              </div>

              <div>
                <label for="user-fullname" class="block font-semibold text-[#3E473F] mb-1.5">
                  Nama Lengkap & Gelar <span class="text-[#991B1B]">*</span>
                </label>
                <input
                  id="user-fullname"
                  name="user-fullname"
                  type="text"
                  v-model="formFullName"
                  placeholder="Contoh: Ahmad Faisal, S.Kom., M.T."
                  class="w-full px-3.5 py-2.5 rounded-xl border border-[#D9D8D1] bg-white text-xs placeholder:text-[#A8A29E] focus:border-[#214C38] focus:ring-1 focus:ring-[#214C38] outline-hidden transition-all shadow-xs"
                />
              </div>

              <div>
                <label for="user-email" class="block font-semibold text-[#3E473F] mb-1.5">
                  Alamat Email Kampus <span class="text-[#991B1B]">*</span>
                </label>
                <input
                  id="user-email"
                  name="user-email"
                  type="email"
                  v-model="formEmail"
                  placeholder="Contoh: ahmad.faisal@smartroom.ac.id"
                  class="w-full px-3.5 py-2.5 rounded-xl border border-[#D9D8D1] bg-white text-xs font-mono placeholder:text-[#A8A29E] focus:border-[#214C38] focus:ring-1 focus:ring-[#214C38] outline-hidden transition-all shadow-xs"
                />
              </div>

              <div>
                <label for="user-department" class="block font-semibold text-[#3E473F] mb-1.5">
                  Departemen / Program Studi / Unit Kerja
                </label>
                <input
                  id="user-department"
                  name="user-department"
                  type="text"
                  v-model="formDepartment"
                  placeholder="Contoh: Prodi Teknik Informatika atau Bagian Umum"
                  class="w-full px-3.5 py-2.5 rounded-xl border border-[#D9D8D1] bg-white text-xs placeholder:text-[#A8A29E] focus:border-[#214C38] focus:ring-1 focus:ring-[#214C38] outline-hidden transition-all shadow-xs"
                />
              </div>

              <div>
                <label for="user-password" class="block font-semibold text-[#3E473F] mb-1.5">
                  {{ isEditing ? 'Kata Sandi Baru (Kosongkan jika tidak ingin diubah)' : 'Kata Sandi Akun *' }}
                </label>
                <input
                  id="user-password"
                  name="user-password"
                  type="password"
                  v-model="formPassword"
                  :placeholder="isEditing ? 'Masukkan kata sandi baru...' : 'Minimal 6 karakter'"
                  class="w-full px-3.5 py-2.5 rounded-xl border border-[#D9D8D1] bg-white text-xs font-mono placeholder:text-[#A8A29E] focus:border-[#214C38] focus:ring-1 focus:ring-[#214C38] outline-hidden transition-all shadow-xs"
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
                id="save-user-submit-btn"
                type="button"
                @click="handleSaveUser"
                :disabled="modalLoading"
                class="px-6 py-2.5 rounded-xl halden-btn-primary text-xs font-semibold flex items-center gap-1.5 cursor-pointer disabled:opacity-50 active:scale-95 transition-all"
              >
                <Loader2 v-if="modalLoading" class="w-3.5 h-3.5 animate-spin" />
                <span>{{ modalLoading ? 'Menyimpan...' : (isEditing ? 'Simpan Perubahan' : 'Buat Akun') }}</span>
              </button>
            </div>
          </div>
        </transition>
      </div>
    </Teleport>

    <!-- Delete User Confirmation Modal -->
    <Teleport to="body">
      <div
        v-if="isConfirmDeleteOpen && confirmDeleteUser"
        id="delete-user-confirm-modal"
        class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-[#182019]/45 backdrop-blur-sm"
        role="dialog"
        aria-modal="true"
        aria-labelledby="delete-user-dialog-title"
      >
        <transition name="modal-fade" appear>
          <div class="relative w-full max-w-md rounded-[24px] bg-[#FBFAF7] border border-[#D9D8D1] shadow-2xl p-6 sm:p-7 space-y-4 text-[#182019]">
            <div class="flex items-center gap-3">
              <div class="w-10 h-10 rounded-xl flex items-center justify-center shadow-xs border bg-[#FEE2E2] border-[#FECACA] text-[#991B1B]">
                <Trash2 class="w-5 h-5" />
              </div>
              <div>
                <p class="text-[10px] font-mono uppercase tracking-widest font-bold text-[#991B1B]">
                  Konfirmasi Hapus Akun
                </p>
                <h3 id="delete-user-dialog-title" class="font-serif text-lg font-bold text-[#182019]">
                  Hapus Akun Pengguna?
                </h3>
              </div>
            </div>

            <div class="p-3.5 rounded-xl bg-white border border-[#D9D8D1] text-xs space-y-1">
              <div class="font-bold text-[#182019] text-sm">{{ confirmDeleteUser.fullName }}</div>
              <div class="font-mono text-[#5F675F] text-[11px]">@{{ confirmDeleteUser.username }} &bull; {{ confirmDeleteUser.email }}</div>
              <div class="text-[11px] font-semibold text-[#A45F35]">Peran: {{ confirmDeleteUser.role }}</div>
            </div>

            <p class="text-xs text-[#5F675F] leading-relaxed">
              Apakah Anda yakin ingin menghapus akun <strong>{{ confirmDeleteUser.fullName }}</strong>? Pengguna ini tidak akan dapat masuk kembali ke sistem.
            </p>

            <div class="flex items-center justify-end gap-2.5 pt-3 border-t border-[#D9D8D1]">
              <button
                type="button"
                @click="isConfirmDeleteOpen = false"
                :disabled="deleteLoading"
                class="px-4 py-2.5 rounded-xl halden-btn-secondary text-xs font-semibold cursor-pointer disabled:opacity-50"
              >
                Batal
              </button>
              <button
                id="execute-delete-user-btn"
                type="button"
                @click="executeDeleteUser"
                :disabled="deleteLoading"
                class="px-5 py-2.5 rounded-xl text-xs font-semibold shadow-xs transition-all active:scale-95 cursor-pointer flex items-center gap-1.5 text-white bg-[#991B1B] hover:bg-[#7F1D1D] disabled:opacity-50"
              >
                <Loader2 v-if="deleteLoading" class="w-3.5 h-3.5 animate-spin" />
                <span>{{ deleteLoading ? 'Menghapus...' : 'Ya, Hapus Akun' }}</span>
              </button>
            </div>
          </div>
        </transition>
      </div>
    </Teleport>
  </div>
</template>
