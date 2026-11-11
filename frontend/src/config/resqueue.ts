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

// The host app may run under a path base (e.g. an IIS virtual directory), in which case the bundle is
// served from "<pathBase>/resqueue-4e8efb80-6aae-496f-b8bf-611b63e725bc/assets/...". Derive the path
// base from this module's own URL so API calls and routing stay under it. Empty in dev mode.
const ASSETS_ROOT = "/resqueue-4e8efb80-6aae-496f-b8bf-611b63e725bc/";
const modulePath = new URL(import.meta.url).pathname;
export const pathBase = modulePath.includes(ASSETS_ROOT) ? modulePath.slice(0, modulePath.indexOf(ASSETS_ROOT)) : "";

export default config;
