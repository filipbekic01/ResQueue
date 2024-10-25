<script lang="ts" setup>
defineProps<{
  name: string
  value?: any
}>()
</script>

<template>
  <div class="flex">
    <div class="w-72 shrink-0 text-primary-500">{{ name }}</div>
    <div class="overflow-hidden break-words text-primary-600">
      <slot>{{ value ?? '-' }}</slot>
    </div>
  </div>
</template>
