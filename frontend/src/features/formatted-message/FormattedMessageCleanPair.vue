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
      '': true,
      'border-s border-red-400 bg-red-50 ps-1': isError,
      'border-s border-gray-400 bg-gray-50 ps-1': !isError
    }"
    v-if="showPair"
  >
    <div class="flex shrink-0 basis-80 items-center">
      {{ label }}
    </div>
    <div class="overflow-auto whitespace-nowrap text-slate-600">
      <slot name="default">{{ value }}</slot>
    </div>
  </div>
</template>
