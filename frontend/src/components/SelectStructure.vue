<script lang="ts" setup>
import SelectButton from 'primevue/selectbutton'
import { computed } from 'vue'

export type StructureOption = 'meta' | 'body' | 'both'

const props = defineProps<{
  modelValue: StructureOption
}>()

const emit = defineEmits<{
  (e: 'update:model-value', structure: StructureOption): void
}>()

const opts = [
  {
    label: 'Meta',
    value: 'meta'
  },
  {
    label: 'Body',
    value: 'body'
  },
  {
    label: 'Both',
    value: 'both'
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
