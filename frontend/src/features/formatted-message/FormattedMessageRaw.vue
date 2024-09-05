<script lang="ts" setup>
import type { FormatOption } from '@/components/SelectFormat.vue'
import type { StructureOption } from '@/components/SelectStructure.vue'

defineProps<{
  message: any
  format: FormatOption
  structure: StructureOption
}>()
</script>

<template>
  <div class="flex flex-col gap-2">
    <div v-if="structure === 'meta' || structure === 'both'">
      <div class="mb-1 pb-1" v-if="structure === 'both'">Meta</div>
      {{ message.rabbitmqMetadata }}
    </div>
    <div v-if="structure === 'body' || structure === 'both'">
      <div class="mb-1 pb-1" v-if="structure === 'both'">Body</div>
      {{ message.body }}
    </div>
  </div>
</template>
