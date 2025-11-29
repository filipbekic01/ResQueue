<script lang="ts" setup>
import { useQueryClient } from "@tanstack/vue-query";
import { computed, ref, watchEffect } from "vue";
import { useRouter } from "vue-router";
import { useDeleteMessagesMutation } from "@/api/messages/deleteMessagesMutation";
import { useMessagesQuery } from "@/api/messages/messagesQuery";
import { usePurgeQueueMutation } from "@/api/queues/purgeQueueMutation";
import ArrowLeftIcon from "@/components/icons/ArrowLeftIcon.vue";
import ArrowRightIcon from "@/components/icons/ArrowRightIcon.vue";
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
import { useUserSettings } from "@/composables/userSettingsComposable";
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

const router = useRouter();

const queryClient = useQueryClient();

const { confirm } = useConfirmDialog();
const toast = useToast();

const currentPage = ref(1);
const pageSize = ref(20);

const { settings, updateSettings } = useUserSettings();

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
  updateSettings({ ...settings, queueType: queue.type });
};

watchEffect(() => {
  if (selectedQueueId.value || !queueOptions.value.length || !queueView.value) {
    return;
  }

  selectedQueueId.value = queueOptions.value.find((x) => x.queue.type == settings.queueType)?.queue.id ?? undefined;
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
  computed(() => settings.refetchInterval),
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

const goToQueues = () => {
  router.push({ name: "queues" });
};

const refreshQueue = () => {
  refetchMessages().then(() => {
    toast.success("The queue has been successfully updated.");
  });
  queryClient.invalidateQueries({ queryKey: ["queue-view"] });
};

const purgeQueue = async () => {
  const confirmed = await confirm({
    type: "error",
    title: "Purge Queue",
    message: `Do you want to purge ${getQueueTypeLabel(selectedQueue.value?.type)} queue?`,
    confirmText: "Purge",
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
</script>

<template>
  <MessageDialog v-if="selectedMessage" :selected-message="selectedMessage" @close="toggleMessage(undefined)" />

  <AppLayout>
    <template #menu>
      <!-- Queue Header -->
      <div class="border-base-200 dark:border-base-content/10 bg-base-100 border-b px-4 py-3">
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
              <div class="text-base-content/50 mt-0.5 flex items-center gap-3 text-xs">
                <span v-if="queueView?.queueAutoDelete" class="flex items-center gap-1">
                  Auto-delete: {{ queueView.queueAutoDelete / 60 }}m
                </span>
                <span v-if="queueView?.queueMaxDeliveryCount" class="flex items-center gap-1">
                  Max delivery: {{ queueView.queueMaxDeliveryCount }}
                </span>
              </div>
            </div>
          </div>
          <div v-if="queueView" class="flex items-center gap-2">
            <div class="bg-base-200/50 flex items-center gap-1.5 rounded-lg px-2.5 py-1.5">
              <span class="text-base-content/60 text-xs">Ready</span>
              <span class="text-base-content text-sm font-semibold">{{ queueView.ready }}</span>
            </div>
            <div class="bg-warning/10 flex items-center gap-1.5 rounded-lg px-2.5 py-1.5">
              <span class="text-warning/70 text-xs">Errored</span>
              <span class="text-warning text-sm font-semibold">{{ queueView.errored }}</span>
            </div>
            <div class="bg-error/10 flex items-center gap-1.5 rounded-lg px-2.5 py-1.5">
              <span class="text-error/70 text-xs">Dead</span>
              <span class="text-error text-sm font-semibold">{{ queueView.deadLettered }}</span>
            </div>
            <div class="bg-info/10 flex items-center gap-1.5 rounded-lg px-2.5 py-1.5">
              <span class="text-info/70 text-xs">Scheduled</span>
              <span class="text-info text-sm font-semibold">{{ queueView.scheduled }}</span>
            </div>
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
            <RefreshIcon class="h-4 w-4" />
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
            </button>
            <div
              v-if="requeueSpecificPopoverOpen"
              class="bg-base-100 border-base-200 dark:border-base-content/10 absolute left-0 z-50 mt-1 w-72 rounded-lg border p-4 shadow-lg"
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
              class="bg-base-100 border-base-200 dark:border-base-content/10 absolute left-0 z-50 mt-1 w-72 rounded-lg border p-4 shadow-lg"
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
              class="bg-base-100 border-base-200 dark:border-base-content/10 absolute left-0 z-50 mt-1 w-64 rounded-lg border p-4 shadow-lg"
            >
              <div class="flex flex-col gap-3">
                <label class="flex cursor-pointer items-center gap-2 text-sm">
                  <input type="checkbox" v-model="deleteMessagesTransactional" class="checkbox checkbox-sm" />
                  Within single transaction
                </label>
                <button
                  class="btn btn-error btn-sm"
                  :class="{ loading: isDeleteMessagesPending }"
                  @click="deleteMessages"
                >
                  Delete
                  <ArrowRightIcon class="h-4 w-4" />
                </button>
              </div>
            </div>
          </div>

          <button
            class="text-error/70 hover:text-error hover:bg-error/10 flex cursor-pointer items-center gap-1.5 rounded-md px-3 py-1.5 text-sm font-medium transition-colors disabled:cursor-not-allowed disabled:opacity-40 disabled:hover:bg-transparent"
            :disabled="isPurgeQueuePending"
            @click="purgeQueue"
          >
            <EraserIcon class="h-4 w-4" />
            Purge
          </button>
        </div>

        <!-- Queue type tabs -->
        <div v-if="selectedQueueId" class="ms-auto flex items-center gap-1 px-2">
          <button
            v-for="item in queueOptions"
            :key="item.queue.id"
            class="flex cursor-pointer items-center gap-1.5 rounded-md px-3 py-1.5 text-sm font-medium transition-colors"
            :class="{
              'bg-base-200 text-base-content': selectedQueueId === item.queue.id,
              'text-base-content/60 hover:text-base-content hover:bg-base-200/50': selectedQueueId !== item.queue.id,
            }"
            @click="updateSelectedQueue(item.queue)"
          >
            <component :is="getMessagesIconComponent(item.queue)" class="h-4 w-4" />
            {{ item.queueNameByType }}
          </button>
        </div>
      </div>
    </template>

    <template #bottom>
      <Graph v-if="primaryQueue" :queue="primaryQueue" />
    </template>

    <template v-if="messages?.items.length">
      <div class="flex min-h-0 flex-1 flex-col">
        <!-- Table -->
        <div class="min-h-0 flex-1 overflow-auto">
          <table class="table w-full">
            <thead class="bg-base-100 sticky top-0">
              <tr class="border-base-200 dark:border-base-content/10 border-b">
                <th class="w-0">
                  <input
                    type="checkbox"
                    class="checkbox checkbox-sm"
                    :checked="selectedMessages.length === messages.items.length && messages.items.length > 0"
                    :indeterminate="selectedMessages.length > 0 && selectedMessages.length < messages.items.length"
                    @change="
                      selectedMessages = selectedMessages.length === messages.items.length ? [] : [...messages.items]
                    "
                  />
                </th>
                <th class="text-base-content/60 w-0 text-xs font-medium whitespace-nowrap">ID</th>
                <th
                  :class="{ 'w-0': hasMtFaultMessages }"
                  class="text-base-content/60 text-xs font-medium whitespace-nowrap"
                >
                  URN
                </th>
                <th v-if="hasMtFaultMessages" class="text-base-content/60 text-xs font-medium whitespace-nowrap">
                  Fault Message
                </th>
                <th class="text-base-content/60 w-0 text-xs font-medium whitespace-nowrap">Expires At</th>
                <th class="text-base-content/60 w-0 text-xs font-medium whitespace-nowrap">Recurring</th>
                <th class="text-base-content/60 w-0 text-xs font-medium whitespace-nowrap">Scheduled</th>
                <th class="text-base-content/60 w-0 text-xs font-medium whitespace-nowrap">Locked</th>
                <th class="text-base-content/60 w-0 text-xs font-medium whitespace-nowrap">Priority</th>
                <th class="text-base-content/60 w-0 text-xs font-medium whitespace-nowrap">Enqueue Time</th>
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
                    class="checkbox checkbox-sm"
                    :checked="selectedMessages.some((m) => m.messageDeliveryId === msg.messageDeliveryId)"
                    @change="
                      selectedMessages = selectedMessages.some((m) => m.messageDeliveryId === msg.messageDeliveryId)
                        ? selectedMessages.filter((m) => m.messageDeliveryId !== msg.messageDeliveryId)
                        : [...selectedMessages, msg]
                    "
                  />
                </td>
                <td class="text-base-content/60 py-2.5 text-sm whitespace-nowrap">{{ msg.messageDeliveryId }}</td>
                <td class="text-base-content max-w-xs truncate py-2.5 text-sm font-medium whitespace-nowrap">
                  {{ msg.message?.messageType?.replace("urn:message:", "") }}
                </td>
                <td v-if="hasMtFaultMessages" class="py-2.5">
                  <div
                    v-if="msg.transportHeaders?.['MT-Fault-Message']"
                    class="text-error flex items-center gap-2 text-sm"
                  >
                    <span class="h-1.5 w-1.5 rounded-full bg-current"></span>
                    <span class="max-w-xs truncate">{{ msg.transportHeaders?.["MT-Fault-ExceptionType"] }}</span>
                  </div>
                  <span v-else class="text-base-content/30 text-sm">-</span>
                </td>
                <td class="text-base-content/60 py-2.5 text-sm whitespace-nowrap">{{ msg.expirationTime ?? "-" }}</td>
                <td class="py-2.5 text-center">
                  <span v-if="msg.isRecurring" class="text-success text-sm">✓</span>
                  <span v-else class="text-base-content/20 text-sm">-</span>
                </td>
                <td class="py-2.5 text-center">
                  <span v-if="msg.message?.schedulingTokenId" class="text-info text-sm">✓</span>
                  <span v-else class="text-base-content/20 text-sm">-</span>
                </td>
                <td class="py-2.5 text-center">
                  <span v-if="!msg.lockId" class="text-warning text-sm">✓</span>
                  <span v-else class="text-base-content/20 text-sm">-</span>
                </td>
                <td class="text-base-content/60 py-2.5 text-center text-sm">{{ msg.priority }}</td>
                <td class="text-base-content/60 py-2.5 text-sm whitespace-nowrap">
                  {{ humanDateTime(msg.enqueueTime) }}
                </td>
              </tr>
              <tr v-if="messages.items.length === 0">
                <td :colspan="hasMtFaultMessages ? 10 : 9" class="text-base-content/40 py-12 text-center text-sm">
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
