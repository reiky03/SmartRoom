<script setup lang="ts">
import { ref, computed } from 'vue';
import type { Room, Booking } from '../types';
import { BookingStatus } from '../types';
import { Clock, Plus, BarChart2 } from 'lucide-vue-next';

const props = defineProps<{
  rooms: Room[];
  bookings: Booking[];
  selectedDate: string;
}>();

const emit = defineEmits<{
  (e: 'select-slot', roomId: number, hour: number): void;
}>();

const activeTooltip = ref<string | null>(null);

// Group bookings by roomId for today
const bookingsByRoom = computed(() => {
  const map: Record<number, Booking[]> = {};
  for (const b of props.bookings) {
    if (b.status === BookingStatus.Rejected || b.status === BookingStatus.Cancelled) continue;
    if (!map[b.roomId]) map[b.roomId] = [];
    map[b.roomId].push(b);
  }
  return map;
});

function calculatePosition(startTimeStr: string, endTimeStr: string) {
  const start = new Date(startTimeStr);
  const end = new Date(endTimeStr);

  const startHour = start.getHours() + start.getMinutes() / 60;
  const endHour = end.getHours() + end.getMinutes() / 60;

  // Timeline spans 08:00 to 18:00 (10 hours total)
  const timelineStart = 8;
  const totalHours = 10;

  const leftOffset = Math.max(0, (startHour - timelineStart) / totalHours) * 100;
  const duration = Math.max(0.5, endHour - startHour);
  const width = Math.min(100 - leftOffset, (duration / totalHours) * 100);

  return {
    left: `${leftOffset.toFixed(2)}%`,
    width: `${width.toFixed(2)}%`,
  };
}

function handleTrackClick(event: MouseEvent, roomId: number) {
  const target = event.currentTarget as HTMLElement;
  const rect = target.getBoundingClientRect();
  const clickX = event.clientX - rect.left;
  const pct = clickX / rect.width;
  // 10 hours from 8 to 18
  const clickedHour = Math.floor(8 + pct * 10);
  emit('select-slot', roomId, Math.min(16, Math.max(8, clickedHour)));
}
</script>

