<script lang="ts" setup>
import type { FormatOption } from '@/components/SelectFormat.vue'
import type { StructureOption } from '@/components/SelectStructure.vue'
import type { MessageDto } from '@/dtos/message/messageDto'
import FormattedMessageDivider from './FormattedMessageDivider.vue'

defineProps<{
  message: MessageDto
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
