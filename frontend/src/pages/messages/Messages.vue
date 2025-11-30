<script lang="ts" setup>
import { useQueryClient } from "@tanstack/vue-query";
import { formatDistance, isFuture } from "date-fns";
import { computed, ref, watchEffect } from "vue";
import { useRoute, useRouter } from "vue-router";
import { useDeleteMessagesMutation } from "@/api/messages/deleteMessagesMutation";
import { useMessagesQuery } from "@/api/messages/messagesQuery";
import { usePurgeQueueMutation } from "@/api/queues/purgeQueueMutation";
import ArrowLeftIcon from "@/components/icons/ArrowLeftIcon.vue";
import CheckCircleIcon from "@/components/icons/CheckCircleIcon.vue";
import EraserIcon from "@/components/icons/EraserIcon.vue";
import ExclamationCircleIcon from "@/components/icons/ExclamationCircleIcon.vue";
import HourglassIcon from "@/components/icons/HourglassIcon.vue";
import RefreshIcon from "@/components/icons/RefreshIcon.vue";
import ReplayIcon from "@/components/icons/ReplayIcon.vue";
import TrashIcon from "@/components/icons/TrashIcon.vue";
import XCircleIcon from "@/components/icons/XCircleIcon.vue";
import ZapIcon from "@/components/icons/ZapIcon.vue";
import Pagination from "@/components/Pagination.vue";
import { useQueue } from "@/composables/queueComposable";
import { useConfirmDialog } from "@/composables/useConfirmDialog";
import { useLocalSettings } from "@/composables/useLocalSettings";
import { useToast } from "@/composables/useToast";
import RequeueDialog from "@/dialogs/RequeueDialog.vue";
import type { MessageDeliveryDto } from "@/dtos/message/messageDeliveryDto";
import type { QueueDto } from "@/dtos/queue/queueDto";
import AppLayout from "@/layouts/AppLayout.vue";
import Graph from "@/layouts/Graph.vue";
import { humanDateTime } from "@/utils/dateTimeUtil";
import { errorToToast } from "@/utils/errorUtils";
import MessageDialog from "./MessageDialog.vue";

const props = defineProps<{
  queueName: string;
}>();

const route = useRoute();
const router = useRouter();

const queryClient = useQueryClient();

const { confirm } = useConfirmDialog();
const toast = useToast();

const currentPage = ref(1);

const { refetchInterval } = useLocalSettings();

// Computed page index for API (0-based)
const pageIndex = computed(() => currentPage.value - 1);

// Queues
const {
  queueOptions,
  queryView: { data: queueView },
  query: { data: queues },
  primaryQueue,
  getQueueTypeLabel,
} = useQueue(computed(() => props.queueName));

const selectedQueueId = ref<number>();
const selectedQueue = computed(() => queues.value?.find((x) => x.id === selectedQueueId.value));

const updateSelectedQueue = (queue: QueueDto) => {
  selectedQueueId.value = queue.id;
  router.replace({ query: { ...route.query, queueType: queue.type.toString() } });
};

watchEffect(() => {
  if (selectedQueueId.value || !queueOptions.value.length || !queueView.value) {
    return;
  }

  const queueTypeFromUrl = route.query.queueType ? Number(route.query.queueType) : 1;

  selectedQueueId.value = queueOptions.value.find((x) => x.queue.type == queueTypeFromUrl)?.queue.id ?? undefined;

  // Sync URL if it doesn't have queueType
  if (!route.query.queueType && selectedQueueId.value) {
    const selectedQueue = queueOptions.value.find((x) => x.queue.id === selectedQueueId.value);
    if (selectedQueue) {
      router.replace({ query: { ...route.query, queueType: selectedQueue.queue.type.toString() } });
    }
  }
});

// Purge queue
const { mutateAsync: purgeQueueAsync, isPending: isPurgeQueuePending } = usePurgeQueueMutation();

// Messages
const {
  data: messages,
  refetch: refetchMessages,
  isPending,
} = useMessagesQuery(
  computed(() => selectedQueueId.value),
  pageIndex,
  refetchInterval,
);

const toggleMessage = (msg?: MessageDeliveryDto) => {
  if (!msg) {
    selectedMessageId.value = 0;
  } else if (selectedMessageId.value === msg.messageDeliveryId) {
    selectedMessageId.value = 0;
  } else {
    selectedMessageId.value = msg.messageDeliveryId;
  }
};

