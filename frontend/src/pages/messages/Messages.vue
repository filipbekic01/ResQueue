<script lang="ts" setup>
import { useDeleteMessagesMutation } from '@/api/messages/deleteMessagesMutation.ts'
import { useMessagesQuery } from '@/api/messages/messagesQuery'
import { usePurgeQueueMutation } from '@/api/queues/purgeQueueMutation'
import { useQueue } from '@/composables/queueComposable'
import RequeueDialog from '@/dialogs/RequeueDialog.vue'
import type { MessageDeliveryDto } from '@/dtos/message/messageDeliveryDto'
import AppLayout from '@/layouts/AppLayout.vue'
import { errorToToast } from '@/utils/errorUtils'
import { format, formatDistance } from 'date-fns'
import Column from 'primevue/column'
import DataTable, { type DataTablePageEvent } from 'primevue/datatable'
import type { MenuItem } from 'primevue/menuitem'
import SelectButton from 'primevue/selectbutton'
import { useConfirm } from 'primevue/useconfirm'
import { useToast } from 'primevue/usetoast'
import { computed, ref, watchEffect } from 'vue'
import { useRouter } from 'vue-router'
import MessagesMessage from './MessagesMessage.vue'

const props = defineProps<{
  queueName: string
}>()

const router = useRouter()

const confirm = useConfirm()
const toast = useToast()

const first = ref(0)
const pageIndex = ref(0)

// Queues
const {
  queueOptions,
  queryView: { data: queueView },
  query: { data: queues },
  getQueueTypeLabel
} = useQueue(computed(() => props.queueName))

const selectedQueueId = ref<number>()
const selectedQueue = computed(() => queues.value?.find((x) => x.id === selectedQueueId.value))

watchEffect(() => {
  if (!queueOptions.value.length || !queueView.value) {
    return
  }

  if (queueView.value.errored > 0) {
    selectedQueueId.value = queueOptions.value.find((x) => x.queue.type == 2)?.queue.id ?? undefined
  } else {
    selectedQueueId.value = queueOptions.value.find((x) => x.queue.type == 1)?.queue.id ?? undefined
  }
})

// Purge queue
const { mutateAsync: purgeQueueAsync, isPending: isPurgeQueuePending } = usePurgeQueueMutation()

// Messages
const {
  data: messages,
  refetch: refetchMessages,
  isPending
} = useMessagesQuery(
  computed(() => selectedQueueId.value),
  pageIndex
)

const toggleMessage = (msg?: MessageDeliveryDto) => {
  if (!msg) {
    selectedMessageId.value = 0
  } else if (selectedMessageId.value === msg.messageDeliveryId) {
    selectedMessageId.value = 0
  } else {
    selectedMessageId.value = msg.messageDeliveryId
  }
}

// Delete messages
const { mutateAsync: deleteMessagesAsync, isPending: isDeleteMessagesPending } = useDeleteMessagesMutation()

// Selected messages
const selectedMessageId = ref<number>(24)
const selectedMessage = computed(() =>
  messages.value?.items.find((x) => x.messageDeliveryId === selectedMessageId.value)
)

const selectedMessages = ref<MessageDeliveryDto[]>([])
const selectedMessageIds = computed(() =>
  selectedMessages.value?.length ? selectedMessages.value.map((x) => x.messageDeliveryId) : []
)

const requeuePopover = ref()
const requeueSpecificPopover = ref()
const items = computed((): MenuItem[] => {
  return [
    {
      label: 'Queues',
      icon: 'pi pi-arrow-left',
      command: () => {
        router.push({
          name: 'queues'
        })
      }
    },
    {
      label: `Refresh`,
      icon: `pi pi-refresh`,
      disabled: isPending.value,
      command: () => {
        refetchMessages()
      }
    },
    {
      label: `Requeue ${selectedMessageIds.value.length ? `(${selectedMessageIds.value.length})` : ''}`,
      icon: 'pi pi-replay',
      command: (e) => requeueSpecificPopover.value.toggle(e.originalEvent),
      disabled: !selectedMessageIds.value.length
    },
    {
      label: `Batch Requeue`,
      icon: 'pi pi-replay',
      command: (e) => requeuePopover.value.toggle(e.originalEvent)
    },
    {
      label: 'Delete',
      icon: 'pi pi-trash',
      disabled: isDeleteMessagesPending.value,
      command: () => {
        confirm.require({
          header: 'Delete Messages',
          message: `Do you want to delete ${selectedMessageIds.value.length} messages?`,
          icon: 'pi pi-info-circle',
          rejectProps: {
            label: 'Cancel',
            severity: 'secondary',
            outlined: true
          },
          acceptProps: {
            label: 'Delete',
            severity: 'danger'
          },
          accept: () => {
            deleteMessagesAsync({
              messages: selectedMessages.value.map((msg) => ({
                messageDeliveryId: msg.messageDeliveryId,
                lockId: msg.lockId
              }))
            }).catch((e) => toast.add(errorToToast(e)))
          },
          reject: () => {}
        })
      }
    },
    {
      label: 'Purge',
      icon: 'pi pi-eraser',
      disabled: isPurgeQueuePending.value,
      command: () => {
        confirm.require({
          header: `Purge Queue`,
          message: `Do you want to purge ${getQueueTypeLabel(selectedQueue.value?.type)} queue?`,
          icon: 'pi pi-info-circle',
          rejectProps: {
            label: 'Cancel',
            severity: 'secondary',
            outlined: true
          },
          acceptProps: {
            label: 'Purge',
            severity: 'danger'
          },
          accept: () => {
            if (!selectedQueueId.value) {
              return
            }

            purgeQueueAsync({
              queueId: selectedQueueId.value
            })
              .then(() => {
                toast.add({
                  severity: 'success',
                  summary: 'Purge Completed',
                  detail: `Queue has been purged successfully.`,
                  life: 3000
                })
              })
              .catch((e) => toast.add(errorToToast(e)))
          },
          reject: () => {}
        })
      }
    }
  ]
})

