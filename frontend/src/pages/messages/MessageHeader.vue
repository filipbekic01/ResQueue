<script lang="ts" setup>
defineProps<{
  name: string
}>()
</script>

<template>
  <div class="bg-surface-100 px-3 py-2 text-center dark:bg-surface-800">{{ name }}</div>
</template>
