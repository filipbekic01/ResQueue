<script setup lang="ts">
import { useLogoutMutation } from '@/api/auth/logoutMutation'
import { useResendConfirmatioEmailMutation } from '@/api/auth/resendConfirmationEmailMutation'
import { useUpdateUserAvatarMutation } from '@/api/auth/updateUserAvatarMutation'
import { useUpdateUserMutation } from '@/api/auth/updateUserMutation'
import { useIdentity } from '@/composables/identityComposable'
import SubscriptionDialog from '@/dialogs/SubscriptionDialog.vue'
import AppLayout from '@/layouts/AppLayout.vue'
import { format } from 'date-fns'
import { useConfirm } from 'primevue/useconfirm'
import { useDialog } from 'primevue/usedialog'
import { useToast } from 'primevue/usetoast'
import { ref } from 'vue'
import { useRouter } from 'vue-router'

const router = useRouter()
const dialog = useDialog()
const confirm = useConfirm()

const {
  query: { data: user },
  activeSubscription
} = useIdentity()

const toast = useToast()
const { mutateAsync: logoutAsync } = useLogoutMutation()
const { mutateAsync: resendConfirmationEmailAsync, isPending: isResendConfirmationEmailPending } =
  useResendConfirmatioEmailMutation()
const { mutateAsync: updateUserAsync, isPending: isUpdateUserPending } = useUpdateUserMutation()
const { mutateAsync: updateUserAvatarAsync, isPending: updateUserAvatarIsPending } =
  useUpdateUserAvatarMutation()

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
        life: 3000
      })
    })
  } else {
    toast.add({
      severity: 'error',
      summary: 'E-Mail Missing',
      detail: 'Could not detect your e-mail address.',
      life: 3000
    })
  }
}
const updateUserSettingsShowSyncConfirmDialogs = (value: boolean) => {
  if (!user.value) {
    return
  }

  updateUserAsync({
    fullName: user.value.fullName,
    settings: { ...user.value.settings, showSyncConfirmDialogs: value }
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
    settings: { ...user.value.settings }
  })
}

const updateUserAvatar = () => {
  confirm.require({
    header: 'Generate Avatar',
    message: `Would you like to generate a new avatar? Please note, this action cannot be undone.`,
    icon: 'pi pi-info-circle',
    rejectProps: {
      label: 'Cancel',
      severity: 'secondary',
      outlined: true
    },
    acceptProps: {
      label: 'Continue',
      severity: ''
    },
    accept: () => {
      updateUserAvatarAsync().then(() => {
        toast.add({
          severity: 'success',
          summary: 'Avatar Updated',
          detail: 'Generated new account avatar.',
          life: 3000
        })
      })
    },
    reject: () => {}
  })
}
</script>

<template>
  <AppLayout hide-header>
    <div class="px-7 pt-5 text-3xl font-bold">Control Panel</div>
    <div class="px-7 text-slate-400">Manage settings and access your account details.</div>
    <div class="flex max-w-[60rem] flex-col gap-7 p-7">
      <div class="flex items-start gap-7">
        <div class="flex grow items-center rounded-xl border border-gray-200 p-5">
          <img :src="user?.avatar" class="me-4 w-12 rounded-full" />
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

            <Button
              outlined
              @click="updateUserAvatar"
              :loading="updateUserAvatarIsPending"
              class="ms-3"
              icon="pi pi-sync"
              label="Update Avatar"
            ></Button>
          </div>
        </div>
      </div>
    </div>
    <div class="flex max-w-[60rem] flex-col gap-7 px-7 pb-7">
      <div class="flex items-start gap-7">
        <div class="grow basis-1/2 rounded-xl border border-gray-200 p-5">
          <div class="text-lg font-medium">E-Mail Address</div>
          <div class="text-slate-500">{{ user?.email }}</div>
        </div>
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
      </div>
      <div class="flex items-start gap-7">
        <div class="grow rounded-xl border border-slate-200 p-5">
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
              <div v-if="user?.subscriptions[0]?.endsAt" class="text-red-800">
                Cancelled — grace period until
                {{ format(user?.subscriptions[0].endsAt, 'yyyy/MM/dd') }}
              </div>
            </div>
            <Button
              label="Upgrade Account"
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
                :model-value="user?.settings.showSyncConfirmDialogs"
                @update:model-value="(value) => updateUserSettingsShowSyncConfirmDialogs(value)"
              ></ToggleSwitch>
              Show sync confirmation dialogs
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
