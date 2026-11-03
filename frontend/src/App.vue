<script setup lang="ts">
import { ref } from 'vue';
import Navbar from './components/Navbar.vue';
import BookingDrawer from './components/BookingDrawer.vue';
import ToastContainer from './components/ToastContainer.vue';
import { currentUser } from './stores/auth';

const isDrawerOpen = ref(false);
const drawerRoomId = ref<number | undefined>(undefined);
const drawerHour = ref<number | undefined>(undefined);

function handleOpenBooking(roomId?: number, hour?: number) {
  // Admins manage and review bookings, they never submit room borrowing requests
  if (currentUser.value?.role === 'Admin') return;
  drawerRoomId.value = roomId;
  drawerHour.value = hour;
  isDrawerOpen.value = true;
}

function handleBookingCreated() {
  window.dispatchEvent(new CustomEvent('booking-created'));
}
</script>

<template>
  <div class="app-canvas flex min-h-screen flex-col antialiased relative">
    
    <!-- Clean Architectural Sticky Navigation Header -->
    <Navbar @open-booking="handleOpenBooking" />

    <!-- Main Content Workspace with Fluid Fade Transition -->
    <main id="main-content" class="app-main flex-1" tabindex="-1">
      <router-view v-slot="{ Component }">
        <transition name="page-fade" mode="out-in">
          <component 
            :is="Component" 
            @open-booking="handleOpenBooking" 
          />
        </transition>
      </router-view>
    </main>

    <!-- Global Interactive Architectural Booking Drawer -->
    <BookingDrawer 
      :is-open="isDrawerOpen" 
      :initial-room-id="drawerRoomId"
      :initial-hour="drawerHour"
      @close="isDrawerOpen = false"
      @created="handleBookingCreated"
    />

    <!-- Global Tactile Toast Container -->
    <ToastContainer />

    <!-- Halden Studio Minimalist Architectural Footer -->
    <footer class="mt-auto border-t border-[#D9D8D1] bg-[#FBFAF7] py-8 text-sm text-[#596159]">
      <div class="mx-auto w-full max-w-[1440px] px-4 sm:px-6 lg:px-12">
        <div class="flex flex-col sm:flex-row items-center justify-between gap-4">
          <div class="flex items-center gap-2">
            <span class="font-serif text-lg font-semibold text-[#182019]">SmartRoom</span>
            <span class="h-1 w-1 rounded-full bg-[#A45F35]" aria-hidden="true"></span>
            <span class="text-xs text-[#5F675F]">Reservasi fasilitas kampus yang tertata.</span>
          </div>
          <div class="flex items-center gap-4 text-xs text-[#596159]">
            <span>&copy; {{ new Date().getFullYear() }} SmartRoom</span>
          </div>
        </div>
      </div>
    </footer>
  </div>
</template>
