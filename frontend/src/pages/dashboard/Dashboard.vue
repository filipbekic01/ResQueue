<script setup lang="ts">
import { useLogoutMutation } from '@/api/auth/logoutMutation'
import { useResendConfirmatioEmailMutation } from '@/api/auth/resendConfirmationEmailMutation'
import { useIdentity } from '@/composables/identityComposable'
import SubscriptionDialog from '@/dialogs/SubscriptionDialog.vue'
import AppLayout from '@/layouts/AppLayout.vue'
import Checkbox from 'primevue/checkbox'
import { useDialog } from 'primevue/usedialog'
import { useToast } from 'primevue/usetoast'
import { useRouter } from 'vue-router'

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
const message = messages[randomIndex]

const router = useRouter()
const dialog = useDialog()

const {
  query: { data: user },
  activeSubscription
} = useIdentity()

const toast = useToast()
const { mutateAsync: logoutAsync } = useLogoutMutation()
const { mutateAsync: resendConfirmationEmailAsync, isPending: isResendConfirmationEmailPending } =
  useResendConfirmatioEmailMutation()

const logout = () => {
  logoutAsync().then(() => {
    window.location.href = '/'
  })
}

const resendConfirmationEmail = () => {
  const email = user.value?.email

  if (email) {
    resendConfirmationEmailAsync({ email }).then(() => {
      toast.add({
        severity: 'success',
        summary: 'Confirmation Sent',
        detail: 'Please check your e-mail inbox.',
        life: 6000
      })
    })
  } else {
    toast.add({
      severity: 'error',
      summary: 'E-Mail Missing',
      detail: 'Could not detect your e-mail address.',
      life: 6000
    })
  }
}

const updateUserConfig = (prop: string, value: any) => {
  console.log(prop, value)
}

const openSubscriptionManager = () => {
  dialog.open(SubscriptionDialog, {
    props: {
      header: 'Subscription Manager',
      style: {
        width: '25rem'
      },
      modal: true
    }
  })
}
</script>

<template>
  <AppLayout hide-header>
    <div class="text-3xl font-bold px-7 pt-5">Dashboard</div>
    <div class="text-slate-400 px-7">{{ message }}</div>
    <div class="p-7 flex flex-col gap-7 xl:w-2/3">
      <div class="flex items-start gap-7">
        <div class="grow basis-1/2 border-b border-slate-200 px-3 py-3">
          <div class="text-lg font-semibold">E-Mail Address</div>
          <div class="text-slate-500">{{ user?.email }}</div>
        </div>
        <div class="grow basis-1/2 border-b border-slate-200 px-3 py-3">
          <div class="text-lg font-semibold whitespace-nowrap">
            {{ user?.emailConfirmed ? 'Account Confirmed' : 'Confirmation Pending' }}
          </div>
          <template v-if="user?.emailConfirmed">
            <span
              ><i class="pi pi-check-circle text-green-500"></i> Your account is secured now.</span
            >
          </template>
          <template v-else>
            <i class="pi pi-exclamation-circle text-orange-400 me-1"></i>
            <span
              v-if="!isResendConfirmationEmailPending"
              class="text-slate-500 border-b border-slate-300 border-dashed hover:text-blue-500 hover:border-blue-500 cursor-pointer whitespace-nowrap"
              @click="resendConfirmationEmail"
              >Click to send confirmation link
            </span>
            <span v-else class="whitespace-nowrap"> Sending the e-mail... </span>
          </template>
        </div>
      </div>
      <div>
        <div class="border-b border-slate-200 px-3 py-3">
          <div class="text-lg font-semibold">Subscription</div>
          <div v-if="!activeSubscription">
            <div>
              <i class="pi pi-exclamation-circle text-orange-400 me-1"></i>Upgrade to unlock all
              features.
            </div>
            <Button
              class="mt-3"
              label="Subscribe Now"
              icon="pi pi-arrow-right"
              @click="router.push({ name: 'pricing' })"
              icon-pos="right"
            ></Button>
          </div>
          <div v-else>
            <i class="pi pi-check-circle text-green-600 me-2"></i>Subscribed to
            <a
              @click="openSubscriptionManager"
              class="border-b border-dashed border-gray-400 cursor-pointer hover:border-solid hover:border-blue-500 hover:text-blue-500"
              >{{ activeSubscription.type === 'essentials' ? 'Essentials' : 'Ultimate' }}</a
            >
            plan
          </div>
        </div>
      </div>
      <div>
        <div class="border-b border-slate-200 px-3 py-3">
          <div class="text-lg font-semibold">Configuration</div>

          <div class="mt-3">Dialogs</div>
          <div class="text-slate-600 flex flex-col gap-2 mt-2">
            <div class="flex items-center gap-1.5">
              <Checkbox
                :model-value="user?.userConfig.showBrokerSyncConfirm ?? false"
                @update:model-value="updateUserConfig"
              ></Checkbox>
              Show broker sync confirm dialog
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
