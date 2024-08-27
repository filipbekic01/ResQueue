<script lang="ts" setup>
import { useIdentity } from '@/composables/identityComposable'
import WebLayout from '@/layouts/WebLayout.vue'
import Button from 'primevue/button'
import { useRouter } from 'vue-router'

const router = useRouter()

const {
  query: { data: user }
} = useIdentity()
</script>

<template>
  <WebLayout>
    <div class="flex px-8 py-24">
      <div class="flex grow flex-col items-center justify-center">
        <div class="mx-auto text-4xl font-bold">Effortless Queue Healing</div>
        <div class="mx-auto mt-4 text-xl">
          Dead-Letter Queue Management — Manage and Rescue with Ease
        </div>

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
            ><Button
              size="large"
              label="Go to Dashboard"
              raised
              icon="pi pi-arrow-right"
              icon-pos="right"
            ></Button
          ></RouterLink>
        </div>
      </div>
    </div>

    <div class="flex items-center gap-4">
      <div class="flex-1 rounded-2xl border-slate-400 bg-white p-4 text-slate-700 shadow">
        <div class="flex items-center gap-2 text-xl font-bold">
          <div class="w-8 rounded-lg bg-[#ff6600] p-1.5">
            <img src="/rmq.svg" />
          </div>
          RabbitMQ
          <i class="pi pi-check-circle ms-auto text-emerald-600" style="font-size: 1.15rem"></i>
        </div>
      </div>

      <div class="flex-1 rounded-2xl border-slate-400 bg-white p-4 text-slate-700 shadow">
        <div class="flex items-center gap-2 text-xl font-bold">
          <div class="w-8 rounded-lg bg-[#007FFF] p-1.5">
            <img src="/azure.svg" />
          </div>
          Azure Service Bus
          <i
            v-tooltip.top="'Coming soon...'"
            class="pi pi-calendar-clock ms-auto text-slate-600"
            style="font-size: 1.15rem"
          ></i>
        </div>
      </div>

      <div class="flex-1 rounded-2xl border-slate-400 bg-white p-4 text-slate-700 shadow">
        <div class="flex items-center gap-2 text-xl font-bold">
          <div class="w-8 rounded-lg border border-slate-300 bg-[#fff] p-1.5">
            <img src="/aws.svg" />
          </div>
          Amazon SQS
          <i
            v-tooltip.top="'Coming soon...'"
            class="pi pi-calendar-clock ms-auto text-slate-600"
            style="font-size: 1.15rem"
          ></i>
        </div>
      </div>
    </div>

    <div class="mt-16 flex gap-4">
      <div class="flex-1 rounded-2xl border-slate-400 bg-white p-6 text-slate-700 shadow">
        <div class="text-xl font-bold">
          <i class="pi pi-th-large mr-3" style="font-size: 1.15rem"></i>Connect Multiple Brokers
        </div>
        <div class="ms-8 mt-3 text-lg">
          Seamlessly manage and connect multiple brokers, ensuring a unified and efficient queue
          management experience.
        </div>
      </div>
      <div class="flex-1 rounded-2xl border-slate-400 bg-white p-6 text-slate-700 shadow">
        <div class="text-xl font-bold">
          <i class="pi pi-send mr-3" style="font-size: 1.15rem"></i>Requeue Any Message
        </div>
        <div class="ms-8 mt-3 text-lg">
          Enjoy the flexibility to requeue any message, not just the first one in the stack,
          ensuring greater control over your workflow.
        </div>
      </div>
    </div>
    <div class="mt-4 flex gap-4">
      <div class="flex-1 rounded-2xl border-slate-400 bg-white p-6 text-slate-700 shadow">
        <div class="text-xl font-bold">
          <i class="pi pi-paperclip mr-3" style="font-size: 1.15rem"></i>Message IDs for Easy
          Sharing
        </div>
        <div class="ms-8 mt-3 text-lg">
          Each message is assigned a unique ID, making it simple to share and collaborate with
          colleagues.
        </div>
      </div>
      <div class="flex-1 rounded-2xl border-slate-400 bg-white p-6 text-slate-700 shadow">
        <div class="text-xl font-bold">
          <i class="pi pi-check-circle mr-3" style="font-size: 1.15rem"></i>Mark Messages as
          Reviewed
        </div>
        <div class="ms-8 mt-3 text-lg">
          Improve organization by marking messages as reviewed, making it easier to keep track of
          processed queues.
        </div>
      </div>
    </div>

    <div class="mb-8 mt-20 text-center text-lg">Our Clients</div>

    <div class="flex items-center justify-center">
      <a href="https://mydesigns.io" target="_blank">
        <img src="/md.svg" class="w-64 cursor-pointer opacity-100" />
      </a>
    </div>
  </WebLayout>
</template>
