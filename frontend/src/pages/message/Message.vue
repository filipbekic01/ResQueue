<script lang="ts" setup>
import { usePublishMessagesMutation } from '@/api/messages/publishMessagesMutation'
import { useExchanges } from '@/composables/exchangesComposable'
import AppLayout from '@/layouts/AppLayout.vue'
import { useConfirm } from 'primevue/useconfirm'
import { useToast } from 'primevue/usetoast'
import { computed, ref } from 'vue'
import { useRouter } from 'vue-router'
import MessageRabbitMq from './MessageRabbitMq.vue'
import { useBrokersQuery } from '@/api/broker/brokersQuery'
import { BROKER_SYSTEMS } from '@/constants/brokerSystems'
import { useMessageQuery } from '@/api/messages/messageQuery'

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

const publishMessages = (event: any) => {
  confirm.require({
    target: event.currentTarget,
    message: `Do you want to publish this message?`,
    icon: 'pi pi-info-circle',
    rejectProps: {
      label: 'Cancel',
      severity: 'secondary',
      outlined: true
    },
    acceptProps: {
      label: 'Publish',
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

const isMasstransitFramework = computed(() => message.value?.rawData.includes('MT-Host'))
</script>

<template>
  <AppLayout>
    <template #prepend>
      <div
        class="w-[42px] h-[42px] rounded-xl bg-[#FF6600] items-center justify-center flex text-2xl text-white cursor-pointer active:scale-95"
        @click="backToBroker"
      >
        <img src="/rmq.svg" class="w-7 select-none" />
      </div>
    </template>
    <template #title
      ><span class="hover:underline cursor-pointer" @click="backToBroker">{{
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

    <div class="flex gap-2 px-4 py-2 items-start border-b">
      <Button outlined @click="backToMessages" icon="pi pi-arrow-left" label="Messages"></Button>
      <Select
        v-model="selectedExchange"
        :options="formattedExchanges"
        optionLabel="name"
        placeholder="Select an Exchange"
        class="w-96 ms-auto"
        :virtualScrollerOptions="{ itemSize: 38, style: 'width:900px' }"
      ></Select>
      <Button
        @click="(e) => publishMessages(e)"
        label="Publish"
        iconPos="right"
        icon="pi pi-send"
      ></Button>
    </div>
    <template v-if="message">
      <MessageRabbitMq v-if="broker?.system === BROKER_SYSTEMS.RABBIT_MQ" :message="message" />
    </template>
  </AppLayout>
</template>
