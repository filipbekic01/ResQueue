<script lang="ts" setup>
defineProps<{
  name: string
  value?: any
}>()
</script>

<template>
  <div class="flex border-t even:bg-surface-50">
    <div class="w-72 px-3 py-1 text-end">{{ name }}</div>
    <div class="px-3 py-1">
      <slot>{{ value }}</slot>
    </div>
  </div>
</template>
