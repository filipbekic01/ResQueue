<script setup lang="ts">
import { useLogoutMutation } from '@/api/auth/logoutMutation'
import { useResendConfirmatioEmailMutation } from '@/api/auth/resendConfirmationEmailMutation'
import { useUpdateUserMutation } from '@/api/auth/updateUserMutation'
import { useIdentity } from '@/composables/identityComposable'
import SubscriptionDialog from '@/dialogs/SubscriptionDialog.vue'
import AppLayout from '@/layouts/AppLayout.vue'
import { useDialog } from 'primevue/usedialog'
import { useToast } from 'primevue/usetoast'
import { ref } from 'vue'
import { useRouter } from 'vue-router'

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
const { mutateAsync: updateUserAsync, isPending: isUpdateUserPending } = useUpdateUserMutation()

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
const updateUserConfigShowBrokerSyncConfirm = (value: boolean) => {
  if (!user.value) {
    return
  }

  updateUserAsync({
    fullName: user.value.fullName,
    config: { ...user.value.userConfig, showBrokerSyncConfirm: value }
  })
}

const updateUserConfigShowMessagesSyncConfirm = (value: boolean) => {
  if (!user.value) {
    return
  }

  updateUserAsync({
    fullName: user.value.fullName,
    config: { ...user.value.userConfig, showMessagesSyncConfirm: value }
  })
}

const openSubscriptionManager = () => {
  dialog.open(SubscriptionDialog, {
    props: {
      header: 'Subscription Manager',
      style: {
        width: '25rem'
      },
      modal: true,
      draggable: false
    }
  })
}
const showEditFullName = ref(false)
const updateUserFullNameAsync = (value?: string) => {
  showEditFullName.value = false

  if (!user.value) {
    return
  }

  if (value == user.value.fullName) {
    return
  }

  updateUserAsync({
    fullName: value,
    config: { ...user.value.userConfig }
  })
}
</script>

<template>
  <AppLayout hide-header>
    <div class="px-7 pt-5 text-3xl font-bold">Control Panel</div>
    <div class="px-7 text-slate-400">Manage settings and access your account details.</div>
    <div class="flex max-w-[60rem] flex-col gap-7 p-7">
      <div class="flex items-start gap-7">
        <div class="grow basis-1/2 rounded-xl border border-gray-200 p-5">
          <div class="text-lg font-medium">E-Mail Address</div>
          <div class="text-slate-500">{{ user?.email }}</div>
        </div>
        <div class="flex grow basis-1/2 items-center rounded-xl border border-gray-200 p-5">
          <div>
            <div class="text-lg font-medium">Full Name</div>
            <div class="flex items-center text-slate-500">
              {{ user?.fullName ? user?.fullName : 'Not set' }}
            </div>
          </div>
          <div class="ms-auto">
            <Button
              v-if="!showEditFullName"
              @click="showEditFullName = true"
              icon="pi pi-pencil"
              class="ms-auto"
              label="Edit"
              outlined
              :loading="isUpdateUserPending"
            ></Button>
            <InputText
              v-else
              :value="user?.fullName"
              placeholder="Enter your name..."
              @change="(e) => updateUserFullNameAsync((e.target as any).value)"
            ></InputText>
          </div>
        </div>
      </div>
      <div class="flex items-start gap-7">
        <div class="flex grow basis-1/2 items-center rounded-xl border border-gray-200 p-5">
          <div>
            <div class="whitespace-nowrap text-lg font-medium">Account Security</div>
            <div v-if="user?.emailConfirmed" class="flex items-center gap-2">
              <i class="pi pi-check-circle text-green-500"></i>
              <span> Your account is validated via e-mail.</span>
            </div>
            <div v-else class="flex items-center gap-2">
              <i class="pi pi-exclamation-circle text-orange-400"></i>
              Validate account via e-mail.
            </div>
          </div>
          <Button
            v-if="!user?.emailConfirmed"
            @click="resendConfirmationEmail"
            icon="pi pi-send"
            class="ms-auto"
            label="Send"
            :loading="isResendConfirmationEmailPending"
            outlined
          ></Button>
        </div>
        <div class="basis-1/2 rounded-xl border border-slate-200 p-5">
          <div class="flex items-center">
            <div>
              <div class="text-lg font-medium">Subscription</div>
              <div v-if="!activeSubscription">
                <div class="flex items-center gap-2">
                  <i class="pi pi-exclamation-circle text-orange-400"></i>Upgrade to unlock all
                  features.
                </div>
              </div>
              <div v-else>
                <i class="pi pi-check-circle me-2 text-green-600"></i>Subscribed to
                <a
                  @click="openSubscriptionManager"
                  class="cursor-pointer border-b border-dashed border-gray-400 hover:border-solid hover:border-blue-500 hover:text-blue-500"
                  >{{ activeSubscription.type === 'essentials' ? 'Essentials' : 'Ultimate' }}
                  <i class="pi pi-pencil" style="font-size: 0.8rem"></i
                ></a>
                plan.
              </div>
            </div>
            <Button
              label="Subscribe"
              icon="pi pi-arrow-right"
              v-if="!activeSubscription"
              @click="router.push({ name: 'pricing' })"
              icon-pos="right"
              class="ms-auto"
            ></Button>
          </div>
        </div>
      </div>
      <div>
        <div class="rounded-xl border border-gray-200 p-5">
          <div class="text-lg font-medium">Configuration</div>

          <div class="mt-4 flex flex-col gap-3 text-slate-600">
            <div class="flex items-center gap-3">
              <ToggleSwitch
                :disabled="isUpdateUserPending"
                :model-value="user?.userConfig.showBrokerSyncConfirm"
                @update:model-value="(value) => updateUserConfigShowBrokerSyncConfirm(value)"
              ></ToggleSwitch>
              Show broker sync confirmation dialog
            </div>
            <div class="flex items-center gap-3">
              <ToggleSwitch
                :disabled="isUpdateUserPending"
                :model-value="user?.userConfig.showMessagesSyncConfirm"
                @update:model-value="(value) => updateUserConfigShowMessagesSyncConfirm(value)"
              ></ToggleSwitch>
              Show message sync confirmation dialog
            </div>
          </div>
        </div>
      </div>
      <div class="flex grow p-3">
        <Button
          label="Logout"
          @click="logout"
          outlined
          icon="pi pi-sign-out"
          class="ms-auto"
        ></Button>
      </div>
    </div>
  </AppLayout>
</template>
