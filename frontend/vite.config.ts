import { fileURLToPath, URL } from "node:url";
import tailwindcss from "@tailwindcss/vite";
import vue from "@vitejs/plugin-vue";
import { defineConfig } from "vite";

// https://vitejs.dev/config/
export default defineConfig({
  base: process.env.NODE_ENV === "development" ? undefined : "/resqueue-4e8efb80-6aae-496f-b8bf-611b63e725bc",
  plugins: [
    vue(),
    tailwindcss(),
    {
      name: "inject-resqueue-config",
      transformIndexHtml(html) {
        if (process.env.NODE_ENV === "development") {
          return html;
        }

        return html.replace(
          "<!-- inject:resqueue-config-script -->",
          '<script src="/resqueue-4e8efb80-6aae-496f-b8bf-611b63e725bc/config.js"></script>',
        );
      },
    },
  ],
  resolve: {
    alias: {
      "@": fileURLToPath(new URL("./src", import.meta.url)),
    },
  },
});
