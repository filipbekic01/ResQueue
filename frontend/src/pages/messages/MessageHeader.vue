<script lang="ts" setup>
defineProps<{
  name: string
}>()
</script>

<template>
  <div class="dark:text-surface-0">{{ name }}</div>
</template>
