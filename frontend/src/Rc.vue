<script setup lang="ts">
import { RouterView } from 'vue-router'
import { useIdentity } from './composables/identityComposable'
import DynamicDialog from 'primevue/dynamicdialog'
import Toast from 'primevue/toast'
import ConfirmDialog from 'primevue/confirmdialog'

const {
  query: { data: _ }
} = useIdentity()
</script>

<template>
  <ConfirmDialog></ConfirmDialog>

  <DynamicDialog />

  <Toast position="top-right" />

  <RouterView />
</template>
