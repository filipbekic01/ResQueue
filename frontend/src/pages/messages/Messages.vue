<script lang="ts" setup>
import { useBrokersQuery } from '@/api/broker/brokersQuery'
import { usePublishMessagesMutation } from '@/api/messages/publishMessagesMutation'
import { useSyncMessagesMutation } from '@/api/messages/syncMessagesMutation'
import { useExchanges } from '@/composables/exchangesComposable'
import AppLayout from '@/layouts/AppLayout.vue'
import Button from 'primevue/button'
import { useConfirm } from 'primevue/useconfirm'
import { useToast } from 'primevue/usetoast'
import { computed, ref } from 'vue'
import { useRouter } from 'vue-router'
import { formatDistanceToNow } from 'date-fns'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import Tag from 'primevue/tag'
import { useQueueQuery } from '@/api/queues/queueQuery'
import {
  useReviewMessagesMutation,
  type ReviewMessagesRequest
} from '@/api/messages/useReviewMessagesMutation'
import type { RabbitMqMessageDto } from '@/dtos/rabbitMqMessageDto'
import { useArchiveMessagesMutation } from '@/api/messages/archiveMessagesMutation'
import type { PageState } from 'primevue/paginator'
import { usePaginatedMessagesQuery } from '@/api/messages/paginatedMessagesQuery'
import { useRabbitMqQueues } from '@/composables/rabbitMqQueuesComposable'

const props = defineProps<{
  brokerId: string
  queueId: string
}>()

const router = useRouter()

const confirm = useConfirm()
const toast = useToast()

const { mutateAsync: syncMessagesAsync } = useSyncMessagesMutation()
const { mutateAsync: publishMessagesAsync } = usePublishMessagesMutation()
const { mutateAsync: reviewMessagesAsync } = useReviewMessagesMutation()
const { mutateAsync: archiveMessagesAsync } = useArchiveMessagesMutation()

const pageIndex = ref(0)

const { data: brokers } = useBrokersQuery()
const { formattedExchanges } = useExchanges(computed(() => props.brokerId))
const { data: paginatedMessages, isPending } = usePaginatedMessagesQuery(
  computed(() => props.queueId),
  pageIndex
)

const { data: queue } = useQueueQuery(computed(() => props.queueId))
const broker = computed(() => brokers.value?.find((x) => x.id === props.brokerId))

const queues = computed(() => (queue.value ? [queue.value] : undefined))
const { rabbitMqQueues } = useRabbitMqQueues(queues)
const rabbitMqQueue = computed(() => rabbitMqQueues.value[0] ?? undefined)

const backToBroker = () =>
  router.push({
    name: 'broker',
    params: {
      brokerId: props.brokerId
    }
  })

const syncMessages = () => {
  confirm.require({
    header: 'Sync Messages',
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

const selectedMessages = ref<RabbitMqMessageDto[]>([])
const selectedMessageIds = computed(() => selectedMessages.value.map((x) => x.id))
const selectedExchange = ref()

const reviewMessages = () => {
  const notReviewedIds =
    paginatedMessages.value?.items
      ?.filter((x) => selectedMessageIds.value.includes(x.id))
      .filter((x) => !x.isReviewed)
      .map((x) => x.id) ?? []

  const reviewedIds =
    paginatedMessages.value?.items
      ?.filter((x) => selectedMessageIds.value.includes(x.id))
      .filter((x) => x.isReviewed)
      .map((x) => x.id) ?? []

  let request: ReviewMessagesRequest = {
    idsToFalse: [],
    idsToTrue: []
  }

  if (reviewedIds.length && !notReviewedIds.length) {
    request.idsToFalse = reviewedIds
  } else if (request.idsToFalse.length && request.idsToTrue.length) {
    request.idsToTrue = notReviewedIds
  } else {
    request.idsToTrue = notReviewedIds
  }

  reviewMessagesAsync(request).then(() => {
    toast.add({
      severity: 'info',
      summary: 'Marked as reviewed',
      detail: `Messages successfully reviewed`,
      life: 3000
    })
  })
}

const publishMessages = () => {
  confirm.require({
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
        messageIds: selectedMessages.value.map((x) => x.id)
      }).then(() => {
        toast.add({
          severity: 'info',
          summary: 'Publish completed',
          detail: `Messages published to exchange ...`,
          life: 3000
        })
      })
    },
    reject: () => {}
  })
}

