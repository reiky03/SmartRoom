<script setup lang="ts">
import { ref } from 'vue';
import { useRoute } from 'vue-router';
import { currentUser, isAdmin, switchRole } from '../stores/auth';
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
  Terminal
} from 'lucide-vue-next';

const emit = defineEmits<{
  (e: 'open-booking', roomId?: number): void;
}>();

const route = useRoute();
const mobileOpen = ref(false);
const roleDropdownOpen = ref(false);

function handleRoleChange(role: 'Admin' | 'Staff' | 'Student') {
  switchRole(role);
  roleDropdownOpen.value = false;
}
</script>

<template>
  <header class="sticky top-0 z-40 w-full bg-[#FAFAFA]/85 backdrop-blur-md border-b border-zinc-200/80">
    <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
      <div class="flex items-center justify-between h-16 gap-4">
        
        <!-- Left: Brand Logo & Navigation Links -->
        <div class="flex items-center gap-8">
          <!-- Brand -->
          <router-link to="/" class="flex items-center gap-2.5 text-zinc-900 group">
            <div class="w-8 h-8 rounded-lg bg-zinc-900 text-white flex items-center justify-center font-bold text-sm shadow-xs group-hover:scale-105 transition-transform duration-200">
              <span class="font-mono tracking-tighter">S</span>
            </div>
            <div class="flex items-baseline gap-1.5">
              <span class="font-bold text-base tracking-tight text-zinc-950">SmartRoom</span>
              <span class="inline-flex items-center gap-1 px-1.5 py-0.5 rounded-full text-[10px] font-mono text-zinc-500 bg-zinc-100 border border-zinc-200/80 font-medium">
                <span class="w-1.5 h-1.5 rounded-full bg-emerald-500 animate-pulse"></span> v2.4
              </span>
            </div>
          </router-link>

          <!-- Desktop Nav Links (Clean Segmented Style) -->
          <nav class="hidden md:flex items-center gap-1 bg-zinc-100 p-1 rounded-xl border border-zinc-200/60 text-xs font-medium text-zinc-600">
            <router-link 
              to="/" 
              class="px-3 py-1.5 rounded-lg transition-all"
              :class="route.path === '/' ? 'bg-white text-zinc-950 shadow-xs font-semibold' : 'hover:text-zinc-950 hover:bg-white/50'"
            >
              Dashboard
            </router-link>
            <router-link 
              to="/rooms" 
              class="px-3 py-1.5 rounded-lg transition-all"
              :class="route.path === '/rooms' ? 'bg-white text-zinc-950 shadow-xs font-semibold' : 'hover:text-zinc-950 hover:bg-white/50'"
            >
              Katalog Ruangan
            </router-link>
            <router-link 
              to="/bookings" 
              class="px-3 py-1.5 rounded-lg transition-all"
              :class="route.path === '/bookings' ? 'bg-white text-zinc-950 shadow-xs font-semibold' : 'hover:text-zinc-950 hover:bg-white/50'"
            >
              Jadwal Peminjaman
            </router-link>
            <router-link 
              v-if="isAdmin"
              to="/admin/approvals" 
              class="px-3 py-1.5 rounded-lg transition-all flex items-center gap-1.5"
              :class="route.path === '/admin/approvals' ? 'bg-white text-zinc-950 shadow-xs font-semibold' : 'hover:text-zinc-950 hover:bg-white/50'"
            >
              <span>Approval Queue</span>
              <span class="px-1.5 py-0.2 rounded text-[10px] font-mono bg-zinc-900 text-white font-bold">Admin</span>
            </router-link>
          </nav>
        </div>

        <!-- Right: Action CTA & User Menu -->
        <div class="flex items-center gap-2.5">
          
          <!-- Swagger Doc Link -->
          <a 
            href="/swagger" 
            target="_blank" 
            class="hidden sm:inline-flex items-center gap-1.5 px-3 py-1.5 rounded-lg text-xs font-mono text-zinc-600 hover:text-zinc-950 hover:bg-zinc-100 transition-colors border border-transparent hover:border-zinc-200"
          >
            <Terminal class="w-3.5 h-3.5 text-zinc-500" />
            <span>API Docs</span>
            <ExternalLink class="w-3 h-3 text-zinc-400" />
          </a>

          <!-- Primary Booking Button (Opens Interactive Drawer) -->
          <button 
            type="button"
            @click="emit('open-booking')"
            class="inline-flex items-center gap-1.5 px-3.5 py-2 rounded-xl bg-zinc-900 hover:bg-zinc-800 text-white text-xs font-medium shadow-xs transition-all active:scale-[0.98]"
          >
            <Plus class="w-3.5 h-3.5" />
            <span>Pinjam Ruangan</span>
          </button>

          <!-- 1-Click Role Switcher & Profile Dropdown -->
          <div class="relative">
            <button 
              type="button"
              @click="roleDropdownOpen = !roleDropdownOpen" 
              class="flex items-center gap-2 p-1.5 pl-2 rounded-xl border border-zinc-200/80 bg-white hover:bg-zinc-50 transition-colors shadow-xs"
            >
              <div class="w-6 h-6 rounded-md bg-zinc-100 text-zinc-800 font-mono text-[11px] font-bold flex items-center justify-center border border-zinc-200">
                {{ currentUser.avatarInitials }}
              </div>
              <div class="hidden lg:flex flex-col text-left">
                <span class="text-xs font-semibold text-zinc-800 leading-none">{{ currentUser.fullName.split(' ')[0] }}</span>
              </div>
              <span 
                class="px-2 py-0.5 rounded-full text-[10px] font-mono font-semibold border"
                :class="{
                  'bg-rose-50 text-rose-700 border-rose-200': currentUser.role === 'Admin',
                  'bg-blue-50 text-blue-700 border-blue-200': currentUser.role === 'Staff',
                  'bg-emerald-50 text-emerald-700 border-emerald-200': currentUser.role === 'Student'
                }"
              >
                {{ currentUser.role }}
              </span>
              <ChevronDown class="w-3.5 h-3.5 text-zinc-400" />
            </button>

            <!-- Dropdown Menu -->
            <transition name="modal-fade">
              <div 
                v-if="roleDropdownOpen"
                @click.outside="roleDropdownOpen = false"
                class="absolute right-0 mt-2 w-72 bg-white rounded-2xl shadow-xl border border-zinc-200/80 py-2 text-xs text-zinc-700 z-50 font-sans"
              >
                <!-- Profile Header -->
                <div class="px-4 py-2.5 border-b border-zinc-100">
                  <div class="text-[10px] font-mono text-zinc-400 uppercase tracking-wider mb-1">Pengguna Aktif</div>
                  <div class="font-bold text-zinc-950 text-sm leading-tight">{{ currentUser.fullName }}</div>
                  <div class="text-zinc-500 font-mono text-[11px] truncate">{{ currentUser.email }}</div>
                  <div class="mt-1.5 text-[10px] text-zinc-500 font-mono bg-zinc-50 p-1.5 rounded-md border border-zinc-100">
                    {{ currentUser.department }}
                  </div>
                </div>

                <!-- 1-Click Role Switcher -->
                <div class="px-4 py-3 bg-zinc-50/60 border-b border-zinc-100">
                  <div class="text-[10px] font-mono uppercase tracking-wider text-zinc-400 font-semibold mb-2">
                    Ganti Role Pengujian (1-Klik Instan)
                  </div>
                  <div class="grid grid-cols-3 gap-1.5 font-mono">
                    <button 
                      type="button" 
                      @click="handleRoleChange('Admin')"
                      class="py-1.5 px-2 rounded-lg text-[10px] font-semibold transition-all text-center border"
                      :class="currentUser.role === 'Admin' ? 'bg-rose-100 text-rose-800 border-rose-300 font-bold' : 'bg-white text-zinc-600 border-zinc-200 hover:bg-zinc-100'"
                    >
                      👑 Admin
                    </button>
                    <button 
                      type="button" 
                      @click="handleRoleChange('Staff')"
                      class="py-1.5 px-2 rounded-lg text-[10px] font-semibold transition-all text-center border"
                      :class="currentUser.role === 'Staff' ? 'bg-blue-100 text-blue-800 border-blue-300 font-bold' : 'bg-white text-zinc-600 border-zinc-200 hover:bg-zinc-100'"
                    >
                      👔 Staff
                    </button>
                    <button 
                      type="button" 
                      @click="handleRoleChange('Student')"
                      class="py-1.5 px-2 rounded-lg text-[10px] font-semibold transition-all text-center border"
                      :class="currentUser.role === 'Student' ? 'bg-emerald-100 text-emerald-800 border-emerald-300 font-bold' : 'bg-white text-zinc-600 border-zinc-200 hover:bg-zinc-100'"
                    >
                      🎓 Student
                    </button>
                  </div>
                </div>
              </div>
            </transition>
          </div>

          <!-- Mobile Hamburger -->
          <button @click="mobileOpen = !mobileOpen" class="md:hidden p-2 rounded-xl text-zinc-600 hover:bg-zinc-100">
            <Menu v-if="!mobileOpen" class="w-5 h-5" />
            <X v-else class="w-5 h-5" />
          </button>
        </div>
      </div>

      <!-- Mobile Navigation Drawer -->
      <div v-if="mobileOpen" class="md:hidden py-3 border-t border-zinc-200/80 space-y-1">
        <router-link to="/" @click="mobileOpen = false" class="block px-3 py-2 rounded-lg text-xs font-medium text-zinc-700 hover:bg-zinc-100">
          Dashboard
        </router-link>
        <router-link to="/rooms" @click="mobileOpen = false" class="block px-3 py-2 rounded-lg text-xs font-medium text-zinc-700 hover:bg-zinc-100">
          Katalog Ruangan
        </router-link>
        <router-link to="/bookings" @click="mobileOpen = false" class="block px-3 py-2 rounded-lg text-xs font-medium text-zinc-700 hover:bg-zinc-100">
          Jadwal Peminjaman
        </router-link>
        <router-link v-if="isAdmin" to="/admin/approvals" @click="mobileOpen = false" class="block px-3 py-2 rounded-lg text-xs font-medium text-zinc-700 hover:bg-zinc-100">
          Approval Queue (Admin)
        </router-link>
      </div>
    </div>
  </header>
</template>
