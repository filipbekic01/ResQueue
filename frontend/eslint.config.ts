import skipFormatting from '@vue/eslint-config-prettier/skip-formatting'
import { defineConfigWithVueTs, vueTsConfigs } from "@vue/eslint-config-typescript";
import pluginVue from 'eslint-plugin-vue'
import eslintPluginPrettierRecommended from "eslint-plugin-prettier/recommended";
import { globalIgnores } from "eslint/config";
import prettierConfig from "@vue/eslint-config-prettier";

export default defineConfigWithVueTs(
  {
    name: 'app/files-to-lint',
    files: ['**/*.{ts,mts,tsx,vue}'],
  },
  globalIgnores([
    "**/dist/**",
    "**/dist-ssr/**",
    "**/coverage/**",
  ]),
  pluginVue.configs["flat/strongly-recommended"],
  vueTsConfigs.recommended,
  skipFormatting,
  eslintPluginPrettierRecommended,
  prettierConfig, // loads .prettierrc.cjs
   {
    rules: {
      "vue/multi-word-component-names": "off",
      "@typescript-eslint/no-explicit-any": "off",
      "vue/require-default-prop": "error",
      "@typescript-eslint/no-unused-vars": [
        "warn",
        {
          vars: "all",
          args: "after-used",
          ignoreRestSiblings: false,
          argsIgnorePattern: "^_",
        },
      ],
    },
    languageOptions: {
      globals: {
        globalThis: false, // means it is not writeable
      },
    },
  },
);
