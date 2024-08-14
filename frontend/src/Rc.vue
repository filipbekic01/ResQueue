<script setup lang="ts">
import { RouterView } from 'vue-router'
import { useMeQuery } from './api/auth/meQuery'
import { watch } from 'vue'
import { useIdentity } from './composables/identityComposable'
import ConfirmPopup from 'primevue/confirmpopup'
import DynamicDialog from 'primevue/dynamicdialog'
import Toast from 'primevue/toast'
import ConfirmDialog from 'primevue/confirmdialog'

const { data: me } = useMeQuery()
const { user } = useIdentity()

watch(
  () => me.value,
  (value) => {
    if (!value) {
      return
    }
    user.value = value
  }
)
</script>

<template>
  <!-- <ConfirmPopup></ConfirmPopup> -->
  <ConfirmDialog></ConfirmDialog>
  <DynamicDialog />
  <Toast position="top-right" />

  <RouterView />
</template>
