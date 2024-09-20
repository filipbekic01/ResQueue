<script lang="ts" setup>
import Tag from 'primevue/tag'
import Tooltip from 'primevue/tooltip'

defineProps<{
  tier: string
  text: string
  textTooltip: string
  price: number
  features: string[]
  disabled: boolean
  recommended: boolean
}>()

const emit = defineEmits<{
  (e: 'get-started'): void
}>()
</script>

<template>
  <div
    class="relative basis-1/3 rounded-xl border border-slate-200 bg-white p-8 text-center"
    :class="[
      {
        'scale-105 border border-slate-300 shadow-lg shadow-blue-100': recommended,
        'border-slate-200 shadow': !recommended,
        'select-none opacity-50': disabled
      }
    ]"
  >
    <Tag
      severity="info"
      v-if="recommended"
      style="font-weight: 600"
      class="absolute left-0 right-1/2 top-0 w-2/3 -translate-y-4 translate-x-1/4 border border-slate-400"
      ><i class="pi pi-thumbs-up me-1"></i>Recommended</Tag
    >
    <div class="mb-4 text-2xl font-semibold">{{ tier }}</div>
    <div class="mb-6 flex items-center justify-center gap-2 text-slate-500">
      {{ text }}
      <i v-if="textTooltip" class="pi pi-question-circle" v-tooltip="textTooltip"></i>
    </div>
    <div class="mb-6 text-4xl font-bold">${{ price }}<span class="text-xl">/mo</span></div>
    <ul class="mb-6 space-y-2 text-center">
      <li v-for="ft in features" :key="ft">{{ ft }}</li>
    </ul>
    <Button :disabled="disabled" label="Get Started" @click="emit('get-started')"></Button>
  </div>
</template>
