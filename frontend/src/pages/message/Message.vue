<script lang="ts" setup>
import { useExchangesQuery } from '@/api/exchanges/exchangesQuery'
import { useMessagesQuery } from '@/api/messages/messagesQuery'
import { usePublishMessagesMutation } from '@/api/messages/publishMessagesMutation'
import AppLayout from '@/layouts/AppLayout.vue'
import { useConfirm } from 'primevue/useconfirm'
import { useToast } from 'primevue/usetoast'
import { computed, ref } from 'vue'
import { useRouter } from 'vue-router'

const props = defineProps<{
  brokerId: string
  queueId: string
  messageId: string
}>()

const router = useRouter()

const confirm = useConfirm()
const toast = useToast()

const { mutateAsync: publishMessagesAsync } = usePublishMessagesMutation()

const { data: messages } = useMessagesQuery(props.queueId)
const { data: exchanges } = useExchangesQuery(props.brokerId)

const message = computed(() => messages.value?.find((x) => x.id === props.messageId))

const rabbitMqMessage = computed(() => {
  if (!message.value) {
    return {}
  }

  return {
    ...message.value,
    ...JSON.parse(message.value.rawData)
  }
})

const rabbitMqExchanges = computed(() =>
  exchanges.value?.map((x) => ({
    ...x,
    ...JSON.parse(x.rawData)
  }))
)

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
        :options="rabbitMqExchanges"
        optionLabel="name"
        placeholder="Select an Exchange"
        class="w-96 ms-auto"
        :virtualScrollerOptions="{ itemSize: 38, style: 'width:900px' }"
      ></Select>
      <Button @click="(e) => publishMessages(e)">Publish </Button>
    </div>
    <div
      v-for="(value, key) in rabbitMqMessage"
      :key="key"
      class="flex border-gray-200 border-b px-5 py-3"
    >
      <div class="basis-44 shrink-0">{{ key }}:</div>
      <div class="text-green-600 shrink-o">{{ value }}</div>
    </div>
  </AppLayout>
</template>
