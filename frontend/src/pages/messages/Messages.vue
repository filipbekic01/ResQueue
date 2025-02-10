<script lang="ts" setup>
import { useDeleteMessagesMutation } from '@/api/messages/deleteMessagesMutation'
import { useMessagesQuery } from '@/api/messages/messagesQuery'
import { usePurgeQueueMutation } from '@/api/queues/purgeQueueMutation'
import { useQueue } from '@/composables/queueComposable'
import { useUserSettings } from '@/composables/userSettingsComposable'
import RequeueDialog from '@/dialogs/RequeueDialog.vue'
import type { MessageDeliveryDto } from '@/dtos/message/messageDeliveryDto'
import type { QueueDto } from '@/dtos/queue/queueDto'
import AppLayout from '@/layouts/AppLayout.vue'
import Graph from '@/layouts/Graph.vue'
import { humanDateTime } from '@/utils/dateTimeUtil'
import { errorToToast } from '@/utils/errorUtils'
import { useQueryClient } from '@tanstack/vue-query'
import Column from 'primevue/column'
import DataTable, { type DataTablePageEvent } from 'primevue/datatable'
import type { MenuItem } from 'primevue/menuitem'
import Tabs from 'primevue/tabs'
import { useConfirm } from 'primevue/useconfirm'
import { useToast } from 'primevue/usetoast'
import { computed, ref, watchEffect } from 'vue'
import { useRouter } from 'vue-router'
import MessageDialog from './MessageDialog.vue'

const props = defineProps<{
  queueName: string
}>()

const router = useRouter()

const queryClient = useQueryClient()

const confirm = useConfirm()
const toast = useToast()

const first = ref(0)
const pageIndex = ref(0)

const { settings, updateSettings } = useUserSettings()

// Queues
const {
  queueOptions,
  queryView: { data: queueView },
  query: { data: queues },
  primaryQueue,
  getQueueTypeLabel,
} = useQueue(computed(() => props.queueName))

const selectedQueueId = ref<number>()
const selectedQueue = computed(() => queues.value?.find((x) => x.id === selectedQueueId.value))

const updateSelectedQueue = (queue: QueueDto) => {
  selectedQueueId.value = queue.id
  updateSettings({ ...settings, queueType: queue.type })
}

watchEffect(() => {
  if (selectedQueueId.value || !queueOptions.value.length || !queueView.value) {
    return
  }

  selectedQueueId.value =
    queueOptions.value.find((x) => x.queue.type == settings.queueType)?.queue.id ?? undefined
})

// Purge queue
const { mutateAsync: purgeQueueAsync, isPending: isPurgeQueuePending } = usePurgeQueueMutation()

