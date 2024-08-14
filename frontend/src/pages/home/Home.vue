<script lang="ts" setup>
import { useIdentity } from '@/composables/identityComposable'
import WebLayout from '@/layouts/WebLayout.vue'
import Button from 'primevue/button'

const { user } = useIdentity()
</script>

<template>
  <WebLayout>
    <div class="rounded-t-xl xxx">
      <div class="text-center pt-20">
        <div class="text-4xl font-bold mx-auto text-center">Welcome to Resqueue</div>
        <div class="mx-auto mt-4 text-xl text-center">
          Effortless Queue Management. <br />Streamline, Manage, and Requeue with Ease.
        </div>
      </div>
      <div class="flex mt-4">
        <div class="border border-zinc-400 rounded-lg p-0.5 mx-auto">
          <Button v-if="!user" label="Start Now"></Button>
          <RouterLink v-else :to="{ name: 'app' }"><Button label="Open App"></Button></RouterLink>
        </div>
      </div>
    </div>

    <div class="flex gap-4 mt-20">
      <div class="bg-gray-200 flex-1 text-gray-700 rounded-2xl p-6">
        <div class="text-xl font-bold flex gap-2">
          <div class="bg-[#ff6600] rounded-lg w-7 p-1.5"><img class="w-full" src="/rmq.svg" /></div>
          RabbitMQ
        </div>
        <div class="mt-3">Released on 2024/12/01</div>
      </div>
      <div class="bg-gray-200 flex-1 text-gray-700 rounded-2xl p-6">
        <div class="text-xl font-bold flex gap-2">
          <div class="bg-[#007FFF] rounded-lg w-7 p-1.5">
            <img class="w-full" src="/azure.svg" />
          </div>
          Azure Service Bus
        </div>
        <div class="mt-3">Under active development.</div>
      </div>
      <div class="bg-gray-200 flex-1 text-gray-700 rounded-2xl p-6">
        <div class="text-xl font-bold flex gap-2">
          <div class="bg-[#fff] rounded-lg w-7 p-1.5"><img class="w-full" src="/aws.svg" /></div>
          Amazon SQS
        </div>
        <div class="mt-3">Under active development.</div>
      </div>
    </div>

    <!-- <div class="text-center mt-20 mb-8 text-xl">Highlighted Features</div> -->

    <div class="flex gap-4 mt-16">
      <div class="bg-gray-200 flex-1 text-gray-700 rounded-2xl p-6">
        <div class="text-xl font-bold">
          <i class="pi pi-check-circle mr-3" style="font-size: 1.15rem"></i>Reliable Performance
        </div>
        <div class="mt-3 ms-8">
          Depend on consistent, uninterrupted service that ensures your queues are managed with
          precision, day in and day out.
        </div>
      </div>
      <div class="bg-gray-200 flex-1 text-gray-700 rounded-2xl p-6">
        <div class="text-xl font-bold">
          <i class="pi pi-bolt mr-3" style="font-size: 1.15rem"></i>Lightning-Fast Processing
        </div>
        <div class="mt-3 ms-8">
          Experience rapid queue management with optimized processing, ensuring that your tasks are
          completed in record time.
        </div>
      </div>
      <div class="bg-gray-200 flex-1 text-gray-700 rounded-2xl p-6">
        <div class="text-xl font-bold">
          <i class="pi pi-lock mr-3" style="font-size: 1.15rem"></i>Enhanced Security
        </div>
        <div class="mt-3 ms-8">
          Keep your data safe with advanced security protocols that protect your queues and ensure
          the integrity of your transactions.
        </div>
      </div>
    </div>
    <div class="flex gap-4 mt-4">
      <div class="bg-gray-200 flex-1 text-gray-700 rounded-2xl p-6">
        <div class="text-xl font-bold">
          <i class="pi pi-cog mr-3" style="font-size: 1.15rem"></i>Intuitive Interface
        </div>
        <div class="mt-3 ms-8">
          Navigate with ease using a user-friendly interface designed for efficiency, making queue
          management a seamless experience.
        </div>
      </div>
      <div class="bg-gray-200 flex-1 text-gray-700 rounded-2xl p-6">
        <div class="text-xl font-bold">
          <i class="pi pi-chart-line mr-3" style="font-size: 1.15rem"></i>Scalable Solution
        </div>
        <div class="mt-3 ms-8">
          Grow with confidence, knowing that Resqueue can scale alongside your business, handling
          increased loads with ease.
        </div>
      </div>
      <div class="bg-gray-200 flex-1 text-gray-700 rounded-2xl p-6">
        <div class="text-xl font-bold">
          <i class="pi pi-question-circle mr-3" style="font-size: 1.15rem"></i>Comprehensive Support
        </div>
        <div class="mt-3 ms-8">
          Benefit from dedicated support and detailed documentation, ensuring you have the help you
          need, whenever you need it.
        </div>
      </div>
    </div>
  </WebLayout>
</template>
