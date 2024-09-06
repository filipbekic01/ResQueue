<script lang="ts" setup>
import type { FormatOption } from '@/components/SelectFormat.vue'
import type { StructureOption } from '@/components/SelectStructure.vue'
import FormattedMessageDivider from './FormattedMessageDivider.vue'

defineProps<{
  message: any
  format: FormatOption
  structure: StructureOption
}>()
</script>

<template>
  <div class="flex flex-col gap-2">
    <div v-if="structure === 'meta' || structure === 'both'">
      <FormattedMessageDivider label="Meta" />
      {{ message.rabbitmqMetadata }}
    </div>
    <div v-if="structure === 'body' || structure === 'both'">
      <FormattedMessageDivider label="Body" />
      {{ message.body }}
    </div>
  </div>
</template>
