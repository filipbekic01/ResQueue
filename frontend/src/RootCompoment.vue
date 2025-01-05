<script setup lang="ts">
import ConfirmDialog from 'primevue/confirmdialog'
import DynamicDialog from 'primevue/dynamicdialog'
import Toast from 'primevue/toast'
import { onBeforeMount } from 'vue'
import { RouterView } from 'vue-router'
import { useUserSettings } from './composables/userSettingsComposable'

const { init } = useUserSettings()

onBeforeMount(() => {
  init()
})
</script>

<template>
  <ConfirmDialog></ConfirmDialog>

  <DynamicDialog />

  <Toast position="top-right" />

  <RouterView />
</template>
