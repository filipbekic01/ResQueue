<script lang="ts" setup>
import { formatDistance, isFuture, isPast } from "date-fns";
import { computed, ref } from "vue";
import ClockExpiredIcon from "@/components/icons/ClockExpiredIcon.vue";
import HourglassIcon from "@/components/icons/HourglassIcon.vue";
import ZapIcon from "@/components/icons/ZapIcon.vue";
import type { MessageDeliveryDto } from "@/dtos/message/messageDeliveryDto";
import { humanDateTime } from "@/utils/dateTimeUtil";

const props = defineProps<{
  messages: MessageDeliveryDto[];
  isReadyQueue: boolean;
}>();

const selectedMessages = defineModel<MessageDeliveryDto[]>("selectedMessages", { required: true });

const emit = defineEmits<{
  (e: "message:click", msg: MessageDeliveryDto): void;
}>();

// Shift selection
const lastSelectedIndex = ref<number | null>(null);

const handleCheckboxChange = (msg: MessageDeliveryDto, event: Event) => {
  const isChecked = (event.target as HTMLInputElement).checked;
  const currentIndex = props.messages.findIndex((m) => m.messageDeliveryId === msg.messageDeliveryId);

  if ((event as MouseEvent).shiftKey && lastSelectedIndex.value !== null && props.messages) {
    // Shift+click: select range
    const start = Math.min(lastSelectedIndex.value, currentIndex);
    const end = Math.max(lastSelectedIndex.value, currentIndex);
    const rangeItems = props.messages.slice(start, end + 1);

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

const hasMtFaultMessages = computed(() => {
  return props.messages.some((x) => x.transportHeaders["MT-Fault-Message"]);
});

// Get the last word from URN by splitting on : or .
const getShortUrn = (urn: string | undefined): string => {
  if (!urn) return "-";
  const parts = urn.split(/[:.]/);
  return parts[parts.length - 1] || urn;
};

// Check if message is scheduled (enqueue time is in the future)
const isScheduled = (enqueueTime: string | undefined): boolean => {
  if (!enqueueTime) return false;
  return isFuture(new Date(enqueueTime));
};

// Check if message is expired (expiration time is in the past)
const isExpired = (expirationTime: string | undefined): boolean => {
  if (!expirationTime) return false;
  return isPast(new Date(expirationTime));
};

// Get scheduled status text with relative time
const getScheduledStatus = (enqueueTime: string | undefined): string => {
  if (!enqueueTime) return "Scheduled";
  const date = new Date(enqueueTime);
  return `Scheduled (in ${formatDistance(date, new Date())})`;
};

// Get expired status text with relative time
const getExpiredStatus = (expirationTime: string | undefined): string => {
  if (!expirationTime) return "Expired";
  const date = new Date(expirationTime);
  return `Expired (${formatDistance(date, new Date())} ago)`;
};

// Get failed status text with relative time
const getFailedStatus = (lastDelivered: string | undefined): string => {
  if (!lastDelivered) return "Failed";
  const date = new Date(lastDelivered);
  return `Failed (${formatDistance(date, new Date())} ago)`;
};

const toggleSelectAll = () => {
  if (selectedMessages.value.length === props.messages.length) {
    selectedMessages.value = [];
  } else {
    selectedMessages.value = [...props.messages];
  }
};
</script>

<template>
  <div class="min-h-0 flex-1 overflow-auto">
    <table class="table w-full">
      <thead class="bg-base-100 sticky top-0 z-10">
        <tr class="border-base-200 dark:border-base-content/10 border-b">
          <th class="w-0">
            <input
              type="checkbox"
              class="checkbox checkbox-xs"
              :checked="selectedMessages.length === messages.length && messages.length > 0"
              :indeterminate="selectedMessages.length > 0 && selectedMessages.length < messages.length"
              @change="toggleSelectAll"
            />
          </th>
          <th class="text-base-content/60 w-0 text-xs font-medium whitespace-nowrap">ID</th>
          <th class="text-base-content/60 w-0 text-xs font-medium whitespace-nowrap">URN</th>
          <th class="text-base-content/60 w-0 text-xs font-medium whitespace-nowrap">Status</th>
          <th v-if="hasMtFaultMessages" class="text-base-content/60 text-xs font-medium whitespace-nowrap">
            Fault Message
          </th>
          <th class="text-base-content/60 text-xs font-medium whitespace-nowrap">Payload</th>
          <!-- Spacer column: expands to fill available space, pushing other columns to be as narrow as possible -->
          <th v-if="!hasMtFaultMessages"></th>
          <th v-if="isReadyQueue" class="text-base-content/60 w-0 text-xs font-medium whitespace-nowrap">Expires At</th>
          <th v-if="isReadyQueue" class="text-base-content/60 w-0 text-xs font-medium whitespace-nowrap">Recurring</th>
        </tr>
      </thead>
      <tbody>
        <tr
          v-for="msg in messages"
          :key="msg.messageDeliveryId"
          class="border-base-200 dark:border-base-content/5 cursor-pointer border-b transition-colors"
          :class="{
            'bg-base-200': selectedMessages.some((m) => m.messageDeliveryId === msg.messageDeliveryId),
            'hover:bg-base-200/50': !selectedMessages.some((m) => m.messageDeliveryId === msg.messageDeliveryId),
          }"
          @click="emit('message:click', msg)"
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
                v-if="isExpired(msg.expirationTime)"
                class="text-warning flex items-center gap-2 text-sm"
                title="Message has expired"
              >
                <ClockExpiredIcon class="h-4 w-4 shrink-0" />
                <span class="whitespace-nowrap">
                  {{ getExpiredStatus(msg.expirationTime) }}
                </span>
              </div>
              <div
                v-else-if="isScheduled(msg.enqueueTime)"
                class="text-info flex items-center gap-2 text-sm"
                title="Scheduled for future delivery"
              >
                <HourglassIcon class="h-4 w-4 shrink-0" />
                <span class="whitespace-nowrap">
                  {{ getScheduledStatus(msg.enqueueTime) }}
                </span>
              </div>
              <div v-else class="text-base-content/60 flex items-center gap-2 text-sm">
                <HourglassIcon class="h-4 w-4 shrink-0" />
                <span class="whitespace-nowrap">Pending</span>
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
          <td v-if="hasMtFaultMessages" class="max-w-xs py-2.5">
            <div v-if="msg.transportHeaders?.['MT-Fault-Message']" class="text-error truncate text-sm">
              {{ msg.transportHeaders?.["MT-Fault-Message"] }}
            </div>
            <span v-else class="text-base-content/30 text-sm">-</span>
          </td>
          <td class="max-w-xs py-2.5">
            <div class="text-base-content/60 truncate font-mono text-xs">
              {{ msg.message?.body ? JSON.stringify(JSON.parse(msg.message.body)) : "-" }}
            </div>
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
        <tr v-if="messages.length === 0">
          <td
            :colspan="(hasMtFaultMessages ? 1 : 0) + (isReadyQueue ? 2 : 0) + 6"
            class="text-base-content/40 py-12 text-center text-sm"
          >
            No messages found
          </td>
        </tr>
      </tbody>
    </table>
  </div>
</template>
