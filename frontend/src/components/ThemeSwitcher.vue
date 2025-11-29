<template>
  <button
    class="btn btn-circle btn-ghost btn-sm flex h-10 w-10 items-center justify-center transition-all"
    @click="cycleTheme"
    :title="`Theme: ${currentTheme}`"
  >
    <!-- Dynamic icon based on current theme -->
    <SunIcon v-if="currentTheme === 'light'" size="sm" />
    <MoonIcon v-else-if="currentTheme === 'dark'" size="sm" />
    <ComputerIcon v-else size="sm" />
  </button>
</template>

<script setup lang="ts">
import { useTheme } from "../composables/useTheme";
import { ComputerIcon, MoonIcon, SunIcon } from "./icons";

const { currentTheme, setTheme } = useTheme();

// Cycle through themes: light -> dark -> system -> light
const cycleTheme = () => {
  if (currentTheme.value === "light") {
    setTheme("dark");
  } else if (currentTheme.value === "dark") {
    setTheme("system");
  } else {
    setTheme("light");
  }
};
</script>
