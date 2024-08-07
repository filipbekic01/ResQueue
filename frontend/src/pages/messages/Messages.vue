<script lang="ts" setup>
import { useBrokersQuery } from '@/api/broker/brokersQuery'
import { useMessagesQuery } from '@/api/messages/messagesQuery'
import { useQueuesQuery } from '@/api/queues/queuesQuery'
import { useIdentity } from '@/composables/identityComposable'
import AppLayout from '@/layouts/AppLayout.vue'
import Breadcrumb from 'primevue/breadcrumb'
import { computed } from 'vue'

const props = defineProps<{
  brokerId: string
  queueId: string
}>()

const { user } = useIdentity()
const { data: messages } = useMessagesQuery(props.queueId)
const { data: brokers } = useBrokersQuery()
const { data: queues } = useQueuesQuery(props.brokerId)

const broker = computed(() => brokers.value?.find((x) => x.id === props.brokerId))
const queue = computed(() => queues.value?.find((x) => x.id === props.queueId))

const breadcrumbs = computed(() => {
  return [
    { label: `broker ${broker.value?.name}` },
    { label: `queue ${queue.value?.id}` },
    { label: `messages` }
  ]
})
</script>

<template>
  <AppLayout>
    <Breadcrumb :model="breadcrumbs" />
    <DataTable :value="messages"> </DataTable>
  </AppLayout>
</template>
