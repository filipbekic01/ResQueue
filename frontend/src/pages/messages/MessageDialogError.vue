<script lang="ts" setup>
import { format, formatDistance } from "date-fns";
import CircleFilledIcon from "@/components/icons/CircleFilledIcon.vue";
import type { MessageDeliveryDto } from "@/dtos/message/messageDeliveryDto";

defineProps<{
  selectedMessage: MessageDeliveryDto;
}>();
</script>

<template>
  <div
    class="border-base-200 dark:border-base-content/10 bg-error/5 flex basis-1/3 flex-col gap-3 overflow-auto border-t p-6"
  >
    <div class="flex items-center gap-2">
      <div class="text-error flex items-center gap-2 font-medium">
        <CircleFilledIcon class="h-2.5 w-2.5" />
        <span class="text-sm">{{ selectedMessage.transportHeaders["MT-Fault-ExceptionType"] }}</span>
      </div>
      <span class="text-base-content/30">•</span>
      <div class="text-base-content/50 text-sm">
        {{ format(selectedMessage.transportHeaders["MT-Fault-Timestamp"], "MMM dd HH:mm:ss") }}
        (failed
        {{ formatDistance(selectedMessage.transportHeaders["MT-Fault-Timestamp"], new Date()) }}
        ago)
      </div>
    </div>

    <div class="text-error/90 text-sm font-medium">
      {{ selectedMessage.transportHeaders["MT-Fault-Message"] }}
    </div>

    <div class="text-base-content/60 bg-base-200/50 overflow-auto rounded-lg p-4 font-mono text-xs whitespace-pre">
      {{
        selectedMessage.transportHeaders["MT-Fault-StackTrace"]
          ? selectedMessage.transportHeaders["MT-Fault-StackTrace"]
          : "Stack trace missing."
      }}
    </div>
  </div>
</template>
