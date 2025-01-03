<script lang="ts" setup>
import { useJobStateQuery } from '@/api/jobs/jobStateQuery'
import { useSingleMessageQuery } from '@/api/messages/singleMessageQuery';
import type { MessageDeliveryDto } from '@/dtos/message/messageDeliveryDto'
import { humanDateTime } from '@/utils/dateTimeUtil'
import { highlightJson } from '@/utils/jsonUtils'
import Button from 'primevue/button'
import { computed, onBeforeUnmount, onMounted, ref } from 'vue'
import MessageBlock from './MessageBlock.vue'
import MessageDialogError from './MessageDialogError.vue'
import MessageHeader from './MessageHeader.vue'

const props = defineProps<{
  selectedMessage: MessageDeliveryDto
}>()

const emit = defineEmits<{
  (e: 'close'): void
}>()

const body = computed(() => {
  try {
    const content = JSON.parse(props.selectedMessage.message.body)
    return content
  } catch (e) {
    console.error(e)
    return undefined
  }
})
const { data: job } = useJobStateQuery(body.value['jobId'])
const { data: fetchedMessage } = useSingleMessageQuery(props.selectedMessage.transportMessageId)

const transportHeadersTrimmed = computed(() => {
  const th = { ...props.selectedMessage.transportHeaders }

  if (th['MT-Fault-StackTrace']?.length > 30) {
    th['MT-Fault-StackTrace'] = `${th['MT-Fault-StackTrace'].slice(0, 30)}...`
  }

  return th
})

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

