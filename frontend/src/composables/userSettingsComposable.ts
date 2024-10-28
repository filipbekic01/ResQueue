import { reactive } from 'vue'

interface UserSettings {
  queueType: number
  sortField?: string
  sortOrder?: number
  refetchInterval: number
}

const storageKey = 'userSettings'

const settings = reactive<UserSettings>({
  queueType: 1,
  sortField: undefined,
  sortOrder: undefined,
  refetchInterval: 5000
})

const loadSettings = () => {
  const storedSettings = localStorage.getItem(storageKey)
  if (storedSettings) {
    Object.assign(settings, JSON.parse(storedSettings))
  }
}

const saveSettings = () => {
  localStorage.setItem(storageKey, JSON.stringify(settings))
}

const clearSettings = () => {
  localStorage.removeItem(storageKey)
  Object.assign(settings, { theme: 'light', fontSize: 16, language: 'en' })
}

const updateSettings = (newSettings: UserSettings) => {
  Object.assign(settings, newSettings)
  saveSettings()
}

loadSettings()

export function useUserSettings() {
  return {
    settings,
    saveSettings,
    clearSettings,
    updateSettings
  }
}
