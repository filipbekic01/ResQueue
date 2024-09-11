<script setup lang="ts">
import type { HeaderValue } from '@/dtos/rabbitMQMessagePropsDto'
import RabbitMQHeaderValue from './RabbitMQHeaderValue.vue'

defineProps<{
  headerKey?: string
  modelValue: string | number | boolean | HeaderValue[]
}>()

const formatStackTrace = (trace: string | number | boolean) =>
  trace
    .toString()
    .split(/\bat\b/g)
    .map((x) => x.trim()) // Remove start and end spacing
    .filter((x) => x) // Remove empty strings
</script>

<template>
  <div v-if="Array.isArray(modelValue)" class="flex flex-col">
    <RabbitMQHeaderValue
      v-for="(nested, i) of modelValue"
      :key="i"
      :model-value="nested"
      :class="Array.isArray(nested) ? 'ps-3' : ''"
    />
  </div>
  <div v-else class="overflow-auto">
    <template v-if="headerKey === 'MT-Fault-StackTrace'">
      <div v-for="sline in formatStackTrace(modelValue)" :key="sline" class="whitespace-nowrap">
        at {{ sline }}
      </div>
    </template>
    <template v-else>
      {{ modelValue }}
    </template>
  </div>
</template>
