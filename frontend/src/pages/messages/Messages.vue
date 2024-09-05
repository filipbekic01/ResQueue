<script lang="ts" setup>
import { useBrokersQuery } from '@/api/broker/brokersQuery'
import { useSyncBrokerMutation } from '@/api/broker/syncBrokerMutation'
import { useArchiveMessagesMutation } from '@/api/messages/archiveMessagesMutation'
import { usePaginatedMessagesQuery } from '@/api/messages/paginatedMessagesQuery'
import { usePublishMessagesMutation } from '@/api/messages/publishMessagesMutation'
import { useSyncMessagesMutation } from '@/api/messages/syncMessagesMutation'
import {
  useReviewMessagesMutation,
  type ReviewMessagesRequest
} from '@/api/messages/useReviewMessagesMutation'
import { useQueueQuery } from '@/api/queues/queueQuery'
import { useExchanges } from '@/composables/exchangesComposable'
import { useIdentity } from '@/composables/identityComposable'
import { useRabbitMqQueues } from '@/composables/rabbitMqQueuesComposable'
import type { RabbitMqMessageDto } from '@/dtos/rabbitMqMessageDto'
import AppLayout from '@/layouts/AppLayout.vue'
import { formatDistanceToNow } from 'date-fns'
import Button from 'primevue/button'
import ButtonGroup from 'primevue/buttongroup'
import Column from 'primevue/column'
import DataTable from 'primevue/datatable'
import type { PageState } from 'primevue/paginator'
import SelectButton from 'primevue/selectbutton'
import Tag from 'primevue/tag'
import { useConfirm } from 'primevue/useconfirm'
import { useToast } from 'primevue/usetoast'
import { computed, ref, watch } from 'vue'
import { useRouter } from 'vue-router'

const props = defineProps<{
  brokerId: string
  queueId: string
}>()

const router = useRouter()

const confirm = useConfirm()
const toast = useToast()

const {
  query: { data: user }
} = useIdentity()

const { mutateAsync: syncMessagesAsync, isPending: isSyncMessagesPending } =
  useSyncMessagesMutation()

const { mutateAsync: publishMessagesAsync, isPending: isPublishMessagesPending } =
  usePublishMessagesMutation()

const { mutateAsync: reviewMessagesAsync, isPending: isReviewMessagesPending } =
  useReviewMessagesMutation()

const { mutateAsync: archiveMessagesAsync, isPending: isArchiveMessagesPending } =
  useArchiveMessagesMutation()

const { mutateAsync: syncBrokerAsync, isPending: isSyncBrokerPending } = useSyncBrokerMutation()

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

const backToBroker = () => {
  router.back()
}

const syncMessages = () => {
  if (!user.value?.settings.showSyncConfirmDialogs) {
    syncMessagesRequest()
    return
  }

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
    accept: () => syncMessagesRequest(),
    reject: () => {}
  })
}

