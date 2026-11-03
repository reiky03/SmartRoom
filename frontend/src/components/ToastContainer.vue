<script setup lang="ts">
import { toasts, removeToast } from '../stores/auth';
import { CheckCircle2, AlertCircle, Info, X } from 'lucide-vue-next';
</script>

<template>
  <div class="pointer-events-none fixed bottom-4 right-4 z-50 flex w-[calc(100%-2rem)] max-w-sm flex-col gap-2.5" aria-live="polite" aria-atomic="false">
    <transition-group name="modal-fade">
      <div
        v-for="toast in toasts"
        :key="toast.id"
        class="floating-surface pointer-events-auto flex items-start justify-between gap-3 rounded-[16px] p-4 text-sm"
        :class="{
          'border-l-4 border-l-emerald-600': toast.type === 'success',
          'border-l-4 border-l-rose-600': toast.type === 'error',
          'border-l-4 border-l-[#1C1917]': toast.type === 'info',
        }"
      >
        <div class="flex items-center gap-2.5">
          <CheckCircle2 v-if="toast.type === 'success'" class="w-4 h-4 flex-shrink-0 text-emerald-600" />
          <AlertCircle v-else-if="toast.type === 'error'" class="w-4 h-4 flex-shrink-0 text-rose-600" />
          <Info v-else class="w-4 h-4 flex-shrink-0 text-[#1C1917]" />
          <span class="leading-tight font-sans font-medium text-[#1C1917]">{{ toast.message }}</span>
        </div>
        <button @click="removeToast(toast.id)" class="flex h-9 w-9 flex-shrink-0 items-center justify-center rounded-lg text-[#8A908A] transition-colors hover:bg-[#F1EFE9] hover:text-[#182019]" aria-label="Tutup pesan">
          <X class="w-3.5 h-3.5" />
        </button>
      </div>
    </transition-group>
  </div>
</template>
