<script lang="ts" setup>
import WebLayout from '@/layouts/WebLayout.vue'

const openDiscord = () => {
  const discordInviteLink = 'discord://discord.gg/age9EsKx5N'
  window.location.href = discordInviteLink
}
</script>

<template>
  <WebLayout>
    <div class="flex grow px-8 py-14">
      <div class="flex grow flex-col items-center rounded-xl bg-white pb-16 pt-8 shadow">
        <div class="text-4xl font-bold">Support Center</div>
        <div class="mt-4 text-xl">Reach out for assistance with any questions or concerns.</div>
        <div class="mt-4 text-slate-500">
          For now, we only support Discord as our main community discussion platform.
        </div>
        <img src="/help.svg" class="my-14 w-40 opacity-25" />
        <Button
          @click="openDiscord"
          style="background-color: rgb(79 70 229); border-color: rgb(79 70 229)"
          label="Discord Channel"
          icon="pi pi-discord"
        ></Button>
      </div>
    </div>
  </WebLayout>
</template>
