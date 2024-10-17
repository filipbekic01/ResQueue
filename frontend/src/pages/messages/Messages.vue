<script lang="ts" setup>
import { useBrokersQuery } from '@/api/brokers/brokersQuery'
import { useMoveMessagesMutation } from '@/api/messages/moveMessagesMutation'
import { usePaginatedMessagesQuery } from '@/api/messages/paginatedMessagesQuery'
import { useRequeueMessagesMutation } from '@/api/messages/requeueMessagesMutation'
import eboxUrl from '@/assets/ebox.svg'
import pgLogoUrl from '@/assets/postgres.svg'
import type { MessageDeliveryDto } from '@/dtos/message/messageDeliveryDto'
import Avatars from '@/features/avatars/Avatars.vue'
import AppLayout from '@/layouts/AppLayout.vue'
import { errorToToast } from '@/utils/errorUtils'
import { highlightJson } from '@/utils/jsonUtils'
import Button from 'primevue/button'
import Column from 'primevue/column'
import DataTable from 'primevue/datatable'
import type { PageState } from 'primevue/paginator'
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

const pageIndex = ref(0)

const { data: brokers } = useBrokersQuery()

const { data: paginatedMessages, isPending } = usePaginatedMessagesQuery(
  computed(() => props.brokerId),
  computed(() => props.queueId),
  pageIndex
)

const broker = computed(() => brokers.value?.find((x) => x.id === props.brokerId))

const backToQueues = () => {
  router.push({
    name: 'queues',
    params: {
      brokerId: props.brokerId
    }
  })
}

const changePage = (e: PageState) => {
  pageIndex.value = e.page
}

const toggleMessage = (msg?: MessageDeliveryDto) => {
  if (!msg) {
    selectedMessageId.value = 0
  } else if (selectedMessageId.value === msg.message_delivery_id) {
    selectedMessageId.value = 0
  } else {
    selectedMessageId.value = msg.message_delivery_id
  }
}

const selectedQueueType = ref('Active Messages')
const queueTypeOptions = ['Active Messages', 'Errors Messages', 'Skipped Messages']

// Selected messages
const selectedMessageId = ref<number>(24)
const selectedMessage = computed(() =>
  paginatedMessages.value?.items.find((x) => x.message_delivery_id === selectedMessageId.value)
)

const selectedMessages = ref<MessageDeliveryDto[]>([])
const selectedMessageIds = computed(() =>
  selectedMessages.value?.length ? selectedMessages.value.map((x) => x.message_delivery_id) : []
)

// Requeue messages
const { mutateAsync: requeueMessagesAsync, isPending: isRequeueMessagesPending } = useRequeueMessagesMutation()
const requeueMessages = () => {
  confirm.require({
    header: 'Requeue Messages',
    message: `Do you want to requeue all the messages?`,
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
      requeueMessagesAsync({
        messageCount: 1,
        queueName: props.queueId,
        redeliveryCoun: 10,
        sourceQueueType: 1,
        targetQueueType: 2
      })
        .then(() => {
          toast.add({
            severity: 'info',
            summary: 'Requeue Completed',
            detail: `Messages requeued to destination.`,
            life: 3000
          })
        })
        .catch((e) => toast.add(errorToToast(e)))
    },
    reject: () => {}
  })
}

// Move messages
const { mutateAsync: moveMessagesAsync, isPending: isMoveMessagesPending } = useMoveMessagesMutation()
const moveMessages = () => {
  confirm.require({
    header: 'Move Messages',
    message: `Do you want to move selected the messages?`,
    icon: 'pi pi-info-circle',
    rejectProps: {
      label: 'Cancel',
      severity: 'secondary',
      outlined: true
    },
    acceptProps: {
      label: 'Move',
      severity: ''
    },
    accept: () => {
      moveMessagesAsync({
        queueName: props.queueId,
        queueType: 1,
        messageDeliveryId: selectedMessageIds.value
      })
        .then(() => {
          toast.add({
            severity: 'info',
            summary: 'Move completed',
            detail: `Message move to destination.`,
            life: 3000
          })
        })
        .catch((e) => toast.add(errorToToast(e)))
    },
    reject: () => {}
  })
}
</script>

