<script lang="ts" setup>
import { useBrokersQuery } from '@/api/brokers/brokersQuery'
import { useMoveMessagesMutation } from '@/api/messages/moveMessagesMutation'
import { usePaginatedMessagesQuery } from '@/api/messages/paginatedMessagesQuery'
import { useRequeueMessagesMutation } from '@/api/messages/requeueMessagesMutation'
import { useQueuesQuery } from '@/api/queues/queuesQuery'
import { useQueuesViewQuery } from '@/api/queues/queuesViewQuery'
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
import InputNumber from 'primevue/inputnumber'
import InputText from 'primevue/inputtext'
import { useConfirm } from 'primevue/useconfirm'
import { useToast } from 'primevue/usetoast'
import { computed, ref, watch, watchEffect } from 'vue'
import { useRouter } from 'vue-router'

const props = defineProps<{
  brokerId: string
  queueName: string
}>()

// Utils
const router = useRouter()
const confirm = useConfirm()
const toast = useToast()

const pageIndex = ref(0)

const backToQueues = () => {
  router.push({
    name: 'queues',
    params: {
      brokerId: props.brokerId
    }
  })
}

// Broker
const { data: brokers } = useBrokersQuery()
const broker = computed(() => brokers.value?.find((x) => x.id === props.brokerId))

// Queues View
const { data: queuesView } = useQueuesViewQuery(computed(() => props.brokerId))

// Queues
const { data: queues } = useQueuesQuery(props.queueName)

const selectedQueueId = ref<number>()
const selectedQueue = computed(() => queues.value?.find((x) => x.id === selectedQueueId.value))

const getQueueName = (type: number) => {
  if (type === 1) {
    return 'Active Messages'
  } else if (type === 2) {
    return 'Error Messages'
  } else if (type === 3) {
    return 'Skipped Messages'
  } else {
    return 'Unknown'
  }
}

const queueOptions = computed(() => {
  if (!queues.value) {
    return []
  }

  return [...queues.value]
    .sort((a, b) => a.type - b.type)
    .map((qt) => ({
      label: getQueueName(qt.type),
      value: qt.id
    }))
})

watchEffect(() => {
  if (!queueOptions.value.length) {
    return
  }

  selectedQueueId.value = queueOptions.value.find((x) => x)?.value ?? undefined
})

