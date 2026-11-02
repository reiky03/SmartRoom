<script setup lang="ts">
import { ref } from 'vue';
import { useRouter } from 'vue-router';
import { login, quickLoginAs, addToast } from '../stores/auth';
import { Lock, User, Eye, EyeOff, LogIn, ArrowRight, ShieldCheck, GraduationCap, Briefcase } from 'lucide-vue-next';

const router = useRouter();

const username = ref('student');
const password = ref('student123');
const showPassword = ref(false);
const errorMessage = ref('');
const isSubmitting = ref(false);

function handleLogin() {
  errorMessage.value = '';
  isSubmitting.value = true;

  try {
    const result = login(username.value, password.value);
    if (!result.success) {
      errorMessage.value = result.message;
      addToast(result.message, 'error');
      return;
    }

    if (result.role === 'Admin') {
      router.push('/admin/dashboard');
    } else {
      router.push('/student/dashboard');
    }
  } finally {
    isSubmitting.value = false;
  }
}

function handleQuickAccess(role: 'Admin' | 'Staff' | 'Student') {
  const profile = quickLoginAs(role);
  if (profile.role === 'Admin') {
    router.push('/admin/dashboard');
  } else {
    router.push('/student/dashboard');
  }
}
</script>

<template>
  <div class="min-h-[85vh] flex items-center justify-center py-10 px-4">
    <div class="w-full max-w-md space-y-6">
      
      <!-- Brand Header -->
      <div class="text-center space-y-2">
        <div class="inline-flex items-center justify-center w-12 h-12 rounded-2xl bg-zinc-950 text-white font-bold text-lg shadow-sm">
          <span class="font-mono">S</span>
        </div>
        <h1 class="text-2xl font-extrabold tracking-tight text-zinc-950">
          SmartRoom Enterprise
        </h1>
        <p class="text-xs text-zinc-500 max-w-xs mx-auto">
          Sistem Terintegrasi Peminjaman Ruangan &amp; Otentikasi Role-Based Access Control (RBAC)
        </p>
      </div>

      <!-- Main Login Card -->
      <div class="bg-white border border-zinc-200/80 rounded-2xl p-6 sm:p-8 shadow-xs space-y-5">
        
        <!-- Error Feedback Banner -->
        <div v-if="errorMessage" class="p-3.5 rounded-xl bg-[#FEE2E2] border border-[#FECACA] text-[#991B1B] text-xs font-medium">
          {{ errorMessage }}
        </div>

        <form @submit.prevent="handleLogin" class="space-y-4">
          <!-- Username Input -->
          <div>
            <label class="block text-[11px] font-mono uppercase tracking-wider font-semibold text-zinc-700 mb-1.5">
              Username atau Email
            </label>
            <div class="relative">
              <span class="absolute inset-y-0 left-0 flex items-center pl-3.5 text-zinc-400">
                <User class="w-4 h-4" />
              </span>
              <input 
                v-model="username"
                type="text" 
                required
                placeholder="student / admin / staff"
                class="w-full pl-9 pr-3.5 py-2.5 rounded-xl bg-zinc-50 border border-zinc-200 text-zinc-900 placeholder-zinc-400 text-xs focus:bg-white focus:border-zinc-900 focus:outline-none transition-all font-mono"
              />
            </div>
          </div>

          <!-- Password Input -->
          <div>
            <div class="flex items-center justify-between mb-1.5">
              <label class="block text-[11px] font-mono uppercase tracking-wider font-semibold text-zinc-700">
                Kata Sandi (Password)
              </label>
            </div>
            <div class="relative">
              <span class="absolute inset-y-0 left-0 flex items-center pl-3.5 text-zinc-400">
                <Lock class="w-4 h-4" />
              </span>
              <input 
                v-model="password"
                :type="showPassword ? 'text' : 'password'"
                required
                placeholder="••••••••"
                class="w-full pl-9 pr-10 py-2.5 rounded-xl bg-zinc-50 border border-zinc-200 text-zinc-900 placeholder-zinc-400 text-xs focus:bg-white focus:border-zinc-900 focus:outline-none transition-all font-mono"
              />
              <button 
                type="button" 
                @click="showPassword = !showPassword"
                class="absolute inset-y-0 right-0 flex items-center pr-3 text-zinc-400 hover:text-zinc-700"
              >
                <EyeOff v-if="showPassword" class="w-4 h-4" />
                <Eye v-else class="w-4 h-4" />
              </button>
            </div>
          </div>

          <!-- Submit Button -->
          <button 
            type="submit" 
            :disabled="isSubmitting"
            class="w-full py-2.5 px-4 rounded-xl bg-zinc-950 hover:bg-zinc-800 text-white font-medium text-xs shadow-xs transition-all active:scale-[0.98] flex items-center justify-center gap-2 pt-2.5"
          >
            <LogIn class="w-4 h-4" />
            <span>Masuk ke Sistem</span>
          </button>
        </form>

        <!-- Fast-Track Evaluator / Portfolio Reviewer Section -->
        <div class="pt-5 border-t border-zinc-100 space-y-2.5">
          <div class="flex items-center justify-between">
            <span class="text-[10px] font-mono uppercase tracking-wider text-zinc-400 font-semibold">
              Mode Peninjauan Portofolio (1-Klik Masuk):
            </span>
          </div>

          <!-- Persona 1: Student -->
          <button 
            type="button" 
            @click="handleQuickAccess('Student')"
            class="w-full p-2.5 rounded-xl border border-zinc-200 hover:border-zinc-900 hover:bg-zinc-50/80 transition-all flex items-center justify-between group text-left"
          >
            <div class="flex items-center gap-2.5">
              <div class="w-7 h-7 rounded-lg bg-emerald-50 text-emerald-700 flex items-center justify-center">
                <GraduationCap class="w-4 h-4" />
              </div>
              <div>
                <div class="text-xs font-semibold text-zinc-950 flex items-center gap-1.5">
                  <span>Mahasiswa / Civitas</span>
                  <span class="px-1.5 py-0.2 rounded text-[9px] font-mono font-medium bg-emerald-100 text-emerald-800">Student Role</span>
                </div>
                <div class="text-[10px] text-zinc-500 font-mono">student / student123 &bull; Akses: Booking &amp; Riwayat Pribadi</div>
              </div>
            </div>
            <ArrowRight class="w-4 h-4 text-zinc-400 group-hover:text-zinc-900 group-hover:translate-x-0.5 transition-all" />
          </button>

          <!-- Persona 2: Admin -->
          <button 
            type="button" 
            @click="handleQuickAccess('Admin')"
            class="w-full p-2.5 rounded-xl border border-zinc-200 hover:border-zinc-900 hover:bg-zinc-50/80 transition-all flex items-center justify-between group text-left"
          >
            <div class="flex items-center gap-2.5">
              <div class="w-7 h-7 rounded-lg bg-rose-50 text-rose-700 flex items-center justify-center">
                <ShieldCheck class="w-4 h-4" />
              </div>
              <div>
                <div class="text-xs font-semibold text-zinc-950 flex items-center gap-1.5">
                  <span>Administrator Fasilitas</span>
                  <span class="px-1.5 py-0.2 rounded text-[9px] font-mono font-medium bg-rose-100 text-rose-800">Admin Role</span>
                </div>
                <div class="text-[10px] text-zinc-500 font-mono">admin / admin123 &bull; Akses: Approval, Manajemen Ruang &amp; Audit</div>
              </div>
            </div>
            <ArrowRight class="w-4 h-4 text-zinc-400 group-hover:text-zinc-900 group-hover:translate-x-0.5 transition-all" />
          </button>

          <!-- Persona 3: Staff -->
          <button 
            type="button" 
            @click="handleQuickAccess('Staff')"
            class="w-full p-2.5 rounded-xl border border-zinc-200 hover:border-zinc-900 hover:bg-zinc-50/80 transition-all flex items-center justify-between group text-left"
          >
            <div class="flex items-center gap-2.5">
              <div class="w-7 h-7 rounded-lg bg-blue-50 text-blue-700 flex items-center justify-center">
                <Briefcase class="w-4 h-4" />
              </div>
              <div>
                <div class="text-xs font-semibold text-zinc-950 flex items-center gap-1.5">
                  <span>Staff / Dosen Pengajar</span>
                  <span class="px-1.5 py-0.2 rounded text-[9px] font-mono font-medium bg-blue-100 text-blue-800">Staff Role</span>
                </div>
                <div class="text-[10px] text-zinc-500 font-mono">staff / staff123 &bull; Akses: Prioritas Laboratorium</div>
              </div>
            </div>
            <ArrowRight class="w-4 h-4 text-zinc-400 group-hover:text-zinc-900 group-hover:translate-x-0.5 transition-all" />
          </button>
        </div>
      </div>

      <!-- Footer Note -->
      <div class="text-center text-[11px] text-zinc-400 font-mono">
        ASP.NET Core 8 &bull; EF Core &bull; Vue 3 &bull; Role-Based Access Control
      </div>
    </div>
  </div>
</template>
