<script setup lang="ts">
import { toasts, removeToast } from '../stores/auth';
import { CheckCircle2, AlertCircle, Info, X } from 'lucide-vue-next';
</script>

<template>
  <div class="fixed top-5 right-5 z-50 flex flex-col gap-2 max-w-sm w-full pointer-events-none">
    <transition-group name="modal-fade">
      <div
        v-for="toast in toasts"
        :key="toast.id"
        class="pointer-events-auto p-3.5 rounded-xl border shadow-lg flex items-start justify-between gap-3 text-xs font-medium backdrop-blur-md transition-all"
        :class="{
          'bg-[#EDF3EC]/95 border-[#BBF7D0] text-[#166534]': toast.type === 'success',
          'bg-[#FEE2E2]/95 border-[#FECACA] text-[#991B1B]': toast.type === 'error',
          'bg-zinc-900/95 border-zinc-800 text-white': toast.type === 'info',
        }"
      >
        <div class="flex items-center gap-2">
          <CheckCircle2 v-if="toast.type === 'success'" class="w-4 h-4 flex-shrink-0 text-[#166534]" />
          <AlertCircle v-else-if="toast.type === 'error'" class="w-4 h-4 flex-shrink-0 text-[#991B1B]" />
          <Info v-else class="w-4 h-4 flex-shrink-0 text-zinc-400" />
          <span class="leading-tight">{{ toast.message }}</span>
        </div>
        <button @click="removeToast(toast.id)" class="opacity-60 hover:opacity-100 p-0.5 rounded transition-opacity">
          <X class="w-3.5 h-3.5" />
        </button>
      </div>
    </transition-group>
  </div>
</template>
