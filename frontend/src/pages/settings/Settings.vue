<script lang="ts" setup>
import { useAppConfig } from '@/composables/appConfigComposable'
import AppLayout from '@/layouts/AppLayout.vue'
import Button from 'primevue/button'

const { localAppConfig, updateAppConfig } = useAppConfig()

const changeFontSize = (size: '14px' | '16px') =>
  updateAppConfig({
    ...localAppConfig.value,
    fontSize: size
  })
</script>

<template>
  <AppLayout>
    <template #title>Your Settings</template>
    <template #description>Customize your experience</template>
    <div class="gap-2 flex items-center px-7 py-3">
      Font Size: {{ localAppConfig.fontSize }}px
      <Button size="small" @click="changeFontSize('14px')" label="A"></Button>
      <Button @click="changeFontSize('16px')" label="A"></Button>
    </div>
  </AppLayout>
</template>
