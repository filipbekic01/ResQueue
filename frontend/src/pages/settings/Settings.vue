<script lang="ts" setup>
import { useIdentity } from '@/composables/identityComposable'
import AppLayout from '@/layouts/AppLayout.vue'

const { user } = useIdentity()
</script>

<template>
  <AppLayout hide-header>
    <div class="text-3xl font-bold px-7 pt-4">Settings</div>
    <div class="text-gray-400 px-7">Customize your experience.</div>
    <div class="flex items-start pt-10">
      <div class="px-7">
        <div class="text-lg font-semibold">Unique ID</div>
        <div class="text-gray-500">{{ user?.id }}</div>
      </div>
      <div class="px-7">
        <div class="text-lg font-semibold">E-Mail</div>
        <div class="text-gray-500">{{ user?.email }}</div>
      </div>
    </div>
    <div class="px-7 pt-7">
      <div class="text-lg font-semibold">Subscription</div>
      <div class="text-gray-500">Free Trial</div>
      <Button
        class="mt-3"
        label="Upgrade Account"
        size="small"
        icon="pi pi-arrow-up"
        severity="success"
      ></Button>
    </div>
    <div class="px-7 pt-7">
      <div class="text-lg font-semibold">Configuration</div>
      <div class="text-gray-500">{{ user?.userConfig }}</div>
    </div>
  </AppLayout>
</template>
