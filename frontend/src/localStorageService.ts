// Caution: After modifying LocalStorage type, change verison.
const key = 'resqueue-v1'

export interface LocalStorage {
  fontSize: '14px' | '16px'
}

const defaultValue: LocalStorage = {
  fontSize: '14px'
}

export function saveConfig(config: LocalStorage) {
  const serializedConfig = JSON.stringify(config)
  localStorage.setItem(key, serializedConfig)
}

export function loadConfig(): LocalStorage | null {
  const serializedConfig = localStorage.getItem(key)

  if (serializedConfig) {
    return JSON.parse(serializedConfig) as LocalStorage
  }

  return defaultValue
}
