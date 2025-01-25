import { reactive } from 'vue'

interface UserSettings {
  queueType: number
  sortField?: string
  sortOrder?: number
  queueSearch: string
  topicSearch: string
  refetchInterval: number
  darkMode: boolean
}

const storageKey = 'userSettings'

const settings = reactive<UserSettings>({
  queueType: 1,
  sortField: undefined,
  sortOrder: undefined,
  queueSearch: '',
  topicSearch: '',
  refetchInterval: 5000,
  darkMode: false,
})

const init = () => {
  const storedSettings = localStorage.getItem(storageKey)
  if (storedSettings) {
    Object.assign(settings, JSON.parse(storedSettings))
  }

  loadDarkMode()
}

const updateSettings = (newSettings: UserSettings) => {
  Object.assign(settings, newSettings)
  localStorage.setItem(storageKey, JSON.stringify(settings))
}

const toggleDarkMode = () => {
  updateSettings({ ...settings, darkMode: !settings.darkMode })
  loadDarkMode()
}

const loadDarkMode = () => {
  const htmlElement = document.documentElement

  if (settings.darkMode === true) {
    htmlElement.classList.add('dark')
  } else {
    htmlElement.classList.remove('dark')
  }
}

export function useUserSettings() {
  return {
    settings,
    init,
    toggleDarkMode,
    updateSettings,
  }
}
