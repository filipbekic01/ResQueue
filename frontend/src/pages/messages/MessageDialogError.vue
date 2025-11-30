<script lang="ts" setup>
import DotnetStackTraceHighlighter from "@/components/DotnetStackTraceHighlighter.vue";
import CopyIcon from "@/components/icons/CopyIcon.vue";
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
    class="border-base-200 dark:border-base-content/10 bg-base-content/3 relative flex max-h-[40%] shrink flex-col gap-2 overflow-auto border-t shadow"
  >
    <!-- Error header -->
    <div class="flex items-start justify-between px-6 pt-4">
      <div class="flex min-w-0 flex-col gap-0.5">
        <div class="flex items-center gap-1.5">
          <span class="text-error truncate text-sm">{{ selectedMessage.transportHeaders["MT-Fault-Message"] }}</span>
        </div>
        <span class="text-base-content/50 text-error pl-3 text-sm"
          >└ {{ selectedMessage.transportHeaders["MT-Fault-ExceptionType"] }}</span
        >
      </div>
    </div>

    <!-- Stack trace (main content) -->
    <div class="min-h-0 flex-1">
      <button
        v-if="selectedMessage.transportHeaders['MT-Fault-StackTrace']"
        class="btn btn-ghost btn-xs btn-circle absolute top-3 right-3 z-10"
        @click="copyToClipboard(selectedMessage.transportHeaders['MT-Fault-StackTrace'])"
        title="Copy to clipboard"
      >
        <CopyIcon class="h-3.5 w-3.5" />
      </button>
      <div class="h-full ps-10 pt-1">
        <DotnetStackTraceHighlighter
          v-if="selectedMessage.transportHeaders['MT-Fault-StackTrace']"
          :stack-trace="selectedMessage.transportHeaders['MT-Fault-StackTrace']"
        />
        <span v-else class="text-base-content/60 font-mono text-xs">Stack trace missing.</span>
      </div>
    </div>
  </div>
</template>
