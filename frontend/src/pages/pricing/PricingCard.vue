<script lang="ts" setup>
import { useIdentity } from '@/composables/identityComposable'
import Tag from 'primevue/tag'

defineProps<{
  tier: string
  text: string
  price: number
  features: string[]
  severity: string
  recommended: boolean
}>()

const emit = defineEmits<{
  (e: 'get-started'): void
}>()

const {
  query: { data: user }
} = useIdentity()
</script>

<template>
  <div
    class="relative basis-1/3 rounded-xl border border-slate-200 bg-white p-8 text-center"
    :class="[
      {
        'scale-105 border border-slate-300 shadow-lg': recommended,
        'border-slate-200 shadow': !recommended
      }
    ]"
  >
    <Tag
      severity="success"
      v-if="recommended"
      style="font-weight: 600"
      class="absolute left-0 right-1/2 top-0 -translate-y-4 translate-x-1/2 border border-slate-400"
      ><i class="pi pi-sparkles me-1"></i>Recommended</Tag
    >
    <div class="mb-4 text-2xl font-semibold">{{ tier }}</div>
    <div class="mb-6 text-slate-500">{{ text }}</div>
    <div class="mb-6 text-4xl font-bold">${{ price }}<span class="text-xl">/mo</span></div>
    <ul class="mb-6 space-y-2 text-center">
      <li v-for="ft in features" :key="ft">{{ ft }}</li>
    </ul>
    <Button
      :outlined="tier.toLowerCase() !== 'essentials'"
      label="Get Started"
      :disabled="tier.toLowerCase() === 'free' && !!user"
      @click="emit('get-started')"
    ></Button>
  </div>
</template>