// Messages
const { data: paginatedMessages, isPending } = usePaginatedMessagesQuery(
  computed(() => props.brokerId),
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

// Requeue messages
const { mutateAsync: requeueMessagesAsync, isPending: isRequeueMessagesPending } = useRequeueMessagesMutation()

const requeuePopover = ref()

const requeueMessageCount = ref(0)
const requeueRedeliveryCount = ref(10)
const requeueDelay = ref('0 seconds')
const requeueTargetQueueId = ref<number>()
const requeueTargetQueue = computed(() => queues.value?.find((x) => x.id === requeueTargetQueueId.value))
const requeueTargetQueueOptions = computed(() => queueOptions.value.filter((x) => x.value !== selectedQueue.value?.id))

watchEffect(() => {
  requeueTargetQueueId.value = requeueTargetQueueOptions.value.find((x) => x)?.value
})

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
      if (!selectedQueue.value || !requeueTargetQueue.value) {
        return
      }

      requeueMessagesAsync({
        queueName: selectedQueue.value.name,
        sourceQueueType: selectedQueue.value.type,
        targetQueueType: requeueTargetQueue.value?.type,
        messageCount: requeueMessageCount.value,
        redeliveryCount: requeueRedeliveryCount.value,
        delay: requeueDelay.value
      })
        .then(() => {
          toast.add({
            severity: 'success',
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

const movePopover = ref()

const moveQueueViewQueueName = ref(props.queueName)
const { data: moveQueues } = useQueuesQuery(moveQueueViewQueueName.value)
const moveQueueId = ref<number>()
const moveQueue = computed(() => moveQueues.value?.find((x) => x.id === moveQueueId.value))
const moveTransactional = ref(false)

watchEffect(() => {
  const sortedQueues = [...(moveQueues.value ?? [])].sort((a, b) => a.type - b.type)

  if (moveQueueViewQueueName.value === props.queueName) {
    moveQueueId.value = sortedQueues.filter((x) => x.type !== selectedQueue.value?.type).find((x) => x)?.id
  } else {
    moveQueueId.value = sortedQueues.find((x) => x)?.id
  }
})

const moveQueueOptions = computed(() => {
  if (!moveQueues.value) {
    return []
  }

  return [...moveQueues.value]
    .sort((a, b) => a.type - b.type)
    .map((qt) => ({
      label: getQueueName(qt.type),
      value: qt.id
    }))
})

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
      if (
        !moveQueue.value ||
        !moveQueueViewQueueName.value ||
        !selectedMessages.value ||
        !selectedMessages.value.length
      ) {
        return
      }

      const messagesCount = selectedMessages.value.length

      moveMessagesAsync({
        queueName: moveQueueViewQueueName.value,
        queueType: moveQueue.value.type,
        messages: selectedMessages.value.map((msg) => ({
          messageDeliveryId: msg.message_delivery_id,
          lockId: msg.lock_id,
          headers: msg.transport_headers
        }))
      })
        .then((resp) => {
          toast.add({
            severity: 'info',
            summary: 'Move Completed',
            detail: `Moved ${resp.succeededCount}/${messagesCount} to destination.`,
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
    <template #description>Queue: {{ props.queueName }}</template>
    <template #append>
      <Avatars v-if="broker" :user-ids="broker.accessList.map((x) => x.userId)" />
    </template>
    <div class="flex flex-wrap items-start gap-2 border-b px-4 py-2">
      <Button @click="backToQueues" outlined label="Queues" icon="pi pi-arrow-left"></Button>

      <!-- <Button
        outlined
        :loading="isMoveMessagesPending"
        label="Move"
        @click="(e) => movePopover.toggle(e)"
        icon="pi pi-sync"
      ></Button>
      <Popover ref="movePopover" class="w-72">
        <div class="flex flex-col gap-3">
          <div class="flex flex-col gap-1">
            <label>Target queue</label>
            <Select
              filter
              v-model="moveQueueViewQueueName"
              :options="queuesView"
              option-label="queueName"
              option-value="queueName"
            ></Select>
          </div>
          <div class="flex flex-col gap-1">
            <label>Target queue type</label>
            <Select
              v-model="moveQueueId"
              :options="moveQueueOptions"
              option-label="label"
              option-value="value"
            ></Select>
          </div>

          <div class="flex items-center gap-2">
            <Checkbox v-model="moveTransactional" binary />
            <label>In transaction</label>
          </div>

          <Button @click="moveMessages" icon="pi pi-arrow-right" icon-pos="right" label="Move"></Button>
        </div>
      </Popover> -->

      <Button
        outlined
        :loading="isRequeueMessagesPending"
        label="Requeue"
        @click="(e) => requeuePopover.toggle(e)"
        icon="pi pi-sync"
      ></Button>
      <Popover ref="requeuePopover" class="w-72">
        <div class="flex flex-col gap-3">
          <div class="flex flex-col gap-1">
            <label>Target queue</label>
            <Select
              v-model="requeueTargetQueueId"
              :options="requeueTargetQueueOptions"
              option-label="label"
              option-value="value"
            ></Select>
          </div>
          <div class="flex flex-col gap-1">
            <label>Message count</label>
            <InputNumber name="requeueMessageCount" v-model="requeueMessageCount"></InputNumber>
          </div>
          <div class="flex flex-col gap-1">
            <label>Delay (Postgres interval)</label>
            <InputText v-model="requeueDelay"></InputText>
          </div>
          <div class="flex flex-col gap-1">
            <label>Redelivery count (default 10)</label>
            <InputNumber v-model="requeueRedeliveryCount"></InputNumber>
          </div>

          <Button @click="requeueMessages" icon="pi pi-arrow-right" icon-pos="right" label="Requeue"></Button>
        </div>
      </Popover>
      <!-- <Button outlined label="Delete" icon="pi pi-trash"></Button> -->

      <Select
        class="ms-auto"
        option-label="label"
        option-value="value"
        :allow-empty="false"
        v-model="selectedQueueId"
        :options="queueOptions"
      ></Select>
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
