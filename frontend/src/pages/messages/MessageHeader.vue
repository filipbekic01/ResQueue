<script lang="ts" setup>
defineProps<{
  name: string;
}>();
</script>

<template>
  <div class="text-base-content text-sm font-semibold tracking-wide uppercase">{{ name }}</div>
</template>
