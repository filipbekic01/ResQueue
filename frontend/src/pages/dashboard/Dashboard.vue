<script setup lang="ts">
import { useLogoutMutation } from '@/api/auth/logoutMutation'
import { useIdentity } from '@/composables/identityComposable'
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

const {
  query: { data: user }
} = useIdentity()

const { mutateAsync: logoutAsync } = useLogoutMutation()

const logout = () => {
  logoutAsync().then(() => {
    window.location.href = '/'
  })
}
</script>

<template>
  <AppLayout hide-header>
    <div class="text-3xl font-bold px-7 pt-4">Dashboard</div>
    <div class="text-slate-400 px-7">{{ getRandomWelcomeBack() }}</div>
    <div class="flex items-start pt-10">
      <div class="px-7">
        <div class="text-lg font-semibold">Unique ID</div>
        <div class="text-slate-500">{{ user?.id }}</div>
      </div>
      <div class="px-7">
        <div class="text-lg font-semibold">E-Mail</div>
        <div class="text-slate-500">{{ user?.email }}</div>
      </div>
    </div>
    <div class="px-7 pt-7">
      <div class="text-lg font-semibold">Subscription</div>
      <div class="text-slate-500">{{ user?.subscriptionId }}</div>
      <Button
        v-if="!user?.isSubscribed"
        class="mt-3"
        label="Upgrade Account"
        size="small"
        icon="pi pi-arrow-up"
        severity="success"
      ></Button>
      <div v-else>
        <i class="pi pi-check-circle text-green-600 mt-3 me-2"></i>Subscribed to
        {{ user.subscriptionId }} plan.
      </div>
    </div>
    <div class="px-7 pt-7">
      <div class="text-lg font-semibold">Configuration</div>
      <div class="text-slate-500">{{ user?.userConfig }}</div>
    </div>
    <div class="px-7 mt-16">
      <Button label="Logout" @click="logout" outlined icon="pi pi-sign-out  "></Button>
    </div>
  </AppLayout>
</template>
