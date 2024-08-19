<script lang="ts" setup>
import Tag from 'primevue/tag'

defineProps<{
  tier: string
  text: string
  price: number
  features: string[]
  severity: string
  recommended: boolean
}>()
</script>

<template>
  <div
    class="border basis-1/3 bg-white border-gray-200 rounded-xl p-8 text-center relative"
    :class="[
      {
        'border-gray-400 border scale-105': recommended,
        'border-gray-200': !recommended
      }
    ]"
  >
    <Tag
      icon="pi pi-sparkles"
      v-if="recommended"
      style="font-weight: 600"
      class="-translate-y-4 translate-x-1/2 border-gray-400 border top-0 right-1/2 left-0 absolute"
      >Most Popular</Tag
    >
    <div class="text-2xl font-semibold mb-4">{{ tier }}</div>
    <div class="text-gray-500 mb-6">{{ text }}</div>
    <div class="text-4xl font-bold mb-6">${{ price }}<span class="text-xl">/mo</span></div>
    <ul class="space-y-2 mb-6 text-center">
      <li v-for="ft in features" :key="ft">{{ ft }}</li>
    </ul>
    <Button label="Get Started" :severity="severity"></Button>
  </div>
</template>
