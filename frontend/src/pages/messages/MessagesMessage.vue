<script lang="ts" setup>
import type { MessageDeliveryDto } from '@/dtos/message/messageDeliveryDto'
import { highlightJson } from '@/utils/jsonUtils'
import Button from 'primevue/button'
import { onBeforeUnmount, onMounted } from 'vue'
import MessageBlock from './MessageBlock.vue'
import MessageHeader from './MessageHeader.vue'

defineProps<{
  selectedMessage: MessageDeliveryDto
}>()

const emit = defineEmits<{
  (e: 'close'): void
}>()

const handleEscKey = (event: KeyboardEvent) => {
  if (event.key === 'Escape') {
    emit('close')
  }
}

onMounted(() => {
  window.addEventListener('keydown', handleEscKey)
})

onBeforeUnmount(() => {
  window.removeEventListener('keydown', handleEscKey)
})
</script>
<template>
  <div class="click absolute start-0 top-0 z-40 h-full w-full backdrop-contrast-75" @click="emit('close')"></div>
  <div
    class="absolute bottom-0 end-0 start-0 z-50 mx-auto flex h-[70%] w-full flex-col overflow-auto bg-surface-0 shadow-2xl dark:bg-surface-900"
  >
    <div class="flex h-[100%] flex-col overflow-hidden">
      <div
        class="flex items-center justify-between border-y bg-surface-100 px-3 py-2 dark:border-y-surface-700 dark:bg-surface-950"
      >
        Message {{ selectedMessage?.message_delivery_id }}
        <Button text size="small" icon="pi pi-times" @click="emit('close')"></Button>
      </div>

      <div class="flex grow flex-col overflow-auto">
        <div class="flex shrink-0 grow basis-1/2 overflow-auto">
          <div class="flex w-[45%] flex-col overflow-auto">
            <MessageHeader name="Delivery Details" />
            <MessageBlock name="message_delivery_id" :value="selectedMessage.message_delivery_id" />
            <MessageBlock name="transport_message_id" :value="selectedMessage.transport_message_id" />
            <MessageBlock name="queue_id" :value="selectedMessage.queue_id" />
            <MessageBlock name="priority" :value="selectedMessage.priority" />
            <MessageBlock name="enqueue_time" :value="selectedMessage.enqueue_time" />
            <MessageBlock name="expiration_time" :value="selectedMessage.expiration_time" />
            <MessageBlock name="partition_key" :value="selectedMessage.partition_key" />
            <MessageBlock name="routing_key" :value="selectedMessage.routing_key" />
            <MessageBlock name="consumer_id" :value="selectedMessage.consumer_id" />
            <MessageBlock name="lock_id" :value="selectedMessage.lock_id" />
            <MessageBlock name="delivery_count" :value="selectedMessage.delivery_count" />
            <MessageBlock name="max_delivery_count" :value="selectedMessage.max_delivery_count" />
            <MessageBlock name="last_delivered" :value="selectedMessage.last_delivered" />
            <MessageBlock name="transport_headers" :value="selectedMessage.transport_headers">
              <div class="overflow-auto whitespace-pre" v-html="highlightJson(selectedMessage.transport_headers)"></div>
            </MessageBlock>

            <MessageHeader name="Message Content" class="border-t" />

            <MessageBlock name="transport_message_id" :value="selectedMessage.message.transport_message_id" />
            <MessageBlock name="content_type" :value="selectedMessage.message.content_type" />
            <MessageBlock name="message_type" :value="selectedMessage.message.message_type" />
            <MessageBlock name="message_id" :value="selectedMessage.message.message_id" />
            <MessageBlock name="correlation_id" :value="selectedMessage.message.correlation_id" />
            <MessageBlock name="conversation_id" :value="selectedMessage.message.conversation_id" />
            <MessageBlock name="request_id" :value="selectedMessage.message.request_id" />
            <MessageBlock name="initiator_id" :value="selectedMessage.message.initiator_id" />
            <MessageBlock name="scheduling_token_id" :value="selectedMessage.message.scheduling_token_id" />
            <MessageBlock name="source_address" :value="selectedMessage.message.source_address" />
            <MessageBlock name="destination_address" :value="selectedMessage.message.destination_address" />
            <MessageBlock name="response_address" :value="selectedMessage.message.response_address" />
            <MessageBlock name="fault_address" :value="selectedMessage.message.fault_address" />
            <MessageBlock name="sent_time" :value="selectedMessage.message.sent_time" />
            <MessageBlock name="headers" :value="selectedMessage.message.headers" />
            <MessageBlock name="host">
              <div class="whitespace-pre" v-html="highlightJson(selectedMessage.message.host)"></div>
            </MessageBlock>
          </div>
          <div class="flex w-[55%] flex-col overflow-auto">
            <MessageHeader name="Error Details" />
            <div
              v-if="selectedMessage.transport_headers['MT-Fault-StackTrace']"
              class="basis-1/2 overflow-auto bg-red-100"
            >
              <div class="whitespace-pre px-3 pt-3 text-red-700">
                {{ selectedMessage.transport_headers['MT-Fault-Message'] }}
              </div>
              <div class="whitespace-pre p-3 text-red-900">
                {{ selectedMessage.transport_headers['MT-Fault-StackTrace'] }}
              </div>
            </div>
            <MessageHeader name="Body" />

            <div
              class="grow overflow-auto whitespace-pre border-t p-3"
              v-html="highlightJson(JSON.parse(selectedMessage.message.body))"
            ></div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>
