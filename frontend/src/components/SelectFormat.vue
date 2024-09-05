<script lang="ts" setup>
import SelectButton from 'primevue/selectbutton'
import { computed } from 'vue'

export type FormatOption = 'raw' | 'json' | 'clean'

const props = defineProps<{
  modelValue: FormatOption
}>()

const emit = defineEmits<{
  (e: 'update:model-value', format: FormatOption): void
}>()

const opts = [
  {
    label: 'Raw',
    value: 'raw'
  },
  {
    label: 'JSON',
    value: 'json'
  },
  {
    label: 'Clean',
    value: 'clean'
  }
]

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
  <SelectButton
    v-model="model"
    :options="opts"
    option-label="label"
    option-value="value"
    aria-labelledby="basic"
  />
</template>
