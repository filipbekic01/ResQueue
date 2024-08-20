<script lang="ts" setup>
import WebLayout from '@/layouts/WebLayout.vue'
</script>

<template>
  <WebLayout>
    <div class="flex px-8 py-20 grow">
      <div class="flex flex-col items-center grow">
        <div class="text-4xl font-bold">Support Center</div>
        <div class="mt-4 text-xl">Reach out for assistance with any questions or concerns.</div>
        <div class="mt-4 text-gray-500 mb-8">
          For now, we only support Discord as our main community discussion platform.
        </div>
        <Button
          style="background-color: rgb(79 70 229); border-color: rgb(79 70 229)"
          label="Discord Channel"
          icon="pi pi-discord"
        ></Button>
      </div>
    </div>
  </WebLayout>
</template>
