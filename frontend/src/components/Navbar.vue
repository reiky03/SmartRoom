<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { currentUser, isAuthenticated, isAdmin, isStudent, logout, quickLoginAs } from '../stores/auth';
import { api } from '../api/client';
import { BookingStatus } from '../types';
import { 
  Building2, 
  Plus, 
  ExternalLink, 
  ChevronDown, 
  Menu, 
  X,
  ShieldCheck,
  Calendar,
  DoorOpen,
  LayoutDashboard,
  Terminal,
  LogOut,
  UserCheck
} from 'lucide-vue-next';

const emit = defineEmits<{
  (e: 'open-booking'): void;
}>();

const route = useRoute();
const router = useRouter();
const mobileOpen = ref(false);
const profileDropdownOpen = ref(false);
const pendingApprovalCount = ref(0);

onMounted(async () => {
  if (isAdmin.value) {
    try {
      const all = await api.getBookings({ status: BookingStatus.Pending });
      pendingApprovalCount.value = all.length;
    } catch (e) {
      // ignore
    }
  }
});

function handleLogout() {
  logout();
  profileDropdownOpen.value = false;
  router.push('/login');
}

function handleSwitchRole(role: 'Admin' | 'Staff' | 'Student') {
  quickLoginAs(role);
  profileDropdownOpen.value = false;
  if (role === 'Admin') {
    router.push('/admin/dashboard');
  } else {
    router.push('/student/dashboard');
  }
}
</script>

