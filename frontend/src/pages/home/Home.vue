<script lang="ts" setup>
import { useIdentity } from '@/composables/identityComposable'
import WebLayout from '@/layouts/WebLayout.vue'
import Button from 'primevue/button'
import { useRouter } from 'vue-router'

const router = useRouter()

const { user } = useIdentity()
</script>

<template>
  <WebLayout>
    <div class="flex px-8 py-24">
      <div class="flex flex-col justify-center items-center grow">
        <div class="text-4xl font-bold mx-auto">Effortless Queue Healing</div>
        <div class="mx-auto mt-4 text-xl">
          Dead-Letter Queue (DLQ) Management • Manage and Re(s)queue with Ease
        </div>

        <div class="flex mt-8">
          <Button
            v-if="!user"
            size="large"
            label="Begin Your Journey"
            icon="pi pi-arrow-right"
            icon-pos="right"
            @click="router.push({ name: 'pricing' })"
          ></Button>
          <RouterLink v-else :to="{ name: 'app' }"
            ><Button
              size="large"
              label="Go to Dashboard"
              icon="pi pi-arrow-right"
              icon-pos="right"
            ></Button
          ></RouterLink>
        </div>
      </div>
    </div>

    <div class="flex gap-4 items-center">
      <div class="shadow bg-white border-slate-400 flex-1 text-slate-700 rounded-2xl p-4">
        <div class="text-xl font-bold flex gap-2 items-center">
          <div class="bg-[#ff6600] rounded-lg w-8 p-1.5">
            <img src="/rmq.svg" />
          </div>
          RabbitMQ
          <i class="pi pi-check-circle ms-auto text-emerald-600" style="font-size: 1.15rem"></i>
        </div>
      </div>

      <div class="shadow bg-white border-slate-400 flex-1 text-slate-700 rounded-2xl p-4">
        <div class="text-xl font-bold flex gap-2 items-center">
          <div class="bg-[#007FFF] rounded-lg w-8 p-1.5">
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

      <div class="shadow bg-white border-slate-400 flex-1 text-slate-700 rounded-2xl p-4">
        <div class="text-xl font-bold flex gap-2 items-center">
          <div class="bg-[#fff] rounded-lg w-8 p-1.5 border border-slate-300">
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

    <div class="flex gap-4 mt-16">
      <div class="shadow bg-white border-slate-400 flex-1 text-slate-700 rounded-2xl p-6">
        <div class="text-xl font-bold">
          <i class="pi pi-th-large mr-3" style="font-size: 1.15rem"></i>Connect Multiple Brokers
        </div>
        <div class="mt-3 ms-8 text-lg">
          Seamlessly manage and connect multiple brokers, ensuring a unified and efficient queue
          management experience.
        </div>
      </div>
      <div class="shadow bg-white border-slate-400 flex-1 text-slate-700 rounded-2xl p-6">
        <div class="text-xl font-bold">
          <i class="pi pi-send mr-3" style="font-size: 1.15rem"></i>Requeue Any Message
        </div>
        <div class="mt-3 ms-8 text-lg">
          Enjoy the flexibility to requeue any message, not just the first one in the stack,
          ensuring greater control over your workflow.
        </div>
      </div>
    </div>
    <div class="flex gap-4 mt-4">
      <div class="shadow bg-white border-slate-400 flex-1 text-slate-700 rounded-2xl p-6">
        <div class="text-xl font-bold">
          <i class="pi pi-paperclip mr-3" style="font-size: 1.15rem"></i>Message IDs for Easy
          Sharing
        </div>
        <div class="mt-3 ms-8 text-lg">
          Each message is assigned a unique ID, making it simple to share and collaborate with
          colleagues.
        </div>
      </div>
      <div class="shadow bg-white border-slate-400 flex-1 text-slate-700 rounded-2xl p-6">
        <div class="text-xl font-bold">
          <i class="pi pi-check-circle mr-3" style="font-size: 1.15rem"></i>Mark Messages as
          Reviewed
        </div>
        <div class="mt-3 ms-8 text-lg">
          Improve organization by marking messages as reviewed, making it easier to keep track of
          processed queues.
        </div>
      </div>
    </div>

    <div class="text-center mt-20 mb-8 text-lg">Our Clients</div>

    <div class="flex items-center justify-center">
      <a href="https://mydesigns.io" target="_blank">
        <img src="/md.svg" class="opacity-100 w-64 cursor-pointer" />
      </a>
    </div>
  </WebLayout>
</template>
