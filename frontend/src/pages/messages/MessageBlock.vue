<script lang="ts" setup>
defineProps<{
  name: string;
  value?: any;
}>();
</script>

<template>
  <div class="flex text-sm">
    <div class="text-base-content/50 w-48 shrink-0 font-medium">{{ name }}</div>
    <div class="text-base-content/80 overflow-hidden break-words">
      <slot>{{ value ?? "-" }}</slot>
    </div>
  </div>
</template>
