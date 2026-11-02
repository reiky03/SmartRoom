import { ref, computed } from 'vue';
import type { UserProfile } from '../types';

export interface ToastMessage {
  id: string;
  type: 'success' | 'error' | 'info';
  message: string;
}

const PRESET_USERS: Record<'Admin' | 'Staff' | 'Student', UserProfile> = {
  Admin: {
    username: 'admin',
    fullName: 'Dr. Hendra Wijaya, M.Kom',
    email: 'admin@kampus.ac.id',
    role: 'Admin',
    department: 'Biro Administrasi Sarana & Kampus',
    avatarInitials: 'HW',
  },
  Staff: {
    username: 'staff',
    fullName: 'Siti Rahmawati, S.T.',
    email: 'staff@kampus.ac.id',
    role: 'Staff',
    department: 'Laboratorium Terpadu FTI',
    avatarInitials: 'SR',
  },
  Student: {
    username: 'student',
    fullName: 'Reiky Pratama',
    email: 'student@students.ac.id',
    role: 'Student',
    department: 'Teknik Informatika (Civitas)',
    avatarInitials: 'RP',
  },
};

// Initial state from localStorage or default to Admin
const savedRole = (localStorage.getItem('smartroom_role') as 'Admin' | 'Staff' | 'Student') || 'Admin';
export const currentUser = ref<UserProfile>(PRESET_USERS[savedRole] || PRESET_USERS.Admin);

export const toasts = ref<ToastMessage[]>([]);

export function addToast(message: string, type: 'success' | 'error' | 'info' = 'success') {
  const id = Math.random().toString(36).substring(2, 9);
  toasts.value.push({ id, message, type });
  setTimeout(() => {
    removeToast(id);
  }, 4000);
}

export function removeToast(id: string) {
  toasts.value = toasts.value.filter((t) => t.id !== id);
}

export function switchRole(role: 'Admin' | 'Staff' | 'Student') {
  currentUser.value = PRESET_USERS[role];
  localStorage.setItem('smartroom_role', role);
  addToast(`Beralih peran ke ${currentUser.value.fullName} (${role})`, 'info');
}

export const isAdmin = computed(() => currentUser.value.role === 'Admin');
export const isStaff = computed(() => currentUser.value.role === 'Staff');
export const isStudent = computed(() => currentUser.value.role === 'Student');