// Delete messages
const { mutateAsync: deleteMessagesAsync, isPending: isDeleteMessagesPending } = useDeleteMessagesMutation();
const deleteMessagesTransactional = ref(false);
const deleteMessagesDropdownOpen = ref(false);

const deleteMessages = () => {
  deleteMessagesAsync({
    messageDeliveryIds: selectedMessages.value.map((m) => m.messageDeliveryId),
    transactional: deleteMessagesTransactional.value,
  })
    .then(() => {
      onActionComplete();
      deleteMessagesDropdownOpen.value = false;
      toast.success("Messages delete procedure ran successfully.");
    })
    .catch((e) => {
      const err = errorToToast(e);
      toast.error(err.detail);
    });
};

// Selected messages
const selectedMessageId = ref<number>(24);
const selectedMessage = computed(() =>
  messages.value?.items.find((x) => x.messageDeliveryId === selectedMessageId.value),
);

const selectedMessages = ref<MessageDeliveryDto[]>([]);
const selectedMessageIds = computed(() =>
  selectedMessages.value?.length ? selectedMessages.value.map((x) => x.messageDeliveryId) : [],
);

// Shift selection
const lastSelectedIndex = ref<number | null>(null);

const handleCheckboxChange = (msg: MessageDeliveryDto, event: Event) => {
  const isChecked = (event.target as HTMLInputElement).checked;
  const currentIndex = messages.value?.items.findIndex((m) => m.messageDeliveryId === msg.messageDeliveryId) ?? -1;

  if ((event as MouseEvent).shiftKey && lastSelectedIndex.value !== null && messages.value?.items) {
    // Shift+click: select range
    const start = Math.min(lastSelectedIndex.value, currentIndex);
    const end = Math.max(lastSelectedIndex.value, currentIndex);
    const rangeItems = messages.value.items.slice(start, end + 1);

    if (isChecked) {
      // Add range to selection (avoiding duplicates)
      const newSelection = [...selectedMessages.value];
      for (const item of rangeItems) {
        if (!newSelection.some((m) => m.messageDeliveryId === item.messageDeliveryId)) {
          newSelection.push(item);
        }
      }
      selectedMessages.value = newSelection;
    } else {
      // Remove range from selection
      selectedMessages.value = selectedMessages.value.filter(
        (m) => !rangeItems.some((r) => r.messageDeliveryId === m.messageDeliveryId),
      );
    }
  } else {
    // Normal click: toggle single item
    if (isChecked) {
      selectedMessages.value = [...selectedMessages.value, msg];
    } else {
      selectedMessages.value = selectedMessages.value.filter((m) => m.messageDeliveryId !== msg.messageDeliveryId);
    }
  }

  lastSelectedIndex.value = currentIndex;
};

const requeuePopoverOpen = ref(false);
const requeueSpecificPopoverOpen = ref(false);

const isRefreshing = ref(false);

const goToQueues = () => {
  router.push({ name: "queues" });
};

const refreshQueue = () => {
  isRefreshing.value = true;
  refetchMessages()
    .then(() => {
      toast.success("The queue has been successfully updated.");
    })
    .finally(() => {
      setTimeout(() => {
        isRefreshing.value = false;
      }, 300);
    });
  queryClient.invalidateQueries({ queryKey: ["queue-view"] });
};

const purgeQueue = async () => {
  const confirmed = await confirm({
    type: "error",
    title: "Purge Queue",
    message: `This will permanently delete all messages in the ${getQueueTypeLabel(selectedQueue.value?.type)} queue. This action cannot be undone.`,
    confirmText: `Purge "${getQueueTypeLabel(selectedQueue.value?.type)}" queue`,
    cancelText: "Cancel",
  });

  if (confirmed && selectedQueueId.value) {
    purgeQueueAsync({ queueId: selectedQueueId.value })
      .then(() => {
        onActionComplete();
        toast.success("Queue has been purged successfully.");
      })
      .catch((e) => {
        const err = errorToToast(e);
        toast.error(err.detail);
      });
  }
};

const onRequeueComplete = () => {
  requeueSpecificPopoverOpen.value = false;
  requeuePopoverOpen.value = false;

  onActionComplete();
};

const onActionComplete = () => {
  selectedMessages.value = [];
};

const getMessagesIconComponent = (queue: QueueDto) => {
  if (queue.type == 1) {
    return CheckCircleIcon;
  } else if (queue.type == 2) {
    return ExclamationCircleIcon;
  } else if (queue.type == 3) {
    return XCircleIcon;
  }
  return CheckCircleIcon;
};

