<script setup lang="ts">
import { ref, watch } from 'vue';
import type { Booking } from '../types';
import { X, XCircle } from 'lucide-vue-next';

const props = defineProps<{
  isOpen: boolean;
  booking: Booking | null;
}>();

const emit = defineEmits<{
  (e: 'close'): void;
  (e: 'confirm', notes: string): void;
}>();

const notes = ref('');

watch(() => props.isOpen, (open) => {
  if (open) {
    notes.value = '';
  }
});

function handleConfirm() {
  if (!notes.value.trim()) return;
  emit('confirm', notes.value.trim());
}
</script>

<template>
  <div v-if="isOpen && booking" class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-zinc-950/50 backdrop-blur-xs">
    <transition name="modal-fade" appear>
      <div 
        class="bg-white rounded-2xl max-w-md w-full p-6 shadow-2xl border border-zinc-200 text-xs"
        @click.outside="emit('close')"
      >
        <div class="flex items-center justify-between pb-3 border-b border-zinc-100">
          <div class="flex items-center gap-2">
            <div class="w-7 h-7 rounded-lg bg-rose-50 text-rose-600 flex items-center justify-center">
              <XCircle class="w-4 h-4" />
            </div>
            <h3 class="text-sm font-bold text-zinc-950">Tolak Pengajuan Peminjaman</h3>
          </div>
          <button @click="emit('close')" class="p-1 rounded text-zinc-400 hover:text-zinc-600">
            <X class="w-4 h-4" />
          </button>
        </div>

        <div class="py-3 text-zinc-600">
          Menolak peminjaman referensi <strong class="text-zinc-900 font-mono">{{ booking.bookingReference }}</strong> atas nama <strong class="text-zinc-900">{{ booking.borrowerName }}</strong>.
        </div>

        <div class="mb-4">
          <label class="block text-[11px] font-mono uppercase tracking-wider font-semibold text-zinc-700 mb-1.5">
            Alasan Penolakan <span class="text-rose-500">*</span>
          </label>
          <textarea 
            v-model="notes" 
            rows="3" 
            required
            placeholder="Contoh: Ruangan dialokasikan untuk kegiatan evaluasi mendesak rektorat."
            class="w-full px-3 py-2 rounded-xl bg-zinc-50 border border-zinc-200 text-xs text-zinc-800 focus:bg-white focus:border-zinc-900 focus:outline-none transition-all"
          ></textarea>
        </div>

        <div class="flex items-center justify-end gap-2.5 pt-3 border-t border-zinc-100">
          <button 
            type="button" 
            @click="emit('close')"
            class="px-3.5 py-2 rounded-xl border border-zinc-200 text-xs font-medium text-zinc-600 hover:bg-zinc-50 transition-colors"
          >
            Batal
          </button>
          <button 
            type="button" 
            @click="handleConfirm"
            :disabled="!notes.trim()"
            class="px-4 py-2 rounded-xl bg-rose-600 hover:bg-rose-500 disabled:bg-rose-300 text-white text-xs font-medium shadow-xs transition-colors"
          >
            Konfirmasi Penolakan
          </button>
        </div>
      </div>
    </transition>
  </div>
</template>
