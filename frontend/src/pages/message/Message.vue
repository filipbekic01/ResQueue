<script lang="ts" setup>
import { useIdentity } from '@/composables/identityComposable'
import AppLayout from '@/layouts/AppLayout.vue'

const { user } = useIdentity()
</script>

<template>
  <AppLayout>
    {{ user }}
  </AppLayout>
</template>
