<script lang="ts" setup>
import { useRequeueMessagesMutation } from '@/api/messages/requeueMessagesMutation'
import { useQueues } from '@/composables/queueComposable'
import { errorToToast } from '@/utils/errorUtils'
import Button from 'primevue/button'
import ButtonGroup from 'primevue/buttongroup'
import InputGroup from 'primevue/inputgroup'
import InputNumber from 'primevue/inputnumber'
import InputText from 'primevue/inputtext'
import Popover from 'primevue/popover'
import Select from 'primevue/select'
import Tab from 'primevue/tab'
import Tabs from 'primevue/tabs'
import ToggleButton from 'primevue/togglebutton'
import { useConfirm } from 'primevue/useconfirm'
import { useToast } from 'primevue/usetoast'
import { computed, ref, watchEffect } from 'vue'

const props = defineProps<{
  queueName: string
  selectedQueueId?: number
}>()

const confirm = useConfirm()
const toast = useToast()

const { mutateAsync: requeueMessagesAsync, isPending: isRequeueMessagesPending } = useRequeueMessagesMutation()
const {
  query: { data: queues },
  queueOptions
} = useQueues(computed(() => props.queueName))

const selectedQueue = computed(() => queues.value?.find((x) => x.id === props.selectedQueueId))

const requeuePopover = ref()

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
</script>

<template>
  <Button
    :loading="isRequeueMessagesPending"
    label="Requeue"
    @click="(e) => requeuePopover.toggle(e)"
    icon="pi pi-replay"
  ></Button>
  <Popover ref="requeuePopover" class="w-72">
    <div class="flex flex-col gap-3">
      <div class="flex flex-col gap-1">
        <label class="flex">Message count<span class="ms-auto text-blue-500">bulk mode</span></label>
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
  </Popover>
</template>
