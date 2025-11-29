import { reactive } from "vue";
import { useTheme } from "./useTheme";

interface UserSettings {
  queueType: number;
  sortField?: string;
  sortOrder?: number;
  queueSearch: string;
  topicSearch: string;
  refetchInterval: number;
  showGraph: boolean;
}

const storageKey = "userSettings";

const settings = reactive<UserSettings>({
  queueType: 1,
  sortField: undefined,
  sortOrder: undefined,
  queueSearch: "",
  topicSearch: "",
  refetchInterval: 5000,
  showGraph: true,
});

const init = () => {
  const storedSettings = localStorage.getItem(storageKey);
  if (storedSettings) {
    Object.assign(settings, JSON.parse(storedSettings));
  }

  // Initialize theme system
  const { init: initTheme } = useTheme();
  initTheme();
};

const updateSettings = (newSettings: UserSettings) => {
  Object.assign(settings, newSettings);
  localStorage.setItem(storageKey, JSON.stringify(settings));
};

const toggleGraph = () => {
  updateSettings({ ...settings, showGraph: !settings.showGraph });
};

export function useUserSettings() {
  return {
    settings,
    init,
    toggleGraph,
    updateSettings,
  };
}
