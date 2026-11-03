<script setup lang="ts">
import { ref } from 'vue';
import { useRouter } from 'vue-router';
import { login, quickLoginAs, addToast } from '../stores/auth';
import {
  Lock,
  User,
  Eye,
  EyeOff,
  LogIn,
  ArrowRight,
  ShieldCheck,
  GraduationCap,
  Briefcase,
  CalendarCheck2,
  SplitSquareVertical,
  Shield,
} from 'lucide-vue-next';

const router = useRouter();
const username = ref('student');
const password = ref('student123');
const showPassword = ref(false);
const errorMessage = ref('');
const isSubmitting = ref(false);

async function handleLogin() {
  errorMessage.value = '';
  isSubmitting.value = true;

  try {
    const result = await login(username.value, password.value);
    if (!result.success) {
      errorMessage.value = result.message;
      addToast(result.message, 'error');
      return;
    }

    router.push(result.role === 'Admin' ? '/admin/dashboard' : '/student/dashboard');
  } finally {
    isSubmitting.value = false;
  }
}

function handleQuickAccess(role: 'Admin' | 'Staff' | 'Student') {
  const profile = quickLoginAs(role);
  router.push(profile.role === 'Admin' ? '/admin/dashboard' : '/student/dashboard');
}
</script>

<template>
  <section class="mx-auto grid min-h-[calc(100dvh-13rem)] w-full max-w-6xl items-center gap-10 py-8 lg:grid-cols-[1.05fr_.95fr] lg:gap-16 lg:py-14">
    <div class="order-2 lg:order-1">
      <h1 class="max-w-[10ch] text-[clamp(3.4rem,7.6vw,6.8rem)] font-medium leading-[0.88] text-[#182019]">
        Ruang tepat. Waktu pasti.
      </h1>
      <p class="mt-7 max-w-xl text-base leading-7 text-[#5F675F] sm:text-lg">
        Satu meja kerja untuk mencari ruang, memeriksa ketersediaan, mengajukan peminjaman, dan menuntaskan persetujuan tanpa jadwal yang saling bertabrakan.
      </p>

      <div class="mt-9 grid gap-3 sm:grid-cols-3 lg:max-w-2xl">
        <div class="rounded-[16px] bg-[#FBFAF7] p-4">
          <CalendarCheck2 class="h-5 w-5 text-[#214C38]" aria-hidden="true" />
          <p class="mt-4 text-sm font-bold text-[#182019]">Ketersediaan langsung</p>
          <p class="mt-1 text-xs leading-5 text-[#596159]">Slot terisi dan jam luang dalam satu tampilan.</p>
        </div>
        <div class="rounded-[16px] bg-[#FBFAF7] p-4">
          <SplitSquareVertical class="h-5 w-5 text-[#A45F35]" aria-hidden="true" />
          <p class="mt-4 text-sm font-bold text-[#182019]">Portal sesuai peran</p>
          <p class="mt-1 text-xs leading-5 text-[#596159]">Alur mahasiswa dan operasi tetap terpisah.</p>
        </div>
        <div class="rounded-[16px] bg-[#FBFAF7] p-4">
          <Shield class="h-5 w-5 text-[#214C38]" aria-hidden="true" />
          <p class="mt-4 text-sm font-bold text-[#182019]">Validasi bentrok</p>
          <p class="mt-1 text-xs leading-5 text-[#596159]">Irisan waktu diperiksa sebelum persetujuan.</p>
        </div>
      </div>
    </div>

    <div class="order-1 lg:order-2">
      <div class="surface-card relative overflow-hidden p-5 sm:p-8">
        <div class="absolute inset-x-0 top-0 h-1 bg-[#214C38]" aria-hidden="true"></div>

        <div class="mb-7">
          <p class="text-sm font-semibold text-[#A45F35]">Portal SmartRoom</p>
          <h2 class="mt-2 text-4xl font-semibold leading-none text-[#182019]">Masuk ke ruang kerja</h2>
          <p class="mt-3 text-sm leading-6 text-[#596159]">Gunakan akun Anda atau pilih mode demo untuk meninjau setiap alur.</p>
        </div>

        <div v-if="errorMessage" class="status-danger mb-5 flex items-start gap-2 rounded-xl border p-3.5 text-sm" role="alert">
          <span class="mt-0.5 font-bold" aria-hidden="true">×</span>
          <span>{{ errorMessage }}</span>
        </div>

        <form class="space-y-5" @submit.prevent="handleLogin">
          <div>
            <label for="login-username" class="mb-2 block text-sm font-semibold text-[#3E473F]">Username atau email</label>
            <div class="relative">
              <User class="pointer-events-none absolute left-4 top-1/2 h-4 w-4 -translate-y-1/2 text-[#596159]" aria-hidden="true" />
              <input
                id="login-username"
                v-model="username"
                type="text"
                required
                autocomplete="username"
                placeholder="student atau admin"
                class="halden-input w-full py-3 pl-11 pr-4 text-sm"
              />
            </div>
          </div>

          <div>
            <label for="login-password" class="mb-2 block text-sm font-semibold text-[#3E473F]">Kata sandi</label>
            <div class="relative">
              <Lock class="pointer-events-none absolute left-4 top-1/2 h-4 w-4 -translate-y-1/2 text-[#596159]" aria-hidden="true" />
              <input
                id="login-password"
                v-model="password"
                :type="showPassword ? 'text' : 'password'"
                required
                autocomplete="current-password"
                placeholder="Masukkan kata sandi"
                class="halden-input w-full py-3 pl-11 pr-12 text-sm"
              />
              <button
                type="button"
                class="absolute right-1 top-1/2 flex h-10 w-10 -translate-y-1/2 items-center justify-center rounded-lg text-[#596159] hover:bg-[#F1EFE9] hover:text-[#182019]"
                :aria-label="showPassword ? 'Sembunyikan kata sandi' : 'Tampilkan kata sandi'"
                :aria-pressed="showPassword"
                @click="showPassword = !showPassword"
              >
                <EyeOff v-if="showPassword" class="h-4 w-4" aria-hidden="true" />
                <Eye v-else class="h-4 w-4" aria-hidden="true" />
              </button>
            </div>
          </div>

          <button type="submit" :disabled="isSubmitting" class="halden-btn-primary flex w-full items-center justify-center gap-2 px-5 py-3 text-sm disabled:cursor-not-allowed disabled:opacity-50">
            <LogIn class="h-4 w-4" aria-hidden="true" />
            <span>{{ isSubmitting ? 'Memverifikasi…' : 'Masuk ke SmartRoom' }}</span>
          </button>
        </form>

        <div class="mt-7 rounded-[16px] bg-[#F5F3ED] p-4">
          <div class="mb-3 flex items-center justify-between gap-3">
            <p class="text-sm font-bold text-[#182019]">Akses demo satu klik</p>
            <p class="text-xs text-[#596159]">Untuk peninjauan portofolio</p>
          </div>

          <div class="space-y-2.5">
            <button id="quick-login-student-btn" type="button" @click="handleQuickAccess('Student')" class="group flex min-h-[64px] w-full items-center justify-between rounded-[14px] border border-[#D9D8D1] bg-white px-3.5 text-left transition-[border-color,background-color] hover:border-[#214C38] hover:bg-[#F7FAF8]">
              <span class="flex min-w-0 items-center gap-3">
                <span class="flex h-9 w-9 shrink-0 items-center justify-center rounded-xl bg-[#E4EEE8] text-[#214C38]"><GraduationCap class="h-4 w-4" aria-hidden="true" /></span>
                <span class="min-w-0"><span class="block text-sm font-bold text-[#182019]">Mahasiswa / Civitas</span><span class="block truncate text-xs text-[#596159]">Katalog, reservasi, dan riwayat pribadi</span></span>
              </span>
              <ArrowRight class="h-4 w-4 shrink-0 text-[#8A908A] transition-transform group-hover:translate-x-1 group-hover:text-[#214C38]" aria-hidden="true" />
            </button>

            <button id="quick-login-admin-btn" type="button" @click="handleQuickAccess('Admin')" class="group flex min-h-[64px] w-full items-center justify-between rounded-[14px] border border-[#D9D8D1] bg-white px-3.5 text-left transition-[border-color,background-color] hover:border-[#A45F35] hover:bg-[#FCF7F4]">
              <span class="flex min-w-0 items-center gap-3">
                <span class="flex h-9 w-9 shrink-0 items-center justify-center rounded-xl bg-[#F3E7DF] text-[#A45F35]"><ShieldCheck class="h-4 w-4" aria-hidden="true" /></span>
                <span class="min-w-0"><span class="block text-sm font-bold text-[#182019]">Administrator Fasilitas</span><span class="block truncate text-xs text-[#596159]">Persetujuan, inventaris, dan audit</span></span>
              </span>
              <ArrowRight class="h-4 w-4 shrink-0 text-[#8A908A] transition-transform group-hover:translate-x-1 group-hover:text-[#A45F35]" aria-hidden="true" />
            </button>

            <button type="button" @click="handleQuickAccess('Staff')" class="group flex min-h-[64px] w-full items-center justify-between rounded-[14px] border border-[#D9D8D1] bg-white px-3.5 text-left transition-[border-color,background-color] hover:border-[#214C38] hover:bg-[#F7FAF8]">
              <span class="flex min-w-0 items-center gap-3">
                <span class="flex h-9 w-9 shrink-0 items-center justify-center rounded-xl bg-[#EEF0EC] text-[#3E473F]"><Briefcase class="h-4 w-4" aria-hidden="true" /></span>
                <span class="min-w-0"><span class="block text-sm font-bold text-[#182019]">Staff / Dosen</span><span class="block truncate text-xs text-[#596159]">Akses civitas untuk kebutuhan pengajaran</span></span>
              </span>
              <ArrowRight class="h-4 w-4 shrink-0 text-[#8A908A] transition-transform group-hover:translate-x-1 group-hover:text-[#214C38]" aria-hidden="true" />
            </button>
          </div>
        </div>
      </div>
    </div>
  </section>
</template>
