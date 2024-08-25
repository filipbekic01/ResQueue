<script setup lang="ts">
import type { HeaderValue } from '@/dtos/rabbitmqMessagePropertiesDto'

defineProps<{ modelValue: HeaderValue }>()
</script>
<template>
  <div class="text-slate-500">
    <template v-if="hKey === 'MT-Fault-Message'"> ... </template>
    <template v-else-if="hKey === 'MT-Fault-StackTrace'"> ... </template>
    <template v-else>
      {{ modelValue }}
    </template>
  </div>
</template>
