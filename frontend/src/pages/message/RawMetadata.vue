<script setup lang="ts">
import { computed } from 'vue'

const props = defineProps<{ metadata: any }>()

const json = computed(() => JSON.stringify(props.metadata, null, 2))
</script>

<template>
  <div class="whitespace-break-spaces px-5">{{ json }}</div>
</template>
