<script lang="ts" setup>
import { computed, onBeforeUnmount, onMounted, ref } from "vue";
import { useJobStateQuery } from "@/api/jobs/jobStateQuery";
import { useSingleMessageQuery } from "@/api/messages/singleMessageQuery";
import ClockIcon from "@/components/icons/ClockIcon.vue";
import CopyIcon from "@/components/icons/CopyIcon.vue";
import { useJson } from "@/composables/jsonComposable";
import type { MessageDeliveryDto } from "@/dtos/message/messageDeliveryDto";
import { humanDateTime } from "@/utils/dateTimeUtil";
import MessageBlock from "./MessageBlock.vue";
import MessageDialogError from "./MessageDialogError.vue";
import MessageHeader from "./MessageHeader.vue";

const copyToClipboard = async (data: unknown) => {
  const text = typeof data === "string" ? data : JSON.stringify(data, null, 2);
  await navigator.clipboard.writeText(text);
};

const props = defineProps<{
  selectedMessage: MessageDeliveryDto;
}>();

const emit = defineEmits<{
  (e: "close"): void;
}>();

const { highlightJson } = useJson();

const body = computed(() => {
  if (!props.selectedMessage.message) {
    return undefined;
  }

  try {
    const content = JSON.parse(props.selectedMessage.message.body);
    return content;
  } catch (e) {
    console.error(e);
    return undefined;
  }
});
const { data: job } = useJobStateQuery(body.value["jobId"]);
const { data: fetchedMessage } = useSingleMessageQuery(props.selectedMessage.transportMessageId);

const displayedMessage = computed<MessageDeliveryDto>(() => fetchedMessage.value ?? props.selectedMessage);
const hasAdditionalData = computed(() => Object.keys(displayedMessage.value.additionalData).length > 0);

const transportHeadersTrimmed = computed(() => {
  const th = { ...displayedMessage.value.transportHeaders };

  if (th["MT-Fault-StackTrace"]?.length > 30) {
    th["MT-Fault-StackTrace"] = `${th["MT-Fault-StackTrace"].slice(0, 30)}...`;
  }

  return th;
});

const handleEscKey = (event: KeyboardEvent) => {
  if (event.key === "Escape") {
    emit("close");
  }
};

onMounted(() => {
  window.addEventListener("keydown", handleEscKey);
});

onBeforeUnmount(() => {
  window.removeEventListener("keydown", handleEscKey);
});

