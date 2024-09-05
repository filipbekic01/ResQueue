<script lang="ts" setup>
import type { FormatOption } from '@/components/SelectFormat.vue'
import type { StructureOption } from '@/components/SelectStructure.vue'
import FormattedMessageJson from './FormattedMessageJson.vue'
import FormattedMessageRaw from './FormattedMessageRaw.vue'

defineProps<{
  message: any
  format: FormatOption
  structure: StructureOption
}>()
</script>

<template>
  <div class="flex flex-col">
    <FormattedMessageRaw
      v-if="format === 'raw'"
      :message="message"
      :format="format"
      :structure="structure"
    />

    <FormattedMessageJson
      v-else-if="format === 'json'"
      :message="message"
      :format="format"
      :structure="structure"
    />

    <!-- <RawMetadata v-else-if="format === 'clean'" :metadata="message.rabbitmqMetadata" /> -->

    <template v-else>Invalid message format.</template>
  </div>

  <!-- <div class="rounded bg-gray-100/50 ps-5">
    <div class="my-1 rounded-lg font-semibold">Body</div>
    <div class="whitespace-break-spaces text-gray-500">{{ message.body }}</div>
  </div> -->
</template>
