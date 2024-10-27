<script lang="ts" setup>
import type { MessageDeliveryDto } from '@/dtos/message/messageDeliveryDto'
import { highlightJson } from '@/utils/jsonUtils'
import { format, formatDistance } from 'date-fns'
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
  <div
    class="click absolute start-0 top-0 z-40 h-full w-full animate-fadein backdrop-brightness-50 animate-duration-75"
    @click="emit('close')"
  ></div>
  <div
    class="absolute bottom-0 end-0 z-50 mx-auto flex h-[100%] w-[90%] flex-col overflow-auto rounded-s-xl bg-surface-0 shadow-2xl dark:bg-surface-900"
  >
    <div class="flex h-[100%] flex-col overflow-hidden">
      <div class="absolute end-0 top-0 p-6">
        <Button icon="pi pi-times" severity="secondary" @click="emit('close')"></Button>
      </div>
      <div class="border-b px-8 pb-6 pt-8">
        <div class="mb-2 text-primary-500">
          {{ format(selectedMessage.enqueueTime, 'MMM dd HH:mm:ss') }} (enqueued
          {{ formatDistance(selectedMessage.enqueueTime, new Date()) }}
          ago)
        </div>
        <div class="flex items-center gap-3 text-2xl">
          <span class="text-primary-500">URN</span>
          <span class="text-primary-700">{{ selectedMessage.message.messageType.replace('urn:message:', '') }}</span>
        </div>
        <!-- <div class="mt-4" v-if="selectedMessage.transport_headers['MT-Fault-ExceptionType']">
          <Tag severity="danger">{{ selectedMessage.transport_headers['MT-Fault-ExceptionType'] }}</Tag>
        </div> -->
      </div>

      <div class="flex grow flex-col overflow-auto">
        <div class="flex shrink-0 grow basis-2/3 overflow-auto">
          <div class="flex w-[45%] flex-col overflow-auto border-e">
            <div class="flex flex-col gap-4 p-8">
              <MessageHeader name="Delivery" />
              <MessageBlock name="Message Delivery ID" :value="selectedMessage.messageDeliveryId" />
              <MessageBlock name="Transport Message ID" :value="selectedMessage.transportMessageId" />
              <MessageBlock name="Queue ID" :value="selectedMessage.queueId" />
              <MessageBlock name="Priority" :value="selectedMessage.priority" />
              <MessageBlock name="Enqueue Time" :value="selectedMessage.enqueueTime" />
              <MessageBlock name="Expiration Time" :value="selectedMessage.expirationTime" />
              <MessageBlock name="Partition Key" :value="selectedMessage.partitionKey" />
              <MessageBlock name="Routing Key" :value="selectedMessage.routingKey" />
              <MessageBlock name="Consumer ID" :value="selectedMessage.consumerId" />
              <MessageBlock name="Lock ID" :value="selectedMessage.lockId" />
              <MessageBlock name="Delivery Count" :value="selectedMessage.deliveryCount" />
              <MessageBlock name="Max. Delivery Count" :value="selectedMessage.maxDeliveryCount" />
              <MessageBlock name="Last Delivered" :value="selectedMessage.lastDelivered" />
              <MessageBlock name="Transport Headers" :value="selectedMessage.transportHeaders">
                <div v-html="highlightJson(selectedMessage.transportHeaders, {}, false)"></div>
              </MessageBlock>
            </div>
            <div class="flex flex-col gap-4 border-t p-8">
              <MessageHeader name="Message" />
              <MessageBlock name="Transport Message ID" :value="selectedMessage.message.transportMessageId" />
              <MessageBlock name="Content Type" :value="selectedMessage.message.contentType" />
              <MessageBlock name="Message Type" :value="selectedMessage.message.messageType" />
              <MessageBlock name="Message ID" :value="selectedMessage.message.messageId" />
              <MessageBlock name="Correlation ID" :value="selectedMessage.message.correlationId" />
              <MessageBlock name="Conversation ID" :value="selectedMessage.message.conversationId" />
              <MessageBlock name="Request ID" :value="selectedMessage.message.requestId" />
              <MessageBlock name="Initiator ID" :value="selectedMessage.message.initiatorId" />
              <MessageBlock name="Scheduling Token ID" :value="selectedMessage.message.schedulingTokenId" />
              <MessageBlock name="Source Address" :value="selectedMessage.message.sourceAddress" />
              <MessageBlock name="Destination Address" :value="selectedMessage.message.destinationAddress" />
              <MessageBlock name="Response Address" :value="selectedMessage.message.responseAddress" />
              <MessageBlock name="Fault Address" :value="selectedMessage.message.faultAddress" />
              <MessageBlock name="Sent Time" :value="selectedMessage.message.sentTime" />
              <MessageBlock name="Headers">
                <div v-html="highlightJson(selectedMessage.message.headers, {}, false)"></div>
              </MessageBlock>
              <MessageBlock name="Host">
                <div v-html="highlightJson(selectedMessage.message.host, {}, false)"></div>
              </MessageBlock>
            </div>
          </div>
          <div class="flex w-[55%] flex-col gap-4 overflow-auto">
            <div class="relative p-8">
              <div class="absolute end-8 flex justify-between">
                <span class="text-primary-500">{{ selectedMessage.message.contentType }}</span>
              </div>

              <div class="grow whitespace-pre" v-html="highlightJson(JSON.parse(selectedMessage.message.body))"></div>
            </div>
          </div>
        </div>
        <div
          v-if="selectedMessage.transportHeaders['MT-Reason'] == 'fault'"
          class="flex basis-1/3 flex-col gap-2 overflow-auto border-s-4 border-t border-s-red-400 p-6"
        >
          <div class="flex items-center gap-2">
            <!-- <Tag severity="danger">{{ selectedMessage.transportHeaders['MT-Fault-ExceptionType'] }}</Tag> -->
            <div class="items-cener flex gap-3">
              <i class="pi pi-circle-fill text-red-400"></i
              >{{ selectedMessage.transportHeaders['MT-Fault-ExceptionType'] }}
            </div>
            <span>•</span>
            <div class="text-primary-500">
              {{ format(selectedMessage.transportHeaders['MT-Fault-Timestamp'], 'MMM dd HH:mm:ss') }} (failed
              {{ formatDistance(selectedMessage.transportHeaders['MT-Fault-Timestamp'], new Date()) }}
              ago)
            </div>
          </div>

          <div class="font-mono text-red-700">
            {{ selectedMessage.transportHeaders['MT-Fault-Message'] }}
          </div>

          <div class="whitespace-pre px-4 font-mono text-red-900">
            {{
              selectedMessage.transportHeaders['MT-Fault-StackTrace']
                ? selectedMessage.transportHeaders['MT-Fault-StackTrace']
                : 'Stack trace missing.'
            }}<br />
            feqw<br />
            feqw<br />
            feqw<br />
            feqw<br />
            feqw<br />
            feqw<br />
            feqw<br />
            feqw<br />
            feqw11111<br />
            feqw<br />
            feqw<br />
            feqw<br />
            feqw<br />
            feqw<br />
            feqw<br />
            feqw<br />
            vfewqfewq
          </div>
        </div>
      </div>
    </div>
  </div>
</template>
