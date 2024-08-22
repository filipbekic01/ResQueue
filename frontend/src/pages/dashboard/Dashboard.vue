<script setup lang="ts">
import { useLogoutMutation } from '@/api/auth/logoutMutation'
import { useIdentity } from '@/composables/identityComposable'
import AppLayout from '@/layouts/AppLayout.vue'
import Checkbox from 'primevue/checkbox'

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
    <div class="text-3xl font-bold px-7 pt-5">Dashboard</div>
    <div class="text-slate-400 px-7">{{ getRandomWelcomeBack() }}</div>
    <div class="p-7 flex flex-col gap-7 xl:w-2/3">
      <div class="flex items-start gap-7">
        <div class="grow basis-1/2 border-b border-slate-200 px-3 py-3">
          <div class="text-lg font-semibold">Account ID</div>
          <div class="text-slate-500">{{ user?.id }}</div>
        </div>
        <div class="grow basis-1/2 border-b border-slate-200 px-3 py-3">
          <div class="text-lg font-semibold">E-Mail</div>
          <div class="text-slate-500">{{ user?.email }}</div>
        </div>
      </div>
      <div>
        <div class="border-b border-slate-200 px-3 py-3">
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
            {{ user.subscriptionPlan === 'essentials' ? 'Essentials' : 'Ultimate' }} plan
          </div>
        </div>
      </div>
      <div>
        <div class="border-b border-slate-200 px-3 py-3">
          <div class="text-lg font-semibold">Configuration</div>

          <div class="mt-3">Dialogs</div>
          <div class="text-slate-600 flex flex-col gap-2 mt-2">
            <div class="flex items-center gap-1.5">
              <Checkbox :model-value="user?.userConfig.showBrokerSyncConfirm"></Checkbox> Show
              broker sync confirm dialog
            </div>
            <div class="flex items-center gap-1.5">
              <Checkbox :model-value="user?.userConfig.showMessagesSyncConfirm"></Checkbox> Show
              message sync confirm dialog
            </div>
          </div>
        </div>
      </div>
      <div>
        <Button label="Logout" @click="logout" outlined icon="pi pi-sign-out"></Button>
      </div>
    </div>
  </AppLayout>
</template>
