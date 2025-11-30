<script lang="ts" setup>
import { useQueryClient } from "@tanstack/vue-query";
import { computed, ref, watchEffect } from "vue";
import { useRoute, useRouter } from "vue-router";
import { useDeleteMessagesMutation } from "@/api/messages/deleteMessagesMutation";
import { useMessagesQuery } from "@/api/messages/messagesQuery";
import { usePurgeQueueMutation } from "@/api/queues/purgeQueueMutation";
import ArrowLeftIcon from "@/components/icons/ArrowLeftIcon.vue";
import CheckCircleIcon from "@/components/icons/CheckCircleIcon.vue";
import EraserIcon from "@/components/icons/EraserIcon.vue";
import ExclamationCircleIcon from "@/components/icons/ExclamationCircleIcon.vue";
import RefreshIcon from "@/components/icons/RefreshIcon.vue";
import ReplayIcon from "@/components/icons/ReplayIcon.vue";
import TrashIcon from "@/components/icons/TrashIcon.vue";
import XCircleIcon from "@/components/icons/XCircleIcon.vue";
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
import { errorToToast } from "@/utils/errorUtils";
import MessageDialog from "./MessageDialog.vue";
import MessagesTable from "./MessagesTable.vue";

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

// Show status column for READY queue (type 1)
const isReadyQueue = computed(() => {
  return selectedQueue.value?.type === 1;
});
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
          'from-error/10 via-base-200 to-base-200 bg-gradient-to-r': selectedQueue?.type === 2,
          'from-base-content/10 via-base-200 to-base-200 bg-gradient-to-r': selectedQueue?.type === 3,
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
                'bg-error/15 text-error ring-error/30 ring-1':
                  selectedQueueId === item.queue.id && item.queue.type === 2,
                'bg-base-content/15 text-base-content ring-base-content/30 ring-1':
                  selectedQueueId === item.queue.id && item.queue.type === 3,
                'text-success/60 hover:text-success hover:bg-success/10':
                  selectedQueueId !== item.queue.id && item.queue.type === 1,
                'text-error/60 hover:text-error hover:bg-error/10':
                  selectedQueueId !== item.queue.id && item.queue.type === 2,
                'text-base-content/60 hover:text-base-content hover:bg-base-content/10':
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
        <MessagesTable
          :messages="messages.items"
          :is-ready-queue="isReadyQueue"
          v-model:selected-messages="selectedMessages"
          @message:click="toggleMessage"
        />

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