<template>
  <AppLayout>
    <template #prepend>
      <div
        class="flex h-[42px] w-[42px] cursor-pointer items-center justify-center rounded-xl bg-[#336791] text-2xl text-white active:scale-95"
        @click="backToQueues"
      >
        <img :src="pgLogoUrl" class="w-7 select-none" />
      </div>
    </template>
    <template #title>
      <span class="cursor-pointer hover:underline" @click="backToQueues">{{ broker?.name }}</span>
    </template>
    <template #description>Queue: {{ props.queueId }}</template>
    <template #append>
      <Avatars v-if="broker" :user-ids="broker.accessList.map((x) => x.userId)" />
    </template>
    <div class="flex flex-wrap items-start gap-2 border-b px-4 py-2">
      <Button @click="backToQueues" outlined label="Queues" icon="pi pi-arrow-left"></Button>

      <ButtonGroup>
        <Button outlined label="Requeue" icon="pi pi-sync"></Button>
        <Button outlined label="Delete" icon="pi pi-trash"></Button>
      </ButtonGroup>

      <ButtonGroup>
        <Button outlined label="Retry " icon="pi pi-arrow-top"></Button>
      </ButtonGroup>

      <Select class="ms-auto" :allow-empty="false" v-model="selectedQueueType" :options="queueTypeOptions"></Select>
      <!-- <Paginator
        class="ms-auto"
        @page="changePage"
        :rows="50"
        :always-show="false"
        :total-records="paginatedMessages?.totalCount"
      ></Paginator> -->
    </div>
    <template v-if="isPending">
      <div class="p-5"><i class="pi pi-spinner pi-spin me-2"></i>Loading messages...</div>
    </template>
    <template v-else-if="paginatedMessages?.items.length">
      <div class="flex flex-col overflow-auto">
        <div
          class="flex grow flex-col overflow-auto"
          :class="[
            {
              'basis-1/2': selectedMessageId
            }
          ]"
        >
          <DataTable
            v-model:selection="selectedMessages"
            :value="paginatedMessages?.items"
            data-key="message_delivery_id"
            scrollable
            class="max-w-full grow overflow-hidden"
            striped-rows
            scroll-height="flex"
            :lazy="true"
          >
            <Column selectionMode="multiple" class="w-0" style="vertical-align: top; text-align: center"></Column>
            <Column field="message_delivery_id" header="ID" class="w-0 whitespace-nowrap">
              <template #body="{ data }">
                <div class="flex items-center gap-2">
                  <div>#{{ data.message_delivery_id }}</div>
                  <span class="cursor-pointer text-blue-500 hover:text-blue-300" @click="toggleMessage(data)"
                    >(view)</span
                  >
                </div>
              </template>
            </Column>
            <Column field="message.body" header="Body"></Column>
            <Column field="message.sent_time" header="Sent" class="w-0 whitespace-nowrap"></Column>
          </DataTable>
        </div>
        <div v-if="selectedMessage" class="flex basis-1/2 flex-col overflow-auto border-t">
          <div class="flex items-center justify-between">
            Message #{{ selectedMessage?.message_delivery_id }}
            <Button text size="small" icon="pi pi-times" @click="toggleMessage(undefined)"></Button>
          </div>
          <div class="overflow-auto whitespace-pre">
            <div class="bg-gray-100 px-3 pb-3" v-html="highlightJson(selectedMessage)"></div>
          </div>
        </div>
      </div>
    </template>
    <template v-else>
      <div class="mt-24 flex grow flex-col items-center">
        <img :src="eboxUrl" class="w-56 pb-5 opacity-50" />
        <div class="text-lg">No Messages</div>
        <div class="">Make sure you pull the messages.</div>
      </div>
    </template>
  </AppLayout>
</template>
