<script lang="ts" setup>
import { useBrokersQuery } from '@/api/broker/brokersQuery'
import { useMessagesQuery } from '@/api/messages/messagesQuery'
import { useSyncMessagesMutation } from '@/api/messages/syncMessagesMutation'
import { useQueuesQuery } from '@/api/queues/queuesQuery'
import AppLayout from '@/layouts/AppLayout.vue'
import Breadcrumb from 'primevue/breadcrumb'
import Column from 'primevue/column'
import { useConfirm } from 'primevue/useconfirm'
import { useToast } from 'primevue/usetoast'
import { computed } from 'vue'

const props = defineProps<{
  brokerId: string
  queueId: string
}>()

const confirm = useConfirm()
const toast = useToast()

const { mutateAsync: syncMessagesAsync } = useSyncMessagesMutation()

const { data: messages } = useMessagesQuery(props.queueId)
const { data: brokers } = useBrokersQuery()
const { data: queues } = useQueuesQuery(props.brokerId)

const broker = computed(() => brokers.value?.find((x) => x.id === props.brokerId))
const queue = computed(() => queues.value?.find((x) => x.id === props.queueId))

const rabbitMqMessages = computed(() =>
  messages.value?.map((x) => ({
    _id: x.id,
    ...JSON.parse(x.rawData)
  }))
)

const breadcrumbs = computed(() => {
  return [
    { label: `broker ${broker.value?.name}` },
    { label: `queue ${queue.value?.id}` },
    { label: `messages` }
  ]
})

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
</script>

<template>
  <AppLayout>
    <Breadcrumb :model="breadcrumbs" />
    <div>
      <Button @click="(e) => syncMessages(e)">Sync messages</Button>
    </div>
    <DataTable :value="rabbitMqMessages">
      <Column field="payload_bytes" header="Payload Bytes"></Column>
      <Column field="redelivered" header="Redelivered"></Column>
    </DataTable>
  </AppLayout>
</template>
