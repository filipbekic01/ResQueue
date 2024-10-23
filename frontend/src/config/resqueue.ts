export default (
  globalThis as typeof globalThis & {
    resqueueConfig: {
      prefix: string
    }
  }
).resqueueConfig
