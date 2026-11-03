<script setup lang="ts">
import { ref, computed } from 'vue';
import type { Room, Booking } from '../types';
import { BookingStatus } from '../types';
import { Clock, Plus, ExternalLink, ShieldCheck, Hourglass } from 'lucide-vue-next';
import { addToast } from '../stores/auth';

const props = withDefaults(defineProps<{
  rooms: Room[];
  bookings: Booking[];
  selectedDate?: string;
  isAdmin?: boolean;
  startHour?: number;
  endHour?: number;
}>(), {
  isAdmin: false,
  startHour: 8,
  endHour: 18,
});

const totalHours = computed(() => Math.max(1, props.endHour - props.startHour));

const emit = defineEmits<{
  (e: 'select-slot', payload: { roomId: number; hour: number }): void;
  (e: 'select-booking', booking: Booking): void;
  (e: 'select-room', roomId: number): void;
}>();

const hoveredBooking = ref<Booking | null>(null);
const tooltipCoords = ref<{ x: number; y: number; placeBelow: boolean }>({
  x: 0,
  y: 0,
  placeBelow: false,
});

function handleMouseEnter(event: MouseEvent, b: Booking) {
  const target = event.currentTarget as HTMLElement;
  const rect = target.getBoundingClientRect();
  const rawX = rect.left + rect.width / 2;
  const x = Math.max(150, Math.min(window.innerWidth - 150, Math.round(rawX)));
  const placeBelow = rect.top < 170;
  const y = placeBelow ? Math.round(rect.bottom + 8) : Math.round(rect.top - 8);

  tooltipCoords.value = { x, y, placeBelow };
  hoveredBooking.value = b;
}

function handleMouseLeave() {
  hoveredBooking.value = null;
}

function handleBookingClick(b: Booking) {
  emit('select-booking', b);
}

function handleRoomClick(roomId: number) {
  emit('select-room', roomId);
}

// Group bookings by roomId
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

  const timelineStart = props.startHour;
  const hoursSpan = totalHours.value;

  const leftOffset = Math.max(0, (startHour - timelineStart) / hoursSpan) * 100;
  const duration = Math.max(0.4, endHour - startHour);
  const width = Math.min(100 - leftOffset, (duration / hoursSpan) * 100);

  return {
    left: `${leftOffset.toFixed(2)}%`,
    width: `${width.toFixed(2)}%`,
  };
}

function handleTrackClick(event: MouseEvent, room: Room) {
  if (event.detail === 0) {
    handleTrackKeyboard(room);
    return;
  }

  const target = event.currentTarget as HTMLElement;
  const rect = target.getBoundingClientRect();
  const clickX = event.clientX - rect.left;
  const pct = clickX / rect.width;
  const clickedHour = Math.floor(props.startHour + pct * totalHours.value);
  const startH = clickedHour.toString().padStart(2, '0') + ':00';
  const endH = (clickedHour + 1).toString().padStart(2, '0') + ':00';

  if (props.isAdmin) {
    addToast(`Slot ${room.name} (${startH} – ${endH} WIB) kosong & siap digunakan.`, 'info');
    return;
  }
  emit('select-slot', { roomId: room.id, hour: Math.min(16, Math.max(8, clickedHour)) });
}

function handleTrackKeyboard(room: Room) {
  if (props.isAdmin) {
    addToast(`${room.name} memiliki slot kosong mulai pukul 08:00 WIB.`, 'info');
    return;
  }
  emit('select-slot', { roomId: room.id, hour: 8 });
}

