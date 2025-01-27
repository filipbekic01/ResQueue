<script lang="ts" setup>
import { useRequeueMessagesMutation } from '@/api/messages/requeueMessagesMutation'
import { useRequeueSpecificMessagesMutation } from '@/api/messages/requeueSpecificMessagesMutation'
import { useQueue } from '@/composables/queueComposable'
import { errorToToast } from '@/utils/errorUtils'
import Button from 'primevue/button'
import Checkbox from 'primevue/checkbox'
import InputNumber from 'primevue/inputnumber'
import Select from 'primevue/select'
import { useToast } from 'primevue/usetoast'
import { computed, ref, watchEffect } from 'vue'
import { useRoute } from 'vue-router'

const props = defineProps<{
  selectedQueueId: number
  deliveryMessageIds: number[]
  batch: boolean
}>()

const emit = defineEmits<{
  (e: 'requeue:complete'): void
}>()

const toast = useToast()
const route = useRoute()

const { mutateAsync: requeueMessagesAsync } = useRequeueMessagesMutation()
const { mutateAsync: requeueSpecificMessagesAsync } = useRequeueSpecificMessagesMutation()
const {
  query: { data: queues },
  queueOptions,
} = useQueue(computed(() => route.params.queueName.toString()))

const selectedQueue = computed(() => queues.value?.find((x) => x.id === props.selectedQueueId))

const requeueMessageCount = ref(0)
const requeueRedeliveryCount = ref(10)
const requeueDelay = ref(0)
const requeueTargetQueueId = ref<number>()
const requeueTargetQueue = computed(() =>
  queues.value?.find((x) => x.id === requeueTargetQueueId.value),
)
const requeueTargetQueueOptions = computed(() =>
  queueOptions.value.filter((x) => x.queue.id !== props.selectedQueueId),
)
const requeueTransactional = ref(false)

watchEffect(() => {
  requeueTargetQueueId.value = requeueTargetQueueOptions.value.find((x) => x)?.queue.id
})

const requeueMessages = () => {
  if (!selectedQueue.value || !requeueTargetQueue.value) {
    return
  }

  if (props.batch) {
    requeueMessagesAsync({
      queueName: selectedQueue.value.name,
      sourceQueueType: selectedQueue.value.type,
      targetQueueType: requeueTargetQueue.value?.type,
      messageCount: requeueMessageCount.value,
      redeliveryCount: requeueRedeliveryCount.value,
      delay: requeueDelay.value,
    })
      .then(() => {
        emit('requeue:complete')

        toast.add({
          severity: 'success',
          summary: 'Batch Requeue Completed',
          detail: `Messages requeued to destination.`,
          life: 3000,
        })
      })
      .catch((e) => toast.add(errorToToast(e)))
  } else {
    requeueSpecificMessagesAsync({
      messageDeliveryIds: props.deliveryMessageIds,
      targetQueueType: requeueTargetQueue.value?.type,
      redeliveryCount: requeueRedeliveryCount.value,
      delay: requeueDelay.value,
      transactional: requeueTransactional.value,
    })
      .then(() => {
        emit('requeue:complete')

        toast.add({
          severity: 'success',
          summary: 'Requeue Completed',
          detail: `Messages requeued to destination.`,
          life: 3000,
        })
      })
      .catch((e) => toast.add(errorToToast(e)))
  }
}
</script>

<template>
  <div class="flex flex-col gap-3">
    <div v-if="batch" class="flex flex-col gap-1">
      <label for="requeue-message-count" class="flex">Message count</label>
      <InputNumber
        :invalid="requeueMessageCount <= 0"
        id="requeue-message-count"
        v-model="requeueMessageCount"
        aria-describedby="requeue-message-count-help"
      ></InputNumber>
      <small id="requeue-message-count-help">Takes first N messages from the top.</small>
    </div>
    <div class="flex flex-col gap-1">
      <label>Destination</label>
      <Select
        v-model="requeueTargetQueueId"
        :options="requeueTargetQueueOptions"
        option-label="queueNameByType"
        option-value="queue.id"
      ></Select>
    </div>

    <div class="flex flex-col gap-1">
      <label>Delay in seconds</label>
      <InputNumber :step="1" v-model="requeueDelay"></InputNumber>
    </div>
    <div class="flex flex-col gap-1">
      <label>Redelivery count</label>
      <InputNumber :step="1" v-model="requeueRedeliveryCount"></InputNumber>
    </div>

    <div v-if="!batch" class="flex items-center gap-2">
      <Checkbox id="transactional" v-model="requeueTransactional" binary></Checkbox>
      <label for="transactional">Within single transaction</label>
    </div>
    <div v-else>Batch requeue uses single transaction.</div>

    <Button
      @click="requeueMessages"
      icon="pi pi-arrow-right"
      icon-pos="right"
      label="Requeue"
    ></Button>
  </div>
</template>
