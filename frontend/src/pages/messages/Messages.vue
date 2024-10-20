<script lang="ts" setup>
import { usePaginatedMessagesQuery } from '@/api/messages/paginatedMessagesQuery'
import eboxUrl from '@/assets/ebox.svg'
import { useQueues } from '@/composables/queuesComposable'
import type { MessageDeliveryDto } from '@/dtos/message/messageDeliveryDto'
import AppLayout from '@/layouts/AppLayout.vue'
import { highlightJson } from '@/utils/jsonUtils'
import Button from 'primevue/button'
import Column from 'primevue/column'
import DataTable from 'primevue/datatable'
import type { MenuItem } from 'primevue/menuitem'
import SelectButton from 'primevue/selectbutton'
import { computed, ref, watchEffect } from 'vue'
import { useRouter } from 'vue-router'
import MessagesRequeue from './MessagesRequeue.vue'

const props = defineProps<{
  queueName: string
}>()

const router = useRouter()

const pageIndex = ref(0)

// Queues
const { queueOptions } = useQueues(computed(() => props.queueName))

const selectedQueueId = ref<number>()

watchEffect(() => {
  if (!queueOptions.value.length) {
    return
  }

  selectedQueueId.value = queueOptions.value.find((x) => x)?.queue.id ?? undefined
})

// Messages
const { data: paginatedMessages, isPending } = usePaginatedMessagesQuery(
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
      label: 'Requeue',
      icon: 'pi pi-replay',
      items: [
        {
          label: 'Requeue Selected'
        },
        {
          label: 'Requeue Bulk'
        }
      ]
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
    <div class="flex items-center border-b">
      <Menubar :model="items" class="border-0" />
      <SelectButton
        class="me-3 ms-auto"
        option-label="queueNameByType"
        option-value="queue.id"
        :allow-empty="false"
        v-model="selectedQueueId"
        :options="queueOptions"
      ></SelectButton>
    </div>
    <!-- <Button @click="backToQueues" icon="pi pi-arrow-left"></Button>

      <MessagesRequeue :queue-name="props.queueName" :selected-queue-id="selectedQueueId" />

      <SelectButton
        class="ms-auto"
        option-label="queueNameByType"
        option-value="queue.id"
        :allow-empty="false"
        v-model="selectedQueueId"
        :options="queueOptions"
      ></SelectButton> -->
    <!-- <Paginator
        class="ms-auto"
        @page="changePage"
        :rows="50"
        :always-show="false"
        :total-records="paginatedMessages?.totalCount"
      ></Paginator> -->
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
            <Column field="message.message_type" header="URN" class="whitespace-nowrap"></Column>
            <Column field="priority" header="Priority" class="w-0 whitespace-nowrap"></Column>
            <Column field="enqueue_time" header="Enqueue Time" class="w-0 whitespace-nowrap"></Column>
          </DataTable>
        </div>
        <div v-if="selectedMessage" class="flex basis-1/2 flex-col overflow-auto border-t">
          <div class="flex items-center justify-between">
            Message #{{ selectedMessage?.message_delivery_id }}
            <Button text size="small" icon="pi pi-times" @click="toggleMessage(undefined)"></Button>
          </div>
          <div class="overflow-auto whitespace-pre">
            <div class="px-3 pb-3" v-html="highlightJson(selectedMessage)"></div>
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
