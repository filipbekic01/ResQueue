<script setup lang="ts">
import { useBrokersQuery } from '@/api/broker/brokersQuery'
import AppLayout from '@/layouts/AppLayout.vue'
import Card from 'primevue/card'
import { format } from 'date-fns'
import { useRouter } from 'vue-router'
import Button from 'primevue/button'

const { data: brokers } = useBrokersQuery()
const router = useRouter()

function getRandomWelcomeBack() {
  const messages = [
    'Great to see you again!',
    'Glad to have you back!',
    'Nice to have you here again!',
    'Happy to see you back!',
    "Welcome back, it's good to have you!",
    'It’s wonderful to see you again!',
    'Back in action, I see!',
    "Welcome back! We've missed you!",
    'Hey there! Long time no see!',
    'You’re back! Let’s pick up where we left off!'
  ]

  const randomIndex = Math.floor(Math.random() * messages.length)
  return messages[randomIndex]
}
</script>

<template>
  <AppLayout hide-header>
    <div class="text-3xl font-bold px-7 pt-4">Dashboard</div>
    <div class="text-gray-400 px-7">{{ getRandomWelcomeBack() }}</div>
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
          <Button outlined label="Manage Subscription"></Button>
        </div>
      </div>
    </div>
  </AppLayout>
</template>
