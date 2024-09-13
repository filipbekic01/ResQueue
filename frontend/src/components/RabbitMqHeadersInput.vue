<script lang="ts" setup>
import type { HeaderValue } from '@/dtos/rabbitMQMessagePropsDto'
import { ref } from 'vue'

const props = defineProps<{
  modelValue?: Record<string, HeaderValue>
}>()

const newHeaderName = ref('')

const emit = defineEmits<{
  (e: 'update:model-value', headers: Record<string, HeaderValue>): void
}>()

const addHeader = (header: string) => {
  emit('update:model-value', { ...props.modelValue, [header]: '' })
  newHeaderName.value = ''
}

const removeHeader = (header: string) => {
  const headersCopy = { ...props.modelValue }
  delete headersCopy[header]

  emit('update:model-value', headersCopy)
}

const updateHeaderValue = (header: string, value: HeaderValue) => {
  emit('update:model-value', { ...props.modelValue, [header]: value })
}
</script>

<template>
  <div class="flex flex-col gap-2">
    <div
      v-for="[header, value] in Object.entries(modelValue ?? {})"
      :key="header"
      class="flex items-center gap-2"
    >
      <label>{{ header }}</label>
      <InputText
        v-if="typeof value === 'string'"
        :modelValue="value"
        @update:modelValue="(newValue) => newValue && updateHeaderValue(header, newValue)"
      />
      <template v-else>NOT IMPLEMENTED</template>
      <Button
        type="button"
        icon="pi pi-minus"
        severity="danger"
        @click="removeHeader(header)"
      ></Button>
    </div>
    <div class="mt-2 flex gap-2">
      <InputText v-model="newHeaderName" placeholder="Enter a header name" />
      <Button
        :disabled="
          !newHeaderName || (modelValue && Object.keys(modelValue).includes(newHeaderName))
        "
        type="button"
        icon="pi pi-plus"
        severity="secondary"
        @click="addHeader(newHeaderName)"
      ></Button>
    </div>
  </div>
</template>
