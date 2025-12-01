/** @type {import('tailwindcss').Config} */
const primeui = require("tailwindcss-primeui");

module.exports = {
  content: ["./src/**/*.{html,ts,scss,css}", "./index.html"],
  safelist: [
    // Badge color classes - must be safelisted for dynamic template usage
    'bg-badge-success', 'text-badge-success',
    'bg-badge-info', 'text-badge-info',
    'bg-badge-warn', 'text-badge-warn',
    'bg-badge-danger', 'text-badge-danger',
    'bg-badge-secondary', 'text-badge-secondary',
    'bg-badge-teal', 'text-badge-teal',
  ],
  plugins: [primeui],
  theme: {
    screens: {
      // UNOPS responsive breakpoints
      "unops-mobile": { "max": "599px" },
      "unops-tablet": { "min": "600px", "max": "839px" },
      "unops-desktop": { "min": "840px" },
      // Keep existing for compatibility
      sm: "576px",
      md: "768px",
      lg: "992px",
      xl: "1200px",
      "2xl": "1920px",
    },
    extend: {
      // UNOPS Color Palette
      colors: {
        // Primary Colors
        "unops-primary": {
          DEFAULT: "#0092d1",
          light: "#1aa3db",
          lighter: "#4ec3e0", 
          dark: "#007bb8",
          darker: "#005a8a",
          on: "#ffffff",
          container: "#b3e0f7",
          "container-soft": "#e3f2fd",
          "on-container": "#004976",
          50: "#f0f8ff",
          100: "#e3f2fd",
          200: "#b3e0f7",
          300: "#7dccf0",
          400: "#4ec3e0",
          500: "#0092d1",
          600: "#007bb8",
          700: "#005a8a",
          800: "#004976",
          900: "#003455"
        },
        // Secondary Colors
        "unops-secondary": {
          DEFAULT: "#004976",
          light: "#1a5d8a",
          lighter: "#4080b8",
          dark: "#003a5f",
          darker: "#002b47",
          on: "#ffffff",
          container: "#b3d4f7",
          "container-soft": "#e3f0fd",
          "on-container": "#ffffff",
          50: "#f0f6ff",
          100: "#e3f0fd",
          200: "#b3d4f7",
          300: "#7db8f0",
          400: "#4080b8",
          500: "#004976",
          600: "#003a5f",
          700: "#002b47",
          800: "#001f35",
          900: "#001426"
        },
        // Accent Colors
        "unops-accent": {
          yellow: "#f8ea44",
          "yellow-light": "#fdf168",
          "yellow-soft": "#fefce8",
          orange: "#e85c0e",
          "orange-light": "#ff7849",
          "orange-soft": "#fff7ed",
          cherry: "#991e66",
          "cherry-light": "#c2185b",
          "cherry-soft": "#fdf2f8",
          lime: "#c4d600",
          "lime-light": "#d4e157",
          "lime-soft": "#f9fbe7",
          teal: "#00a997",
          "teal-light": "#1bb3a6",
          "teal-soft": "#e8f7f5",
          ocean: "#4ec3e0",
          "ocean-light": "#7dd3ea",
          "ocean-soft": "#f0f9ff"
        },
        // Neutral Colors  
        "unops-neutral": {
          grey: "#97999b",
          "grey-light": "#b8babb",
          "grey-lighter": "#d1d2d3",
          "grey-lightest": "#e8e9ea",
          "grey-dark": "#7a7c7e",
          "grey-darker": "#5c5e60",
          black: "#00070a",
          "black-soft": "#1a1b1c",
          white: "#ffffff",
          "white-soft": "#fefefe",
          50: "#f9fafb",
          100: "#f3f4f6",
          200: "#e8e9ea",
          300: "#d1d2d3",
          400: "#b8babb",
          500: "#97999b",
          600: "#7a7c7e",
          700: "#5c5e60",
          800: "#1a1b1c",
          900: "#00070a"
        },
        // Special Colors
        "unops-midnight-blue": "#004976",
        "unops-deep-sea": "#0f172a", 
        "unops-white": "#ffffff",
        // Surface Colors
        "unops-surface": {
          primary: "#ffffff",
          secondary: "#f8fafc",
          elevated: "#ffffff",
          cool: "#f6f9fc",
          warm: "#fef8f5",
          neutral: "#f7f6f5",
          cream: "#fdfaf1",
          fresh: "#f5fbfd",
          mint: "#ecf8f7",
        },
        // State Colors
        "unops-error": {
          DEFAULT: "#991e66",
          light: "#a91e6b",
          lighter: "#c2185b",
          dark: "#7b1538",
          darker: "#5d0f2a",
          on: "#ffffff"
        },
        "unops-success": {
          DEFAULT: "#10b981",
          light: "#16c487",
          lighter: "#34d399",
          dark: "#059669",
          darker: "#047857",
          on: "#ffffff"
        },
        "unops-info": {
          DEFAULT: "#4ec3e0",
          light: "#58c6e1",
          lighter: "#7dd3ea",
          dark: "#0891b2",
          darker: "#0e7490",
          on: "#ffffff"
        },
        "unops-warning": {
          DEFAULT: "#cc8400",
          light: "#d49207",
          lighter: "#f59e0b",
          dark: "#92400e",
          darker: "#78350f",
          on: "#ffffff"
        }
      },
      // UNOPS Typography
      fontFamily: {
        "unops-display": ["Inter", "Noto Sans", "-apple-system", "BlinkMacSystemFont", "Segoe UI", "system-ui", "sans-serif"],
        "unops-body": ["Inter", "Noto Sans", "-apple-system", "BlinkMacSystemFont", "Segoe UI", "system-ui", "sans-serif"]
      },
      fontWeight: {
        "unops-light": "300",
        "unops-regular": "400", 
        "unops-medium": "500",
        "unops-semibold": "600",
        "unops-bold": "700"
      },
      fontSize: {
        "unops-xs": "0.75rem",
        "unops-sm": "0.875rem", 
        "unops-base": "1rem",
        "unops-lg": "1.125rem",
        "unops-xl": "1.25rem",
        "unops-2xl": "1.5rem",
        "unops-display-large": "2.5rem",
        "unops-display-medium": "2rem",
        "unops-display-small": "1.5rem",
        "unops-headline-large": "1.875rem",
        "unops-headline-medium": "1.5rem", 
        "unops-headline-small": "1.25rem",
        "unops-body-large": "1rem",
        "unops-body-medium": "0.875rem",
        "unops-body-small": "0.75rem",
        "unops-label-large": "0.875rem",
        "unops-label-medium": "0.75rem",
        "unops-label-small": "0.6875rem"
      },
      // UNOPS Spacing
      spacing: {
        "unops-xs": "4px",
        "unops-sm": "8px",
        "unops-md": "16px", 
        "unops-lg": "24px",
        "unops-xl": "32px",
        "unops-2xl": "48px",
        "unops-3xl": "64px"
      },
      // UNOPS Border Radius
      borderRadius: {
        "unops-xs": "4px",
        "unops-sm": "6px",
        "unops-md": "8px",
        "unops-lg": "12px",
        "unops-xl": "16px",
        "unops-2xl": "20px",
        "unops-full": "9999px"
      },
      // UNOPS Box Shadow
      boxShadow: {
        "unops-sm": "0 1px 2px 0 rgba(0, 0, 0, 0.05)",
        "unops-md": "0 4px 6px -1px rgba(0, 0, 0, 0.1), 0 2px 4px -1px rgba(0, 0, 0, 0.06)",
        "unops-lg": "0 10px 15px -3px rgba(0, 0, 0, 0.1), 0 4px 6px -2px rgba(0, 0, 0, 0.05)",
        "unops-xl": "0 20px 25px -5px rgba(0, 0, 0, 0.1), 0 10px 10px -5px rgba(0, 0, 0, 0.04)"
      },
      // UNOPS Layout
      zIndex: {
        "unops-dropdown": "1000",
        "unops-modal": "1050", 
        "unops-toast": "1100",
        "unops-tooltip": "1200"
      },
      minHeight: {
        "unops-touch": "44px"
      },
      minWidth: {
        "unops-touch": "44px"
      },
      // UNOPS Animation
      transitionDuration: {
        "unops-fast": "150ms",
        "unops-short": "200ms",
        "unops-medium": "300ms", 
        "unops-normal": "250ms",
        "unops-slow": "350ms"
      },
      transitionTimingFunction: {
        "unops-standard": "cubic-bezier(0.2, 0.0, 0, 1.0)",
        "unops-decelerate": "cubic-bezier(0.0, 0.0, 0.2, 1.0)",
        "unops-smooth": "cubic-bezier(0.4, 0, 0.2, 1)"
      },
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
        'fade-in': 'fadeIn 0.3s ease-out forwards'
      },
      // Badge color utilities matching PrimeNG severities EXACTLY
      backgroundColor: {
        'badge-success': '#d1fae5',
        'badge-info': '#dbeafe',
        'badge-warn': '#fff7ed',     // UNOPS warning lighter - soft orange background for opportunity LIST view
        'badge-danger': '#fee2e2',
        'badge-secondary': '#f3f4f6',
        'badge-teal': '#e8f7f5',     // UNOPS teal accent soft (kept for other entities if needed)
      },
      textColor: {
        'badge-success': '#059669',
        'badge-info': '#2563eb',
        'badge-warn': '#cc8400',     // UNOPS warning DEFAULT - dark orange text for opportunity LIST view
        'badge-danger': '#dc2626',
        'badge-secondary': '#4b5563',
        'badge-teal': '#00a997',     // UNOPS teal accent (kept for other entities if needed)
      }
    }
  }
};
