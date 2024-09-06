<script lang="ts" setup>
import { computed, useSlots } from 'vue'

const props = defineProps<{
  label: string
  value?: string | number | boolean
}>()

const slots = useSlots()

const isError = computed(
  () =>
    props.label.toLowerCase().includes('fail') ||
    props.label.toLowerCase().includes('error') ||
    props.label.toLowerCase().includes('fault')
)

const showPair = computed(() => !!props.value?.toString() || !!slots['default'])
</script>

<template>
  <div
    class="flex items-start overflow-hidden"
    :class="{
      'bg-gray-50': true,
      'border-s-2 border-red-500 ps-1': isError,
      'border-s-2 border-gray-500 ps-1': !isError
    }"
    v-if="showPair"
  >
    <div class="flex shrink-0 basis-80 items-center">
      {{ label }}
    </div>
    <div class="text-slate-600">
      <slot name="default">{{ value }}</slot>
    </div>
  </div>
</template>
