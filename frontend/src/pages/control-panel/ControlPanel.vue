<script setup lang="ts">
import { useLogoutMutation } from '@/api/auth/logoutMutation'
import { useResendConfirmatioEmailMutation } from '@/api/auth/resendConfirmationEmailMutation'
import { useUpdateUserAvatarMutation } from '@/api/auth/updateUserAvatarMutation'
import { useUpdateUserMutation } from '@/api/auth/updateUserMutation'
import { useChangePlanMutation } from '@/api/stripe/changePlanMutation'
import { useUpdateSeatsMutation } from '@/api/stripe/updateSeatsMutation'
import { useIdentity } from '@/composables/identityComposable'
import FaqDialog from '@/dialogs/FaqDialog.vue'
import SubscriptionDialog from '@/dialogs/SubscriptionDialog.vue'
import PricingCards from '@/features/pricing-cards/PricingCards.vue'
import AppLayout from '@/layouts/AppLayout.vue'
import { errorToToast } from '@/utils/errorUtils'
import { format } from 'date-fns'
import { useConfirm } from 'primevue/useconfirm'
import { useDialog } from 'primevue/usedialog'
import { useToast } from 'primevue/usetoast'
import { computed, ref } from 'vue'

const dialog = useDialog()
const confirm = useConfirm()

const {
  query: { data: user },
  activeSubscription,
  allowedUpgradeToUltimate
} = useIdentity()

const toast = useToast()
const { mutateAsync: logoutAsync } = useLogoutMutation()
const { mutateAsync: resendConfirmationEmailAsync, isPending: isResendConfirmationEmailPending } =
  useResendConfirmatioEmailMutation()
const { mutateAsync: updateUserAsync, isPending: isUpdateUserPending } = useUpdateUserMutation()
const { mutateAsync: updateUserAvatarAsync } = useUpdateUserAvatarMutation()
const { mutateAsync: changePlanAsync, isPending: isChangePlanPending } = useChangePlanMutation()
const { mutateAsync: updateSeatsAsync, isPending: isUpdateSeatsPending } = useUpdateSeatsMutation()

const logout = () => {
  logoutAsync().then(() => {
    window.location.href = '/'
  })
}

const resendConfirmationEmail = () => {
  const email = user.value?.email

  if (email) {
    resendConfirmationEmailAsync({ email })
      .then(() => {
        toast.add({
          severity: 'success',
          summary: 'Confirmation Sent',
          detail: 'Please check your e-mail inbox.',
          life: 3000
        })
      })
      .catch((e) => toast.add(errorToToast(e)))
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
  }).catch((e) => toast.add(errorToToast(e)))
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
const tempFullName = ref('')
const updateUserFullNameAsync = () => {
  if (!user.value) {
    return
  }

  if (tempFullName.value === user.value.fullName) {
    showEditFullName.value = false
    return
  }

  updateUserAsync({
    fullName: tempFullName.value,
    settings: { ...user.value.settings }
  })
    .then(() => {
      showEditFullName.value = false
    })
    .catch((e) => toast.add(errorToToast(e)))
}

const updateUserAvatar = () => {
  confirm.require({
    header: 'Generate New Avatar',
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
      updateUserAvatarAsync()
        .then(() => {
          toast.add({
            severity: 'success',
            summary: 'Avatar Updated',
            detail: 'Generated new account avatar.',
            life: 3000
          })
        })
        .catch((e) => toast.add(errorToToast(e)))
    },
    reject: () => {}
  })
}

const upgradePlan = () => {
  confirm.require({
    header: 'Upgrade to Ultimate',
    message: `You're about to upgrade to ultimate.`,
    icon: 'pi pi-info-circle',
    rejectProps: {
      label: 'Cancel',
      severity: 'secondary',
      outlined: true
    },
    acceptProps: {
      label: 'Upgrade Now'
    },
    accept: () => {
      changePlanAsync()
        .then(() => {
          toast.add({
            severity: 'success',
            summary: 'Ultimate Activated',
            detail: 'Successfully upgraded account.',
            life: 3000
          })
        })
        .catch((e) => toast.add(errorToToast(e)))
    },
    reject: () => {}
  })
}

const openFaqDialog = () => {
  dialog.open(FaqDialog, {
    props: {
      header: 'Frequently Asked Questions',
      modal: true,
      draggable: false
    }
  })
}

const updateSeats = (seats: number) => {
  updateSeatsAsync({ seats })
    .then(() => {
      console.log('done!')
    })
    .catch((e) => toast.add(errorToToast(e)))
}

