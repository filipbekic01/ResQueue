<script lang="ts" setup>
defineProps<{
  label: string
}>()
</script>

<template>
  <div class="my-2 flex flex-row items-center gap-3">
    <div class="text-xs uppercase text-gray-500">{{ label }}</div>
    <div class="h-px grow bg-slate-200"></div>
  </div>
</template>