// Messages
const {
  data: messages,
  refetch: refetchMessages,
  isPending,
} = useMessagesQuery(
  computed(() => selectedQueueId.value),
  pageIndex,
  computed(() => settings.refetchInterval),
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
const { mutateAsync: deleteMessagesAsync, isPending: isDeleteMessagesPending } =
  useDeleteMessagesMutation()
const deleteMessagesTransactional = ref(false)

const deleteMessages = (e: any) => {
  deleteMessagesAsync({
    messageDeliveryIds: selectedMessages.value.map((m) => m.messageDeliveryId),
    transactional: deleteMessagesTransactional.value,
  })
    .then(() => {
      onActionComplete()
      deleteMessagesPopover.value.hide(e.originalEvent)
      toast.add({
        severity: 'success',
        summary: 'Messages Deleted',
        detail: `Messages delete procedure ran successfully.`,
        life: 3000,
      })
    })
    .catch((e) => toast.add(errorToToast(e)))
}

// Selected messages
const selectedMessageId = ref<number>(24)
const selectedMessage = computed(() =>
  messages.value?.items.find((x) => x.messageDeliveryId === selectedMessageId.value),
)

const selectedMessages = ref<MessageDeliveryDto[]>([])
const selectedMessageIds = computed(() =>
  selectedMessages.value?.length ? selectedMessages.value.map((x) => x.messageDeliveryId) : [],
)

const requeuePopover = ref()
const requeueSpecificPopover = ref()
const deleteMessagesPopover = ref()

const items = computed((): MenuItem[] => {
  return [
    {
      label: 'Queues',
      icon: 'pi pi-arrow-left',
      command: () => {
        router.push({
          name: 'queues',
        })
      },
    },
    {
      label: `Refresh`,
      icon: `pi pi-refresh`,
      disabled: isPending.value,
      command: () => {
        refetchMessages().then(() => {
          toast.add({
            severity: 'success',
            summary: 'Queue Refreshed',
            detail: 'The queue has been successfully updated.',
            life: 1000,
          })
        })

        queryClient.invalidateQueries({ queryKey: ['queue-view'] })
      },
    },
    {
      label: `Requeue`,
      icon: 'pi pi-replay',
      command: (e) => requeueSpecificPopover.value.toggle(e.originalEvent),
      disabled: !selectedMessageIds.value.length,
    },
    {
      label: `Batch Requeue`,
      icon: 'pi pi-replay',
      command: (e) => requeuePopover.value.toggle(e.originalEvent),
    },
    {
      label: 'Delete',
      icon: 'pi pi-trash',
      disabled: !selectedMessageIds.value.length,
      command: (e) => {
        deleteMessagesPopover.value.toggle(e.originalEvent)
      },
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
            outlined: true,
          },
          acceptProps: {
            label: 'Purge',
            severity: 'danger',
          },
          accept: () => {
            if (!selectedQueueId.value) {
              return
            }

            purgeQueueAsync({
              queueId: selectedQueueId.value,
            })
              .then(() => {
                onActionComplete()

                toast.add({
                  severity: 'success',
                  summary: 'Purge Completed',
                  detail: `Queue has been purged successfully.`,
                  life: 3000,
                })
              })
              .catch((e) => toast.add(errorToToast(e)))
          },
          reject: () => {},
        })
      },
    },
  ]
})

const onRequeueComplete = () => {
  requeueSpecificPopover.value.hide()
  requeuePopover.value.hide()

  onActionComplete()
}

const onActionComplete = () => {
  selectedMessages.value = []
}

const onPage = (event: DataTablePageEvent) => {
  pageIndex.value = event.page
  first.value = event.first
}

const getMessagesIcon = (queue: QueueDto) => {
  if (queue.type == 1) {
    return 'pi-check-circle'
  } else if (queue.type == 2) {
    return 'pi-exclamation-circle'
  } else if (queue.type == 3) {
    return 'pi-times-circle'
  }
}

const hasMtFaultMessages = computed(() => {
  return messages.value?.items.some((x) => x.transportHeaders['MT-Fault-Message'])
})
</script>

