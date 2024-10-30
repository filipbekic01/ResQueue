<script lang="ts" setup>
import type { MessageDeliveryDto } from '@/dtos/message/messageDeliveryDto'
import { format, formatDistance } from 'date-fns'

defineProps<{
  selectedMessage: MessageDeliveryDto
}>()
</script>

<template>
  <div class="flex basis-1/3 flex-col gap-2 overflow-auto border-s-4 border-t border-s-red-400 p-6">
    <div class="flex items-center gap-2">
      <div class="items-cener flex gap-3">
        <i class="pi pi-circle-fill text-red-400"></i>
        {{ selectedMessage.transportHeaders['MT-Fault-ExceptionType'] }}
      </div>
      <span>•</span>
      <div class="text-primary-500">
        {{ format(selectedMessage.transportHeaders['MT-Fault-Timestamp'], 'MMM dd HH:mm:ss') }} (failed
        {{ formatDistance(selectedMessage.transportHeaders['MT-Fault-Timestamp'], new Date()) }}
        ago)
      </div>
    </div>

    <div class="font-mono text-red-700">
      {{ selectedMessage.transportHeaders['MT-Fault-Message'] }}
    </div>

    <div class="whitespace-pre px-4 font-mono text-red-900">
      {{
        selectedMessage.transportHeaders['MT-Fault-StackTrace']
          ? selectedMessage.transportHeaders['MT-Fault-StackTrace']
          : 'Stack trace missing.'
      }}
    </div>
  </div>
</template>
