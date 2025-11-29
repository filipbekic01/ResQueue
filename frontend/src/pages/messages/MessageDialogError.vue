<script lang="ts" setup>
import { format, formatDistance } from "date-fns";
import type { MessageDeliveryDto } from "@/dtos/message/messageDeliveryDto";

defineProps<{
  selectedMessage: MessageDeliveryDto;
}>();
</script>

<template>
  <div
    class="dark:border-t-surface-700 flex basis-1/3 flex-col gap-2 overflow-auto border-s-4 border-t border-s-red-400 p-6"
  >
    <div class="flex items-center gap-2">
      <div class="items-cener flex gap-3 dark:text-red-400">
        <i class="pi pi-circle-fill text-red-400"></i>
        {{ selectedMessage.transportHeaders["MT-Fault-ExceptionType"] }}
      </div>
      <span class="dark:text-surface-300">•</span>
      <div class="text-surface-500 dark:text-surface-300">
        {{ format(selectedMessage.transportHeaders["MT-Fault-Timestamp"], "MMM dd HH:mm:ss") }}
        (failed
        {{ formatDistance(selectedMessage.transportHeaders["MT-Fault-Timestamp"], new Date()) }}
        ago)
      </div>
    </div>

    <div class="text-red-700 dark:text-red-300">
      {{ selectedMessage.transportHeaders["MT-Fault-Message"] }}
    </div>

    <div class="dark:text-surface-400 px-4 whitespace-pre text-red-900">
      {{
        selectedMessage.transportHeaders["MT-Fault-StackTrace"]
          ? selectedMessage.transportHeaders["MT-Fault-StackTrace"]
          : "Stack trace missing."
      }}
    </div>
  </div>
</template>
