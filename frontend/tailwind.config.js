/** @type {import('tailwindcss').Config} */
export default {
  content: [
    "./index.html",
    "./src/**/*.{vue,js,ts,jsx,tsx}",
  ],
  theme: {
    extend: {
      fontFamily: {
        sans: ['"Plus Jakarta Sans"', '-apple-system', 'BlinkMacSystemFont', 'sans-serif'],
        mono: ['"JetBrains Mono"', 'monospace'],
      },
      colors: {
        canvas: '#FAFAFA',
        surface: '#FFFFFF',
        borderSubtle: '#E4E4E7',
        textPrimary: '#09090B',
        textSecondary: '#71717A',
        textMuted: '#A1A1AA',
        pastelGreen: {
          bg: '#EDF3EC',
          border: '#BBF7D0',
          text: '#166534',
        },
        pastelAmber: {
          bg: '#FEF9C3',
          border: '#FEF08A',
          text: '#854D0E',
        },
        pastelRose: {
          bg: '#FEE2E2',
          border: '#FECACA',
          text: '#991B1B',
        }
      },
      boxShadow: {
        'xs': '0 1px 2px rgba(0, 0, 0, 0.04)',
        'subtle': '0 2px 10px rgba(0, 0, 0, 0.03)',
      }
    },
  },
  plugins: [],
}
