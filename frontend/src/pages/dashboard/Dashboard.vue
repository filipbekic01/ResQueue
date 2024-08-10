<script setup lang="ts">
import { useBrokersQuery } from '@/api/broker/brokersQuery'
import AppLayout from '@/layouts/AppLayout.vue'
import Card from 'primevue/card'
import { formatDistanceToNow } from 'date-fns'
import { useRouter } from 'vue-router'

const { data: brokers } = useBrokersQuery()
const router = useRouter()

const openBroker = (id: string) => {
  router.push({
    name: 'broker',
    params: {
      brokerId: id
    }
  })
}
</script>

<template>
  <AppLayout>
    <div class="flex m-4">
      <div class="max-w-sm overflow-hidden border border-gray-200 rounded-2xl p-6">
        <div class="flex justify-between items-center">
          <div class="text-lg font-semibold text-gray-800">Active Subscription</div>
          <div class="text-sm text-gray-500">
            <!-- Add a subscription icon here if needed -->
          </div>
        </div>
        <div class="mt-4">
          <p class="text-gray-600">Plan: <span class="font-medium text-gray-800">Premium</span></p>
          <p class="text-gray-600">
            Status: <span class="font-medium text-green-500">Active</span>
          </p>
          <p class="text-gray-600">
            Next Billing: <span class="font-medium text-gray-800">August 25, 2024</span>
          </p>
        </div>
        <div class="mt-6">
          <button
            class="bg-blue-500 text-white font-semibold py-2 px-4 rounded hover:bg-blue-600 focus:outline-none"
          >
            Manage Subscription
          </button>
        </div>
      </div>
    </div>
  </AppLayout>
</template>
