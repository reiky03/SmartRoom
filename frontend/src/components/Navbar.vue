<script setup lang="ts">
import { ref, onMounted, onUnmounted } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { currentUser, isAuthenticated, isAdmin, isStudent, logout, quickLoginAs } from '../stores/auth';
import { api } from '../api/client';
import { BookingStatus } from '../types';
import {
  Plus,
  ExternalLink,
  ChevronDown,
  Menu,
  X,
  Terminal,
  LogOut,
  ShieldCheck,
  GraduationCap,
} from 'lucide-vue-next';

const emit = defineEmits<{
  (e: 'open-booking'): void;
}>();

const route = useRoute();
const router = useRouter();
const mobileOpen = ref(false);
const profileDropdownOpen = ref(false);
const pendingApprovalCount = ref(0);
const studentBookingsCount = ref(0);

onMounted(async () => {
  await loadCounts();
  window.addEventListener('booking-created', loadCounts);
});

onUnmounted(() => {
  window.removeEventListener('booking-created', loadCounts);
});

async function loadCounts() {
  if (isAdmin.value) {
    try {
      const pending = await api.getBookings({ status: BookingStatus.Pending });
      pendingApprovalCount.value = pending.length;
    } catch {
      // Navigation remains usable if the count endpoint is unavailable.
    }
  } else if (isStudent.value && currentUser.value?.email) {
    try {
      const my = await api.getBookings({ borrowerEmail: currentUser.value.email });
      studentBookingsCount.value = my.length;
    } catch {
      // Navigation remains usable if the count endpoint is unavailable.
    }
  }
}

function handleLogout() {
  logout();
  profileDropdownOpen.value = false;
  mobileOpen.value = false;
  router.push('/login');
}

function handleSwitchRole(role: 'Admin' | 'Staff' | 'Student') {
  quickLoginAs(role);
  profileDropdownOpen.value = false;
  mobileOpen.value = false;
  router.push(role === 'Admin' ? '/admin/dashboard' : '/student/dashboard');
  loadCounts();
}

function handleStudentNav(target: 'catalog' | 'bookings') {
  if (target === 'catalog') {
    if (route.path !== '/student/dashboard') {
      router.push('/student/dashboard');
    } else {
      window.scrollTo({ top: 0, behavior: 'smooth' });
    }
    return;
  }

  if (route.path !== '/student/dashboard') {
    router.push('/student/dashboard#my-bookings-section');
  } else {
    document.getElementById('my-bookings-section')?.scrollIntoView({ behavior: 'smooth' });
  }
}
</script>

