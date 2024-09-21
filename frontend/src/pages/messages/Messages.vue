<script lang="ts" setup>
import { useBrokersQuery } from '@/api/brokers/brokersQuery'
import { useSyncBrokerMutation } from '@/api/brokers/syncBrokerMutation'
import { useCloneMessageMutation } from '@/api/messages/cloneMessageMutation'
import { usePaginatedMessagesQuery } from '@/api/messages/paginatedMessagesQuery'
import { useSyncMessagesMutation } from '@/api/messages/syncMessagesMutation'
import { useQueueQuery } from '@/api/queues/queueQuery'
import { type FormatOption } from '@/components/SelectFormat.vue'
import type { StructureOption } from '@/components/SelectStructure.vue'
import { useIdentity } from '@/composables/identityComposable'
import { useRabbitMqQueues } from '@/composables/rabbitMqQueuesComposable'
import UpsertMessageDialog from '@/dialogs/UpsertMessageDialog.vue'
import type { RabbitMQMessageDto } from '@/dtos/message/rabbitMQMessageDto'
import Avatars from '@/features/avatars/Avatars.vue'
import FormattedMessage from '@/features/formatted-message/FormattedMessage.vue'
import MessageCopy from '@/features/message-copy/MessageCopy.vue'
import MessageActions from '@/features/message/MessageActions.vue'
import AppLayout from '@/layouts/AppLayout.vue'
import { errorToToast } from '@/utils/errorUtils'
import { messageSummary } from '@/utils/messageUtils'
import { formatDistanceToNow } from 'date-fns'
import Button from 'primevue/button'
import ButtonGroup from 'primevue/buttongroup'
import Column from 'primevue/column'
import DataTable from 'primevue/datatable'
import type { PageState } from 'primevue/paginator'
import type { PopoverMethods } from 'primevue/popover'
import Tag from 'primevue/tag'
import { useConfirm } from 'primevue/useconfirm'
import { useDialog } from 'primevue/usedialog'
import { useToast } from 'primevue/usetoast'
import { computed, ref, watch } from 'vue'
import { useRouter } from 'vue-router'

const props = defineProps<{
  brokerId: string
  queueId: string
}>()

const router = useRouter()

const confirm = useConfirm()
const dialog = useDialog()
const toast = useToast()

const {
  query: { data: user }
} = useIdentity()

const { mutateAsync: syncMessagesAsync, isPending: isSyncMessagesPending } =
  useSyncMessagesMutation()

const { mutateAsync: syncBrokerAsync, isPending: isSyncBrokerPending } = useSyncBrokerMutation()

const { mutateAsync: cloneMessageAsync, isPending: isCloneMessagePending } =
  useCloneMessageMutation()

const pageIndex = ref(0)

const { data: brokers } = useBrokersQuery()

const { data: paginatedMessages, isPending } = usePaginatedMessagesQuery(
  computed(() => props.queueId),
  pageIndex
)

const { data: queue } = useQueueQuery(computed(() => props.queueId))
const broker = computed(() => brokers.value?.find((x) => x.id === props.brokerId))

const queues = computed(() => (queue.value ? [queue.value] : undefined))
const { rabbitMqQueues } = useRabbitMqQueues(queues)
const rabbitMqQueue = computed(() => rabbitMqQueues.value[0] ?? undefined)

const backToQueues = () => {
  router.push({
    name: 'queues',
    params: {
      brokerId: props.brokerId
    }
  })
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
  })
    .then(() => {
      toast.add({
        severity: 'success',
        summary: 'Sync Completed!',
        detail: `Messages for queue ${queue.value?.id} synced!`,
        life: 3000
      })
    })
    .catch((e) => toast.add(errorToToast(e)))
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

const selectedMessages = ref<RabbitMQMessageDto[]>([])
const selectedMessageIds = computed(() => selectedMessages.value.map((x) => x.id))

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

const idPopovers = ref<PopoverMethods[]>([])

