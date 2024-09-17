<script lang="ts" setup>
import { useBrokersQuery } from '@/api/broker/brokersQuery'
import { useMessageQuery } from '@/api/messages/messageQuery'
import { useQueueQuery } from '@/api/queues/queueQuery'
import type { FormatOption } from '@/components/SelectFormat.vue'
import { type StructureOption } from '@/components/SelectStructure.vue'
import { useRabbitMqQueues } from '@/composables/rabbitMqQueuesComposable'
import Avatars from '@/features/avatars/Avatars.vue'
import FormattedMessage from '@/features/formatted-message/FormattedMessage.vue'
import MessageCopy from '@/features/message-copy/MessageCopy.vue'
import MessageActions from '@/features/message/MessageActions.vue'
import AppLayout from '@/layouts/AppLayout.vue'
import { formatDistanceToNow } from 'date-fns'
import Button from 'primevue/button'
import type { PopoverMethods } from 'primevue/popover'
import { computed, ref } from 'vue'
import { useRouter } from 'vue-router'

const props = defineProps<{
  brokerId: string
  queueId: string
  messageId: string
}>()

const router = useRouter()

const { data: brokers } = useBrokersQuery()
const broker = computed(() => brokers.value?.find((x) => x.id === props.brokerId))
const { data: message } = useMessageQuery(computed(() => props.messageId))
const { data: queue } = useQueueQuery(computed(() => props.queueId))
const queues = computed(() => (queue.value ? [queue.value] : undefined))
const { rabbitMqQueues } = useRabbitMqQueues(queues)
const rabbitMqQueue = computed(() => rabbitMqQueues.value[0] ?? undefined)

const selectedMessageFormat = ref<FormatOption>('raw')
const selectedMessageStructure = ref<StructureOption>('body')

const backToBroker = () =>
  router.push({
    name: 'queues',
    params: {
      brokerId: props.brokerId
    }
  })

const backToMessages = () =>
  router.push({
    name: 'messages',
    params: {
      brokerId: props.brokerId,
      queueId: props.queueId
    }
  })

const popover = ref<PopoverMethods>()

const togglePopover = (e: Event) => {
  popover.value?.show(e)
}

const handleCopied = () => {
  popover.value?.hide()
}
</script>

<template>
  <AppLayout>
    <template #prepend>
      <div
        class="flex h-[42px] w-[42px] cursor-pointer items-center justify-center rounded-xl bg-[#FF6600] text-2xl text-white active:scale-95"
        @click="backToBroker"
      >
        <img src="/rmq.svg" class="w-7 select-none" />
      </div>
    </template>
    <template #title
      ><span class="cursor-pointer hover:underline" @click="backToBroker">{{
        broker?.name
      }}</span></template
    >
    <template #description>
      <div class="flex items-center">
        <span class="cursor-pointer hover:underline" @click="backToMessages">{{
          rabbitMqQueue?.parsed.name
        }}</span>
        <i class="pi pi-angle-right mx-1"></i>
        {{ message?.id }}
      </div>
    </template>
    <template #append>
      <Avatars v-if="broker" :user-ids="broker.accessList.map((x) => x.userId)" />
    </template>
    <div class="flex items-start gap-2 border-b px-4 py-2">
      <Button outlined @click="backToMessages" icon="pi pi-arrow-left" label="Messages"></Button>

      <MessageActions
        v-if="broker && message && rabbitMqQueue"
        :broker="broker"
        :rabbit-mq-queue="rabbitMqQueue"
        :selected-message-ids="[message.id]"
        :messages="[message]"
        v-model:message-format="selectedMessageFormat"
        v-model:message-structure="selectedMessageStructure"
        @archive:message="backToMessages"
      />
    </div>

    <div v-if="message" class="mb-5 overflow-auto rounded-lg">
      <div class="pb mx-0 mb-1 mt-3 flex items-center px-5">
        <span class="text-lg font-medium">Message</span>
        <div
          class="ms-2 flex cursor-pointer items-center gap-1 hover:text-blue-500"
          @click="togglePopover"
        >
          <i class="pi pi-copy"></i>copy
        </div>
        <Popover ref="popover">
          <MessageCopy
            v-if="broker && queue"
            :broker="broker"
            :queue="queue"
            :message="message"
            @copied="handleCopied"
          />
        </Popover>

        <div class="ms-auto flex gap-3">
          <span class="text-slate-500"
            >updated
            {{ message.updatedAt ? formatDistanceToNow(message.updatedAt) : 'never' }} ago</span
          >created {{ formatDistanceToNow(message.createdAt) }} ago
        </div>
      </div>

      <div class="px-5">
        <FormattedMessage
          :message="message"
          :format="selectedMessageFormat"
          :structure="selectedMessageStructure"
        />
      </div>
    </div>
  </AppLayout>
</template>
