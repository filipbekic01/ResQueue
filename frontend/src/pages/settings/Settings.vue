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
  <AppLayout hide-header>
    <div class="text-3xl font-bold px-7 pt-4">Settings</div>
    <div class="text-gray-400 px-7">Customize your experience.</div>
    <div class="gap-2 flex items-center px-7 py-3">
      Font Size: {{ localAppConfig.fontSize }}px
      <Button size="small" @click="changeFontSize('14px')" label="A"></Button>
      <Button @click="changeFontSize('16px')" label="A"></Button>
    </div>
  </AppLayout>
</template>
