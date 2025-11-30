import { computed, ref, watch } from "vue";

// Theme definition type
export interface ThemeOption {
  name: string;
  label: string;
  recommended?: boolean;
}

// Dark themes
export const darkThemes: ThemeOption[] = [
  { name: "dark", label: "Dark", recommended: true },
  { name: "synthwave", label: "Synthwave" },
  { name: "halloween", label: "Halloween" },
  { name: "forest", label: "Forest" },
  { name: "black", label: "Black" },
  { name: "luxury", label: "Luxury" },
  { name: "dracula", label: "Dracula" },
  { name: "business", label: "Business" },
  { name: "night", label: "Night" },
  { name: "coffee", label: "Coffee" },
  { name: "dim", label: "Dim" },
  { name: "sunset", label: "Sunset" },
];

// Light themes
export const lightThemes: ThemeOption[] = [
  { name: "light", label: "Light" },
  { name: "cupcake", label: "Cupcake" },
  { name: "bumblebee", label: "Bumblebee" },
  { name: "emerald", label: "Emerald" },
  { name: "corporate", label: "Corporate" },
  { name: "retro", label: "Retro" },
  { name: "cyberpunk", label: "Cyberpunk" },
  { name: "valentine", label: "Valentine" },
  { name: "garden", label: "Garden" },
  { name: "lofi", label: "Lo-Fi" },
  { name: "pastel", label: "Pastel" },
  { name: "fantasy", label: "Fantasy" },
  { name: "wireframe", label: "Wireframe" },
  { name: "cmyk", label: "CMYK" },
  { name: "autumn", label: "Autumn" },
  { name: "acid", label: "Acid" },
  { name: "lemonade", label: "Lemonade" },
  { name: "winter", label: "Winter" },
  { name: "nord", label: "Nord" },
  { name: "aqua", label: "Aqua" },
];

// All themes combined for validation
export const allThemes: ThemeOption[] = [...darkThemes, ...lightThemes];

export type Theme = string;

// Global shared state for theme
const currentTheme = ref<Theme>("dark");
let initialized = false;

export function useTheme() {
  // Apply theme to DOM
  const applyTheme = () => {
    if (typeof document === "undefined") return;
    document.documentElement.setAttribute("data-theme", currentTheme.value);
  };

  // Load saved theme from localStorage
  const loadTheme = () => {
    if (typeof localStorage === "undefined") {
      currentTheme.value = "dark";
      return;
    }

    const savedTheme = localStorage.getItem("theme") as Theme;
    const validThemes = allThemes.map((t) => t.name);
    if (savedTheme && validThemes.includes(savedTheme)) {
      currentTheme.value = savedTheme;
    } else {
      currentTheme.value = "dark";
    }
  };

  // Set theme and save to localStorage
  const setTheme = (theme: Theme) => {
    currentTheme.value = theme;
    if (typeof localStorage !== "undefined") {
      localStorage.setItem("theme", theme);
    }
    applyTheme();
  };

  // Initialize theme system
  const init = () => {
    if (initialized) return;
    initialized = true;

    loadTheme();
    applyTheme();

    // Watch for theme changes
    watch(currentTheme, () => {
      applyTheme();
    });
  };

  // Check if current theme is dark
  const isThemeDark = computed(() => {
    return darkThemes.some((t) => t.name === currentTheme.value);
  });

  return {
    currentTheme,
    darkThemes,
    lightThemes,
    isThemeDark,
    setTheme,
    init,
  };
}
