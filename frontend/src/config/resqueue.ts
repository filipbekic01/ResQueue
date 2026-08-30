export interface ResqeueConfig {
  prefix: string;
}

const rawConfig: ResqeueConfig = (
  globalThis as typeof globalThis & {
    resqueueConfig?: ResqeueConfig;
  }
).resqueueConfig ?? {
  prefix: "",
};

const config: ResqeueConfig = {
  prefix: rawConfig.prefix.startsWith("/") ? rawConfig.prefix : rawConfig.prefix ? `/${rawConfig.prefix}` : "",
};

export default config;