function getRoomLiveStatus(room: Room) {
  const roomBookings = bookingsByRoom.value[room.id] || [];
  if (!roomBookings.length) return { label: 'Tersedia', inUse: false };

  const now = new Date();
  const currentMinutes = now.getHours() * 60 + now.getMinutes();

  for (const b of roomBookings) {
    if (b.status !== BookingStatus.Approved) continue;
    const start = new Date(b.startTime);
    const end = new Date(b.endTime);
    const startMin = start.getHours() * 60 + start.getMinutes();
    const endMin = end.getHours() * 60 + end.getMinutes();

    if (currentMinutes >= startMin && currentMinutes < endMin) {
      return { label: 'Sedang Dipakai', inUse: true };
    }
  }
  return { label: 'Tersedia', inUse: false };
}
</script>

<template>
  <div class="overflow-x-auto pb-2">
    <div :style="{ minWidth: `${Math.max(800, 240 + totalHours * 58)}px` }">

      <!-- Hours Header Ruler (startHour to endHour with explicit closing hour) -->
      <div class="relative mb-2.5 pl-[240px] select-none h-5">
        <div class="relative w-full h-full">
          <div
            v-for="h in (totalHours + 1)"
            :key="h"
            class="absolute top-0 text-[11px] font-mono tracking-tight"
            :class="[
              h === 1
                ? 'left-0 text-left font-semibold text-[#182019]'
                : h === (totalHours + 1)
                  ? 'right-0 text-right font-bold text-[#182019]'
                  : '-translate-x-1/2 text-center text-[#6E766F]'
            ]"
            :style="h > 1 && h < (totalHours + 1) ? { left: `${((h - 1) / totalHours) * 100}%` } : undefined"
          >
            {{ (h + props.startHour - 1).toString().padStart(2, '0') }}:00
          </div>
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
          <button
            type="button"
            class="w-[240px] flex-shrink-0 pr-4 text-left transition-colors"
            :class="isAdmin ? 'cursor-pointer group/room' : 'cursor-default'"
            @click="isAdmin ? handleRoomClick(room.id) : undefined"
            :disabled="!isAdmin"
            :title="isAdmin ? 'Klik untuk kelola ruangan' : room.name"
          >
            <div
              class="font-serif font-bold text-sm text-[#182019] truncate transition-colors"
              :class="isAdmin ? 'group-hover/room:text-[#214C38]' : ''"
              :title="room.name"
            >
              {{ room.name }}
            </div>
            <div class="text-[10px] font-mono text-[#6E766F] flex items-center gap-1.5 mt-0.5">
              <span class="px-2 py-0.5 rounded bg-white text-[#182019] border border-[#D9D8D1] font-bold shadow-xs">
                {{ room.roomCode }}
              </span>
              <span>&bull;</span>
              <span>{{ room.capacity }} Kursi</span>
              <span>&bull;</span>
              <span
                class="inline-flex items-center gap-1 font-semibold"
                :class="getRoomLiveStatus(room).inUse ? 'text-[#A43D3D]' : 'text-[#214C38]'"
              >
                <span class="w-1.5 h-1.5 rounded-full" :class="getRoomLiveStatus(room).inUse ? 'bg-[#A43D3D] animate-ping' : 'bg-[#214C38]'"></span>
                <span>{{ getRoomLiveStatus(room).label }}</span>
              </span>
            </div>
          </button>

          <!-- Runway Track (Quiet Campus Atelier Runway) -->
          <div
            class="relative h-11 flex-1 overflow-hidden rounded-xl border border-[#D9D8D1] bg-white"
            :class="isAdmin ? 'cursor-pointer' : 'cursor-crosshair'"
          >
            <button
              type="button"
              class="absolute inset-0 z-0 w-full text-left transition-colors hover:bg-[#F7FAF8] focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-[-3px] focus-visible:outline-[#214C38]"
              @click="handleTrackClick($event, room)"
              :aria-label="isAdmin ? `Lihat slot kosong ${room.name}` : `Pilih slot kosong ${room.name}`"
              :title="isAdmin ? 'Klik untuk info slot kosong' : 'Klik pada area kosong untuk memesan jadwal'"
            />

            <!-- Hourly Guideline Bars -->
            <div class="absolute inset-0 flex pointer-events-none">
              <div v-for="i in totalHours" :key="i" class="flex-1 border-r last:border-r-0 border-dashed border-[#D9D8D1]/70"></div>
            </div>

            <!-- Booking Blocks (Quiet Campus Atelier Capsules) -->
            <template v-if="bookingsByRoom[room.id]">
              <button
                type="button"
                v-for="b in bookingsByRoom[room.id]"
                :key="b.id"
                @click.stop="handleBookingClick(b)"
                @mouseenter="handleMouseEnter($event, b)"
                @mouseleave="handleMouseLeave"
                class="absolute inset-y-1 z-10 flex items-center justify-between truncate rounded-lg border px-2.5 text-[11px] font-mono font-medium transition-transform active:scale-[0.98] shadow-xs"
                :class="b.status === BookingStatus.Approved
                  ? 'status-success bg-[#E4EEE8] text-[#214C38] border-[#C3D9CD] hover:bg-[#D4E4D9]'
                  : 'status-warning bg-[#F8EFD7] text-[#996515] border-[#E4CD95] hover:bg-[#F1E3BE]'"
                :style="calculatePosition(b.startTime, b.endTime)"
              >
                <div class="truncate flex items-center gap-1.5">
                  <span class="font-bold">
                    {{ new Date(b.startTime).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }) }}&ndash;{{ new Date(b.endTime).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }) }}
                  </span>
                  <span class="truncate hidden lg:inline">({{ b.borrowerName }})</span>
                </div>
              </button>
            </template>
          </div>
        </div>
      </div>
    </div>

    <!-- Dynamic Floating Tooltip Popover (Teleported to body for zero clipping and exact positioning) -->
    <Teleport to="body">
      <Transition name="fade">
        <div
          v-if="hoveredBooking"
          class="floating-surface pointer-events-none fixed z-50 w-72 rounded-[18px] p-4 text-xs text-[#182019] bg-white border border-[#D9D8D1] shadow-2xl"
          :style="{
            left: `${tooltipCoords.x}px`,
            top: `${tooltipCoords.y}px`,
            transform: tooltipCoords.placeBelow ? 'translate(-50%, 0)' : 'translate(-50%, -100%)',
          }"
        >
          <div class="flex items-center justify-between gap-3 mb-1.5">
            <span class="font-mono text-[10px] text-[#6E766F]">{{ hoveredBooking.bookingReference }}</span>
            <span
              class="px-2 py-0.5 rounded text-[9px] font-mono font-bold uppercase"
              :class="hoveredBooking.status === BookingStatus.Approved ? 'bg-[#E4EEE8] text-[#214C38] border border-[#C3D9CD]' : 'bg-[#F8EFD7] text-[#996515] border border-[#E4CD95]'"
            >
              {{ hoveredBooking.status === BookingStatus.Approved ? 'Approved' : 'Pending' }}
            </span>
          </div>
          <div class="font-serif font-bold text-sm text-[#182019]">{{ hoveredBooking.borrowerName }}</div>
          <div class="text-[11px] text-[#5F675F] font-mono mt-0.5">
            {{ new Date(hoveredBooking.startTime).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }) }} &mdash;
            {{ new Date(hoveredBooking.endTime).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }) }}
          </div>
          <div class="text-[11px] text-[#5F675F] mt-1.5 line-clamp-2 leading-relaxed">{{ hoveredBooking.purpose }}</div>
          <div v-if="isAdmin" class="mt-2.5 pt-2 border-t border-[#D9D8D1] text-[10px] text-[#6E766F] font-mono flex items-center justify-between">
            <span>{{ hoveredBooking.status === BookingStatus.Pending ? 'Klik untuk tinjau persetujuan' : 'Klik untuk rincian riwayat' }}</span>
            <span>&rarr;</span>
          </div>
        </div>
      </Transition>
    </Teleport>
  </div>
</template>
