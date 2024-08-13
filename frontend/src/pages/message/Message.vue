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
import { useMessagesQuery } from '@/api/messages/messagesQuery'

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
const { data: messages } = useMessagesQuery(props.queueId)
const message = computed(() => messages.value?.find((x) => x.id === props.messageId))

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
    <div class="flex gap-2 mx-2 mt-2">
      <Button @click="backToMessages">Back to messages</Button>
      <Select
        v-model="selectedExchange"
        :options="formattedExchanges"
        optionLabel="name"
        placeholder="Select an Exchange"
        class="w-96 ms-auto"
        :virtualScrollerOptions="{ itemSize: 38, style: 'width:900px' }"
      ></Select>
      <Button @click="(e) => publishMessages(e)">Publish </Button>
    </div>
    <template v-if="message">
      <MessageRabbitMq v-if="broker?.system === BROKER_SYSTEMS.RABBIT_MQ" :message="message" />
    </template>
  </AppLayout>
</template>
