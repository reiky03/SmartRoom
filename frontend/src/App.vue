<script setup lang="ts">
import { ref } from 'vue';
import Navbar from './components/Navbar.vue';
import BookingDrawer from './components/BookingDrawer.vue';
import ToastContainer from './components/ToastContainer.vue';

const isDrawerOpen = ref(false);
const drawerRoomId = ref<number | undefined>(undefined);
const drawerHour = ref<number | undefined>(undefined);

function handleOpenBooking(roomId?: number, hour?: number) {
  drawerRoomId.value = roomId;
  drawerHour.value = hour;
  isDrawerOpen.value = true;
}

function handleBookingCreated() {
  // If needed, refresh or trigger global bus
}
</script>

<template>
  <div class="min-h-screen flex flex-col antialiased selection:bg-zinc-900 selection:text-white relative bg-[#FAFAFA]">
    
    <!-- Ambient Subtle Radial Glow -->
    <div class="fixed inset-0 pointer-events-none z-0 overflow-hidden">
      <div class="absolute -top-40 left-1/2 -translate-x-1/2 w-[1000px] h-[500px] bg-gradient-to-b from-zinc-200/40 via-zinc-100/20 to-transparent blur-3xl opacity-60"></div>
    </div>

    <!-- Navigation Header -->
    <Navbar @open-booking="handleOpenBooking" />

    <!-- Main Workspace with Fluid Page Fade Transition -->
    <main class="flex-1 max-w-7xl w-full mx-auto px-4 sm:px-6 lg:px-8 py-8 relative z-10">
      <router-view v-slot="{ Component }">
        <transition name="page-fade" mode="out-in">
          <component 
            :is="Component" 
            @open-booking="handleOpenBooking" 
          />
        </transition>
      </router-view>
    </main>

    <!-- Global Interactive Booking Drawer -->
    <BookingDrawer 
      :is-open="isDrawerOpen" 
      :initial-room-id="drawerRoomId"
      :initial-hour="drawerHour"
      @close="isDrawerOpen = false"
      @created="handleBookingCreated"
    />

    <!-- Global Toast Container -->
    <ToastContainer />

    <!-- Sleek Minimalist Footer -->
    <footer class="mt-auto border-t border-zinc-200/80 bg-white/50 py-6 text-xs text-zinc-500 relative z-10">
      <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 flex flex-col sm:flex-row items-center justify-between gap-4">
        <div class="flex items-center gap-2">
          <span class="font-bold text-zinc-950">SmartRoom</span>
          <span>&bull;</span>
          <span>Campus &amp; Office Facility Reservation System</span>
        </div>
        <div class="flex items-center gap-4 font-mono text-[11px] text-zinc-500">
          <a href="/swagger" target="_blank" class="hover:text-zinc-950 transition-colors">Swagger API</a>
          <span>&bull;</span>
          <span class="text-zinc-700">Vue 3 + Vite + ASP.NET Core 8 REST API</span>
        </div>
      </div>
    </footer>
  </div>
</template>
