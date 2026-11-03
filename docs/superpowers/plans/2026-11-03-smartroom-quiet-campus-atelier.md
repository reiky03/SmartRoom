# SmartRoom Quiet Campus Atelier Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]` / `- [x]`) syntax for tracking.

**Goal:** Menyelesaikan redesign Vue SPA SmartRoom menjadi antarmuka minimal, elegan, interaktif, dan konsisten tanpa mengubah perilaku bisnis.

**Architecture:** Terapkan token dan primitive global terlebih dahulu, lalu gunakan primitive tersebut pada shell, autentikasi, dashboard, dan overlay. Tidak ada dependency baru; seluruh motion memakai Vue transitions dan CSS transform/opacity.

**Tech Stack:** Vue 3.5, TypeScript, Vue Router, Tailwind CSS 3, Lucide Vue, Vite 8, ASP.NET Core 8.

**Spec:** `docs/superpowers/specs/2026-11-03-smartroom-quiet-campus-atelier-design.md`

## Global Constraints

- Pertahankan seluruh ID E2E, event emit, route, pemanggilan API, dan role switcher.
- Jangan ubah backend atau database.
- Jangan menambah dependency.
- Hormati `prefers-reduced-motion`.

## Review Focus

- Quick login dan pergantian role tetap mengarah ke portal yang benar.
- Drawer tetap membuka ruangan/jam awal yang dipilih dan dapat ditutup.
- Filter serta action approval tetap dapat dioperasikan dengan keyboard.
- Layout tidak overflow pada 390px.
- Semua halaman admin tetap dapat dibaca pada data table yang padat.

---

### Task 1: Design tokens and application shell

**Files:** `frontend/src/style.css`, `frontend/src/App.vue`, `DESIGN.md`

- [x] Replace conflicting visual sources with semantic tokens and reusable primitives.
- [x] Add motion/accessibility safeguards and responsive shell spacing.
- [x] Run `npm run build` from `frontend`.

### Task 2: Navigation and authentication experience

**Files:** `frontend/src/components/Navbar.vue`, `frontend/src/views/LoginView.vue`

- [x] Preserve E2E IDs and role behavior while rebuilding visual hierarchy.
- [x] Add dynamic ARIA state, larger touch targets, and origin-aware menu motion.
- [x] Verify login and role switch journey in browser.

### Task 3: Student and admin primary dashboards

**Files:** `frontend/src/views/StudentDashboardView.vue`, `frontend/src/views/AdminDashboardView.vue`, `frontend/src/components/TimelineMatrix.vue`

- [x] Replace decorative labels with useful context and calmer typography.
- [x] Unify cards, status treatments, room photography, and action hierarchy.
- [x] Verify desktop and mobile dashboard layouts.

### Task 4: Operations screens and overlays

**Files:** admin views and shared drawer/modal/toast components.

- [x] Apply the same surface, form, table, status, and motion language.
- [x] Preserve create/edit/toggle/approve/reject/filter flows.
- [x] Verify no console errors and no horizontal overflow.

### Task 5: Delivery gate

- [x] Run frontend build and .NET build.
- [x] Run non-mutating RBAC/navigation E2E smoke test.
- [x] Capture desktop and mobile screenshots and inspect them.
- [x] Run accessibility and reduced-motion checks available in the browser environment.
