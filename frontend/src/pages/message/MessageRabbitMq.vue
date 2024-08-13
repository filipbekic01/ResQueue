<script lang="ts" setup>
import { useRabbitMqMessage } from '@/composables/rabbitMqMessageComposable'
import type { MessageDto } from '@/dtos/messageDto'

const props = defineProps<{
  message: MessageDto
}>()

const { message } = useRabbitMqMessage(props.message)
</script>

<template>
  <div class="p-4">
    <div class="text-lg">Headers</div>
    <div v-for="(value, key) in message.parsed.properties.headers" :key="key">
      {{ key }}: {{ value }}
    </div>
  </div>
</template>
