<script lang="ts" setup>
import { useBrokersQuery } from '@/api/broker/brokersQuery'
import { useMessageQuery } from '@/api/messages/messageQuery'
import { usePublishMessagesMutation } from '@/api/messages/publishMessagesMutation'
import { useExchanges } from '@/composables/exchangesComposable'
import AppLayout from '@/layouts/AppLayout.vue'
import { formatDistanceToNow } from 'date-fns'
import { useConfirm } from 'primevue/useconfirm'
import { useToast } from 'primevue/usetoast'
import { computed, ref } from 'vue'
import { useRouter } from 'vue-router'
import RabbitMqMetadata from './RabbitMqMetadata.vue'
import RawMetadata from './RawMetadata.vue'

type FormatOption = 'Formatted' | 'Raw View'

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

const { mutateAsync: publishMessagesAsync } = usePublishMessagesMutation()

const selectedExchange = ref()

const formatOptions = ref<FormatOption[]>(['Formatted', 'Raw View'])
const selectedFormatOption = ref<FormatOption>('Formatted')

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
    name: 'broker',
    params: {
      brokerId: props.brokerId
    }
  })

const backToMessages = () => {
  router.push({
    name: 'messages',
    params: {
      brokerId: props.brokerId,
      queueId: props.queueId
    }
  })
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
        <span class="cursor-pointer hover:underline" @click="backToMessages">Messages</span>
        <i class="pi pi-angle-right mx-1"></i>
        {{ message?.id }}
      </div>
    </template>
    <div class="flex items-start gap-2 border-b px-4 py-2">
      <Button outlined @click="backToMessages" icon="pi pi-arrow-left" label="Messages"></Button>
      <Select
        v-model="selectedExchange"
        :options="formattedExchanges"
        optionLabel="parsed.name"
        placeholder="Select an Exchange"
        class="ms-auto w-96"
        :virtualScrollerOptions="{ itemSize: 38, style: 'width:900px' }"
      ></Select>
      <Button @click="publishMessages()" label="Requeue" iconPos="right" icon="pi pi-send"></Button>
    </div>

    <template v-if="message">
      <div class="my-3 rounded-lg">
        <div class="mx-5 mb-4 flex items-center">
          <div>
            <div class="text-2xl">Message</div>
            <div class="text-slate-500">
              Pulled {{ formatDistanceToNow(message.createdAt) }} • Updated
              {{ message.updatedAt ? formatDistanceToNow(message.updatedAt) : 'never' }}
            </div>
          </div>

          <SelectButton
            class="ms-auto"
            v-model="selectedFormatOption"
            :options="formatOptions"
            aria-labelledby="basic"
          />
        </div>

        <div class="flex flex-col overflow-auto rounded bg-gray-100/50">
          <div class="my-1 rounded-lg px-5 font-semibold">Metadata</div>
          <RawMetadata
            v-if="message.rabbitmqMetadata && selectedFormatOption === 'Raw View'"
            :metadata="message.rabbitmqMetadata"
          />
          <RabbitMqMetadata
            v-else-if="message.rabbitmqMetadata"
            :metadata="message.rabbitmqMetadata"
          />
        </div>

        <div class="rounded bg-gray-100/50 ps-5">
          <div class="my-1 rounded-lg font-semibold">Body</div>
          <div class="whitespace-break-spaces text-gray-500">{{ message.body }}</div>
        </div>
      </div>
    </template>
  </AppLayout>
</template>
