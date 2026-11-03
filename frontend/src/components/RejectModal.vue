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
  <div v-if="isOpen && booking" class="fixed inset-0 z-50 flex items-center justify-center bg-[#182019]/35 p-4 backdrop-blur-sm" role="dialog" aria-modal="true" aria-labelledby="reject-dialog-title">
    <transition name="modal-fade" appear>
      <div 
        class="floating-surface relative w-full max-w-md space-y-5 rounded-[20px] p-5 sm:p-6"
      >
        <div class="flex items-center justify-between pb-3 border-b border-[#E5DFD5]">
          <div class="flex items-center gap-3">
            <div class="w-8 h-8 rounded-lg bg-rose-50 border border-rose-200 text-rose-700 flex items-center justify-center shadow-xs">
              <XCircle class="w-4 h-4" />
            </div>
            <div>
              <span class="text-sm font-semibold text-[#A43D3D]">Keputusan administrator</span>
              <h3 id="reject-dialog-title" class="mt-1 font-serif text-2xl font-semibold text-[#182019]">Tolak pengajuan</h3>
            </div>
          </div>
          <button @click="emit('close')" class="flex h-11 w-11 items-center justify-center rounded-xl bg-[#F1EFE9] text-[#5F675F] hover:bg-[#E8E5DC] hover:text-[#182019]" aria-label="Tutup dialog penolakan">
            <X class="w-4 h-4" />
          </button>
        </div>

        <div class="p-3.5 rounded-lg bg-white border border-[#E5DFD5] text-stone-700 leading-relaxed font-sans">
          Menolak peminjaman referensi <strong class="text-[#1C1917] font-mono bg-[#F5F2EB] border border-[#E5DFD5] px-1.5 py-0.5 rounded text-[11px]">{{ booking.bookingReference }}</strong> atas nama <strong class="text-[#1C1917]">{{ booking.borrowerName }}</strong>.
        </div>

        <div>
          <label for="rejection-notes" class="mb-2 block text-sm font-semibold text-[#3E473F]">
            Alasan Resmi Penolakan <span class="text-rose-600">*</span>
          </label>
          <textarea
            id="rejection-notes"
            name="rejection-notes"
            v-model="notes" 
            rows="3" 
            required
            placeholder="Tuliskan alasan penolakan secara jelas agar pemohon dapat mengetahuinya pada portal mereka..."
            class="halden-input w-full resize-none px-3.5 py-3 text-sm placeholder:text-[#8A908A]"
          ></textarea>
        </div>

        <div class="flex items-center justify-end gap-2.5 pt-3 border-t border-[#E5DFD5]">
          <button 
            type="button" 
            @click="emit('close')"
            class="halden-btn-secondary px-4 text-sm"
          >
            Batal
          </button>
          <button 
            type="button" 
            @click="handleConfirm"
            :disabled="!notes.trim()"
            class="min-h-11 rounded-xl bg-[#A43D3D] px-4 text-sm font-bold text-white transition-colors hover:bg-[#843030] disabled:cursor-not-allowed disabled:opacity-40"
          >
            Konfirmasi Penolakan
          </button>
        </div>
      </div>
    </transition>
  </div>
</template>
