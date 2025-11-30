<script lang="ts" setup>
import { computed, ref, watchEffect } from "vue";
import { useRoute } from "vue-router";
import { useRequeueMessagesMutation } from "@/api/messages/requeueMessagesMutation";
import { useRequeueSpecificMessagesMutation } from "@/api/messages/requeueSpecificMessagesMutation";
import ArrowRightIcon from "@/components/icons/ArrowRightIcon.vue";
import CheckCircleIcon from "@/components/icons/CheckCircleIcon.vue";
import ExclamationCircleIcon from "@/components/icons/ExclamationCircleIcon.vue";
import XCircleIcon from "@/components/icons/XCircleIcon.vue";
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

const getQueueIcon = (type: number) => {
  if (type === 1) return CheckCircleIcon;
  if (type === 2) return ExclamationCircleIcon;
  if (type === 3) return XCircleIcon;
  return CheckCircleIcon;
};

const getQueueLabel = (type: number) => {
  if (type === 1) return "Ready";
  if (type === 2) return "Error";
  if (type === 3) return "Dead-Letter";
  return "Unknown";
};

const getQueueColorClasses = (type: number, isSelected: boolean) => {
  const baseClasses = {
    1: isSelected
      ? "bg-success/15 text-success ring-2 ring-success/40"
      : "bg-success/5 text-success/60 hover:bg-success/10 hover:text-success",
    2: isSelected
      ? "bg-error/15 text-error ring-2 ring-error/40"
      : "bg-error/5 text-error/60 hover:bg-error/10 hover:text-error",
    3: isSelected
      ? "bg-base-content/15 text-base-content ring-2 ring-base-content/40"
      : "bg-base-content/5 text-base-content/60 hover:bg-base-content/10 hover:text-base-content",
  };
  return baseClasses[type as keyof typeof baseClasses] || "";
};

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
  <div class="flex flex-col gap-4">
    <!-- Transfer Flow Visualization -->
    <div class="flex flex-col gap-3">
      <!-- Source Row -->
      <div
        v-if="selectedQueue"
        class="flex items-center justify-center gap-2 rounded-lg px-3 py-2"
        :class="getQueueColorClasses(selectedQueue.type, true)"
      >
        <component :is="getQueueIcon(selectedQueue.type)" class="h-4 w-4" />
        <span class="text-sm font-medium">{{ getQueueLabel(selectedQueue.type) }}</span>
      </div>

      <!-- Arrow with message count -->
      <div class="text-base-content/30 flex items-center justify-center gap-2">
        <div class="bg-base-content/10 h-px flex-1"></div>
        <div class="flex items-center gap-1.5">
          <ArrowRightIcon class="h-4 w-4 rotate-90" />
          <span
            v-if="batch && requeueMessageCount > 0"
            class="bg-base-200 text-base-content/60 rounded-full px-2 py-0.5 text-xs font-medium"
          >
            {{ requeueMessageCount }} msg
          </span>
          <span
            v-else-if="!batch && deliveryMessageIds.length > 0"
            class="bg-base-200 text-base-content/60 rounded-full px-2 py-0.5 text-xs font-medium"
          >
            {{ deliveryMessageIds.length }} msg
          </span>
        </div>
        <div class="bg-base-content/10 h-px flex-1"></div>
      </div>

      <!-- Destination Row -->
      <div class="flex gap-2">
        <button
          v-for="option in requeueTargetQueueOptions"
          :key="option.queue.id"
          class="flex flex-1 cursor-pointer items-center justify-center gap-2 rounded-lg px-3 py-2 transition-all"
          :class="getQueueColorClasses(option.queue.type, requeueTargetQueueId === option.queue.id)"
          @click="requeueTargetQueueId = option.queue.id"
        >
          <component :is="getQueueIcon(option.queue.type)" class="h-4 w-4" />
          <span class="text-sm font-medium">{{ getQueueLabel(option.queue.type) }}</span>
        </button>
      </div>
    </div>

    <!-- Divider -->
    <div class="border-base-200 dark:border-base-content/10 border-t"></div>

    <!-- Options Section -->
    <div class="flex flex-col gap-3">
      <span class="text-base-content/50 text-xs font-medium tracking-wide uppercase">Options</span>

      <!-- Message Count (Batch only) -->
      <div v-if="batch" class="flex flex-col gap-1">
        <label for="requeue-message-count" class="text-base-content/70 flex text-sm">Message count</label>
        <input
          type="number"
          id="requeue-message-count"
          v-model.number="requeueMessageCount"
          class="input input-bordered input-sm w-full"
          :class="{ 'input-error': requeueMessageCount <= 0 }"
          placeholder="Number of messages"
        />
        <small class="text-base-content/50 text-xs">Takes first N messages from the top</small>
      </div>

      <!-- Delay and Redelivery in a row -->
      <div class="grid grid-cols-2 gap-3">
        <div class="flex flex-col gap-1">
          <label class="text-base-content/70 text-sm">Delay (sec)</label>
          <input
            type="number"
            v-model.number="requeueDelay"
            class="input input-bordered input-sm w-full"
            step="1"
            min="0"
            placeholder="0"
          />
        </div>
        <div class="flex flex-col gap-1">
          <label class="text-base-content/70 text-sm">Redelivery count</label>
          <input
            type="number"
            v-model.number="requeueRedeliveryCount"
            class="input input-bordered input-sm w-full"
            step="1"
            min="0"
            placeholder="10"
          />
        </div>
      </div>

      <!-- Transaction option -->
      <div v-if="!batch" class="flex items-center gap-2">
        <input type="checkbox" id="transactional" v-model="requeueTransactional" class="checkbox checkbox-xs" />
        <label for="transactional" class="text-base-content/70 cursor-pointer text-sm">Within single transaction</label>
      </div>
      <div v-else class="text-base-content/50 text-xs italic">Batch requeue uses single transaction</div>
    </div>

    <!-- Requeue Button -->
    <button
      class="btn btn-primary btn-sm w-full"
      :disabled="!requeueTargetQueueId || (batch && requeueMessageCount <= 0)"
      @click="requeueMessages"
    >
      <ArrowRightIcon class="h-4 w-4" />
      Requeue to {{ requeueTargetQueue ? getQueueLabel(requeueTargetQueue.type) : "..." }}
    </button>
  </div>
</template>