const onPage = (event: DataTablePageEvent) => {
  pageIndex.value = event.page
  first.value = event.first
}
</script>

<template>
  <AppLayout>
    <MessagesMessage v-if="selectedMessage" :selected-message="selectedMessage" @close="toggleMessage(undefined)" />

    <Popover ref="requeueSpecificPopover">
      <RequeueDialog
        v-if="selectedQueueId"
        :selected-queue-id="selectedQueueId"
        :batch="false"
        :delivery-message-ids="selectedMessageIds"
      />
    </Popover>
    <Popover ref="requeuePopover">
      <RequeueDialog
        v-if="selectedQueueId"
        :selected-queue-id="selectedQueueId"
        :batch="true"
        :delivery-message-ids="[]"
      />
    </Popover>
    <div class="flex items-center border-b dark:border-b-surface-700">
      <Menubar :model="items" class="border-0" />
      <div class="ms-auto flex items-center gap-3">
        <SelectButton
          class="me-3"
          option-label="queueNameByType"
          option-value="queue.id"
          :allow-empty="false"
          v-model="selectedQueueId"
          :options="queueOptions"
        ></SelectButton>
      </div>
    </div>
    <template v-if="messages?.items.length">
      <div class="flex grow flex-col overflow-auto">
        <DataTable
          v-model:selection="selectedMessages"
          :value="messages.items"
          :rows="messages.pageSize"
          :total-records="messages.totalCount"
          :loading="isPending"
          :first="first"
          :paginator="messages.totalPages > 1"
          lazy
          data-key="messageDeliveryId"
          scrollable
          striped-rows
          scroll-height="flex"
          row-hover
          @page="onPage"
          @row-click="(e) => toggleMessage(e.data)"
        >
          <Column selectionMode="multiple" class="w-0" style="vertical-align: top; text-align: center"></Column>
          <Column field="messageDeliveryId" header="ID" class="w-0 whitespace-nowrap"> </Column>
          <Column field="message.messageType" header="URN" class="w-0 whitespace-nowrap">
            <template #body="{ data }">
              {{ data.message.messageType.replace('urn:message:', '') }}
            </template>
          </Column>
          <Column field="message.transportHeaders" header="" class="whitespace-nowrap">
            <template #body="{ data }">
              <div v-if="data.transportHeaders['MT-Fault-Message']" class="flex gap-3">
                <i class="pi pi-circle-fill text-red-400" style="font-size: 0.625rem"></i
                >{{ data.transportHeaders['MT-Fault-ExceptionType'] }}
              </div>
            </template>
          </Column>
          <Column field="message.lockId" header="" class="w-0 whitespace-nowrap">
            <template #body="{ data }">
              <i :class="`pi pi-${data.lockId ? 'lock' : ''}`"></i>
            </template>
          </Column>
          <Column field="priority" header="Priority" class="w-0 whitespace-nowrap"></Column>
          <Column field="enqueueTime" header="Enqueue Time" header-class="" class="w-0 whitespace-nowrap">
            <template #body="{ data }">
              <div class="flex gap-2" v-if="data.enqueueTime">
                {{ format(data.enqueueTime, 'MMM dd HH:mm:ss') }} (
                {{ formatDistance(data.enqueueTime, new Date()) }}
                ago)
              </div>
            </template></Column
          >
        </DataTable>
      </div>
    </template>
  </AppLayout>
</template>
