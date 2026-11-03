import { createRouter, createWebHistory, type RouteRecordRaw } from 'vue-router';
import { currentUser, isAuthenticated, isAdmin, addToast } from '../stores/auth';

import LoginView from '../views/LoginView.vue';
import StudentDashboardView from '../views/StudentDashboardView.vue';
import AdminDashboardView from '../views/AdminDashboardView.vue';
import AdminApprovalsView from '../views/AdminApprovalsView.vue';
import AdminRoomsView from '../views/AdminRoomsView.vue';
import AdminAllBookingsView from '../views/AdminAllBookingsView.vue';
import AdminUsersView from '../views/AdminUsersView.vue';

const routes: RouteRecordRaw[] = [
  {
    path: '/login',
    name: 'Login',
    component: LoginView,
    meta: { guestOnly: true }
  },
  {
    path: '/student/dashboard',
    name: 'StudentDashboard',
    component: StudentDashboardView,
    meta: { requiresAuth: true, role: 'Student' }
  },
  {
    path: '/admin/dashboard',
    name: 'AdminDashboard',
    component: AdminDashboardView,
    meta: { requiresAuth: true, role: 'Admin' }
  },
  {
    path: '/admin/approvals',
    name: 'AdminApprovals',
    component: AdminApprovalsView,
    meta: { requiresAuth: true, role: 'Admin' }
  },
  {
    path: '/admin/rooms',
    name: 'AdminRooms',
    component: AdminRoomsView,
    meta: { requiresAuth: true, role: 'Admin' }
  },
  {
    path: '/admin/bookings',
    name: 'AdminBookings',
    component: AdminAllBookingsView,
    meta: { requiresAuth: true, role: 'Admin' }
  },
  {
    path: '/admin/users',
    name: 'AdminUsers',
    component: AdminUsersView,
    meta: { requiresAuth: true, role: 'Admin' }
  },
  {
    path: '/',
    name: 'Root',
    redirect: () => {
      if (!isAuthenticated.value) {
        return '/login';
      }
      return isAdmin.value ? '/admin/dashboard' : '/student/dashboard';
    }
  },
  {
    path: '/:pathMatch(.*)*',
    redirect: '/'
  }
];

const router = createRouter({
  history: createWebHistory(),
  routes,
  scrollBehavior() {
    return { top: 0 };
  }
});

router.beforeEach((to, _from, next) => {
  // If route is guest-only and user is already logged in
  if (to.meta.guestOnly && isAuthenticated.value) {
    if (isAdmin.value) {
      return next('/admin/dashboard');
    }
    return next('/student/dashboard');
  }

  // If route requires authentication
  if (to.meta.requiresAuth && !isAuthenticated.value) {
    addToast('Sesi belum aktif. Silakan masuk terlebih dahulu.', 'info');
    return next({ path: '/login', query: { redirect: to.fullPath } });
  }

  // If route requires Admin role
  if (to.meta.role === 'Admin' && currentUser.value?.role !== 'Admin') {
    addToast('Akses Ditolak: Halaman ini memerlukan hak akses Administrator.', 'error');
    return next('/student/dashboard');
  }

  next();
});

export default router;