<template>
  <AppLayout>
    <template #topright> qwe </template>
    <MessageDialog
      v-if="selectedMessage"
      :selected-message="selectedMessage"
      @close="toggleMessage(undefined)"
    />

    <Popover ref="deleteMessagesPopover">
      <div class="flex flex-col gap-3">
        <div class="flex items-center gap-2">
          <Checkbox id="transactional" v-model="deleteMessagesTransactional" binary></Checkbox>
          <label for="transactional">Within single transaction</label>
        </div>
        <Button
          icon="pi pi-arrow-right"
          severity="danger"
          :loading="isDeleteMessagesPending"
          icon-pos="right"
          :label="`Delete`"
          @click="deleteMessages"
        ></Button>
      </div>
    </Popover>
    <Popover ref="requeueSpecificPopover">
      <RequeueDialog
        v-if="selectedQueueId"
        :selected-queue-id="selectedQueueId"
        :batch="false"
        :delivery-message-ids="selectedMessageIds"
        @requeue:complete="onRequeueComplete"
      />
    </Popover>
    <Popover ref="requeuePopover">
      <RequeueDialog
        v-if="selectedQueueId"
        :selected-queue-id="selectedQueueId"
        :batch="true"
        :delivery-message-ids="[]"
        @requeue:complete="onRequeueComplete"
      />
    </Popover>
    <div class="flex items-center">
      <Menubar
        :model="items"
        class="w-full rounded-none border-0 border-b dark:border-b-surface-700"
      />
      <Tabs v-if="selectedQueueId" :value="selectedQueueId" class="ms-auto">
        <TabList>
          <Tab
            v-for="item in queueOptions"
            :key="item.queue.id"
            :value="item.queue.id"
            class="flex gap-2"
            @click="updateSelectedQueue(item.queue)"
          >
            <i class="pi" :class="getMessagesIcon(item.queue)"></i>
            {{ item.queueNameByType }}
          </Tab>
        </TabList>
      </Tabs>
    </div>
    <div class="flex border-b">
      <Graph class="me-3 border-e pe-3" v-if="primaryQueue" :queue="primaryQueue" />
      <div class="my-auto flex flex-col font-mono text-gray-500">
        <div>Con. per second: 0/s</div>
        <div>Err. per second: 0/s</div>
        <div>Dea. per second: 0/s</div>
      </div>
    </div>
    <template v-if="messages?.items.length">
      <div class="flex grow flex-col overflow-auto">
        <DataTable
          show-gridlines
          class="rq-grid"
          :show-headers="true"
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
          <!-- Selection -->

          <Column
            selectionMode="multiple"
            class="w-0"
            style="vertical-align: top; text-align: center"
          ></Column>

          <!-- ID -->

          <Column field="messageDeliveryId" header="ID" class="w-0 whitespace-nowrap"> </Column>

          <!-- URN -->

          <Column
            field="message.messageType"
            header="URN"
            class="whitespace-nowrap"
            :class="[
              {
                'w-0': hasMtFaultMessages,
              },
            ]"
          >
            <template #body="{ data }">
              {{ data.message.messageType.replace('urn:message:', '') }}
            </template>
          </Column>

          <!-- Fault Message -->

          <Column
            v-if="hasMtFaultMessages"
            field="message.transportHeaders"
            class="whitespace-nowrap"
            header="Fault Message"
          >
            <template #body="{ data }">
              <div
                v-if="data.transportHeaders['MT-Fault-Message']"
                class="flex gap-3 dark:text-surface-400"
              >
                <i class="pi pi-circle-fill text-red-400" style="font-size: 0.625rem"></i
                >{{ data.transportHeaders['MT-Fault-ExceptionType'] }}
              </div>
              <div v-else>-</div>
            </template>
          </Column>

          <!-- Expires At -->

          <Column field="message.expirationTime" header="Expires At" class="w-0 whitespace-nowrap">
            <template #body="{ data }">
              {{ data.message.expirationTime }}
            </template>
          </Column>

          <!-- Recurring -->

          <Column field="message.isRecurring" header="Recurring" class="w-0 whitespace-nowrap">
            <template #header></template>
            <template #body="{ data }">
              <div class="flex items-center justify-center">
                <i v-if="data.isRecurring" class="pi pi-check" style="font-size: 0.825rem"></i>
              </div>
            </template>
          </Column>

          <!-- Scheduled -->

          <Column
            field="message.schedulingTokenId"
            header="Scheduled"
            class="w-0 whitespace-nowrap"
          >
            <template #header></template>
            <template #body="{ data }">
              <div class="flex items-center justify-center">
                <i
                  v-if="data.message.schedulingTokenId"
                  class="pi pi-check"
                  style="font-size: 0.825rem"
                ></i>
              </div>
            </template>
          </Column>

          <!-- Locked -->

          <Column field="message.lockedId" header="Locked" class="w-0 whitespace-nowrap">
            <template #header></template>
            <template #body="{ data }">
              <div class="flex items-center justify-center" v-if="!data.lockId">
                <i class="pi pi-check" style="font-size: 0.825rem"></i>
              </div>
            </template>
          </Column>

          <!-- Priority -->

          <Column field="priority" header="Priority" class="w-0 justify-center whitespace-nowrap">
            <template #body="{ data }">
              <div class="text-center">
                {{ data.priority }}
              </div>
            </template>
          </Column>

          <!-- Enqueue Time -->

          <Column
            field="enqueueTime"
            header="Enqueue Time"
            header-class=""
            class="w-0 whitespace-nowrap"
          >
            <template #body="{ data }">
              <div class="flex gap-2">
                {{ humanDateTime(data.enqueueTime) }}
              </div>
            </template>
          </Column>
        </DataTable>
      </div>
    </template>
  </AppLayout>
</template>
