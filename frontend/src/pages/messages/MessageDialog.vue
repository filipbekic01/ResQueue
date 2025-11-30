<script lang="ts" setup>
import { computed, onBeforeUnmount, onMounted, ref } from "vue";
import { useJobStateQuery } from "@/api/jobs/jobStateQuery";
import { useSingleMessageQuery } from "@/api/messages/singleMessageQuery";
import ClockIcon from "@/components/icons/ClockIcon.vue";
import CopyIcon from "@/components/icons/CopyIcon.vue";
import XMarkIcon from "@/components/icons/XMarkIcon.vue";
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
      <div class="absolute end-0 top-0 p-6">
        <button class="btn btn-ghost btn-sm btn-circle" @click="emit('close')">
          <XMarkIcon class="h-4 w-4" />
        </button>
      </div>
      <div class="border-base-200 dark:border-base-content/10 border-b px-8 pt-8 pb-6">
        <div class="text-base-content/50 mb-1 text-sm">
          {{ humanDateTime(displayedMessage.message?.sentTime) }}
        </div>
        <div class="flex items-center gap-2.5">
          <span class="text-base-content text-xl font-semibold">{{
            displayedMessage.message?.messageType.replace("urn:message:", "")
          }}</span>
        </div>
        <div class="mt-5 flex flex-wrap gap-x-8 gap-y-3">
          <div>
            <div class="text-base-content/50 text-xs font-medium tracking-wide uppercase">Machine</div>
            <div class="text-base-content/70 text-sm">
              {{ displayedMessage.message?.host?.machineName }}
            </div>
          </div>
          <div>
            <div class="text-base-content/50 text-xs font-medium tracking-wide uppercase">Process Name</div>
            <div class="text-base-content/70 text-sm">
              {{ displayedMessage.message?.host?.processName }}
            </div>
          </div>
          <div>
            <div class="text-base-content/50 text-xs font-medium tracking-wide uppercase">PID</div>
            <div class="text-base-content/70 text-sm">
              {{ displayedMessage.message?.host?.processId }}
            </div>
          </div>
          <div>
            <div class="text-base-content/50 text-xs font-medium tracking-wide uppercase">Assembly</div>
            <div class="text-base-content/70 text-sm">
              {{ displayedMessage.message?.host?.assembly }} ({{ displayedMessage.message?.host?.assemblyVersion }})
            </div>
          </div>
          <div>
            <div class="text-base-content/50 text-xs font-medium tracking-wide uppercase">Framework</div>
            <div class="text-base-content/70 text-sm">
              {{ displayedMessage.message?.host?.frameworkVersion }}
            </div>
          </div>
          <div>
            <div class="text-base-content/50 text-xs font-medium tracking-wide uppercase">MassTransit</div>
            <div class="text-base-content/70 text-sm">
              {{ displayedMessage.message?.host?.massTransitVersion }}
            </div>
          </div>
          <div>
            <div class="text-base-content/50 text-xs font-medium tracking-wide uppercase">OS</div>
            <div class="text-base-content/70 text-sm">
              {{ displayedMessage.message?.host?.operatingSystemVersion }}
            </div>
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
                <div v-if="job" class="text-sm">
                  <span class="text-base-content/70"
                    >The message belongs to the {{ job.isRecurring ? "recurring" : "" }} job —</span
                  >
                  <span
                    @click="jobStatePopoverOpen = !jobStatePopoverOpen"
                    class="text-primary cursor-pointer hover:underline"
                    >click for details.</span
                  >
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
              <MessageBlock name="Last Delivered" :value="displayedMessage.lastDelivered" />
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
              <MessageBlock name="Sent Time" :value="displayedMessage.message?.sentTime" />
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
              <!-- Job State Popover/Modal -->
              <div
                v-if="jobStatePopoverOpen && job"
                class="bg-base-100 border-base-200 dark:border-base-content/10 absolute top-6 left-6 z-50 flex flex-col gap-3 rounded-xl border p-5 shadow-xl"
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
