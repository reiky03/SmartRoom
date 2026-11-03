# SmartRoom Design System — Quiet Campus Atelier

SmartRoom memakai bahasa visual fasilitas kampus modern: tenang, material, presisi, dan mudah dipindai. Sistem ini orisinal untuk konteks reservasi ruangan; ia tidak meniru identitas produk pihak ketiga.

## Atmosphere

- Limestone canvas dan paper-white surfaces membuat data padat tetap ringan.
- Deep forest ink memberi wibawa tanpa kontras hitam yang keras.
- Burnished bronze dipakai hanya untuk pilihan aktif atau perhatian penting.
- Foto ruangan adalah elemen visual utama; dekorasi abstrak tidak diperlukan.
- Transparansi hanya digunakan ketika elemen benar-benar mengambang: dropdown, toast, modal, dan drawer.

## Tokens

```css
--canvas: #F1EFE9;
--paper: #FBFAF7;
--surface: #FFFFFF;
--ink: #182019;
--text: #3E473F;
--muted: #6E766F;
--line: #D9D8D1;
--forest: #214C38;
--forest-soft: #E4EEE8;
--bronze: #A45F35;
--bronze-soft: #F3E7DF;
--danger: #A43D3D;
--danger-soft: #F7E8E6;
```

## Typography

- Newsreader: page title, room title, and one memorable statement per screen.
- Plus Jakarta Sans: navigation, forms, buttons, tables, descriptions, and status copy.
- JetBrains Mono: room codes, booking references, dates, and time ranges only.
- Avoid decorative uppercase labels, numbered section markers, and styled accent words.

## Components

- Buttons: 12px radius, 44px minimum height, forest primary, paper secondary.
- Inputs: 12px radius, visible label, 48px height, forest focus ring.
- Cards: 18px radius, tone separation first, subtle border second, no resting glow.
- Hero: 26px radius and asymmetric content hierarchy; one primary action.
- Status: text and icon accompany color; status colors never stand alone.
- Tables: generous row height, visible focus, responsive horizontal wrapper when unavoidable.

## Motion

- Fast feedback: 160ms.
- Menus and state transitions: 220–280ms.
- Drawer: 360ms using `cubic-bezier(0.22, 1, 0.36, 1)`.
- Animate transform, opacity, and limited filter only.
- Never pulse calls to action or animate keyboard-initiated actions.
- Always provide a `prefers-reduced-motion` path.

## Responsive behavior

- 390px: one-column flow, 16px gutters, 44px controls, no horizontal page overflow.
- 768px: two-column cards where content remains readable.
- 1024px+: full navigation, dashboard grids, timeline/data-table regions.
- Sticky chrome reserves space and must not obscure keyboard focus.
