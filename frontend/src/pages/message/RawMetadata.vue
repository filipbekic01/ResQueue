<script setup lang="ts">
import { computed } from 'vue'

const props = defineProps<{ metadata: any }>()

const json = computed(() => JSON.stringify(props.metadata, null, 2))
</script>

<template>
  <div class="px-5 whitespace-break-spaces">{{ json }}</div>
</template>
