<script lang="ts" setup>
import { useMessagesQuery } from '@/api/messages/messagesQuery'
import { useQueue } from '@/composables/queueComposable'
import RequeueDialog from '@/dialogs/RequeueDialog.vue'
import type { MessageDeliveryDto } from '@/dtos/message/messageDeliveryDto'
import AppLayout from '@/layouts/AppLayout.vue'
import { format, formatDistance } from 'date-fns'
import Column from 'primevue/column'
import DataTable, { type DataTablePageEvent } from 'primevue/datatable'
import type { MenuItem } from 'primevue/menuitem'
import SelectButton from 'primevue/selectbutton'
import { computed, ref, watchEffect } from 'vue'
import { useRouter } from 'vue-router'
import MessagesMessage from './MessagesMessage.vue'

const props = defineProps<{
  queueName: string
}>()

const router = useRouter()

const first = ref(0)
const pageIndex = ref(0)

// Queues
const {
  queueOptions,
  queryView: { data: queueView }
} = useQueue(computed(() => props.queueName))

const selectedQueueId = ref<number>()

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
  } else if (selectedMessageId.value === msg.message_delivery_id) {
    selectedMessageId.value = 0
  } else {
    selectedMessageId.value = msg.message_delivery_id
  }
}

// Selected messages
const selectedMessageId = ref<number>(24)
const selectedMessage = computed(() =>
  messages.value?.items.find((x) => x.message_delivery_id === selectedMessageId.value)
)

const selectedMessages = ref<MessageDeliveryDto[]>([])
const selectedMessageIds = computed(() =>
  selectedMessages.value?.length ? selectedMessages.value.map((x) => x.message_delivery_id) : []
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
      command: () => {
        router.push({
          name: 'queues'
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
          data-key="message_delivery_id"
          scrollable
          striped-rows
          scroll-height="flex"
          row-hover
          @page="onPage"
          @row-click="(e) => toggleMessage(e.data)"
        >
          <Column selectionMode="multiple" class="w-0" style="vertical-align: top; text-align: center"></Column>
          <Column field="message_delivery_id" header="ID" class="w-0 whitespace-nowrap"> </Column>
          <Column field="message.message_type" header="URN" class="w-0 whitespace-nowrap">
            <template #body="{ data }">
              {{ data.message.message_type.replace('urn:message:', '') }}
            </template>
          </Column>
          <Column field="message.transport_headers" header="" class="whitespace-nowrap">
            <template #body="{ data }">
              <div v-if="data.transport_headers['MT-Fault-Message']" class="flex gap-3">
                <i class="pi pi-circle-fill text-red-400" style="font-size: 0.825rem"></i
                >{{ data.transport_headers['MT-Fault-ExceptionType'] }}
              </div>
            </template>
          </Column>
          <Column field="message.lock_id" header="" class="w-0 whitespace-nowrap">
            <template #body="{ data }">
              <i :class="`pi pi-${data.lock_id ? 'lock' : ''}`"></i>
            </template>
          </Column>
          <Column field="priority" header="Priority" class="w-0 whitespace-nowrap"></Column>
          <Column field="enqueue_time" header="Enqueue Time" header-class="" class="w-0 whitespace-nowrap">
            <template #body="{ data }">
              <div class="flex gap-2" v-if="data.enqueue_time">
                {{ format(data.enqueue_time, 'MMM dd HH:mm:ss') }} (
                {{ formatDistance(data.enqueue_time, new Date()) }}
                ago)
              </div>
            </template></Column
          >
        </DataTable>
      </div>
    </template>
  </AppLayout>
</template>