const toggleIdPopover = (e: Event) => {
  idPopovers.value[0]?.toggle(e)
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

const handleCopied = () => {
  idPopovers.value.forEach((x) => {
    x.hide()
  })
}

const selectedMessageFormat = ref<FormatOption>('raw')
const selectedMessageStructure = ref<StructureOption>('body')

watch(
  () => broker.value,
  (broker) => {
    const access = broker?.accessList.find((x) => x.userId === user.value?.id)
    const format = access?.settings.messageFormat
    const structure = access?.settings.messageStructure

    if (format && structure) {
      selectedMessageFormat.value = format
      selectedMessageStructure.value = structure
    }
  },
  {
    immediate: true
  }
)

const editMessage = (id: string) => {
  dialog.open(UpsertMessageDialog, {
    data: {
      broker: broker.value,
      queue: rabbitMqQueue.value,
      message: paginatedMessages.value?.items.find((x) => x.id === id)
    },
    props: {
      header: 'Message Editor',
      position: 'top',
      modal: true,
      draggable: false
    }
  })
}

const cloneMessage = (id: string) => {
  cloneMessageAsync(id).catch((e) => toast.add(errorToToast(e)))
}
</script>

<template>
  <AppLayout>
    <template #prepend>
      <div
        class="flex h-[42px] w-[42px] cursor-pointer items-center justify-center rounded-xl bg-[#FF6600] text-2xl text-white active:scale-95"
        @click="backToQueues"
      >
        <img src="/rmq.svg" class="w-7 select-none" />
      </div>
    </template>
    <template #title>
      <span class="cursor-pointer hover:underline" @click="backToQueues">{{ broker?.name }}</span>
    </template>
    <template #description>{{ rabbitMqQueue?.parsed.name }}</template>
    <template #append>
      <Avatars v-if="broker" :user-ids="broker.accessList.map((x) => x.userId)" />
    </template>
    <div class="flex flex-wrap items-start gap-2 border-b px-4 py-2">
      <Button @click="backToQueues" outlined label="Queues" icon="pi pi-arrow-left"></Button>
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

      <MessageActions
        v-if="broker && paginatedMessages?.items && rabbitMqQueue"
        :broker="broker"
        :rabbit-mq-queue="rabbitMqQueue"
        :selected-message-ids="selectedMessageIds"
        :messages="paginatedMessages?.items"
        v-model:message-structure="selectedMessageStructure"
        v-model:message-format="selectedMessageFormat"
      />
    </div>
    <template v-if="isPending">
      <div class="p-5"><i class="pi pi-spinner pi-spin me-2"></i>Loading messages...</div>
    </template>
    <template v-else-if="paginatedMessages?.items.length">
      <Popover v-for="it in paginatedMessages?.items" :key="it.id" ref="idPopovers">
        <MessageCopy
          v-if="broker && queue"
          :broker="broker"
          :queue="queue"
          :message="it"
          @copied="handleCopied"
        />
      </Popover>

      <DataTable
        v-model:selection="selectedMessages"
        :value="paginatedMessages?.items"
        data-key="id"
        scrollable
        class="max-w-full grow overflow-hidden"
        scroll-height="flex"
        v-model:expandedRows="expandedRows"
      >
        <Column
          selectionMode="multiple"
          class="w-0"
          style="vertical-align: top; text-align: center"
        ></Column>
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
            <div class="flex items-center gap-2">
              <Button text size="small" @click="(e) => toggleIdPopover(e)"
                ><i class="pi pi-copy"></i
              ></Button>
              <span
                @click="openMessage(data.id)"
                class="text border-b border-dashed border-slate-600 hover:cursor-pointer hover:border-blue-500 hover:text-blue-500"
                >{{ data.id.slice(-8) }}
              </span>
            </div>
          </template>
        </Column>
        <Column field="flags" class="w-[0%]">
          <template #body="{ data }">
            <div class="flex items-center gap-2">
              <Tag v-if="data.isReviewed" icon="pi pi-check"></Tag>
            </div>
          </template>
        </Column>

        <Column field="summary" header="Summary" class="overflow-hidden">
          <template #body="{ data }">
            <div class="overflow-hidden overflow-ellipsis text-nowrap">
              {{ messageSummary({ ...data }) }}
            </div>
          </template>
        </Column>

        <Column field="edit" header="" class="w-[0%]">
          <template #body="{ data }">
            <div class="flex">
              <Button text icon="pi pi-pencil" size="small" @click="editMessage(data.id)"></Button>
              <Button
                text
                icon="pi pi-clone"
                size="small"
                :loading="isCloneMessagePending"
                @click="cloneMessage(data.id)"
              ></Button>
            </div>
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

        <template #expansion="{ data }">
          <FormattedMessage
            :message="data"
            :format="selectedMessageFormat"
            :structure="selectedMessageStructure"
          />
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
