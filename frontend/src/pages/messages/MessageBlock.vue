<script lang="ts" setup>
defineProps<{
  name: string
  value?: any
}>()
</script>

<template>
  <div class="flex">
    <div class="w-72 shrink-0 text-surface-500 dark:text-surface-400">{{ name }}</div>
    <div class="overflow-hidden break-words text-surface-600 dark:text-surface-500">
      <slot>{{ value ?? '-' }}</slot>
    </div>
  </div>
</template>
