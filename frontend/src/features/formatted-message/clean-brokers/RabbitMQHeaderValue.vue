<script setup lang="ts">
import type { HeaderValue } from '@/dtos/message/rabbitMQMessagePropsDto'
import { formatISO } from 'date-fns'
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

const parse = (value: string | number | boolean) => {
  const str = value.toString()

  if (str.startsWith('((time_t)')) {
    var numbers = str.match(/\d+/)
    if (numbers?.length) {
      const timestamp = parseInt(`${numbers[0]}`)
      if (!isNaN(timestamp)) {
        const date = new Date(timestamp)

        return formatISO(date)
      }
    }
  }

  return value
}
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
      <div v-for="sline in formatStackTrace(modelValue)" :key="sline" class="whitespace-nowrap">at {{ sline }}</div>
    </template>
    <template v-else>
      {{ parse(modelValue) }}
    </template>
  </div>
</template>
