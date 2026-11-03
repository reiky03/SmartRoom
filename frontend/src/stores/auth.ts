import { ref, computed } from 'vue';
import type { UserProfile } from '../types';
import { api } from '../api/client';

export interface ToastMessage {
  id: string;
  type: 'success' | 'error' | 'info';
  message: string;
}

export const PRESET_USERS: Record<'Admin' | 'Staff' | 'Student', UserProfile & { passwordDefault: string }> = {
  Student: {
    username: 'student',
    passwordDefault: 'student123',
    fullName: 'Rizky Pratama',
    email: 'student@smartroom.ac.id',
    role: 'Student',
    department: 'Teknik Informatika (Mahasiswa)',
    avatarInitials: 'RP',
  },
  Admin: {
    username: 'admin',
    passwordDefault: 'admin123',
    fullName: 'Dr. Hendra Wijaya, M.Sc.',
    email: 'admin@smartroom.ac.id',
    role: 'Admin',
    department: 'Biro Sarana & Fasilitas Kampus',
    avatarInitials: 'HW',
  },
  Staff: {
    username: 'staff',
    passwordDefault: 'staff123',
    fullName: 'Siti Nurhaliza, M.Kom.',
    email: 'staff@smartroom.ac.id',
    role: 'Staff',
    department: 'Laboratorium Rekayasa Perangkat Lunak',
    avatarInitials: 'SN',
  },
};

// Check if user session exists in localStorage
const savedUserJson = localStorage.getItem('smartroom_user_session');
let initialUser: UserProfile | null = null;
if (savedUserJson) {
  try {
    initialUser = JSON.parse(savedUserJson);
  } catch {
    initialUser = null;
  }
}

export const currentUser = ref<UserProfile | null>(initialUser);
export const isAuthenticated = computed(() => currentUser.value !== null);
export const isAdmin = computed(() => currentUser.value?.role === 'Admin');
export const isStudent = computed(() => currentUser.value?.role === 'Student');
export const isStaff = computed(() => currentUser.value?.role === 'Staff');

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

export async function login(usernameInput: string, passwordInput: string): Promise<{ success: boolean; message: string; role?: 'Admin' | 'Staff' | 'Student' }> {
  // Try real backend API authentication first
  try {
    const account = await api.loginApi(usernameInput.trim(), passwordInput);
    if (account && account.username) {
      const userProfile: UserProfile = {
        username: account.username,
        fullName: account.fullName,
        email: account.email,
        role: account.role as 'Admin' | 'Staff' | 'Student',
        department: account.department,
        avatarInitials: account.avatarInitials,
      };

      currentUser.value = userProfile;
      localStorage.setItem('smartroom_user_session', JSON.stringify(userProfile));
      addToast(`Selamat datang kembali, ${userProfile.fullName}! (${userProfile.role})`, 'success');

      return { success: true, message: 'Otentikasi berhasil', role: userProfile.role };
    }
  } catch (apiErr: any) {
    if (apiErr.status === 401 || (apiErr.message && !apiErr.message.includes('Failed to fetch') && !apiErr.message.includes('NetworkError'))) {
      return { success: false, message: apiErr.message || 'Username atau kata sandi tidak sesuai.' };
    }
  }

  // Fallback to presets if backend offline or mocked environment
  const normalizedUser = usernameInput.trim().toLowerCase();
  
  let matchedKey: 'Admin' | 'Staff' | 'Student' | null = null;
  if (normalizedUser === 'student' || normalizedUser === 'student@smartroom.ac.id') {
    matchedKey = 'Student';
  } else if (normalizedUser === 'admin' || normalizedUser === 'admin@smartroom.ac.id') {
    matchedKey = 'Admin';
  } else if (normalizedUser === 'staff' || normalizedUser === 'staff@smartroom.ac.id') {
    matchedKey = 'Staff';
  }

  if (!matchedKey) {
    return { success: false, message: 'Username atau email tidak terdaftar dalam sistem.' };
  }

  const preset = PRESET_USERS[matchedKey];
  if (passwordInput !== preset.passwordDefault) {
    return { success: false, message: 'Kata sandi tidak sesuai. Silakan coba lagi.' };
  }

  const userProfile: UserProfile = {
    username: preset.username,
    fullName: preset.fullName,
    email: preset.email,
    role: preset.role,
    department: preset.department,
    avatarInitials: preset.avatarInitials,
  };

  currentUser.value = userProfile;
  localStorage.setItem('smartroom_user_session', JSON.stringify(userProfile));
  addToast(`Selamat datang kembali, ${userProfile.fullName}! (${userProfile.role})`, 'success');

  return { success: true, message: 'Otentikasi berhasil', role: preset.role };
}

export function quickLoginAs(role: 'Admin' | 'Staff' | 'Student'): UserProfile {
  const preset = PRESET_USERS[role];
  const userProfile: UserProfile = {
    username: preset.username,
    fullName: preset.fullName,
    email: preset.email,
    role: preset.role,
    department: preset.department,
    avatarInitials: preset.avatarInitials,
  };

  currentUser.value = userProfile;
  localStorage.setItem('smartroom_user_session', JSON.stringify(userProfile));
  addToast(`Masuk sebagai ${userProfile.fullName} (${userProfile.role})`, 'success');
  return userProfile;
}

export function logout() {
  currentUser.value = null;
  localStorage.removeItem('smartroom_user_session');
  addToast('Sesi Anda telah berhasil diakhiri.', 'info');
}