const jobStatePopover = ref()
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
        <div class="mb-2 text-primary-500">{{ humanDateTime(selectedMessage.message.sentTime) }}</div>
        <div class="flex items-center gap-2.5 text-2xl">
          <span class="text-primary-700">{{ selectedMessage.message.messageType.replace('urn:message:', '') }}</span>
        </div>
        <div class="mt-4 flex gap-8">
          <div>
            <div class="text-primary-600">Machine</div>
            <div class="text-primary-400">{{ selectedMessage.message.host?.machineName }}</div>
          </div>
          <div>
            <div class="text-primary-600">Process Name</div>
            <div class="text-primary-400">
              {{ selectedMessage.message.host?.processName }}
            </div>
          </div>
          <div>
            <div class="text-primary-600">PID</div>
            <div class="text-primary-400">{{ selectedMessage.message.host?.processId }}</div>
          </div>
          <div>
            <div class="text-primary-600">Assembly</div>
            <div class="text-primary-400">
              {{ selectedMessage.message.host?.assembly }} ({{ selectedMessage.message.host?.assemblyVersion }})
            </div>
          </div>
          <div>
            <div class="text-primary-600">Framework</div>
            <div class="text-primary-400">{{ selectedMessage.message.host?.frameworkVersion }}</div>
          </div>
          <div>
            <div class="text-primary-600">MassTransit</div>
            <div class="text-primary-400">{{ selectedMessage.message.host?.massTransitVersion }}</div>
          </div>
          <div>
            <div class="text-primary-600">OS</div>
            <div class="text-primary-400">{{ selectedMessage.message.host?.operatingSystemVersion }}</div>
          </div>
        </div>
      </div>

      <div class="flex grow flex-col overflow-auto">
        <div class="flex shrink-0 grow basis-2/3 overflow-auto">
          <div class="flex w-[45%] flex-col overflow-auto border-e">
            <div
              class="sticky top-0 flex flex-col gap-2 bg-surface-0 px-8 py-6 shadow"
              v-if="selectedMessage.message.schedulingTokenId || job"
            >
              <div v-if="selectedMessage.message.schedulingTokenId" class="flex items-center gap-2">
                <i class="pi pi-clock"></i>Scheduled Message
              </div>
              <div v-if="job" class="">
                The message belongs to the {{ job.isRecurring ? 'recurring' : '' }} job —
                <span @click="(e) => jobStatePopover.toggle(e)" class="cursor-pointer text-blue-500 hover:text-blue-400"
                  >click for details.</span
                >
              </div>
            </div>
            <template v-if="selectedMessage.additionalData">
              <div class="flex flex-col gap-4 border-b p-8">
                <MessageHeader name="Additional Data" />
                <template v-for="(value, key) in selectedMessage.additionalData" :key="key">
                  <MessageBlock :name="key">
                    <span v-html="value"></span>
                  </MessageBlock>
                </template>
              </div>
            </template>
            <div class="flex flex-col gap-4 p-8">
              <MessageHeader name="Delivery" />
              <MessageBlock name="Message Delivery ID" :value="selectedMessage.messageDeliveryId" />
              <MessageBlock name="Transport Message ID" :value="selectedMessage.transportMessageId" />
              <MessageBlock name="Queue ID" :value="selectedMessage.queueId" />
              <MessageBlock name="Priority" :value="selectedMessage.priority" />
              <MessageBlock name="Enqueue Time">
                {{ humanDateTime(selectedMessage.enqueueTime) }}
              </MessageBlock>
              <MessageBlock name="Expiration Time" :value="selectedMessage.expirationTime" />
              <MessageBlock name="Partition Key" :value="selectedMessage.partitionKey" />
              <MessageBlock name="Routing Key" :value="selectedMessage.routingKey" />
              <MessageBlock name="Consumer ID" :value="selectedMessage.consumerId" />
              <MessageBlock name="Lock ID" :value="selectedMessage.lockId" />
              <MessageBlock name="Delivery Count" :value="selectedMessage.deliveryCount" />
              <MessageBlock name="Max. Delivery Count" :value="selectedMessage.maxDeliveryCount" />
              <MessageBlock name="Last Delivered" :value="selectedMessage.lastDelivered" />
              <MessageBlock class="flex-col gap-2" name="Transport Headers">
                <div class="whitespace-pre" v-html="highlightJson(transportHeadersTrimmed, {}, false)"></div>
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
              <MessageBlock class="flex-col gap-2" name="Headers">
                <div class="whitespace-pre" v-html="highlightJson(selectedMessage.message.headers, {}, false)"></div>
              </MessageBlock>
              <MessageBlock class="flex-col gap-2" name="Host">
                <div class="whitespace-pre" v-html="highlightJson(selectedMessage.message.host, {}, false)"></div>
              </MessageBlock>
            </div>
          </div>
          <div class="flex w-[55%] flex-col gap-4 overflow-auto">
            <div class="relative p-8">
              <Popover ref="jobStatePopover">
                <div v-if="job" class="flex flex-col gap-4 p-5">
                  <MessageHeader name="Job State" />
                  <MessageBlock name="Job ID" :value="job.jobId" />
                  <MessageBlock name="Submitted">
                    {{ humanDateTime(job.submitted) }}
                  </MessageBlock>
                  <MessageBlock name="Started" :value="job.started">
                    {{ humanDateTime(job.started) }}
                  </MessageBlock>
                  <MessageBlock name="Completed" :value="job.completed">
                    {{ humanDateTime(job.completed) }}
                  </MessageBlock>
                  <MessageBlock name="Duration" :value="job.duration" />
                  <MessageBlock name="Faulted" :value="job.faulted" />
                  <MessageBlock name="Reason" :value="job.reason" />
                  <MessageBlock name="Last Retry Attempt" :value="job.lastRetryAttempt" />
                  <MessageBlock name="Current State" :value="job.currentState" />
                  <MessageBlock name="Progress Value" :value="job.progressValue" />
                  <MessageBlock name="Progress Limit" :value="job.progressLimit" />
                  <MessageBlock name="Job State" :value="job.jobState" />
                  <MessageBlock name="Next Start Date" :value="job.nextStartDate">
                    {{ humanDateTime(job.nextStartDate) }}
                  </MessageBlock>
                  <MessageBlock name="Recurring" :value="job.isRecurring" />
                  <MessageBlock name="Start Date" :value="job.startDate" />
                  <MessageBlock name="End Date" :value="job.endDate" />
                </div>
              </Popover>
              <div class="absolute end-8 flex justify-between">
                <span class="text-primary-500">{{ selectedMessage.message.contentType }}</span>
              </div>

              <div class="grow whitespace-pre" v-html="highlightJson(JSON.parse(selectedMessage.message.body))"></div>
            </div>
          </div>
        </div>
        <MessageDialogError
          v-if="selectedMessage.transportHeaders['MT-Reason'] == 'fault'"
          :selected-message="selectedMessage"
        />
      </div>
    </div>
  </div>
</template>
