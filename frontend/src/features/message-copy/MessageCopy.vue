<script lang="ts" setup>
import type { BrokerDto } from '@/dtos/broker/brokerDto'
import type { MessageDto } from '@/dtos/message/messageDto'
import type { QueueDto } from '@/dtos/queues/queueDto'
import { useRouter } from 'vue-router'

const props = defineProps<{
  message: MessageDto
  broker: BrokerDto
  queue: QueueDto
}>()

const emit = defineEmits<{
  (e: 'copied'): void
}>()

const router = useRouter()

const popoverCopyToClipboard = (text: string) => {
  navigator.clipboard.writeText(text)

  emit('copied')
}

const popoverCopyLinkToClipboard = (id: string) => {
  const link = router.resolve({
    name: 'message',
    params: {
      brokerId: props.broker.id,
      queueId: props.queue.id,
      messageId: id
    }
  })

  popoverCopyToClipboard(`${window.location.origin}${link.href}`)

  emit('copied')
}
</script>

<template>
  <div class="mb-2">Copy any part of message to clipboard.</div>
  <div class="flex flex-row items-center gap-2">
    <Button
      @click="popoverCopyToClipboard(JSON.stringify(message))"
      outlined
      size="small"
      label="Both"
    ></Button>
    <Button
      @click="popoverCopyToClipboard(JSON.stringify(message.body))"
      outlined
      size="small"
      label="Body"
    ></Button>
    <Button
      @click="popoverCopyToClipboard(JSON.stringify(message.rabbitmqMetadata))"
      outlined
      size="small"
      label="Meta"
    ></Button>
    <Button @click="popoverCopyToClipboard(message.id)" outlined size="small" label="ID"></Button>
  </div>
  <div
    class="mt-2 cursor-pointer text-blue-500 hover:text-blue-400"
    @click="popoverCopyLinkToClipboard(message.id)"
  >
    <i :class="`pi pi-link`"></i> Copy link to the message
  </div>
</template>
