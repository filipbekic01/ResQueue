<script lang="ts" setup>
defineProps<{
  name: string;
  value?: any;
}>();
</script>

<template>
  <div class="flex">
    <div class="text-surface-500 dark:text-surface-400 w-72 shrink-0">{{ name }}</div>
    <div class="text-surface-600 dark:text-surface-500 overflow-hidden break-words">
      <slot>{{ value ?? "-" }}</slot>
    </div>
  </div>
</template>
