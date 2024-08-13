<script lang="ts" setup>
import { useBrokersQuery } from '@/api/broker/brokersQuery'
import { usePublishMessagesMutation } from '@/api/messages/publishMessagesMutation'
import { useSyncMessagesMutation } from '@/api/messages/syncMessagesMutation'
import { useQueuesQuery } from '@/api/queues/queuesQuery'
import { useExchanges } from '@/composables/exchangesComposable'
import AppLayout from '@/layouts/AppLayout.vue'
import Button from 'primevue/button'
import { useConfirm } from 'primevue/useconfirm'
import { useToast } from 'primevue/usetoast'
import { computed, ref } from 'vue'
import { useRouter } from 'vue-router'
import { formatDistanceToNow } from 'date-fns'
import { useMessagesQuery } from '@/api/messages/messagesQuery'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'

const props = defineProps<{
  brokerId: string
  queueId: string
}>()

const router = useRouter()

const confirm = useConfirm()
const toast = useToast()

const { mutateAsync: syncMessagesAsync } = useSyncMessagesMutation()
const { mutateAsync: publishMessagesAsync } = usePublishMessagesMutation()

const { data: brokers } = useBrokersQuery()
const { formattedExchanges } = useExchanges(props.brokerId)
const { data: messages } = useMessagesQuery(props.queueId)

const { data: queues } = useQueuesQuery(props.brokerId)

const broker = computed(() => brokers.value?.find((x) => x.id === props.brokerId))
const queue = computed(() => queues.value?.find((x) => x.id === props.queueId))

const backToBroker = () =>
  router.push({
    name: 'broker',
    params: {
      brokerId: props.brokerId
    }
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
    <template #description>Messages</template>
    <div class="flex gap-2 mx-5 my-3 items-start">
      <Button @click="backToBroker" outlined label="Broker" icon="pi pi-arrow-left"></Button>
      <Button @click="(e) => syncMessages(e)" outlined label="Sync"></Button>
      <Select
        v-model="selectedExchange"
        :options="formattedExchanges"
        optionLabel="name"
        placeholder="Select an Exchange"
        class="w-96 ms-auto"
        :virtualScrollerOptions="{ itemSize: 38, style: 'width:900px' }"
      ></Select>
      <Button @click="(e) => publishMessages(e)" label="Publish" />
    </div>
    <DataTable
      paginator
      :rows="20"
      v-model:selection="selectedMessages"
      :value="messages"
      data-key="id"
    >
      <Column selectionMode="multiple" headerStyle="width: 3rem"></Column>
      <Column field="id" header="Internal ID">
        <template #body="{ data }">
          <span
            @click="openMessage(data.id)"
            class="border-dashed border-gray-600 border-b hover:cursor-pointer hover:border-blue-500 hover:text-blue-500"
            >{{ data.id }}</span
          >
        </template>
      </Column>
      <Column field="summary" header="Summary"> </Column>
      <Column field="createdAt" header="Created At">
        <template #body="{ data }"> {{ formatDistanceToNow(data.createdAt) }} ago </template>
      </Column>
    </DataTable>
  </AppLayout>
</template>
