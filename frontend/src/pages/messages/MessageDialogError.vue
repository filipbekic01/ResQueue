<script lang="ts" setup>
import { formatDistance } from "date-fns";
import DotnetStackTraceHighlighter from "@/components/DotnetStackTraceHighlighter.vue";
import CopyIcon from "@/components/icons/CopyIcon.vue";
import ZapIcon from "@/components/icons/ZapIcon.vue";
import type { MessageDeliveryDto } from "@/dtos/message/messageDeliveryDto";

defineProps<{
  selectedMessage: MessageDeliveryDto;
}>();

const copyToClipboard = async (text: string) => {
  await navigator.clipboard.writeText(text);
};
</script>

<template>
  <div
    class="border-base-200 dark:border-base-content/10 bg-error/5 relative flex max-h-[50%] shrink flex-col overflow-hidden border-t"
  >
    <!-- Error header -->
    <div class="flex items-start justify-between gap-4 px-4 pt-3">
      <div class="flex min-w-0 flex-col gap-0.5">
        <div class="flex items-center gap-1.5">
          <ZapIcon class="text-error h-3.5 w-3.5 shrink-0" />
          <span class="text-error truncate font-medium">{{
            selectedMessage.transportHeaders["MT-Fault-Message"]
          }}</span>
        </div>
        <span class="text-base-content/50 pl-5 text-xs"
          >└ {{ selectedMessage.transportHeaders["MT-Fault-ExceptionType"] }}</span
        >
      </div>
      <div class="text-base-content/50 shrink-0 text-sm">
        {{ formatDistance(selectedMessage.transportHeaders["MT-Fault-Timestamp"], new Date()) }} ago
      </div>
    </div>

    <!-- Stack trace (main content) -->
    <div class="relative min-h-0 flex-1 overflow-auto px-4 pt-4 pb-4">
      <button
        v-if="selectedMessage.transportHeaders['MT-Fault-StackTrace']"
        class="btn btn-ghost btn-xs btn-circle absolute top-6 right-6 z-10"
        @click="copyToClipboard(selectedMessage.transportHeaders['MT-Fault-StackTrace'])"
        title="Copy to clipboard"
      >
        <CopyIcon class="h-3.5 w-3.5" />
      </button>
      <div class="bg-base-200/50 h-full overflow-auto rounded-lg p-4 pr-10">
        <DotnetStackTraceHighlighter
          v-if="selectedMessage.transportHeaders['MT-Fault-StackTrace']"
          :stack-trace="selectedMessage.transportHeaders['MT-Fault-StackTrace']"
        />
        <span v-else class="text-base-content/60 font-mono text-xs">Stack trace missing.</span>
      </div>
    </div>
  </div>
</template>
