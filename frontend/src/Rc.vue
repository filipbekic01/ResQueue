<script setup lang="ts">
import ConfirmDialog from 'primevue/confirmdialog'
import DynamicDialog from 'primevue/dynamicdialog'
import Toast from 'primevue/toast'
import { RouterView } from 'vue-router'
</script>

<template>
  <ConfirmDialog></ConfirmDialog>

  <DynamicDialog />

  <Toast position="top-right" />

  <RouterView />
</template>