const hasMtFaultMessages = computed(() => {
  return messages.value?.items.some((x) => x.transportHeaders["MT-Fault-Message"]);
});

// Show status column for READY queue (type 1)
const isReadyQueue = computed(() => {
  return selectedQueue.value?.type === 1;
});

// Helper to check if a message was previously faulted
const wasPreviouslyFaulted = (msg: MessageDeliveryDto) => {
  return msg.transportHeaders?.["MT-Reason"] === "fault" || msg.transportHeaders?.["MT-Fault-Message"];
};

// Get the last word from URN by splitting on : or .
const getShortUrn = (urn: string | undefined): string => {
  if (!urn) return "-";
  const parts = urn.split(/[:.]/);
  return parts[parts.length - 1] || urn;
};

// Get pending status text with relative time if in future
const getPendingStatus = (enqueueTime: string | undefined): string => {
  if (!enqueueTime) return "Pending";
  const date = new Date(enqueueTime);
  if (isFuture(date)) {
    return `Pending (in ${formatDistance(date, new Date())})`;
  }
  return "Pending";
};

// Get failed status text with relative time
const getFailedStatus = (lastDelivered: string | undefined): string => {
  if (!lastDelivered) return "Failed";
  const date = new Date(lastDelivered);
  return `Failed (${formatDistance(date, new Date())} ago)`;
};
</script>