<template>
  <div class="bg-white border border-zinc-200/80 rounded-2xl p-6 shadow-xs">
    <!-- Header -->
    <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4 mb-6">
      <div>
        <div class="flex items-center gap-2">
          <h2 class="text-base font-bold text-zinc-950 tracking-tight flex items-center gap-2">
            <BarChart2 class="w-4 h-4 text-zinc-700" />
            <span>Matriks Okupansi Ruangan Interaktif</span>
          </h2>
          <span class="px-2 py-0.5 rounded text-[10px] font-mono bg-zinc-100 text-zinc-600 font-medium border border-zinc-200/60">
            08:00 — 18:00
          </span>
        </div>
        <p class="text-xs text-zinc-500 mt-0.5">
          Arahkan kursor untuk info detail, atau <strong class="text-zinc-800">klik langsung pada slot kosong</strong> untuk memesan jam tersebut.
        </p>
      </div>

      <!-- Legend -->
      <div class="flex items-center gap-3 text-[11px] font-mono">
        <span class="inline-flex items-center gap-1.5 text-zinc-700">
          <span class="w-2.5 h-2.5 rounded-sm bg-[#166534]"></span> Disetujui
        </span>
        <span class="inline-flex items-center gap-1.5 text-zinc-700">
          <span class="w-2.5 h-2.5 rounded-sm bg-[#854D0E]"></span> Pending
        </span>
        <span class="inline-flex items-center gap-1.5 text-zinc-400">
          <span class="w-2.5 h-2.5 rounded-sm bg-zinc-200"></span> Kosong (Klik Pesan)
        </span>
      </div>
    </div>

    <!-- Timeline Grid Scrollable Wrapper -->
    <div class="overflow-x-auto pb-2">
      <div class="min-w-[760px]">
        
        <!-- Hours Header (08:00 to 18:00) -->
        <div class="flex text-[11px] font-mono text-zinc-400 mb-2 pl-[220px]">
          <div v-for="h in 10" :key="h" class="flex-1 border-l border-zinc-200/80 pl-1.5 text-left select-none">
            {{ (h + 7).toString().padStart(2, '0') }}:00
          </div>
        </div>

        <!-- Room Rows -->
        <div class="space-y-3">
          <div 
            v-for="room in rooms" 
            :key="room.id" 
            class="flex items-center group/row"
          >
            <!-- Room Info (Left Column) -->
            <div class="w-[220px] flex-shrink-0 pr-4">
              <div class="font-semibold text-xs text-zinc-950 truncate" :title="room.name">
                {{ room.name }}
              </div>
              <div class="text-[10px] font-mono text-zinc-400 flex items-center gap-1 mt-0.5">
                <span class="px-1.5 py-0.2 rounded bg-zinc-100 text-zinc-600 border border-zinc-200/60 font-semibold">
                  {{ room.roomCode }}
                </span>
                <span>&bull;</span>
                <span>{{ room.capacity }} Kursi</span>
              </div>
            </div>

            <!-- Runway Track -->
            <div 
              class="relative flex-1 h-9 bg-zinc-50 hover:bg-zinc-100/70 rounded-xl border border-zinc-200/80 overflow-hidden cursor-crosshair transition-colors"
              @click="handleTrackClick($event, room.id)"
              title="Klik di mana saja pada area kosong untuk memesan ruangan ini"
            >
              <!-- Hourly Guideline Bars -->
              <div class="absolute inset-0 flex pointer-events-none">
                <div v-for="i in 10" :key="i" class="flex-1 border-r border-dashed border-zinc-200/60"></div>
              </div>

              <!-- Booking Blocks -->
              <template v-if="bookingsByRoom[room.id]">
                <div
                  v-for="b in bookingsByRoom[room.id]"
                  :key="b.id"
                  @click.stop
                  @mouseenter="activeTooltip = b.bookingReference"
                  @mouseleave="activeTooltip = null"
                  class="absolute inset-y-1 rounded-lg px-2 flex items-center justify-between text-[11px] font-mono font-medium truncate border shadow-xs z-10 transition-all cursor-pointer"
                  :class="b.status === BookingStatus.Approved 
                    ? 'bg-[#EDF3EC] border-[#BBF7D0] text-[#166534] hover:bg-[#DCFCE7]' 
                    : 'bg-[#FEF9C3] border-[#FEF08A] text-[#854D0E] hover:bg-[#FEF08A]'"
                  :style="calculatePosition(b.startTime, b.endTime)"
                >
                  <div class="truncate flex items-center gap-1">
                    <span class="font-bold">
                      {{ new Date(b.startTime).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }) }}-{{ new Date(b.endTime).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }) }}
                    </span>
                    <span class="opacity-80 truncate hidden lg:inline">({{ b.borrowerName }})</span>
                  </div>

                  <!-- Tooltip Popover -->
                  <div 
                    v-if="activeTooltip === b.bookingReference" 
                    class="fixed z-50 p-3 bg-zinc-900 text-white rounded-xl shadow-xl text-xs max-w-xs pointer-events-none transform -translate-y-full -mt-2 font-sans border border-zinc-800"
                  >
                    <div class="flex items-center justify-between gap-3 mb-1">
                      <span class="font-mono text-[10px] text-zinc-400">{{ b.bookingReference }}</span>
                      <span 
                        class="px-1.5 py-0.2 rounded text-[9px] font-mono font-bold uppercase"
                        :class="b.status === BookingStatus.Approved ? 'bg-emerald-500/20 text-emerald-300' : 'bg-amber-500/20 text-amber-300'"
                      >
                        {{ b.status === BookingStatus.Approved ? 'Approved' : 'Pending' }}
                      </span>
                    </div>
                    <div class="font-semibold text-white">{{ b.borrowerName }}</div>
                    <div class="text-[11px] text-zinc-400 font-mono">
                      {{ new Date(b.startTime).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }) }} — 
                      {{ new Date(b.endTime).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }) }}
                    </div>
                    <div class="text-[11px] text-zinc-300 mt-1 line-clamp-2">{{ b.purpose }}</div>
                  </div>
                </div>
              </template>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>
