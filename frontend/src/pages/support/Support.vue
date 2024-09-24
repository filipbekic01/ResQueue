<script lang="ts" setup>
import helpLogoUrl from '@/assets/help.svg'
import WebLayout from '@/layouts/WebLayout.vue'
import { useHead } from '@unhead/vue'

const openDiscord = () => {
  const discordInviteLink = 'discord://discord.gg/age9EsKx5N'
  window.location.href = discordInviteLink
}

useHead({
  title: 'Support Center - Queue Management Assistance',
  meta: [
    {
      name: 'description',
      content: 'Get help and support for all your queue management questions through our dedicated Discord channel.'
    },
    {
      name: 'keywords',
      content: 'support, queue management, discord support, help center, customer service, assistance'
    },
    { name: 'robots', content: 'index, follow' },
    { property: 'og:title', content: 'Support Center - Queue Management Assistance' },
    {
      property: 'og:description',
      content: 'Join our Discord channel for direct support and community discussions about queue management.'
    },
    { property: 'og:image', content: 'https://resqueue.io/support-preview-image.jpg' }, // Replace with your support page image URL
    { property: 'og:url', content: 'https://resqueue.io/support' },
    { property: 'og:type', content: 'website' },
    { name: 'twitter:card', content: 'summary_large_image' },
    { name: 'twitter:title', content: 'Support Center - Queue Management Assistance' },
    {
      name: 'twitter:description',
      content: 'Reach out for assistance with queue management via our Discord support channel.'
    },
    { name: 'twitter:image', content: 'https://resqueue.io/twitter-support-image.jpg' }
  ],
  link: [
    { rel: 'canonical', href: 'https://resqueue.io/support' } // Canonical URL for this page
  ],
  htmlAttrs: {
    lang: 'en'
  }
})
</script>

<template>
  <WebLayout>
    <div class="flex grow px-8 pt-14">
      <div class="flex grow flex-col items-center rounded-xl bg-white pb-16 pt-8 shadow-md">
        <div class="text-4xl font-bold">Support Center</div>
        <div class="mt-4 text-xl">Reach out for assistance with any questions or concerns.</div>
        <div class="mt-4 text-slate-500">
          For now, we only support Discord as our main community discussion platform.
        </div>
        <img :src="helpLogoUrl" class="my-14 w-40 opacity-25" />
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