const updateSeatsPopoverRef = ref()
const toggleUpdateSeats = (event: Event) => updateSeatsPopoverRef.value.toggle(event)

const seatsWay = ref('Add')
const seatsWayOptions = ref(['Add', 'Remove'])

const seats = ref(1)
const seatOptions = computed(() => {
  if (!activeSubscription.value?.quantity) {
    return []
  }

  const options = []

  for (let i = 1; i <= activeSubscription.value.quantity; i++) {
    options.push({
      label: `-${i}`,
      value: i
    })
  }

  return options
})
</script>

<template>
  <AppLayout hide-header>
    <div class="px-7 pt-5 text-3xl font-bold">Control Panel</div>
    <div class="px-7 text-slate-600">Manage settings and access your account details.</div>
    <div class="flex max-w-[70rem] flex-col gap-7 p-7">
      <Message severity="secondary">
        <div class="flex items-center gap-2">
          <div
            class="flex cursor-pointer items-center gap-1.5 text-blue-500 hover:text-blue-400"
            @click="openFaqDialog"
          >
            <i class="pi pi-question-circle"></i>Frequently Asked Questions
          </div>
          <div>Please review our FAQ before using the application for the best experience.</div>
        </div>
      </Message>
      <div class="flex items-start gap-7">
        <div class="flex grow items-center rounded-xl border border-gray-200 p-5">
          <img
            :src="user?.avatar"
            class="me-4 w-12 cursor-pointer rounded-full hover:scale-105"
            @click="updateUserAvatar"
          />
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
            <template v-else>
              <InputText
                :model-value="user?.fullName"
                @update:model-value="(e) => (tempFullName = e ?? '')"
                placeholder="Enter your name..."
              ></InputText>
              <Button
                icon="pi pi-check"
                class="ms-1"
                outlined
                :loading="isUpdateUserPending"
                @click="updateUserFullNameAsync"
              ></Button>
            </template>

            <Button label="Logout" @click="logout" outlined icon="pi pi-sign-out" class="ms-3"></Button>
          </div>
        </div>
      </div>
    </div>

    <div class="flex max-w-[70rem] flex-col gap-7 px-7 pb-7">
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
                  <i class="pi pi-exclamation-circle text-orange-400"></i>Your free account is limited, upgrade for
                  better experience.
                </div>
              </div>
              <template v-else>
                <div>
                  <i class="pi pi-check-circle me-2 text-green-600"></i>Subscribed to
                  <a
                    @click="openSubscriptionManager"
                    class="cursor-pointer border-b border-dashed border-gray-400 hover:border-solid hover:border-blue-500 hover:text-blue-500"
                    >{{ activeSubscription.type === 'essentials' ? 'Essentials' : 'Ultimate' }}
                    <i class="pi pi-pencil" style="font-size: 0.8rem"></i
                  ></a>
                  plan.
                </div>
                <div v-if="activeSubscription.endsAt" class="text-red-800">
                  Cancelled — grace period until
                  {{ format(activeSubscription.endsAt, 'yyyy/MM/dd') }}
                </div>
              </template>
            </div>
            <div class="ms-auto">
              <div v-if="activeSubscription?.type === 'ultimate'" class="flex items-center gap-4">
                <div class="text-md flex flex-col">Available seats: {{ activeSubscription.quantity }}</div>
                <Button outlined label="Buy More" @click="toggleUpdateSeats"></Button>
                <Popover ref="updateSeatsPopoverRef" class="w-80">
                  <div class="flex flex-col gap-3">
                    <div>
                      Each additional seat is priced at $4, and the cost will be automatically adjusted on your current
                      billing plan, either added or removed, based on your seat count.
                    </div>
                    <SelectButton
                      :allow-empty="false"
                      v-model="seatsWay"
                      :options="seatsWayOptions"
                      aria-labelledby="basic"
                    />
                    <div class="flex items-center gap-3">
                      <Select v-model="seats" :options="seatOptions" option-label="label" option-value="value"></Select>
                      <i class="pi pi-arrow-right"></i>
                      <div>12</div>
                      <Button label="Update" icon="pi pi-arrow-right" class="ms-auto" outlined></Button>
                    </div>
                  </div>
                </Popover>
              </div>

              <Button
                v-if="allowedUpgradeToUltimate"
                label="Upgrade to Ultimate"
                icon="pi pi-arrow-right"
                @click="upgradePlan"
                :loading="isChangePlanPending"
                icon-pos="right"
                outlined
              ></Button>
            </div>
          </div>
          <div class="my-12" v-if="!activeSubscription">
            <PricingCards />
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
    </div>
  </AppLayout>
</template>
