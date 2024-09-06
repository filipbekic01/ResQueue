<script lang="ts" setup>
import { computed } from 'vue'

export type StructureOption = 'meta' | 'body' | 'both'

const props = defineProps<{
  modelValue: StructureOption
}>()

const emit = defineEmits<{
  (e: 'update:model-value', structure: StructureOption): void
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
      <RadioButton v-model="model" inputId="meta" name="select-format" value="meta" />
      <label for="meta" class="ml-2">Meta</label>
    </div>
    <div class="flex items-center">
      <RadioButton v-model="model" inputId="body" name="select-format" value="body" />
      <label for="body" class="ml-2">Body</label>
    </div>
    <div class="flex items-center">
      <RadioButton v-model="model" inputId="both" name="select-format" value="both" />
      <label for="both" class="ml-2">Both</label>
    </div>
  </div>
</template>
