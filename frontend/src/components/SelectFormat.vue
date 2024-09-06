<script lang="ts" setup>
import { computed } from 'vue'

export type FormatOption = 'raw' | 'json' | 'clean'

const props = defineProps<{
  modelValue: FormatOption
}>()

const emit = defineEmits<{
  (e: 'update:model-value', format: FormatOption): void
}>()

const model = computed({
  get() {
    return props.modelValue
  },
  set(format) {
    emit('update:model-value', format)
  }
})
</script>

<template>
  <div class="flex h-[37px] flex-nowrap gap-4 rounded-md border px-2 py-1">
    <div class="flex items-center">
      <RadioButton v-model="model" inputId="raw" name="select-format" value="raw" />
      <label for="raw" class="ml-2">Raw</label>
    </div>
    <div class="flex items-center">
      <RadioButton v-model="model" inputId="json" name="select-format" value="json" />
      <label for="json" class="ml-2">JSON</label>
    </div>
    <div class="flex items-center">
      <RadioButton v-model="model" inputId="clean" name="select-format" value="clean" />
      <label for="clean" class="ml-2">Clean</label>
    </div>
  </div>
</template>
