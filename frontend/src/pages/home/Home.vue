<script lang="ts" setup>
import awsLogoUrl from '@/assets/aws.svg'
import azureLogoUrl from '@/assets/azure.svg'
import mtLogoUrl from '@/assets/masstransit.svg'
import mdLogoUrl from '@/assets/md.svg'
import { useIdentity } from '@/composables/identityComposable'
import WebLayout from '@/layouts/WebLayout.vue'
import { useHead } from '@unhead/vue'
import Button from 'primevue/button'
import { computed } from 'vue'
import { useRouter } from 'vue-router'

const router = useRouter()

const {
  query: { data: user }
} = useIdentity()

useHead(
  computed(() => ({
    title: 'Effortless Queue Healing - Dead-Letter Queue Management',
    meta: [
      {
        name: 'description',
        content:
          'Seamlessly manage dead-letter queues with ease. Rescue messages, manage brokers, and improve workflow with efficient queue management tools.'
      },
      {
        name: 'keywords',
        content:
          'dead-letter queue management, message requeue, multiple brokers, RabbitMQ, Azure Service Bus, Amazon SQS'
      },
      { name: 'robots', content: 'index, follow' }, // For SEO crawling instructions
      { property: 'og:title', content: 'Effortless Queue Healing - Dead-Letter Queue Management' },
      {
        property: 'og:description',
        content:
          'Manage dead-letter queues effortlessly. Requeue messages and connect multiple brokers with our powerful queue management tool.'
      },
      { property: 'og:image', content: 'https://resqueue.io/your-preview-image.jpg' }, // Social media sharing image
      { property: 'og:url', content: 'https://resqueue.io' }, // The canonical URL for this page
      { property: 'og:type', content: 'website' }, // Open Graph type, 'website' works for a general page
      { name: 'twitter:card', content: 'summary_large_image' }, // Twitter card settings
      { name: 'twitter:title', content: 'Effortless Queue Healing - Dead-Letter Queue Management' },
      {
        name: 'twitter:description',
        content: 'Rescue messages and manage multiple brokers effortlessly with our queue management platform.'
      },
      { name: 'twitter:image', content: 'https://resqueue.io/your-twitter-image.jpg' }
    ],
    link: [
      { rel: 'canonical', href: 'https://resqueue.io' } // Ensures proper URL for search engines
    ],
    htmlAttrs: {
      lang: 'en' // Language setting for the page
    }
  }))
)
</script>

<template>
  <WebLayout>
    <div class="flex px-8 py-24">
      <div class="flex grow flex-col items-center justify-center">
        <h1 class="mx-auto text-4xl font-bold">Unified SQL Broker Manager</h1>
        <h2 class="mx-auto mt-4 text-center text-xl">
          A robust web-based interface for effortlessly managing various <br />SQL broker implementations, including
          MassTransit.
        </h2>

        <div class="mt-8 flex">
          <Button
            v-if="!user"
            size="large"
            label="Begin Your Journey"
            raised
            icon="pi pi-arrow-right"
            icon-pos="right"
            @click="router.push({ name: 'pricing' })"
          ></Button>
          <RouterLink v-else :to="{ name: 'app' }"
            ><Button size="large" label="Go to Dashboard" raised icon="pi pi-arrow-right" icon-pos="right"></Button
          ></RouterLink>
        </div>
      </div>
    </div>

    <div class="mx-auto flex w-[400px] items-center gap-4">
      <div class="flex-1 rounded-2xl border-slate-400 bg-white p-4 text-slate-700 shadow">
        <div class="flex items-center gap-2 text-xl font-bold">
          <div class="w-8 rounded-lg bg-gray-200 p-1">
            <img :src="mtLogoUrl" />
          </div>
          <h3>MassTransit</h3>
          <i class="pi pi-check-circle ms-auto text-emerald-600" style="font-size: 1.15rem"></i>
        </div>
        <ul class="mt-2">
          <li>
            <i class="pi pi-check mx-1 text-emerald-600"></i>
            MassTransit.SqlTransport.PostgreSQL
          </li>
          <li>
            <i class="pi pi-check mx-1 text-emerald-600"></i>
            MassTransit.SqlTransport.SqlServer
          </li>
        </ul>
      </div>
    </div>

    <div class="mt-6 text-center text-gray-600">
      Interested in support for additional SQL transport libraries?
      <RouterLink :to="'support'">Let's discuss it.</RouterLink>
    </div>

    <div class="mt-16 flex gap-4">
      <div class="flex-1 rounded-2xl border-slate-400 bg-white p-6 text-slate-700 shadow">
        <div class="text-xl font-bold">
          <i class="pi pi-th-large mr-3" style="font-size: 1.15rem"></i>Connect Multiple Brokers
        </div>
        <div class="ms-8 mt-3 text-lg">
          Seamlessly manage and connect multiple brokers, ensuring a unified and efficient queue management experience.
        </div>
      </div>
      <div class="flex-1 rounded-2xl border-slate-400 bg-white p-6 text-slate-700 shadow">
        <div class="text-xl font-bold">
          <i class="pi pi-send mr-3" style="font-size: 1.15rem"></i>Requeue Any Message
        </div>
        <div class="ms-8 mt-3 text-lg">
          Enjoy the flexibility to requeue any message, not just the first one in the stack, ensuring greater control
          over your workflow.
        </div>
      </div>
    </div>
    <div class="mt-4 flex gap-4">
      <div class="flex-1 rounded-2xl border-slate-400 bg-white p-6 text-slate-700 shadow">
        <div class="text-xl font-bold">
          <i class="pi pi-paperclip mr-3" style="font-size: 1.15rem"></i>Message IDs for Easy Sharing
        </div>
        <div class="ms-8 mt-3 text-lg">
          Each message is assigned a unique ID, making it simple to share and collaborate with colleagues.
        </div>
      </div>
      <div class="flex-1 rounded-2xl border-slate-400 bg-white p-6 text-slate-700 shadow">
        <div class="text-xl font-bold">
          <i class="pi pi-check-circle mr-3" style="font-size: 1.15rem"></i>Mark Messages as Reviewed
        </div>
        <div class="ms-8 mt-3 text-lg">
          Improve organization by marking messages as reviewed, making it easier to keep track of processed queues.
        </div>
      </div>
    </div>

    <div class="mb-8 mt-20 text-center text-lg">Our Clients</div>

    <div class="flex items-center justify-center">
      <a href="https://mydesigns.io" target="_blank">
        <img :src="mdLogoUrl" class="w-64 cursor-pointer opacity-100" />
      </a>
    </div>
  </WebLayout>
</template>
