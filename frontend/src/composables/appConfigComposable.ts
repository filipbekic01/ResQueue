import { ref } from 'vue'

const localAppConfig = ref<LocalStorage>({
  fontSize: '14px'
})

// Caution: After modifying LocalStorage type, change verison.
const key = 'resqueue-v1'

export interface LocalStorage {
  fontSize: '14px' | '16px'
}

const updateAppConfig = (config: LocalStorage) => {
  const serializedConfig = JSON.stringify(config)
  localStorage.setItem(key, serializedConfig)
  localAppConfig.value = config

  loadFontSize()
}

const loadFontSize = () => {
  document.documentElement.style.fontSize = localAppConfig.value.fontSize
}

const serializedConfig = localStorage.getItem(key)

if (serializedConfig) {
  localAppConfig.value = JSON.parse(serializedConfig) as LocalStorage

  loadFontSize()
}

export function useAppConfig() {
  return {
    updateAppConfig,
    localAppConfig
  }
}