const syncMessagesRequest = () => {
  syncMessagesAsync({
    brokerId: props.brokerId,
    queueId: props.queueId
  }).then(() => {
    toast.add({
      severity: 'success',
      summary: 'Sync Completed!',
      detail: `Messages for queue ${queue.value?.id} synced!`,
      life: 3000
    })
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
watch(
  () => formattedExchanges.value,
  (v) => {
    if (!broker.value || !v || selectedExchange.value) {
      return
    }

    selectedExchange.value = formattedExchanges.value.find(
      (x) =>
        x.parsed.name ==
        rabbitMqQueue.value.parsed.name.replace(
          broker.value?.settings.deadLetterQueueSuffix ?? '',
          ''
        ),
      ''
    )
  },
  {
    immediate: true
  }
)

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
    header: 'Publish Messages',
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
      label: 'Archive',
      severity: 'danger'
    },
    accept: () => {
      archiveMessagesAsync(selectedMessageIds.value).then(() => {
        toast.add({
          severity: 'info',
          summary: 'Archived Messages',
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

const syncBroker = () => {
  if (!user.value?.settings.showSyncConfirmDialogs) {
    syncBrokerRequest()
    return
  }

  confirm.require({
    message:
      'Do you really want to sync with remote broker? You can turn off this dialog on dashboard.',
    icon: 'pi pi-info-circle',
    header: 'Sync Broker',
    rejectProps: {
      label: 'Cancel',
      severity: 'secondary',
      outlined: true
    },
    acceptProps: {
      label: 'Sync Broker',
      severity: ''
    },
    accept: () => syncBrokerRequest(),
    reject: () => {}
  })
}

const syncBrokerRequest = () => {
  if (!broker.value) {
    return
  }

  syncBrokerAsync(broker.value?.id).then(() => {
    toast.add({
      severity: 'success',
      summary: 'Sync Completed!',
      detail: `Broker ${broker.value?.name} synced!`,
      life: 3000
    })
  })
}

const expandedRows = ref({})

const expandAll = () => {
  expandedRows.value =
    paginatedMessages.value?.items.reduce((acc: any, p) => (acc[p.id] = true) && acc, {}) ?? {}
}
const collapseAll = () => {
  expandedRows.value = {}
}

const allExpanded = computed(
  () => Object.keys(expandedRows.value).length === paginatedMessages.value?.items.length
)

const formatPayload = ref('raw')
const formatOptions = [
  {
    label: 'Raw',
    value: 'raw'
  },
  {
    label: 'JSON',
    value: 'formatted'
  }
]
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
    <template #description>{{ rabbitMqQueue?.parsed.name }}</template>
    <div class="flex items-start gap-2 border-b px-4 py-2">
      <Button @click="backToBroker" outlined label="Broker" icon="pi pi-arrow-left"></Button>
      <ButtonGroup>
        <Button
          @click="() => syncMessages()"
          outlined
          :label="syncLabel"
          :loading="isSyncMessagesPending"
          icon="pi pi-download"
        ></Button>
        <Button
          @click="syncBroker()"
          outlined
          :loading="isSyncBrokerPending"
          label="Sync"
          icon="pi pi-sync"
        ></Button>
      </ButtonGroup>
      <Button
        @click="() => reviewMessages()"
        outlined
        :loading="isReviewMessagesPending"
        :disabled="isReviewMessagesPending || !selectedMessages.length"
        :label="reviewMessagesLabel"
        icon="pi pi-check"
      ></Button>
      <Button
        @click="() => archiveMessages()"
        outlined
        :loading="isArchiveMessagesPending"
        :disabled="isArchiveMessagesPending || !selectedMessages.length"
        severity="danger"
        icon="pi pi-trash"
      ></Button>

      <SelectButton
        v-tooltip.top="'Payload format'"
        v-model="formatPayload"
        :options="formatOptions"
        option-label="label"
        option-value="value"
        class="ms-auto"
        aria-labelledby="basic"
      />

      <Select
        v-model="selectedExchange"
        :options="formattedExchanges"
        optionLabel="parsed.name"
        placeholder="Select an exchange"
        class="w-96"
        filter
        severity="danger"
        :virtualScrollerOptions="{ itemSize: 38, style: 'width:900px' }"
      ></Select>
      <Button
        @click="publishMessages"
        :loading="isPublishMessagesPending"
        :disabled="isPublishMessagesPending || !selectedMessageIds.length"
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
        v-model:expandedRows="expandedRows"
        pt:bodyCell:class="bg-red-400"
      >
        <Column selectionMode="multiple" headerStyle="width: 3rem"></Column>
        <Column expander class="w-[0%]">
          <template #header>
            <i
              v-if="!allExpanded"
              class="pi pi-plus grow cursor-pointer text-center font-bold"
              @click="expandAll"
            ></i
            ><i
              v-else
              class="pi pi-minus grow cursor-pointer text-center font-bold"
              @click="collapseAll"
            ></i>
          </template>
        </Column>

        <Column field="id" header="Message" class="w-[0%]">
          <template #body="{ data }">
            <div class="flex">
              <span
                @click="openMessage(data.id)"
                class="border-b border-dashed border-slate-600 hover:cursor-pointer hover:border-blue-500 hover:text-blue-500"
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
            ><div class="whitespace-nowrap text-slate-500">
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

        <template #expansion="slotProps">
          <pre v-if="formatPayload === 'formatted'" class="text-sm">{{ slotProps }}</pre>
          <div v-else>{{ slotProps }}</div>
        </template>
      </DataTable>
      <Paginator
        @page="changePage"
        :rows="50"
        :always-show="false"
        :total-records="paginatedMessages?.totalCount"
      ></Paginator>
    </template>
    <template v-else>
      <div class="mt-24 flex grow flex-col items-center">
        <img src="/ebox.svg" class="w-56 pb-5 opacity-50" />
        <div class="text-lg">No Messages</div>
        <div class="">Make sure you pull the messages.</div>
      </div>
    </template>
  </AppLayout>
</template>
