/** @type {import('tailwindcss').Config} */
const primeui = require("tailwindcss-primeui");
module.exports = {
  darkMode: ["selector", '[class="app-dark"]'],
  content: ["./src/**/*.{html,ts,scss,css}", "./index.html"],
  plugins: [primeui],
  theme: {
    screens: {
      sm: "576px",
      md: "768px",
      lg: "992px",
      xl: "1200px",
      "2xl": "1920px",
    },
    extend: {
      keyframes: {
        scale: {
          '0%, 100%': { transform: 'scale(1)' },
          '50%': { transform: 'scale(1.2)' },
        },
        fadeIn: {
          '0%': { opacity: '0', transform: 'translateY(10px)' },
          '100%': { opacity: '1', transform: 'translateY(0)' },
        }
      },
      animation: {
        'scale': 'scale 1s ease-in-out infinite',
        'fade-in': 'fadeIn 0.3s ease-out forwards',
      },
    },
  },
};
