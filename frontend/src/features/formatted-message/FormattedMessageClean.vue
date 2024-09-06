<script lang="ts" setup>
import type { FormatOption } from '@/components/SelectFormat.vue'
import type { StructureOption } from '@/components/SelectStructure.vue'
import { highlightJson } from '@/utils/jsonUtils'
import { computed } from 'vue'
import RabbitMQMeta from './clean-brokers/RabbitMQMeta.vue'
import FormattedMessageDivider from './FormattedMessageDivider.vue'

const props = defineProps<{
  message: any
  format: FormatOption
  structure: StructureOption
}>()

const tryParseJson = computed(() => {
  try {
    return highlightJson(JSON.parse(JSON.stringify(props.message.body)))
  } catch (e) {
    return props.message.body
  }
})
</script>

<template>
  <div class="flex flex-col gap-1 overflow-hidden">
    <!-- Amazon -->
    <!-- Azure -->
    <RabbitMQMeta v-if="message.rabbitmqMetadata" :metadata="message.rabbitmqMetadata" />
    <!-- ... -->

    <FormattedMessageDivider label="Body" />

    <div class="whitespace-break-spaces" v-html="tryParseJson"></div>
  </div>
</template>