const jobStatePopoverOpen = ref(false);
</script>
<template>
  <div
    class="click animate-fadein animate-duration-75 absolute start-0 top-0 z-40 h-full w-full backdrop-brightness-50"
    @click="emit('close')"
  ></div>

  <div
    class="bg-base-100 absolute end-0 bottom-0 z-50 mx-auto flex h-full w-[90%] flex-col overflow-auto rounded-s-xl shadow-2xl"
  >
    <div class="flex h-full flex-col overflow-hidden">
      <div class="border-base-200 dark:border-base-content/10 border-b px-8 pt-8 pb-6">
        <!-- Message Type & Status Badge -->
        <div class="mb-4 flex items-start justify-between gap-4">
          <div class="min-w-0 flex-1">
            <div class="text-base-content truncate text-xl font-semibold">
              {{ displayedMessage.message?.messageType.replace("urn:message:", "") }}
            </div>
            <div class="text-base-content/50 mt-1 text-sm">
              {{ displayedMessage.message?.sourceAddress }}
            </div>
          </div>
          <div class="flex shrink-0 items-center gap-2">
            <!-- Message State Badge -->
            <span
              v-if="displayedMessage.transportHeaders?.['MT-Reason'] === 'fault'"
              class="bg-error/10 text-error inline-flex items-center gap-1.5 rounded-full px-3 py-1 text-xs font-medium"
            >
              <span class="bg-error h-1.5 w-1.5 rounded-full"></span>
              Faulted
            </span>
            <span
              v-else-if="displayedMessage.deliveryCount >= displayedMessage.maxDeliveryCount"
              class="bg-error/10 text-error inline-flex items-center gap-1.5 rounded-full px-3 py-1 text-xs font-medium"
            >
              <span class="bg-error h-1.5 w-1.5 rounded-full"></span>
              Dead Letter
            </span>
            <span
              v-else
              class="bg-success/10 text-success inline-flex items-center gap-1.5 rounded-full px-3 py-1 text-xs font-medium"
            >
              <span class="bg-success h-1.5 w-1.5 rounded-full"></span>
              Ready
            </span>
            <!-- Scheduled indicator -->
            <span
              v-if="displayedMessage.message?.schedulingTokenId"
              class="bg-info/10 text-info inline-flex items-center gap-1.5 rounded-full px-3 py-1 text-xs font-medium"
            >
              <ClockIcon class="h-3 w-3" />
              Scheduled
            </span>
          </div>
        </div>

        <!-- Key Metrics Row -->
        <div
          class="bg-base-200/50 divide-base-300 dark:divide-base-content/10 -mx-2 flex flex-wrap items-stretch divide-x rounded-lg"
        >
          <div class="flex min-w-0 flex-1 flex-col px-4 py-3">
            <span class="text-base-content/50 text-xs font-medium tracking-wide uppercase">Sent</span>
            <span class="text-base-content mt-0.5 text-sm font-medium">{{
              humanDateTime(displayedMessage.message?.sentTime)
            }}</span>
          </div>
          <div class="flex min-w-0 flex-1 flex-col px-4 py-3">
            <span class="text-base-content/50 text-xs font-medium tracking-wide uppercase">Enqueued</span>
            <span class="text-base-content mt-0.5 text-sm font-medium">{{
              humanDateTime(displayedMessage.enqueueTime)
            }}</span>
          </div>
          <div class="flex min-w-0 flex-1 flex-col px-4 py-3">
            <span class="text-base-content/50 text-xs font-medium tracking-wide uppercase">Last Delivered</span>
            <span class="text-base-content mt-0.5 text-sm font-medium">{{
              humanDateTime(displayedMessage.lastDelivered) || "-"
            }}</span>
          </div>
          <div class="flex min-w-0 flex-1 flex-col px-4 py-3">
            <span class="text-base-content/50 text-xs font-medium tracking-wide uppercase">Delivery</span>
            <span
              class="mt-0.5 text-sm font-medium"
              :class="displayedMessage.deliveryCount > 1 ? 'text-warning' : 'text-base-content'"
            >
              {{ displayedMessage.deliveryCount }} / {{ displayedMessage.maxDeliveryCount }}
              <span v-if="displayedMessage.deliveryCount > 1" class="text-warning/70 text-xs font-normal"
                >(retried)</span
              >
            </span>
          </div>
        </div>
      </div>

      <div class="flex grow flex-col overflow-auto">
        <div class="flex shrink grow basis-1/2 overflow-auto">
          <div class="border-base-300 dark:border-base-content/20 flex w-[45%] flex-col overflow-auto border-e">
            <div
              class="bg-base-100 border-base-200 dark:border-base-content/10 sticky top-0 z-10 flex flex-col gap-2 border-b px-6 py-4"
              v-if="displayedMessage.message?.schedulingTokenId || job"
            >
              <div class="bg-info/5 -mx-6 -my-4 flex flex-col gap-2 px-6 py-4">
                <div
                  v-if="displayedMessage.message?.schedulingTokenId"
                  class="text-info flex items-center gap-2 text-sm"
                >
                  <ClockIcon class="h-4 w-4" />Scheduled Message
                </div>
                <div v-if="job" class="relative text-sm">
                  <span class="text-base-content/70"
                    >The message belongs to the {{ job.isRecurring ? "recurring" : "" }} job —
                  </span>
                  <span
                    @click="jobStatePopoverOpen = !jobStatePopoverOpen"
                    class="text-info cursor-pointer italic hover:underline"
                    >click for details.</span
                  >
                  <!-- Job State Popover Backdrop -->
                  <div v-if="jobStatePopoverOpen" class="fixed inset-0 z-40" @click="jobStatePopoverOpen = false"></div>
                  <!-- Job State Popover -->
                  <div
                    v-if="jobStatePopoverOpen"
                    class="bg-base-100 border-base-200 dark:border-base-content/10 absolute top-full left-0 z-50 mt-2 flex flex-col gap-3 rounded-xl border p-5 shadow-xl"
                  >
                    <button
                      class="btn btn-ghost btn-xs btn-circle absolute top-3 right-3"
                      @click="jobStatePopoverOpen = false"
                    >
                      ✕
                    </button>
                    <MessageHeader name="Job State" />
                    <MessageBlock name="Job ID" :value="job.jobId" />
                    <MessageBlock name="Submitted">
                      {{ humanDateTime(job.submitted) }}
                    </MessageBlock>
                    <MessageBlock name="Started" :value="job.started">
                      {{ humanDateTime(job.started) }}
                    </MessageBlock>
                    <MessageBlock name="Completed" :value="job.completed">
                      {{ humanDateTime(job.completed) }}
                    </MessageBlock>
                    <MessageBlock name="Duration" :value="job.duration" />
                    <MessageBlock name="Faulted" :value="job.faulted" />
                    <MessageBlock name="Reason" :value="job.reason" />
                    <MessageBlock name="Last Retry Attempt" :value="job.lastRetryAttempt" />
                    <MessageBlock name="Current State" :value="job.currentState" />
                    <MessageBlock name="Progress Value" :value="job.progressValue" />
                    <MessageBlock name="Progress Limit" :value="job.progressLimit" />
                    <MessageBlock name="Job State" :value="job.jobState" />
                    <MessageBlock name="Next Start Date" :value="job.nextStartDate">
                      {{ humanDateTime(job.nextStartDate) }}
                    </MessageBlock>
                    <MessageBlock name="Recurring" :value="job.isRecurring" />
                    <MessageBlock name="Start Date" :value="job.startDate" />
                    <MessageBlock name="End Date" :value="job.endDate" />
                  </div>
                </div>
              </div>
            </div>
            <template v-if="hasAdditionalData">
              <div class="border-base-200 dark:border-base-content/10 flex flex-col gap-3 border-b px-6 py-5">
                <MessageHeader name="Additional Data" />
                <template v-for="(value, key) in displayedMessage.additionalData" :key="key">
                  <MessageBlock :name="key">
                    <span v-html="value"></span>
                  </MessageBlock>
                </template>
              </div>
            </template>
            <div class="flex flex-col gap-3 px-6 py-5">
              <MessageHeader name="Delivery" />
              <MessageBlock name="Message Delivery ID" :value="displayedMessage.messageDeliveryId" />
              <MessageBlock name="Transport Message ID" :value="displayedMessage.transportMessageId" />
              <MessageBlock name="Queue ID" :value="displayedMessage.queueId" />
              <MessageBlock name="Priority" :value="displayedMessage.priority" />
              <MessageBlock name="Enqueue Time">
                {{ humanDateTime(displayedMessage.enqueueTime) }}
              </MessageBlock>
              <MessageBlock name="Expiration Time" :value="displayedMessage.expirationTime" />
              <MessageBlock name="Partition Key" :value="displayedMessage.partitionKey" />
              <MessageBlock name="Routing Key" :value="displayedMessage.routingKey" />
              <MessageBlock name="Consumer ID" :value="displayedMessage.consumerId" />
              <MessageBlock name="Lock ID" :value="displayedMessage.lockId" />
              <MessageBlock name="Delivery Count" :value="displayedMessage.deliveryCount" />
              <MessageBlock name="Max. Delivery Count" :value="displayedMessage.maxDeliveryCount" />
              <MessageBlock name="Last Delivered">
                {{ humanDateTime(displayedMessage.lastDelivered) }}
              </MessageBlock>
              <MessageBlock name="Transport Headers">
                <div class="relative">
                  <button
                    class="btn btn-ghost btn-xs btn-circle absolute top-2 right-2 z-10"
                    @click="copyToClipboard(displayedMessage.transportHeaders)"
                    title="Copy to clipboard"
                  >
                    <CopyIcon class="h-3.5 w-3.5" />
                  </button>
                  <div
                    class="bg-base-200/50 overflow-x-auto rounded-md p-3 pr-10 font-mono text-xs whitespace-pre"
                    v-html="highlightJson(transportHeadersTrimmed, true)"
                  ></div>
                </div>
              </MessageBlock>
            </div>
            <div class="border-base-200 dark:border-base-content/10 flex flex-col gap-3 border-t px-6 py-5">
              <MessageHeader name="Message" />
              <MessageBlock name="Transport Message ID" :value="displayedMessage.message?.transportMessageId" />
              <MessageBlock name="Content Type" :value="displayedMessage.message?.contentType" />
              <MessageBlock name="Message Type" :value="displayedMessage.message?.messageType" />
              <MessageBlock name="Message ID" :value="displayedMessage.message?.messageId" />
              <MessageBlock name="Correlation ID" :value="displayedMessage.message?.correlationId" />
              <MessageBlock name="Conversation ID" :value="displayedMessage.message?.conversationId" />
              <MessageBlock name="Request ID" :value="displayedMessage.message?.requestId" />
              <MessageBlock name="Initiator ID" :value="displayedMessage.message?.initiatorId" />
              <MessageBlock name="Scheduling Token ID" :value="displayedMessage.message?.schedulingTokenId" />
              <MessageBlock name="Source Address" :value="displayedMessage.message?.sourceAddress" />
              <MessageBlock name="Destination Address" :value="displayedMessage.message?.destinationAddress" />
              <MessageBlock name="Response Address" :value="displayedMessage.message?.responseAddress" />
              <MessageBlock name="Fault Address" :value="displayedMessage.message?.faultAddress" />
              <MessageBlock name="Sent Time">
                {{ humanDateTime(displayedMessage.message?.sentTime) }}
              </MessageBlock>
              <MessageBlock name="Headers">
                <div
                  class="bg-base-200/50 rounded-md p-3 font-mono text-xs whitespace-pre"
                  v-html="highlightJson(displayedMessage.message?.headers, true)"
                ></div>
              </MessageBlock>
              <MessageBlock name="Host">
                <div
                  class="bg-base-200/50 rounded-md p-3 font-mono text-xs whitespace-pre"
                  v-html="highlightJson(displayedMessage.message?.host, true)"
                ></div>
              </MessageBlock>
            </div>
          </div>
          <div class="flex w-[55%] flex-col overflow-auto">
            <div class="relative h-full p-6">
              <div class="absolute end-6 top-6 flex items-center gap-2">
                <button
                  class="btn btn-ghost btn-xs btn-circle"
                  @click="copyToClipboard(JSON.parse(displayedMessage.message?.body ?? '{}'))"
                  title="Copy to clipboard"
                >
                  <CopyIcon class="h-3.5 w-3.5" />
                </button>
                <span class="text-base-content/40 bg-base-200/50 rounded-md px-2 py-1 text-xs font-medium">{{
                  displayedMessage.message?.contentType
                }}</span>
              </div>

              <div
                class="text-base-content/70 grow font-mono text-sm leading-relaxed whitespace-pre"
                v-if="displayedMessage.message"
                v-html="highlightJson(JSON.parse(displayedMessage.message.body))"
              ></div>
            </div>
          </div>
        </div>
        <MessageDialogError
          v-if="displayedMessage.transportHeaders['MT-Reason'] == 'fault'"
          :selected-message="displayedMessage"
        />
      </div>
    </div>
  </div>
</template>
