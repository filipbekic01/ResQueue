<script lang="ts" setup>
import { useBrokersQuery } from '@/api/broker/brokersQuery'
import { useMessageQuery } from '@/api/messages/messageQuery'
import { usePublishMessagesMutation } from '@/api/messages/publishMessagesMutation'
import { useQueueQuery } from '@/api/queues/queueQuery'
import type { FormatOption } from '@/components/SelectFormat.vue'
import { type StructureOption } from '@/components/SelectStructure.vue'
import { useExchanges } from '@/composables/exchangesComposable'
import { useRabbitMqQueues } from '@/composables/rabbitMqQueuesComposable'
import FormattedMessage from '@/features/formatted-message/FormattedMessage.vue'
import MessageActions from '@/features/message/MessageActions.vue'
import AppLayout from '@/layouts/AppLayout.vue'
import { formatDistanceToNow } from 'date-fns'
import { useConfirm } from 'primevue/useconfirm'
import { useToast } from 'primevue/usetoast'
import { computed, ref, watch } from 'vue'
import { useRouter } from 'vue-router'

const props = defineProps<{
  brokerId: string
  queueId: string
  messageId: string
}>()

const router = useRouter()

const confirm = useConfirm()
const toast = useToast()

const { data: brokers } = useBrokersQuery()
const broker = computed(() => brokers.value?.find((x) => x.id === props.brokerId))
const { formattedExchanges } = useExchanges(props.brokerId)
const { data: message } = useMessageQuery(computed(() => props.messageId))
const { data: queue } = useQueueQuery(computed(() => props.queueId))
const queues = computed(() => (queue.value ? [queue.value] : undefined))
const { rabbitMqQueues } = useRabbitMqQueues(queues)
const rabbitMqQueue = computed(() => rabbitMqQueues.value[0] ?? undefined)

const { mutateAsync: publishMessagesAsync } = usePublishMessagesMutation()

const selectedExchange = ref()

watch(
  () => formattedExchanges.value,
  (v) => {
    if (
      !broker.value ||
      !v ||
      selectedExchange.value ||
      !message.value?.rabbitmqMetadata?.routingKey
    ) {
      return
    }

    const name = message.value.rabbitmqMetadata.routingKey.replace(
      broker.value?.settings.deadLetterQueueSuffix ?? '',
      ''
    )

    selectedExchange.value = formattedExchanges.value.find((x) => x?.parsed.name === name)
  },
  {
    immediate: true
  }
)

const selectedMessageFormat = ref<FormatOption>('raw')
const selectedMessageStructure = ref<StructureOption>('body')

const publishMessages = () => {
  confirm.require({
    header: 'Publish Messages',
    message: `Do you want to publish this message?`,
    icon: 'pi pi-info-circle',
    rejectProps: {
      label: 'Cancel',
      severity: 'secondary',
      outlined: true
    },
    acceptProps: {
      label: 'Requeue',
      severity: ''
    },
    accept: () => {
      publishMessagesAsync({
        exchangeId: selectedExchange.value.id,
        messageIds: [props.messageId]
      }).then(() => {
        toast.add({
          severity: 'info',
          summary: 'Publish Completed!',
          detail: `Messages published to exchange _!`,
          life: 3000
        })
      })
    },
    reject: () => {}
  })
}

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

watch(
  () => broker.value,
  (broker) => {
    if (broker?.settings.messageFormat && broker?.settings.messageStructure) {
      selectedMessageFormat.value = broker.settings.messageFormat
      selectedMessageStructure.value = broker.settings.messageStructure
    }
  },
  {
    immediate: true
  }
)
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
    <div class="flex items-start gap-2 border-b px-4 py-2">
      <Button outlined @click="backToMessages" icon="pi pi-arrow-left" label="Messages"></Button>

      <MessageActions
        v-if="broker && message"
        :broker="broker"
        :selected-message-ids="[message.id]"
        :messages="[message]"
        v-model:message-format="selectedMessageFormat"
        v-model:message-structure="selectedMessageStructure"
        @archive:message="backToMessages"
      />

      <Select
        v-model="selectedExchange"
        :options="formattedExchanges"
        optionLabel="parsed.name"
        placeholder="Select an exchange"
        class="w-96"
        filter
        :virtualScrollerOptions="{ itemSize: 38, style: 'width:900px' }"
      ></Select>
      <Button @click="publishMessages()" label="Requeue" iconPos="right" icon="pi pi-send"></Button>
    </div>

    <template v-if="message">
      <div class="mb-5 rounded-lg">
        <div class="pb mx-0 mb-1 mt-3 flex items-center px-5">
          <span class="text-lg font-medium">Message</span>
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
    </template>
  </AppLayout>
</template>
