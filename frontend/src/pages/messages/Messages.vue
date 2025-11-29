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

  <!-- Delete Messages Dropdown -->
  <div v-if="deleteMessagesDropdownOpen" class="fixed inset-0 z-50" @click="deleteMessagesDropdownOpen = false"></div>

  <AppLayout>
    <template #menu>
      <div class="flex items-center">
        <!-- Menu bar with DaisyUI buttons -->
        <div class="border-base-300 dark:border-base-content/20 flex w-full items-center gap-1 border-b px-2 py-1.5">
          <button class="btn btn-ghost btn-sm" @click="goToQueues">
            <ArrowLeftIcon class="h-4 w-4" />
            Queues
          </button>
          <button class="btn btn-ghost btn-sm" :disabled="isPending" @click="refreshQueue">
            <RefreshIcon class="h-4 w-4" />
            Refresh
          </button>

          <!-- Requeue Specific Dropdown -->
          <div class="dropdown">
            <button
              tabindex="0"
              class="btn btn-ghost btn-sm"
              :disabled="!selectedMessageIds.length"
              @click="requeueSpecificPopoverOpen = !requeueSpecificPopoverOpen"
            >
              <ReplayIcon class="h-4 w-4" />
              Requeue
            </button>
            <div
              v-if="requeueSpecificPopoverOpen"
              tabindex="0"
              class="dropdown-content bg-base-100 z-50 w-72 rounded-lg p-4 shadow-xl"
            >
              <RequeueDialog
                v-if="selectedQueueId"
                :selected-queue-id="selectedQueueId"
                :batch="false"
                :delivery-message-ids="selectedMessageIds"
                @requeue:complete="onRequeueComplete"
              />
            </div>
          </div>

          <!-- Batch Requeue Dropdown -->
          <div class="dropdown">
            <button tabindex="0" class="btn btn-ghost btn-sm" @click="requeuePopoverOpen = !requeuePopoverOpen">
              <ReplayIcon class="h-4 w-4" />
              Batch Requeue
            </button>
            <div
              v-if="requeuePopoverOpen"
              tabindex="0"
              class="dropdown-content bg-base-100 z-50 w-72 rounded-lg p-4 shadow-xl"
            >
              <RequeueDialog
                v-if="selectedQueueId"
                :selected-queue-id="selectedQueueId"
                :batch="true"
                :delivery-message-ids="[]"
                @requeue:complete="onRequeueComplete"
              />
            </div>
          </div>

          <!-- Delete Dropdown -->
          <div class="dropdown">
            <button
              tabindex="0"
              class="btn btn-ghost btn-sm"
              :disabled="!selectedMessageIds.length"
              @click="deleteMessagesDropdownOpen = !deleteMessagesDropdownOpen"
            >
              <TrashIcon class="h-4 w-4" />
              Delete
            </button>
            <div
              v-if="deleteMessagesDropdownOpen"
              tabindex="0"
              class="dropdown-content bg-base-100 z-50 w-64 rounded-lg p-4 shadow-xl"
            >
              <div class="flex flex-col gap-3">
                <div class="flex items-center gap-2">
                  <input
                    type="checkbox"
                    id="delete-transactional"
                    v-model="deleteMessagesTransactional"
                    class="checkbox checkbox-sm"
                  />
                  <label for="delete-transactional">Within single transaction</label>
                </div>
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

          <button class="btn btn-ghost btn-sm" :disabled="isPurgeQueuePending" @click="purgeQueue">
            <EraserIcon class="h-4 w-4" />
            Purge
          </button>
        </div>

        <!-- Tabs for queue types -->
        <div v-if="selectedQueueId" role="tablist" class="tabs tabs-border ms-auto">
          <button
            v-for="item in queueOptions"
            :key="item.queue.id"
            role="tab"
            class="tab flex gap-2"
            :class="{ 'tab-active': selectedQueueId === item.queue.id }"
            @click="updateSelectedQueue(item.queue)"
          >
            <component :is="getMessagesIconComponent(item.queue)" class="h-4 w-4" />
            {{ item.queueNameByType }}
          </button>
        </div>
      </div>
    </template>

    <template #right>
      <div
        v-if="settings.showGraph"
        class="border-base-300 dark:border-base-content/20 flex grow items-center justify-center border-s border-b ps-3"
      >
        <Graph v-if="primaryQueue" :queue="primaryQueue" />
      </div>
    </template>

    <template v-if="messages?.items.length">
      <div class="flex grow flex-col overflow-auto">
        <!-- Table -->
        <div class="grow overflow-auto">
          <table class="table-zebra table-pin-rows table w-full">
            <thead>
              <tr>
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
                <th class="w-0 whitespace-nowrap">ID</th>
                <th :class="{ 'w-0': hasMtFaultMessages }" class="whitespace-nowrap">URN</th>
                <th v-if="hasMtFaultMessages" class="whitespace-nowrap">Fault Message</th>
                <th class="w-0 whitespace-nowrap">Expires At</th>
                <th class="w-0 whitespace-nowrap">Recurring</th>
                <th class="w-0 whitespace-nowrap">Scheduled</th>
                <th class="w-0 whitespace-nowrap">Locked</th>
                <th class="w-0 whitespace-nowrap">Priority</th>
                <th class="w-0 whitespace-nowrap">Enqueue Time</th>
              </tr>
            </thead>
            <tbody>
              <tr
                v-for="msg in messages.items"
                :key="msg.messageDeliveryId"
                class="hover cursor-pointer"
                :class="{ 'bg-base-200': selectedMessages.some((m) => m.messageDeliveryId === msg.messageDeliveryId) }"
                @click="toggleMessage(msg)"
              >
                <td class="w-0" @click.stop>
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
                <td class="whitespace-nowrap">{{ msg.messageDeliveryId }}</td>
                <td class="whitespace-nowrap">{{ msg.message?.messageType?.replace("urn:message:", "") }}</td>
                <td v-if="hasMtFaultMessages">
                  <div v-if="msg.transportHeaders?.['MT-Fault-Message']" class="text-base-content/60 flex gap-3">
                    <span class="inline-block h-2 w-2 rounded-full bg-red-400"></span>
                    {{ msg.transportHeaders?.["MT-Fault-ExceptionType"] }}
                  </div>
                  <div v-else>-</div>
                </td>
                <td class="whitespace-nowrap">{{ msg.expirationTime ?? "-" }}</td>
                <td class="text-center">
                  <span v-if="msg.isRecurring">✓</span>
                </td>
                <td class="text-center">
                  <span v-if="msg.message?.schedulingTokenId">✓</span>
                </td>
                <td class="text-center">
                  <span v-if="!msg.lockId">✓</span>
                </td>
                <td class="text-center">{{ msg.priority }}</td>
                <td class="whitespace-nowrap">{{ humanDateTime(msg.enqueueTime) }}</td>
              </tr>
              <tr v-if="messages.items.length === 0">
                <td :colspan="hasMtFaultMessages ? 10 : 9" class="text-base-content/50 text-center">
                  No messages found
                </td>
              </tr>
            </tbody>
          </table>
        </div>

        <!-- Pagination -->
        <div v-if="messages.totalPages > 1" class="border-base-300 border-t">
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
