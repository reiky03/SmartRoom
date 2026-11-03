/** @type {import('tailwindcss').Config} */
export default {
  content: [
    "./index.html",
    "./src/**/*.{vue,js,ts,jsx,tsx}",
  ],
  theme: {
    extend: {
      fontFamily: {
        serif: ['"Newsreader"', 'Georgia', 'serif'],
        sans: ['"Plus Jakarta Sans"', '-apple-system', 'BlinkMacSystemFont', 'sans-serif'],
        mono: ['"JetBrains Mono"', 'monospace'],
      },
      colors: {
        canvas: '#F5F2EB',
        linen: '#F5F2EB',
        porcelain: '#FAF8F5',
        stoneBorder: '#E5DFD5',
        stoneBorderDark: '#D8D1C5',
        ink: '#1C1917',
        inkMuted: '#57534E',
        inkSubtle: '#78716C',
        terracotta: '#9A3412',
      },
      boxShadow: {
        'xs': '0 1px 2px rgba(28, 25, 23, 0.04)',
        'subtle': '0 2px 12px rgba(28, 25, 23, 0.03)',
        'elevated': '0 10px 30px -5px rgba(28, 25, 23, 0.06)',
      }
    },
  },
  plugins: [],
}