<template>
  <MessageDialog v-if="selectedMessage" :selected-message="selectedMessage" @close="toggleMessage(undefined)" />

  <AppLayout>
    <template #menu>
      <!-- Queue Header -->
      <div
        class="border-base-200 dark:border-base-content/10 border-b px-4 py-3"
        :class="{
          'from-success/5 via-base-200 to-base-200 bg-gradient-to-r': selectedQueue?.type === 1,
          'from-warning/10 via-base-200 to-base-200 bg-gradient-to-r': selectedQueue?.type === 2,
          'from-error/10 via-base-200 to-base-200 bg-gradient-to-r': selectedQueue?.type === 3,
          'bg-base-200': !selectedQueue,
        }"
      >
        <div class="flex items-center justify-between">
          <div class="flex items-center gap-3">
            <button
              class="text-base-content/50 hover:text-base-content hover:bg-base-200 flex cursor-pointer items-center justify-center rounded-lg p-1.5 transition-colors"
              @click="goToQueues"
              title="Back to Queues"
            >
              <ArrowLeftIcon class="h-5 w-5" />
            </button>
            <div>
              <h1 class="text-base-content text-xl font-semibold tracking-tight">{{ queueName }}</h1>
              <div class="text-base-content/50 mt-0.5 flex flex-wrap items-center gap-x-3 gap-y-1 text-xs">
                <span class="flex items-center gap-1">
                  Auto-delete: {{ queueView?.queueAutoDelete ? `${queueView.queueAutoDelete / 60}m` : "Off" }}
                </span>
              </div>
            </div>
          </div>
          <div v-if="selectedQueueId" class="flex items-center gap-1">
            <button
              v-for="item in queueOptions"
              :key="item.queue.id"
              class="flex cursor-pointer items-center gap-1.5 rounded-md px-3 py-1.5 text-sm font-medium transition-all"
              :class="{
                'bg-success/15 text-success ring-success/30 ring-1':
                  selectedQueueId === item.queue.id && item.queue.type === 1,
                'bg-warning/15 text-warning ring-warning/30 ring-1':
                  selectedQueueId === item.queue.id && item.queue.type === 2,
                'bg-error/15 text-error ring-error/30 ring-1':
                  selectedQueueId === item.queue.id && item.queue.type === 3,
                'text-success/60 hover:text-success hover:bg-success/10':
                  selectedQueueId !== item.queue.id && item.queue.type === 1,
                'text-warning/60 hover:text-warning hover:bg-warning/10':
                  selectedQueueId !== item.queue.id && item.queue.type === 2,
                'text-error/60 hover:text-error hover:bg-error/10':
                  selectedQueueId !== item.queue.id && item.queue.type === 3,
              }"
              @click="updateSelectedQueue(item.queue)"
            >
              <component :is="getMessagesIconComponent(item.queue)" class="h-4 w-4" />
              {{ item.queueNameByType }}
            </button>
          </div>
        </div>
      </div>

      <div class="border-base-200 dark:border-base-content/10 flex items-center border-b">
        <!-- Menu bar -->
        <div class="flex items-center gap-1 px-2 py-1.5">
          <button
            class="text-base-content/60 hover:text-base-content hover:bg-base-200 flex cursor-pointer items-center gap-1.5 rounded-md px-3 py-1.5 text-sm font-medium transition-colors disabled:cursor-not-allowed disabled:opacity-40 disabled:hover:bg-transparent"
            :disabled="isPending"
            @click="refreshQueue"
          >
            <RefreshIcon class="h-4 w-4" :class="{ 'refresh-spin': isRefreshing }" />
            Refresh
          </button>

          <!-- Requeue Specific Dropdown -->
          <div class="relative">
            <button
              class="text-base-content/60 hover:text-base-content hover:bg-base-200 flex cursor-pointer items-center gap-1.5 rounded-md px-3 py-1.5 text-sm font-medium transition-colors disabled:cursor-not-allowed disabled:opacity-40 disabled:hover:bg-transparent"
              :disabled="!selectedMessageIds.length"
              @click="requeueSpecificPopoverOpen = !requeueSpecificPopoverOpen"
            >
              <ReplayIcon class="h-4 w-4" />
              Requeue
              <span
                v-if="selectedMessageIds.length"
                class="bg-base-content/10 text-base-content/80 rounded-full px-1.5 py-0.5 text-xs font-medium"
              >
                {{ selectedMessageIds.length }}
              </span>
            </button>
            <div
              v-if="requeueSpecificPopoverOpen"
              class="bg-base-100 border-base-200 dark:border-base-content/10 absolute left-0 z-50 mt-1 w-80 rounded-lg border p-4 shadow-lg"
            >
              <RequeueDialog
                v-if="selectedQueueId"
                :selected-queue-id="selectedQueueId"
                :batch="false"
                :delivery-message-ids="selectedMessageIds"
                @requeue:complete="onRequeueComplete"
              />
            </div>
            <div
              v-if="requeueSpecificPopoverOpen"
              class="fixed inset-0 z-40"
              @click="requeueSpecificPopoverOpen = false"
            ></div>
          </div>

          <!-- Batch Requeue Dropdown -->
          <div class="relative">
            <button
              class="text-base-content/60 hover:text-base-content hover:bg-base-200 flex cursor-pointer items-center gap-1.5 rounded-md px-3 py-1.5 text-sm font-medium transition-colors"
              @click="requeuePopoverOpen = !requeuePopoverOpen"
            >
              <ReplayIcon class="h-4 w-4" />
              Batch Requeue
            </button>
            <div
              v-if="requeuePopoverOpen"
              class="bg-base-100 border-base-200 dark:border-base-content/10 absolute left-0 z-50 mt-1 w-80 rounded-lg border p-4 shadow-lg"
            >
              <RequeueDialog
                v-if="selectedQueueId"
                :selected-queue-id="selectedQueueId"
                :batch="true"
                :delivery-message-ids="[]"
                @requeue:complete="onRequeueComplete"
              />
            </div>
            <div v-if="requeuePopoverOpen" class="fixed inset-0 z-40" @click="requeuePopoverOpen = false"></div>
          </div>

          <!-- Delete Dropdown -->
          <div class="relative">
            <button
              class="text-base-content/60 hover:text-base-content hover:bg-base-200 flex cursor-pointer items-center gap-1.5 rounded-md px-3 py-1.5 text-sm font-medium transition-colors disabled:cursor-not-allowed disabled:opacity-40 disabled:hover:bg-transparent"
              :disabled="!selectedMessageIds.length"
              @click="deleteMessagesDropdownOpen = !deleteMessagesDropdownOpen"
            >
              <TrashIcon class="h-4 w-4" />
              Delete
            </button>
            <div
              v-if="deleteMessagesDropdownOpen"
              class="bg-base-100 border-base-200 dark:border-base-content/10 absolute left-0 z-50 mt-1 w-72 rounded-lg border p-4 shadow-lg"
            >
              <div class="flex flex-col gap-4">
                <!-- Visual indicator -->
                <div class="bg-error/10 flex items-center gap-3 rounded-lg p-3">
                  <div class="bg-error/20 text-error flex h-10 w-10 items-center justify-center rounded-full">
                    <TrashIcon class="h-5 w-5" />
                  </div>
                  <div class="flex flex-col">
                    <span class="text-base-content text-sm font-medium"
                      >{{ selectedMessageIds.length }} message{{ selectedMessageIds.length !== 1 ? "s" : "" }}</span
                    >
                    <span class="text-base-content/50 text-xs">will be permanently deleted</span>
                  </div>
                </div>

                <!-- Divider -->
                <div class="border-base-200 dark:border-base-content/10 border-t"></div>

                <!-- Options -->
                <div class="flex flex-col gap-2">
                  <span class="text-base-content/50 text-xs font-medium tracking-wide uppercase">Options</span>
                  <label class="flex cursor-pointer items-center gap-2 text-sm">
                    <input type="checkbox" v-model="deleteMessagesTransactional" class="checkbox checkbox-xs" />
                    <span class="text-base-content/70">Within single transaction</span>
                  </label>
                </div>

                <!-- Delete button -->
                <button
                  class="btn btn-error btn-sm w-full"
                  :class="{ loading: isDeleteMessagesPending }"
                  @click="deleteMessages"
                >
                  <TrashIcon class="h-4 w-4" />
                  Delete {{ selectedMessageIds.length }} message{{ selectedMessageIds.length !== 1 ? "s" : "" }}
                </button>
              </div>
            </div>
            <div
              v-if="deleteMessagesDropdownOpen"
              class="fixed inset-0 z-40"
              @click="deleteMessagesDropdownOpen = false"
            ></div>
          </div>

          <button
            class="text-base-content/60 hover:text-base-content hover:bg-base-200 flex cursor-pointer items-center gap-1.5 rounded-md px-3 py-1.5 text-sm font-medium transition-colors disabled:cursor-not-allowed disabled:opacity-40 disabled:hover:bg-transparent"
            :disabled="isPurgeQueuePending"
            @click="purgeQueue"
          >
            <EraserIcon class="h-4 w-4" />
            Purge
          </button>
        </div>
      </div>
    </template>

    <template #bottom>
      <Graph v-if="primaryQueue" :queue="primaryQueue" />
    </template>

    <!-- Empty State -->
    <template v-if="messages && !messages.items.length">
      <div class="flex flex-1 flex-col items-center justify-center gap-4 p-8">
        <div class="bg-base-200 flex h-16 w-16 items-center justify-center rounded-full">
          <CheckCircleIcon class="text-base-content/30 h-8 w-8" />
        </div>
        <div class="text-center">
          <h3 class="text-base-content text-lg font-medium">No messages</h3>
          <p class="text-base-content/50 mt-1 text-sm">
            This queue is empty. Messages will appear here when they arrive.
          </p>
        </div>
      </div>
    </template>

    <template v-if="messages?.items.length">
      <div class="flex min-h-0 flex-1 flex-col">
        <!-- Table -->
        <div class="min-h-0 flex-1 overflow-auto">
          <table class="table w-full">
            <thead class="bg-base-100 sticky top-0 z-10">
              <tr class="border-base-200 dark:border-base-content/10 border-b">
                <th class="w-0">
                  <input
                    type="checkbox"
                    class="checkbox checkbox-xs"
                    :checked="selectedMessages.length === messages.items.length && messages.items.length > 0"
                    :indeterminate="selectedMessages.length > 0 && selectedMessages.length < messages.items.length"
                    @change="
                      selectedMessages = selectedMessages.length === messages.items.length ? [] : [...messages.items]
                    "
                  />
                </th>
                <th class="text-base-content/60 w-0 text-xs font-medium whitespace-nowrap">ID</th>
                <th class="text-base-content/60 w-0 text-xs font-medium whitespace-nowrap">URN</th>
                <th class="text-base-content/60 w-0 text-xs font-medium whitespace-nowrap">Status</th>
                <th v-if="hasMtFaultMessages" class="text-base-content/60 text-xs font-medium whitespace-nowrap">
                  Fault Message
                </th>
                <!-- Spacer column: expands to fill available space, pushing other columns to be as narrow as possible -->
                <th v-if="!hasMtFaultMessages"></th>
                <th v-if="isReadyQueue" class="text-base-content/60 w-0 text-xs font-medium whitespace-nowrap">
                  Expires At
                </th>
                <th v-if="isReadyQueue" class="text-base-content/60 w-0 text-xs font-medium whitespace-nowrap">
                  Recurring
                </th>
              </tr>
            </thead>
            <tbody>
              <tr
                v-for="msg in messages.items"
                :key="msg.messageDeliveryId"
                class="border-base-200 dark:border-base-content/5 cursor-pointer border-b transition-colors"
                :class="{
                  'bg-base-200': selectedMessages.some((m) => m.messageDeliveryId === msg.messageDeliveryId),
                  'hover:bg-base-200/50': !selectedMessages.some((m) => m.messageDeliveryId === msg.messageDeliveryId),
                }"
                @click="toggleMessage(msg)"
              >
                <td class="w-0 py-2.5" @click.stop>
                  <input
                    type="checkbox"
                    class="checkbox checkbox-xs"
                    :checked="selectedMessages.some((m) => m.messageDeliveryId === msg.messageDeliveryId)"
                    @click="handleCheckboxChange(msg, $event)"
                  />
                </td>
                <td class="text-base-content/60 w-0 py-2.5 text-sm whitespace-nowrap">{{ msg.messageDeliveryId }}</td>
                <td class="text-base-content w-0 py-2.5 text-sm font-medium whitespace-nowrap">
                  <div class="tooltip tooltip-right" :data-tip="msg.message?.messageType">
                    {{ getShortUrn(msg.message?.messageType) }}
                  </div>
                </td>
                <td class="w-0 py-2.5">
                  <!-- Ready queue statuses -->
                  <template v-if="isReadyQueue">
                    <div
                      v-if="msg.message?.schedulingTokenId"
                      class="text-info flex items-center gap-2 text-sm"
                      title="Scheduled for future delivery"
                    >
                      <HourglassIcon class="h-4 w-4 shrink-0" />
                      <span class="whitespace-nowrap">
                        {{ getPendingStatus(msg.enqueueTime) }}
                      </span>
                    </div>
                    <div
                      v-else-if="wasPreviouslyFaulted(msg)"
                      class="text-warning flex items-center gap-2 text-sm"
                      title="Requeued from error - awaiting retry"
                    >
                      <HourglassIcon class="h-4 w-4 shrink-0" />
                      <span class="whitespace-nowrap">
                        {{ getPendingStatus(msg.enqueueTime) }}
                      </span>
                    </div>
                    <div v-else class="text-base-content/60 flex items-center gap-2 text-sm">
                      <HourglassIcon class="h-4 w-4 shrink-0" />
                      <span class="whitespace-nowrap">{{ getPendingStatus(msg.enqueueTime) }}</span>
                    </div>
                  </template>
                  <!-- Error/Dead-letter queue status -->
                  <template v-else>
                    <div class="text-error flex items-center gap-2 text-sm">
                      <ZapIcon class="h-4 w-4 shrink-0" />
                      <span class="whitespace-nowrap">{{ getFailedStatus(msg.lastDelivered) }}</span>
                    </div>
                  </template>
                </td>
                <td v-if="hasMtFaultMessages" class="max-w-0 py-2.5">
                  <div v-if="msg.transportHeaders?.['MT-Fault-Message']" class="text-error text-sm">
                    <span class="truncate">{{ msg.transportHeaders?.["MT-Fault-Message"] }}</span>
                  </div>
                  <span v-else class="text-base-content/30 text-sm">-</span>
                </td>
                <!-- Spacer column: expands to fill available space, pushing other columns to be as narrow as possible -->
                <td v-if="!hasMtFaultMessages"></td>
                <td v-if="isReadyQueue" class="text-base-content/60 w-0 py-2.5 text-sm whitespace-nowrap">
                  {{ msg.expirationTime ? humanDateTime(msg.expirationTime) : "-" }}
                </td>
                <td v-if="isReadyQueue" class="w-0 py-2.5 text-center">
                  <span v-if="msg.isRecurring" class="text-base-content/60 text-sm">✓</span>
                  <span v-else class="text-base-content/20 text-sm">-</span>
                </td>
              </tr>
              <tr v-if="messages.items.length === 0">
                <td
                  :colspan="(hasMtFaultMessages ? 1 : 0) + (isReadyQueue ? 2 : 0) + (selectedQueue?.type !== 1 ? 8 : 6)"
                  class="text-base-content/40 py-12 text-center text-sm"
                >
                  No messages found
                </td>
              </tr>
            </tbody>
          </table>
        </div>

        <!-- Pagination -->
        <div v-if="messages.totalPages > 1" class="border-base-200 dark:border-base-content/10 shrink-0 border-t">
          <Pagination
            :total-items="messages.totalCount"
            v-model:current-page="currentPage"
            :page-size="messages.pageSize"
            :show-page-size-selector="false"
          />
        </div>
      </div>
    </template>
  </AppLayout>
</template>

<style scoped>
.refresh-spin {
  animation: refresh-spin 0.3s ease-in-out;
}

@keyframes refresh-spin {
  0% {
    transform: scale(1) rotate(0deg);
  }
  50% {
    transform: scale(0.85) rotate(180deg);
  }
  100% {
    transform: scale(1) rotate(360deg);
  }
}
</style>
