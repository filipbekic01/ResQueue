<script lang="ts" setup>
import { useBrokersQuery } from '@/api/broker/brokersQuery'
import { useExchangesQuery } from '@/api/exchanges/exchangesQuery'
import { useMessagesQuery } from '@/api/messages/messagesQuery'
import { usePublishMessagesMutation } from '@/api/messages/publishMessagesMutation'
import { useSyncMessagesMutation } from '@/api/messages/syncMessagesMutation'
import { useQueuesQuery } from '@/api/queues/queuesQuery'
import AppLayout from '@/layouts/AppLayout.vue'
import Breadcrumb from 'primevue/breadcrumb'
import Button from 'primevue/button'
import Column from 'primevue/column'
import { useConfirm } from 'primevue/useconfirm'
import { useToast } from 'primevue/usetoast'
import { computed, ref } from 'vue'
import { useRouter } from 'vue-router'

const props = defineProps<{
  brokerId: string
  queueId: string
}>()

const router = useRouter()

const confirm = useConfirm()
const toast = useToast()

const { mutateAsync: syncMessagesAsync } = useSyncMessagesMutation()
const { mutateAsync: publishMessagesAsync } = usePublishMessagesMutation()

const { data: messages } = useMessagesQuery(props.queueId)
const { data: brokers } = useBrokersQuery()
const { data: queues } = useQueuesQuery(props.brokerId)
const { data: exchanges } = useExchangesQuery(props.brokerId)

const broker = computed(() => brokers.value?.find((x) => x.id === props.brokerId))
const queue = computed(() => queues.value?.find((x) => x.id === props.queueId))

const rabbitMqMessages = computed(() =>
  messages.value?.map((x) => ({
    ...x,
    ...JSON.parse(x.rawData)
  }))
)

const rabbitMqExchanges = computed(() =>
  exchanges.value?.map((x) => ({
    ...x,
    ...JSON.parse(x.rawData)
  }))
)

const syncMessages = (event: any) => {
  confirm.require({
    target: event.currentTarget,
    message: 'Do you want to import new messages?',
    icon: 'pi pi-info-circle',
    rejectProps: {
      label: 'Cancel',
      severity: 'secondary',
      outlined: true
    },
    acceptProps: {
      label: 'Sync Messages',
      severity: ''
    },
    accept: () => {
      syncMessagesAsync(props.queueId).then(() => {
        toast.add({
          severity: 'info',
          summary: 'Sync Completed!',
          detail: `Messages for queue ${queue.value?.id} synced!`,
          life: 3000
        })
      })
    },
    reject: () => {}
  })
}

const openMessage = (id: string) => {
  router.push({
    name: 'message',
    params: {
      brokerId: broker.value?.id,
      queueId: queue.value?.id,
      messageId: id
    }
  })
}

const selectedMessages = ref()
const selectedExchange = ref()

const publishMessages = (event: any) => {
  confirm.require({
    target: event.currentTarget,
    message: `Do you want to publish ${selectedMessages.value.length} messages?`,
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
        messageIds: selectedMessages.value.map((msg: any) => msg.id)
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
</script>

<template>
  <AppLayout>
    <div class="flex gap-2 mx-2">
      <Button @click="(e) => syncMessages(e)">Synchronize</Button>
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
    <DataTable
      paginator
      :rows="20"
      v-model:selection="selectedMessages"
      :value="rabbitMqMessages"
      data-key="id"
    >
      <Column selectionMode="multiple" headerStyle="width: 3rem"></Column>
      <Column field="id" header="Internal ID"></Column>
      <Column field="payload_bytes" header="Payload Bytes"></Column>
      <Column field="redelivered" header="Redelivered"></Column>
      <Column field="" header="qwe">
        <template #body="{ data }">
          <Button size="small" @click="openMessage(data.id)" outlined>more details -></Button>
        </template>
      </Column>
    </DataTable>
  </AppLayout>
</template>