<template>
  <header class="sticky top-0 z-40 w-full bg-[#FAFAFA]/90 backdrop-blur-md border-b border-zinc-200/80">
    <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
      <div class="flex items-center justify-between h-16 gap-4">
        
        <!-- Left: Brand Logo & Navigation Links -->
        <div class="flex items-center gap-8">
          <!-- Brand -->
          <router-link :to="isAuthenticated ? (isAdmin ? '/admin/dashboard' : '/student/dashboard') : '/login'" class="flex items-center gap-2.5 text-zinc-950 group">
            <div class="w-8 h-8 rounded-xl bg-zinc-950 text-white flex items-center justify-center font-bold text-sm shadow-xs group-hover:scale-105 transition-transform duration-200">
              <span class="font-mono tracking-tighter">S</span>
            </div>
            <div class="flex items-baseline gap-1.5">
              <span class="font-bold text-base tracking-tight text-zinc-950">SmartRoom</span>
              <span 
                v-if="isAuthenticated" 
                class="inline-flex items-center gap-1 px-1.5 py-0.2 rounded-full text-[9px] font-mono font-bold uppercase tracking-wider border"
                :class="isAdmin ? 'bg-rose-50 text-rose-700 border-rose-200' : 'bg-emerald-50 text-emerald-700 border-emerald-200'"
              >
                {{ isAdmin ? 'Admin' : 'Civitas' }}
              </span>
            </div>
          </router-link>

          <!-- Desktop Nav Links (Role-Adaptive) -->
          <nav v-if="isAuthenticated" class="hidden md:flex items-center gap-1 bg-zinc-100 p-1 rounded-xl border border-zinc-200/60 text-xs font-medium text-zinc-600">
            
            <!-- STUDENT LINKS -->
            <template v-if="isStudent || (!isAdmin && isAuthenticated)">
              <router-link 
                to="/student/dashboard" 
                class="px-3 py-1.5 rounded-lg transition-all"
                :class="route.path === '/student/dashboard' ? 'bg-white text-zinc-950 shadow-xs font-semibold' : 'hover:text-zinc-950 hover:bg-white/50'"
              >
                Portal Mahasiswa
              </router-link>
            </template>

            <!-- ADMINISTRATOR LINKS -->
            <template v-else-if="isAdmin">
              <router-link 
                to="/admin/dashboard" 
                class="px-3 py-1.5 rounded-lg transition-all"
                :class="route.path === '/admin/dashboard' ? 'bg-white text-zinc-950 shadow-xs font-semibold' : 'hover:text-zinc-950 hover:bg-white/50'"
              >
                Dashboard
              </router-link>
              <router-link 
                to="/admin/approvals" 
                class="px-3 py-1.5 rounded-lg transition-all flex items-center gap-1.5"
                :class="route.path === '/admin/approvals' ? 'bg-white text-zinc-950 shadow-xs font-semibold' : 'hover:text-zinc-950 hover:bg-white/50'"
              >
                <span>Antrean Approval</span>
                <span v-if="pendingApprovalCount > 0" class="px-1.5 py-0.2 rounded text-[9px] font-mono bg-rose-600 text-white font-bold">
                  {{ pendingApprovalCount }}
                </span>
              </router-link>
              <router-link 
                to="/admin/rooms" 
                class="px-3 py-1.5 rounded-lg transition-all"
                :class="route.path === '/admin/rooms' ? 'bg-white text-zinc-950 shadow-xs font-semibold' : 'hover:text-zinc-950 hover:bg-white/50'"
              >
                Kelola Ruangan
              </router-link>
              <router-link 
                to="/admin/bookings" 
                class="px-3 py-1.5 rounded-lg transition-all"
                :class="route.path === '/admin/bookings' ? 'bg-white text-zinc-950 shadow-xs font-semibold' : 'hover:text-zinc-950 hover:bg-white/50'"
              >
                Audit Log
              </router-link>
            </template>
          </nav>
        </div>

        <!-- Right: Action CTA & User Menu -->
        <div class="flex items-center gap-2.5">
          
          <!-- Swagger Doc Link (For Evaluators) -->
          <a 
            href="/swagger" 
            target="_blank" 
            class="hidden sm:inline-flex items-center gap-1.5 px-3 py-1.5 rounded-lg text-xs font-mono text-zinc-600 hover:text-zinc-950 hover:bg-zinc-100 transition-colors border border-transparent hover:border-zinc-200"
          >
            <Terminal class="w-3.5 h-3.5 text-zinc-500" />
            <span>Swagger API</span>
            <ExternalLink class="w-3 h-3 text-zinc-400" />
          </a>

          <!-- Primary Booking Button -->
          <button 
            v-if="isAuthenticated"
            type="button"
            id="navbar-booking-btn"
            @click="emit('open-booking')"
            class="inline-flex items-center gap-1.5 px-3.5 py-2 rounded-xl bg-zinc-950 hover:bg-zinc-800 text-white text-xs font-medium shadow-xs transition-all active:scale-[0.98]"
          >
            <Plus class="w-3.5 h-3.5" />
            <span>Pinjam Ruangan</span>
          </button>

          <!-- Authenticated User Menu with RBAC Switcher -->
          <div v-if="isAuthenticated && currentUser" class="relative">
            <button 
              type="button"
              id="user-profile-btn"
              @click="profileDropdownOpen = !profileDropdownOpen" 
              class="flex items-center gap-2 p-1.5 pl-2.5 rounded-xl border border-zinc-200/80 bg-white hover:bg-zinc-50 transition-colors shadow-xs"
            >
              <div class="w-6 h-6 rounded-md bg-zinc-100 text-zinc-800 font-mono text-[11px] font-bold flex items-center justify-center border border-zinc-200">
                {{ currentUser.avatarInitials }}
              </div>
              <div class="hidden sm:flex flex-col text-left">
                <span class="text-xs font-semibold text-zinc-900 leading-none">{{ currentUser.fullName.split(' ')[0] }}</span>
              </div>
              <span 
                class="px-2 py-0.5 rounded-full text-[10px] font-mono font-semibold border"
                :class="currentUser.role === 'Admin' ? 'bg-rose-50 text-rose-700 border-rose-200' : 'bg-emerald-50 text-emerald-700 border-emerald-200'"
              >
                {{ currentUser.role }}
              </span>
              <ChevronDown class="w-3.5 h-3.5 text-zinc-400" />
            </button>

            <!-- Backdrop for closing dropdown -->
            <div 
              v-if="profileDropdownOpen" 
              class="fixed inset-0 z-40" 
              @click="profileDropdownOpen = false"
            ></div>

            <!-- Dropdown Flyout -->
            <transition name="modal-fade">
              <div 
                v-if="profileDropdownOpen"
                class="absolute right-0 mt-2 w-72 bg-white rounded-2xl shadow-xl border border-zinc-200/80 py-2 text-xs text-zinc-700 z-50 font-sans"
              >
                <div class="px-4 py-3 border-b border-zinc-100">
                  <div class="text-[10px] font-mono text-zinc-400 uppercase tracking-wider mb-1">Akun Terautentikasi</div>
                  <div class="font-bold text-zinc-950 text-sm leading-tight">{{ currentUser.fullName }}</div>
                  <div class="text-zinc-500 font-mono text-[11px] truncate">{{ currentUser.email }}</div>
                  <div class="mt-1.5 text-[10px] text-zinc-500 font-mono bg-zinc-50 p-1.5 rounded-md border border-zinc-100">
                    {{ currentUser.department }}
                  </div>
                </div>

                <!-- Portfolio Reviewer Switcher -->
                <div class="px-4 py-3 bg-zinc-50/60 border-b border-zinc-100">
                  <div class="text-[10px] font-mono uppercase tracking-wider text-zinc-400 font-semibold mb-2">
                    Uji RBAC (Pindah Peran Langsung):
                  </div>
                  <div class="grid grid-cols-2 gap-1.5 font-mono">
                    <button 
                      type="button" 
                      id="switch-to-student-btn"
                      @click="handleSwitchRole('Student')"
                      class="py-1.5 px-2 rounded-lg text-[10px] font-semibold transition-all text-center border"
                      :class="currentUser.role === 'Student' ? 'bg-emerald-100 text-emerald-800 border-emerald-300 font-bold' : 'bg-white text-zinc-600 border-zinc-200 hover:bg-zinc-100'"
                    >
                      🎓 Mahasiswa
                    </button>
                    <button 
                      type="button" 
                      id="switch-to-admin-btn"
                      @click="handleSwitchRole('Admin')"
                      class="py-1.5 px-2 rounded-lg text-[10px] font-semibold transition-all text-center border"
                      :class="currentUser.role === 'Admin' ? 'bg-rose-100 text-rose-800 border-rose-300 font-bold' : 'bg-white text-zinc-600 border-zinc-200 hover:bg-zinc-100'"
                    >
                      👑 Admin
                    </button>
                  </div>
                </div>

                <!-- Logout -->
                <div class="px-2 pt-1">
                  <button 
                    type="button" 
                    @click="handleLogout"
                    class="w-full text-left px-3 py-2 rounded-lg text-rose-600 hover:bg-rose-50 flex items-center gap-2 transition-colors font-medium"
                  >
                    <LogOut class="w-3.5 h-3.5" />
                    <span>Keluar (Logout)</span>
                  </button>
                </div>
              </div>
            </transition>
          </div>

          <!-- Unauthenticated CTA -->
          <router-link 
            v-else
            to="/login"
            class="px-3.5 py-2 rounded-xl bg-zinc-950 text-white text-xs font-medium hover:bg-zinc-800 transition-colors shadow-xs"
          >
            Masuk ke Sistem
          </router-link>

          <!-- Mobile Hamburger -->
          <button @click="mobileOpen = !mobileOpen" class="md:hidden p-2 rounded-xl text-zinc-600 hover:bg-zinc-100">
            <Menu v-if="!mobileOpen" class="w-5 h-5" />
            <X v-else class="w-5 h-5" />
          </button>
        </div>
      </div>

      <!-- Mobile Navigation Drawer -->
      <div v-if="mobileOpen" class="md:hidden py-3 border-t border-zinc-200/80 space-y-1">
        <template v-if="isAuthenticated">
          <router-link 
            v-if="isStudent" 
            to="/student/dashboard" 
            @click="mobileOpen = false" 
            class="block px-3 py-2 rounded-lg text-xs font-medium text-zinc-700 hover:bg-zinc-100"
          >
            Portal Mahasiswa
          </router-link>

          <template v-if="isAdmin">
            <router-link to="/admin/dashboard" @click="mobileOpen = false" class="block px-3 py-2 rounded-lg text-xs font-medium text-zinc-700 hover:bg-zinc-100">
              Dashboard
            </router-link>
            <router-link to="/admin/approvals" @click="mobileOpen = false" class="block px-3 py-2 rounded-lg text-xs font-medium text-zinc-700 hover:bg-zinc-100">
              Antrean Approval
            </router-link>
            <router-link to="/admin/rooms" @click="mobileOpen = false" class="block px-3 py-2 rounded-lg text-xs font-medium text-zinc-700 hover:bg-zinc-100">
              Kelola Ruangan
            </router-link>
            <router-link to="/admin/bookings" @click="mobileOpen = false" class="block px-3 py-2 rounded-lg text-xs font-medium text-zinc-700 hover:bg-zinc-100">
              Audit Log
            </router-link>
          </template>

          <button 
            @click="handleLogout" 
            class="w-full text-left px-3 py-2 text-rose-600 text-xs font-medium hover:bg-rose-50 rounded-lg flex items-center gap-1.5 mt-2"
          >
            <LogOut class="w-3.5 h-3.5" />
            <span>Keluar Akun</span>
          </button>
        </template>
        <template v-else>
          <router-link to="/login" @click="mobileOpen = false" class="block px-3 py-2 rounded-lg text-xs font-medium text-zinc-700 hover:bg-zinc-100">
            Masuk ke Akun
          </router-link>
        </template>
      </div>
    </div>
  </header>
</template>
