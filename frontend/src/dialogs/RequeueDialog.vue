<script lang="ts" setup>
import { computed, ref, watchEffect } from "vue";
import { useRoute } from "vue-router";
import { useRequeueMessagesMutation } from "@/api/messages/requeueMessagesMutation";
import { useRequeueSpecificMessagesMutation } from "@/api/messages/requeueSpecificMessagesMutation";
import { useQueue } from "@/composables/queueComposable";
import { useToast } from "@/composables/useToast";
import { errorToToast } from "@/utils/errorUtils";

const props = defineProps<{
  selectedQueueId: number;
  deliveryMessageIds: number[];
  batch: boolean;
}>();

const emit = defineEmits<{
  (e: "requeue:complete"): void;
}>();

const toast = useToast();
const route = useRoute();

const { mutateAsync: requeueMessagesAsync } = useRequeueMessagesMutation();
const { mutateAsync: requeueSpecificMessagesAsync } = useRequeueSpecificMessagesMutation();
const {
  query: { data: queues },
  queueOptions,
} = useQueue(computed(() => route.params.queueName?.toString() ?? ""));

const selectedQueue = computed(() => queues.value?.find((x) => x.id === props.selectedQueueId));

const requeueMessageCount = ref(0);
const requeueRedeliveryCount = ref(10);
const requeueDelay = ref(0);
const requeueTargetQueueId = ref<number>();
const requeueTargetQueue = computed(() => queues.value?.find((x) => x.id === requeueTargetQueueId.value));
const requeueTargetQueueOptions = computed(() =>
  queueOptions.value.filter((x) => x.queue.id !== props.selectedQueueId),
);
const requeueTransactional = ref(false);

watchEffect(() => {
  requeueTargetQueueId.value = requeueTargetQueueOptions.value.find((x) => x)?.queue.id;
});

const requeueMessages = () => {
  if (!selectedQueue.value || !requeueTargetQueue.value) {
    return;
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
        emit("requeue:complete");

        toast.success("Messages requeued to destination.");
      })
      .catch((e) => {
        const err = errorToToast(e);
        toast.error(err.detail);
      });
  } else {
    requeueSpecificMessagesAsync({
      messageDeliveryIds: props.deliveryMessageIds,
      targetQueueType: requeueTargetQueue.value?.type,
      redeliveryCount: requeueRedeliveryCount.value,
      delay: requeueDelay.value,
      transactional: requeueTransactional.value,
    })
      .then(() => {
        emit("requeue:complete");

        toast.success("Messages requeued to destination.");
      })
      .catch((e) => {
        const err = errorToToast(e);
        toast.error(err.detail);
      });
  }
};
</script>

<template>
  <div class="flex flex-col gap-3">
    <div v-if="batch" class="flex flex-col gap-1">
      <label for="requeue-message-count" class="flex">Message count</label>
      <input
        type="number"
        id="requeue-message-count"
        v-model.number="requeueMessageCount"
        class="input input-bordered w-full"
        :class="{ 'input-error': requeueMessageCount <= 0 }"
        aria-describedby="requeue-message-count-help"
      />
      <small id="requeue-message-count-help" class="text-base-content/60">Takes first N messages from the top.</small>
    </div>
    <div class="flex flex-col gap-1">
      <label>Destination</label>
      <select v-model="requeueTargetQueueId" class="select select-bordered w-full">
        <option v-for="option in requeueTargetQueueOptions" :key="option.queue.id" :value="option.queue.id">
          {{ option.queueNameByType }}
        </option>
      </select>
    </div>

    <div class="flex flex-col gap-1">
      <label>Delay in seconds</label>
      <input type="number" v-model.number="requeueDelay" class="input input-bordered w-full" step="1" />
    </div>
    <div class="flex flex-col gap-1">
      <label>Redelivery count</label>
      <input type="number" v-model.number="requeueRedeliveryCount" class="input input-bordered w-full" step="1" />
    </div>

    <div v-if="!batch" class="flex items-center gap-2">
      <input type="checkbox" id="transactional" v-model="requeueTransactional" class="checkbox checkbox-sm" />
      <label for="transactional">Within single transaction</label>
    </div>
    <div v-else class="text-base-content/60">Batch requeue uses single transaction.</div>

    <button class="btn btn-primary" @click="requeueMessages">
      Requeue
      <i class="pi pi-arrow-right"></i>
    </button>
  </div>
</template>
