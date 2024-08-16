<script lang="ts" setup>
import { useRabbitMqMessage } from '@/composables/rabbitMqMessageComposable'
import type { MessageDto } from '@/dtos/messageDto'
import { computed } from 'vue'

const props = defineProps<{
  message: MessageDto
}>()

const { rabbitMqMessage } = useRabbitMqMessage(computed(() => props.message))
</script>

<template>
  <div class="p-4">
    <div class="text-lg">Headers</div>
    <div v-for="(value, key) in rabbitMqMessage?.parsed.properties.headers" :key="key">
      {{ key }}: {{ value }}
    </div>
    <div class="text-gray-400 mt-12 border-t border-gray-500 border-dashed pt-3">
      {{ rabbitMqMessage }}
    </div>
  </div>
</template>
