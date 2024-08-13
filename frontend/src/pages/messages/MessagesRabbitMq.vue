<script lang="ts" setup>
import { useRabbitMqMessages } from '@/composables/rabbitMqMessagesComposable'
import type { RabbitMqMessageDto } from '@/dtos/rabbitMqMessageDto'
import { formatDistanceToNow } from 'date-fns'
import Column from 'primevue/column'
import { ref, watch } from 'vue'

const props = defineProps<{
  queueId: string
}>()

const emit = defineEmits<{
  (e: 'open', messageId: string): void
  (e: 'select', messageIds: string[]): void
}>()

const { messages } = useRabbitMqMessages(props.queueId)

const selectedMessages = ref<RabbitMqMessageDto[]>()

watch(
  () => selectedMessages.value,
  (x) => {
    emit('select', x?.map((x) => x.id) ?? [])
  }
)
</script>

<template></template>
