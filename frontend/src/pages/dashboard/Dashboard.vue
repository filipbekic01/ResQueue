<script setup lang="ts">
import AppLayout from '@/layouts/AppLayout.vue'

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
    <div class="flex flex-col">
      <div class="text-3xl font-bold px-7 pt-4">Dashboard</div>
      <div class="text-slate-400 px-7">{{ getRandomWelcomeBack() }}</div>
      <div class="flex mt-5">
        <div class="basis-1/3 shrink-0 px-7">
          <div class="text-lg">Account Subscription</div>
        </div>
        <div class="basis-1/3 shrink-0 px-7">
          <div class="text-lg">Platform Limitations</div>
          <ul>
            <li>1mb per message limit</li>
          </ul>
        </div>
        <div class="basis-1/3 shrink-0 px-7">
          <div class="text-lg">Changelog</div>
          <div>v1.0.1</div>
          <div>- text</div>
        </div>
      </div>
    </div>
  </AppLayout>
</template>
