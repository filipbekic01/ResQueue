<script lang="ts" setup>
import type { HeaderValue } from '@/dtos/messages/rabbitMQMessagePropsDto'
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
      <label class="w-72 text-end">{{ header }}:</label>
      <InputText
        v-if="typeof value === 'string'"
        class="grow"
        :model-value="value"
        @update:model-value="(newValue) => newValue && updateHeaderValue(header, newValue)"
      />
      <template v-else>NOT IMPLEMENTED</template>
      <Button
        type="button"
        icon="pi pi-minus"
        outlined
        severity="danger"
        size="small"
        @click="removeHeader(header)"
      ></Button>
    </div>
    <div class="mt-2 flex gap-2">
      <div class="w-72"></div>
      <InputText v-model="newHeaderName" placeholder="Enter a header name" class="grow" />
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