<template>
  <header class="sticky top-0 z-40 border-b border-[#D9D8D1]/90 bg-[#FBFAF7]/94 backdrop-blur-xl">
    <div class="mx-auto w-full max-w-[1440px] px-4 sm:px-6 lg:px-12">
      <div class="flex min-h-[72px] items-center justify-between gap-4">
        <div class="flex min-w-0 items-center gap-7 lg:gap-10">
          <router-link
            :to="isAuthenticated ? (isAdmin ? '/admin/dashboard' : '/student/dashboard') : '/login'"
            class="group flex shrink-0 items-center gap-3 rounded-xl"
          >
            <span class="flex h-9 w-9 items-center justify-center rounded-[11px] bg-[#214C38] font-serif text-lg font-semibold text-white transition-transform duration-200 group-hover:-rotate-2 group-hover:scale-[1.03]">
              S
            </span>
            <span class="hidden flex-col sm:flex">
              <span class="font-serif text-[22px] font-semibold leading-none text-[#182019]">SmartRoom</span>
              <span class="mt-1 hidden text-[10px] font-semibold text-[#596159] sm:block">Campus space desk</span>
            </span>
          </router-link>

          <nav v-if="isAuthenticated" class="hidden items-center gap-1 lg:flex" aria-label="Navigasi utama">
            <template v-if="isStudent || (!isAdmin && isAuthenticated)">
              <button
                type="button"
                @click="handleStudentNav('catalog')"
                class="min-h-11 rounded-xl px-4 text-sm font-semibold transition-colors"
                :class="route.path === '/student/dashboard' && !route.hash.includes('my-bookings')
                  ? 'bg-[#E4EEE8] text-[#214C38]'
                  : 'text-[#5F675F] hover:bg-[#F1EFE9] hover:text-[#182019]'"
                :aria-current="route.path === '/student/dashboard' && !route.hash.includes('my-bookings') ? 'page' : undefined"
              >
                Katalog
              </button>
              <button
                id="nav-my-bookings-btn"
                type="button"
                @click="handleStudentNav('bookings')"
                class="flex min-h-11 items-center gap-2 rounded-xl px-4 text-sm font-semibold transition-colors"
                :class="route.hash.includes('my-bookings')
                  ? 'bg-[#E4EEE8] text-[#214C38]'
                  : 'text-[#5F675F] hover:bg-[#F1EFE9] hover:text-[#182019]'"
                :aria-current="route.hash.includes('my-bookings') ? 'page' : undefined"
              >
                <span>Peminjaman saya</span>
                <span v-if="studentBookingsCount > 0" class="rounded-full bg-[#214C38] px-2 py-0.5 text-[10px] font-bold text-white">
                  {{ studentBookingsCount }}
                </span>
              </button>
            </template>

            <template v-else-if="isAdmin">
              <router-link
                to="/admin/dashboard"
                class="flex min-h-11 items-center rounded-xl px-4 text-sm font-semibold transition-colors"
                active-class="bg-[#E4EEE8] text-[#214C38]"
                exact-active-class="bg-[#E4EEE8] text-[#214C38]"
                :class="route.path !== '/admin/dashboard' ? 'text-[#5F675F] hover:bg-[#F1EFE9] hover:text-[#182019]' : ''"
              >
                Ringkasan
              </router-link>
              <router-link
                to="/admin/approvals"
                class="flex min-h-11 items-center gap-2 rounded-xl px-4 text-sm font-semibold transition-colors"
                active-class="bg-[#E4EEE8] text-[#214C38]"
                :class="route.path !== '/admin/approvals' ? 'text-[#5F675F] hover:bg-[#F1EFE9] hover:text-[#182019]' : ''"
              >
                <span>Persetujuan</span>
                <span v-if="pendingApprovalCount > 0" class="rounded-full bg-[#A43D3D] px-2 py-0.5 text-[10px] font-bold text-white">
                  {{ pendingApprovalCount }}
                </span>
              </router-link>
              <router-link
                to="/admin/rooms"
                class="flex min-h-11 items-center rounded-xl px-4 text-sm font-semibold transition-colors"
                active-class="bg-[#E4EEE8] text-[#214C38]"
                :class="route.path !== '/admin/rooms' ? 'text-[#5F675F] hover:bg-[#F1EFE9] hover:text-[#182019]' : ''"
              >
                Ruangan
              </router-link>
              <router-link
                to="/admin/bookings"
                class="flex min-h-11 items-center rounded-xl px-4 text-sm font-semibold transition-colors"
                active-class="bg-[#E4EEE8] text-[#214C38]"
                :class="route.path !== '/admin/bookings' ? 'text-[#5F675F] hover:bg-[#F1EFE9] hover:text-[#182019]' : ''"
              >
                Riwayat
              </router-link>
            </template>
          </nav>
        </div>

        <div class="flex shrink-0 items-center gap-2 sm:gap-3">
          <button
            v-if="isAuthenticated && currentUser?.role !== 'Admin'"
            id="navbar-booking-btn"
            type="button"
            @click="emit('open-booking')"
            class="halden-btn-primary pressable inline-flex min-h-11 items-center gap-2 px-3.5 text-sm sm:px-5"
          >
            <Plus class="h-4 w-4" aria-hidden="true" />
            <span class="hidden sm:inline">Pinjam ruangan</span>
            <span class="sm:hidden">Pinjam</span>
          </button>

          <div v-if="isAuthenticated && currentUser" class="relative">
            <button
              id="user-profile-btn"
              type="button"
              @click="profileDropdownOpen = !profileDropdownOpen"
              class="flex min-h-11 items-center gap-2 rounded-xl border border-[#D9D8D1] bg-white px-2.5 text-left transition-colors hover:border-[#BFC3BC] hover:bg-[#FBFAF7]"
              :aria-expanded="profileDropdownOpen"
              aria-haspopup="menu"
            >
              <span class="flex h-7 w-7 items-center justify-center rounded-lg bg-[#F1EFE9] text-xs font-bold text-[#214C38]">
                {{ currentUser.avatarInitials }}
              </span>
              <span class="hidden max-w-24 truncate text-sm font-semibold text-[#182019] sm:block">
                {{ currentUser.fullName.split(' ')[0] }}
              </span>
              <span class="hidden rounded-full px-2 py-0.5 text-[10px] font-bold md:inline"
                :class="currentUser.role === 'Admin' ? 'status-danger' : 'status-success'">
                {{ currentUser.role === 'Admin' ? 'Admin' : 'Civitas' }}
              </span>
              <ChevronDown class="h-4 w-4 text-[#596159] transition-transform" :class="profileDropdownOpen ? 'rotate-180' : ''" aria-hidden="true" />
            </button>

            <div v-if="profileDropdownOpen" class="fixed inset-0 z-40 bg-black/20 sm:bg-transparent" @click="profileDropdownOpen = false"></div>

            <transition name="modal-fade">
              <div
                v-if="profileDropdownOpen"
                class="floating-surface fixed inset-x-3 top-[76px] z-50 space-y-4 rounded-[20px] p-4 sm:absolute sm:inset-x-auto sm:right-0 sm:top-full sm:mt-2 sm:w-[21rem] max-w-[calc(100vw-1.5rem)] shadow-xl"
                role="menu"
                aria-label="Menu akun"
              >
                <div class="rounded-[14px] bg-[#F5F3ED] p-4">
                  <p class="text-xs font-semibold text-[#596159]">Akun aktif</p>
                  <p class="mt-1 font-serif text-xl font-semibold leading-tight text-[#182019]">{{ currentUser.fullName }}</p>
                  <p class="mt-1 truncate text-xs text-[#596159]">{{ currentUser.email }}</p>
                  <p class="mt-3 flex items-center gap-2 text-xs font-semibold text-[#214C38]">
                    <span class="h-2 w-2 rounded-full bg-[#214C38]" aria-hidden="true"></span>
                    {{ currentUser.department }}
                  </p>
                </div>

                <div>
                  <div class="mb-2 flex items-center justify-between">
                    <p class="text-xs font-semibold text-[#5F675F]">Mode demo peran</p>
                    <span class="text-[10px] text-[#8A908A]">Akses 1 klik</span>
                  </div>
                  <div class="grid grid-cols-2 gap-2">
                    <button
                      id="switch-to-student-btn"
                      type="button"
                      @click="handleSwitchRole('Student')"
                      class="flex min-h-11 items-center justify-center gap-2 rounded-xl border px-3 text-xs font-bold transition-colors"
                      :class="currentUser.role === 'Student' ? 'border-[#214C38] bg-[#214C38] text-white' : 'border-[#D9D8D1] bg-white text-[#3E473F] hover:border-[#214C38]'"
                    >
                      <GraduationCap class="h-4 w-4" aria-hidden="true" /> Mahasiswa
                    </button>
                    <button
                      id="switch-to-admin-btn"
                      type="button"
                      @click="handleSwitchRole('Admin')"
                      class="flex min-h-11 items-center justify-center gap-2 rounded-xl border px-3 text-xs font-bold transition-colors"
                      :class="currentUser.role === 'Admin' ? 'border-[#214C38] bg-[#214C38] text-white' : 'border-[#D9D8D1] bg-white text-[#3E473F] hover:border-[#214C38]'"
                    >
                      <ShieldCheck class="h-4 w-4" aria-hidden="true" /> Admin
                    </button>
                  </div>
                </div>

                <div class="space-y-1 border-t border-[#D9D8D1] pt-3">
                  <a href="/swagger" target="_blank" class="flex min-h-11 items-center justify-between rounded-xl px-3 text-sm font-semibold text-[#5F675F] hover:bg-[#F5F3ED] hover:text-[#182019]">
                    <span class="flex items-center gap-2"><Terminal class="h-4 w-4" aria-hidden="true" /> Dokumentasi API</span>
                    <ExternalLink class="h-3.5 w-3.5" aria-hidden="true" />
                  </a>
                  <button type="button" @click="handleLogout" class="flex min-h-11 w-full items-center gap-2 rounded-xl px-3 text-sm font-semibold text-[#A43D3D] hover:bg-[#F7E8E6]">
                    <LogOut class="h-4 w-4" aria-hidden="true" /> Keluar akun
                  </button>
                </div>
              </div>
            </transition>
          </div>

          <router-link v-else to="/login" class="halden-btn-primary inline-flex min-h-11 items-center px-5 text-sm">Masuk portal</router-link>

          <button
            v-if="isAuthenticated"
            type="button"
            class="flex h-11 w-11 items-center justify-center rounded-xl border border-[#D9D8D1] bg-white text-[#3E473F] lg:hidden"
            @click="mobileOpen = !mobileOpen"
            :aria-expanded="mobileOpen"
            aria-label="Buka navigasi"
          >
            <X v-if="mobileOpen" class="h-5 w-5" aria-hidden="true" />
            <Menu v-else class="h-5 w-5" aria-hidden="true" />
          </button>
        </div>
      </div>

      <transition name="modal-fade">
        <nav v-if="mobileOpen && isAuthenticated" class="mb-4 grid gap-1 rounded-[18px] border border-[#D9D8D1] bg-white p-2 lg:hidden" aria-label="Navigasi mobile">
          <template v-if="isStudent">
            <button type="button" @click="handleStudentNav('catalog'); mobileOpen = false" class="min-h-11 rounded-xl px-4 text-left text-sm font-semibold text-[#182019] hover:bg-[#F1EFE9]">Katalog ruangan</button>
            <button type="button" @click="handleStudentNav('bookings'); mobileOpen = false" class="flex min-h-11 items-center justify-between rounded-xl px-4 text-left text-sm font-semibold text-[#182019] hover:bg-[#F1EFE9]">
              <span>Peminjaman saya</span>
              <span v-if="studentBookingsCount > 0" class="rounded-full bg-[#214C38] px-2 py-0.5 text-[10px] text-white">{{ studentBookingsCount }}</span>
            </button>
          </template>
          <template v-else-if="isAdmin">
            <router-link to="/admin/dashboard" @click="mobileOpen = false" class="flex min-h-11 items-center rounded-xl px-4 text-sm font-semibold text-[#182019] hover:bg-[#F1EFE9]">Ringkasan</router-link>
            <router-link to="/admin/approvals" @click="mobileOpen = false" class="flex min-h-11 items-center justify-between rounded-xl px-4 text-sm font-semibold text-[#182019] hover:bg-[#F1EFE9]">
              <span>Persetujuan</span><span v-if="pendingApprovalCount > 0" class="rounded-full bg-[#A43D3D] px-2 py-0.5 text-[10px] text-white">{{ pendingApprovalCount }}</span>
            </router-link>
            <router-link to="/admin/rooms" @click="mobileOpen = false" class="flex min-h-11 items-center rounded-xl px-4 text-sm font-semibold text-[#182019] hover:bg-[#F1EFE9]">Ruangan</router-link>
            <router-link to="/admin/bookings" @click="mobileOpen = false" class="flex min-h-11 items-center rounded-xl px-4 text-sm font-semibold text-[#182019] hover:bg-[#F1EFE9]">Riwayat</router-link>
          </template>
        </nav>
      </transition>
    </div>
  </header>
</template>
