export interface ResqeueConfig {
  prefix: string
}

const config: ResqeueConfig = (
  globalThis as typeof globalThis & {
    resqueueConfig?: ResqeueConfig
  }
).resqueueConfig ?? {
  prefix: '',
}

export default config
