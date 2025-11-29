<script lang="ts" setup>
defineProps<{
  name: string;
}>();
</script>

<template>
  <div class="text-base-content text-sm font-semibold uppercase tracking-wide">{{ name }}</div>
</template>