const reviewMessagesLabel = computed(() => {
  let message = 'Mark as Reviewed'

  if (
    selectedMessageIds.value?.length >= 1 &&
    paginatedMessages.value?.items
      ?.filter((x) => selectedMessageIds.value.includes(x.id))
      .every((x) => x.isReviewed)
  ) {
    message = 'Mark as Unreviewed'
  }

  return message
})

const archiveMessages = () => {
  confirm.require({
    header: 'Archive Messages',
    message: `Do you want to archive ${selectedMessages.value.length} messages?`,
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
      archiveMessagesAsync(selectedMessageIds.value).then(() => {
        toast.add({
          severity: 'info',
          summary: 'Archived messages',
          detail: `Messages successfully reviewed!`,
          life: 3000
        })
      })
    },
    reject: () => {}
  })
}

const changePage = (e: PageState) => {
  pageIndex.value = e.page
}

const syncLabel = computed(() => {
  return `Pull (${rabbitMqQueue.value?.parsed.messages})`
})
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
    <template #description>{{ rabbitMqQueue.parsed.name }}</template>
    <div class="flex gap-2 px-4 py-2 items-start border-b">
      <Button @click="backToBroker" outlined label="Broker" icon="pi pi-arrow-left"></Button>
      <Button
        @click="() => syncMessages()"
        outlined
        :label="syncLabel"
        icon="pi pi-download"
      ></Button>
      <Button outlined label="Refresh Table" icon="pi pi-refresh"></Button>
      <Button
        @click="() => reviewMessages()"
        outlined
        :disabled="!selectedMessages.length"
        :label="reviewMessagesLabel"
        icon="pi pi-check"
      ></Button>
      <Button
        @click="() => archiveMessages()"
        outlined
        :disabled="!selectedMessages.length"
        severity="danger"
        icon="pi pi-trash"
      ></Button>

      <Select
        v-model="selectedExchange"
        :options="formattedExchanges"
        optionLabel="name"
        placeholder="Select an Exchange"
        class="w-96 ms-auto"
        :virtualScrollerOptions="{ itemSize: 38, style: 'width:900px' }"
      ></Select>
      <Button
        @click="publishMessages"
        :disabled="!selectedMessageIds.length"
        label="Requeue"
        icon="pi pi-send"
        icon-pos="right"
      />
    </div>
    <template v-if="isPending">
      <div class="p-5"><i class="pi pi-spinner pi-spin me-2"></i>Loading queues...</div>
    </template>
    <template v-else-if="paginatedMessages?.items.length">
      <DataTable
        v-model:selection="selectedMessages"
        :value="paginatedMessages?.items"
        data-key="id"
      >
        <Column selectionMode="multiple" headerStyle="width: 3rem"></Column>

        <Column field="id" header="Message" class="w-[0%]">
          <template #body="{ data }">
            <div class="flex">
              <span
                @click="openMessage(data.id)"
                class="border-dashed border-gray-600 border-b hover:cursor-pointer hover:border-blue-500 hover:text-blue-500"
                >{{ data.id }}</span
              >
            </div>
          </template>
        </Column>
        <Column field="flags" class="w-[0%]">
          <template #body="{ data }">
            <div class="flex gap-2">
              <Tag v-if="data.isReviewed" icon="pi pi-check"></Tag>
            </div>
          </template>
        </Column>
        <Column field="summary" header="Summary">
          <template #body="{ data }">
            {{ data.summary ?? 'No summary available for this message.' }}
          </template>
        </Column>

        <Column field="updatedAt" header="Updated" class="w-[0%]">
          <template #body="{ data }"
            ><div class="whitespace-nowrap text-gray-500">
              {{ data.updatedAt ? `${formatDistanceToNow(data.updatedAt)}` : 'never' }}
            </div></template
          >
        </Column>
        <Column field="createdAt" header="Created" class="w-[0%]">
          <template #body="{ data }"
            ><div class="whitespace-nowrap">
              {{ formatDistanceToNow(data.createdAt) }} ago
            </div></template
          >
        </Column>
      </DataTable>
      <Paginator
        @page="changePage"
        :rows="50"
        :always-show="false"
        :total-records="paginatedMessages?.totalCount"
      ></Paginator>
    </template>
    <template v-else>
      <div class="flex items-center flex-col mt-24 grow">
        <img src="/ebox.svg" class="w-56 opacity-50 pb-5" />
        <div class="text-lg">No Messages</div>
        <div class="">Make sure you pull the messages.</div>
      </div>
    </template>
  </AppLayout>
</template>
