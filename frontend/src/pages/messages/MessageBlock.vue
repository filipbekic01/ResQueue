<script lang="ts" setup>
defineProps<{
  name: string
  value?: any
}>()
</script>

<template>
  <div class="flex border-t even:bg-surface-50 dark:border-t-surface-700 dark:even:bg-surface-800">
    <div class="w-72 shrink-0 px-3 py-1 text-end">{{ name }}</div>
    <div class="overflow-auto text-nowrap px-3 py-1 text-primary-500">
      <slot>{{ value }}</slot>
    </div>
  </div>
</template>
