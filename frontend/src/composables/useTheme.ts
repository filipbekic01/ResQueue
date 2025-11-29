import { onUnmounted, ref, watch } from "vue";

export type Theme = "light" | "dark" | "system";

// Global shared state for dashboard theme
const dashboardTheme = ref<Theme>("system");
let systemThemeListener: (() => void) | null = null;

export function useTheme() {
  // Get system preference
  const getSystemTheme = (): "light" | "dark" => {
    if (typeof window === "undefined") return "light";
    return window.matchMedia("(prefers-color-scheme: dark)").matches ? "dark" : "light";
  };

  // Apply theme to DOM
  const applyTheme = (theme: Theme) => {
    if (typeof document === "undefined") return;

    let actualTheme: "light" | "dark";

    if (theme === "system") {
      actualTheme = getSystemTheme();
    } else {
      actualTheme = theme;
    }

    document.documentElement.setAttribute("data-theme", actualTheme);
  };

  // Force light theme (for non-dashboard pages)
  const forceLightTheme = () => {
    if (typeof document === "undefined") return;
    document.documentElement.setAttribute("data-theme", "light");
  };

  // Load saved theme from localStorage
  const loadDashboardTheme = () => {
    if (typeof localStorage === "undefined") {
      dashboardTheme.value = "system";
      return;
    }

    const savedTheme = localStorage.getItem("theme") as Theme;
    if (savedTheme && ["light", "dark", "system"].includes(savedTheme)) {
      dashboardTheme.value = savedTheme;
    } else {
      dashboardTheme.value = "system";
    }
  };

  // Set theme and save to localStorage
  const setTheme = (theme: Theme) => {
    dashboardTheme.value = theme;
    if (typeof localStorage !== "undefined") {
      localStorage.setItem("theme", theme);
    }
    applyTheme(theme);
  };

  // Setup system theme listener
  const setupSystemThemeListener = () => {
    if (systemThemeListener) return; // Already set up
    if (typeof window === "undefined") return;

    const mediaQuery = window.matchMedia("(prefers-color-scheme: dark)");
    const handleChange = () => {
      if (dashboardTheme.value === "system") {
        applyTheme("system");
      }
    };

    mediaQuery.addEventListener("change", handleChange);
    systemThemeListener = () => {
      mediaQuery.removeEventListener("change", handleChange);
      systemThemeListener = null;
    };
  };

  // Initialize for dashboard (with theme switching)
  const initializeDashboardTheme = () => {
    loadDashboardTheme();
    applyTheme(dashboardTheme.value);
    setupSystemThemeListener();

    // Watch for theme changes
    const stopWatching = watch(dashboardTheme, (newTheme) => {
      applyTheme(newTheme);
    });

    // Cleanup function
    onUnmounted(() => {
      stopWatching();
    });
  };

  // Initialize for non-dashboard pages (forced light theme)
  const initializeLightTheme = () => {
    forceLightTheme();
  };

  return {
    currentTheme: dashboardTheme,
    setTheme,
    initializeDashboardTheme,
    initializeLightTheme,
  };
}
