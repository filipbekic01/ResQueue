<script setup lang="ts">
import AppLayout from '@/layouts/AppLayout.vue'
import Button from 'primevue/button'

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
    <ul>
      <li>10mb message limit</li>
    </ul>
  </AppLayout>
</template>
