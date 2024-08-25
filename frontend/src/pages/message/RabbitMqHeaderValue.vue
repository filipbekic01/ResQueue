<script setup lang="ts">
import type { HeaderValue } from '@/dtos/rabbitmqMessagePropertiesDto'

defineProps<{ modelValue: HeaderValue }>()
</script>
<template>
  <div v-if="Array.isArray(modelValue)" class="flex flex-col">
    <RabbitMqHeaderValue
      v-for="(nested, i) of modelValue"
      :key="i"
      :model-value="nested"
      :class="Array.isArray(nested) ? 'ps-3' : ''"
    />
  </div>
  <div v-else>
    {{ modelValue }}
  </div>
</template>
