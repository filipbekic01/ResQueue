<script lang="ts" setup>
import { useRequeueMessagesMutation } from '@/api/messages/requeueMessagesMutation'
import { useRequeueSpecificMessagesMutation } from '@/api/messages/requeueSpecificMessagesMutation'
import { useQueue } from '@/composables/queueComposable'
import { errorToToast } from '@/utils/errorUtils'
import Button from 'primevue/button'
import InputNumber from 'primevue/inputnumber'
import InputText from 'primevue/inputtext'
import Select from 'primevue/select'
import { useToast } from 'primevue/usetoast'
import { computed, ref, watchEffect } from 'vue'
import { useRoute } from 'vue-router'

const props = defineProps<{
  selectedQueueId: number
  deliveryMessageIds: number[]
  batch: boolean
}>()

// const dialogRef = inject<Ref<DynamicDialogOptions>>('dialogRef')
// const dialogData = computed((): RequeueDialogData => dialogRef?.value.data)

const toast = useToast()
const route = useRoute()

const { mutateAsync: requeueMessagesAsync } = useRequeueMessagesMutation()
const { mutateAsync: requeueSpecificMessagesAsync } = useRequeueSpecificMessagesMutation()
const {
  query: { data: queues },
  queueOptions
} = useQueue(computed(() => route.params.queueName.toString()))

const selectedQueue = computed(() => queues.value?.find((x) => x.id === props.selectedQueueId))

const requeueMessageCount = ref(0)
const requeueRedeliveryCount = ref(10)
const requeueDelay = ref('0 seconds')
const requeueTargetQueueId = ref<number>()
const requeueTargetQueue = computed(() => queues.value?.find((x) => x.id === requeueTargetQueueId.value))
const requeueTargetQueueOptions = computed(() => queueOptions.value.filter((x) => x.queue.id !== props.selectedQueueId))

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
      delay: requeueDelay.value
    })
      .then(() => {
        toast.add({
          severity: 'success',
          summary: 'Batch Requeue Completed',
          detail: `Messages requeued to destination.`,
          life: 3000
        })
      })
      .catch((e) => toast.add(errorToToast(e)))
  } else {
    requeueSpecificMessagesAsync({
      messageDeliveryIds: props.deliveryMessageIds,
      targetQueueType: requeueTargetQueue.value?.type,
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
  }
}
</script>

<template>
  <div class="flex flex-col gap-3">
    <div v-if="batch" class="flex flex-col gap-1">
      <label class="flex">Message count</label>
      <InputNumber name="requeueMessageCount" v-model="requeueMessageCount"></InputNumber>
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
      <label>Delay</label>
      <InputText v-model="requeueDelay"></InputText>
    </div>
    <div class="flex flex-col gap-1">
      <label>Redelivery count (default 10)</label>
      <InputNumber v-model="requeueRedeliveryCount"></InputNumber>
    </div>

    <Button @click="requeueMessages" icon="pi pi-arrow-right" icon-pos="right" label="Requeue"></Button>
  </div>
</template>
