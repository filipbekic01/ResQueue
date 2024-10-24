<script lang="ts" setup>
import { usePaginatedMessagesQuery } from '@/api/messages/paginatedMessagesQuery'
import { useQueue } from '@/composables/queueComposable'
import RequeueDialog from '@/dialogs/RequeueDialog.vue'
import type { MessageDeliveryDto } from '@/dtos/message/messageDeliveryDto'
import AppLayout from '@/layouts/AppLayout.vue'
import Column from 'primevue/column'
import DataTable from 'primevue/datatable'
import type { MenuItem } from 'primevue/menuitem'
import SelectButton from 'primevue/selectbutton'
import { computed, ref, watchEffect } from 'vue'
import { useRouter } from 'vue-router'
import MessagesMessage from './MessagesMessage.vue'

const props = defineProps<{
  queueName: string
}>()

const router = useRouter()

const pageIndex = ref(0)

// Queues
const { queueOptions } = useQueue(computed(() => props.queueName))

const selectedQueueId = ref<number>()

watchEffect(() => {
  if (!queueOptions.value.length) {
    return
  }

  selectedQueueId.value = queueOptions.value.find((x) => x)?.queue.id ?? undefined
})

// Messages
const {
  data: paginatedMessages,
  refetch: refetchPaginatedMessages,
  isPending
} = usePaginatedMessagesQuery(
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
  paginatedMessages.value?.items.find((x) => x.message_delivery_id === selectedMessageId.value)
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
        refetchPaginatedMessages()
      }
    },
    {
      label: `Requeue ${selectedMessageIds.value.length ? `(${selectedMessageIds.value.length})` : ''}`,
      icon: 'pi pi-replay',
      command: (e) => requeueSpecificPopover.value.toggle(e.originalEvent)
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
</script>

<template>
  <AppLayout>
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
    <!-- <Paginator
      class="ms-auto"
      @page="changePage"
      :rows="50"
      :always-show="false"
      :total-records="paginatedMessages?.totalCount"
    ></Paginator>
 -->
    <template v-if="paginatedMessages?.items.length">
      <div class="flex grow flex-col overflow-auto">
        <div
          class="flex grow flex-col overflow-auto"
          :class="[
            {
              'basis-[20%]': selectedMessageId
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
            row-hover
            @row-click="(e) => toggleMessage(e.data)"
          >
            <Column selectionMode="multiple" class="w-0" style="vertical-align: top; text-align: center"></Column>
            <Column field="message_delivery_id" header="ID" class="w-0 whitespace-nowrap"> </Column>
            <!-- <Column field="message.message_type" header="" class="w-0 whitespace-nowrap">
              <template #body="{ data }">
                <span class="cursor-pointer text-blue-500 hover:text-blue-300" @click="toggleMessage(data)"
                  ><i class="pi pi-eye"></i
                ></span>
              </template>
            </Column> -->
            <Column field="message.message_type" header="URN" class="w-0 whitespace-nowrap">
              <template #body="{ data }">
                {{ data.message.message_type.replace('urn:message:', '') }}
              </template>
            </Column>
            <Column field="message.transport_headers" header="" class="whitespace-nowrap">
              <template #body="{ data }">
                <div v-if="data.transport_headers['MT-Fault-Message']" class="flex gap-3 text-red-950">
                  <i class="pi pi-exclamation-circle text-red-700"></i>{{ data.transport_headers['MT-Fault-Message'] }}
                </div>
              </template>
            </Column>
            <Column field="message.lock_id" header="" class="w-0 whitespace-nowrap">
              <template #body="{ data }">
                <i :class="`pi pi-${data.lock_id ? 'lock' : ''}`"></i>
              </template>
            </Column>
            <Column field="priority" header="Priority" class="w-0 whitespace-nowrap"></Column>
            <Column field="enqueue_time" header="Enqueue Time" class="w-0 whitespace-nowrap">
              <template #body="{ data }">
                <div class="flex gap-2" v-if="data.enqueue_time">{{ data.enqueue_time }}</div>
              </template></Column
            >
          </DataTable>
        </div>
        <MessagesMessage v-if="selectedMessage" :selected-message="selectedMessage" @close="toggleMessage(undefined)" />
      </div>
    </template>
  </AppLayout>
</template>
