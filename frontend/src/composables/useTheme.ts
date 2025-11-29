import { computed, ref, watch } from "vue";

export type Theme = "light" | "dark" | "system";

// Global shared state for theme
const currentTheme = ref<Theme>("system");
let initialized = false;

export function useTheme() {
  // Get system preference
  const getSystemTheme = (): "light" | "dark" => {
    if (typeof window === "undefined") return "light";
    return window.matchMedia("(prefers-color-scheme: dark)").matches ? "dark" : "light";
  };

  // Get the actual resolved theme (light or dark)
  const resolvedTheme = computed((): "light" | "dark" => {
    if (currentTheme.value === "system") {
      return getSystemTheme();
    }
    return currentTheme.value;
  });

  // Apply theme to DOM
  const applyTheme = () => {
    if (typeof document === "undefined") return;
    document.documentElement.setAttribute("data-theme", resolvedTheme.value);
  };

  // Load saved theme from localStorage
  const loadTheme = () => {
    if (typeof localStorage === "undefined") {
      currentTheme.value = "system";
      return;
    }

    const savedTheme = localStorage.getItem("theme") as Theme;
    if (savedTheme && ["light", "dark", "system"].includes(savedTheme)) {
      currentTheme.value = savedTheme;
    } else {
      currentTheme.value = "system";
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

  // Cycle through themes: light -> dark -> system -> light
  const cycleTheme = () => {
    if (currentTheme.value === "light") {
      setTheme("dark");
    } else if (currentTheme.value === "dark") {
      setTheme("system");
    } else {
      setTheme("light");
    }
  };

  // Initialize theme system
  const init = () => {
    if (initialized) return;
    initialized = true;

    loadTheme();
    applyTheme();

    // Listen for system theme changes
    if (typeof window !== "undefined") {
      const mediaQuery = window.matchMedia("(prefers-color-scheme: dark)");
      mediaQuery.addEventListener("change", () => {
        if (currentTheme.value === "system") {
          applyTheme();
        }
      });
    }

    // Watch for theme changes
    watch(currentTheme, () => {
      applyTheme();
    });
  };

  return {
    currentTheme,
    resolvedTheme,
    setTheme,
    cycleTheme,
    init,
  };
}
