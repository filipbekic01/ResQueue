<script setup lang="ts">
import type { FormatOption } from '@/components/SelectFormat.vue'
import type { StructureOption } from '@/components/SelectStructure.vue'
import { highlightJson } from '@/utils/jsonUtils'
import { computed } from 'vue'
import FormattedMessageDivider from './FormattedMessageDivider.vue'

const props = defineProps<{
  message: any
  format: FormatOption
  structure: StructureOption
}>()

const meta = computed(() =>
  highlightJson(JSON.parse(JSON.stringify(props.message?.rabbitmqMetadata ?? {})))
)

const body = computed(() => highlightJson(props.message?.body ?? {}))
</script>

<template>
  <div class="flex flex-col gap-2">
    <div v-if="structure === 'meta' || structure === 'both'">
      <FormattedMessageDivider label="Meta" />
      <div class="whitespace-break-spaces" v-html="meta"></div>
    </div>
    <div v-if="structure === 'body' || structure === 'both'">
      <FormattedMessageDivider label="Body" />
      <div class="whitespace-break-spaces" v-html="body"></div>
    </div>
  </div>
</template>
