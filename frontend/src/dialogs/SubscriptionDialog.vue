<script lang="ts" setup>
import { useCancelSubscriptionMutation } from '@/api/stripe/cancelSubscriptionMutation'
import { useChangePlanMutation } from '@/api/stripe/changePlanMutation'
import { useContinueSubscriptionMutation } from '@/api/stripe/continueSubscriptionMutation'
import { useIdentity } from '@/composables/identityComposable'
import { errorToToast } from '@/utils/errorUtils'
import { format } from 'date-fns'
import Button from 'primevue/button'
import Message from 'primevue/message'
import { useConfirm } from 'primevue/useconfirm'
import { useDialog } from 'primevue/usedialog'
import { useToast } from 'primevue/usetoast'
import ChangeCardDialog from './ChangeCardDialog.vue'

const toast = useToast()
const confirm = useConfirm()
const dialog = useDialog()

const {
  query: { data: user },
  activeSubscription,
  allowedUpgradeToEssentials
} = useIdentity()

const { mutateAsync: cancelSubscriptionAsync, isPending } = useCancelSubscriptionMutation()
const { mutateAsync: continueSubscriptionAsync, isPending: isPendingContinueSubscription } =
  useContinueSubscriptionMutation()
const { mutateAsync: changePlanAsync, isPending: isChangePlanPending } = useChangePlanMutation()

const cancel = () => {
  if (!activeSubscription.value) {
    return
  }

  confirm.require({
    header: 'Cancel Subscription',
    message: `You're about to cancel subscription.`,
    icon: 'pi pi-info-circle',
    rejectProps: {
      label: 'Cancel',
      severity: 'secondary',
      outlined: true
    },
    acceptProps: {
      label: 'Cancel',
      severity: 'danger'
    },
    accept: () => {
      cancelSubscriptionAsync()
        .then(() => {})
        .catch((e) => {
          toast.add(errorToToast(e))
        })
    },
    reject: () => {}
  })
}

const continueSubscription = () => {
  if (!activeSubscription.value) {
    return
  }

  continueSubscriptionAsync()
    .then(() => {})
    .catch((e) => {
      toast.add(errorToToast(e))
    })
}

const downgradePlan = () => {
  confirm.require({
    header: 'Downgrade to Essentials',
    message: `You're about to downgrade to essentials.`,
    icon: 'pi pi-info-circle',
    rejectProps: {
      label: 'Cancel',
      severity: 'secondary',
      outlined: true
    },
    acceptProps: {
      label: 'Downgrade Now',
      severity: 'danger'
    },
    accept: () => {
      changePlanAsync()
        .then(() => {
          toast.add({
            severity: 'success',
            summary: 'Essentials Activated',
            detail: 'Successfully downgraded account.',
            life: 3000
          })
        })
        .catch((e) => toast.add(errorToToast(e)))
    },
    reject: () => {}
  })
}

const openChangePaymentMethodDialog = () =>
  dialog.open(ChangeCardDialog, {
    props: {
      header: 'Update Credit Card',
      draggable: false,
      modal: true
    }
  })
</script>

<template>
  <div v-if="activeSubscription" class="flex flex-col gap-5">
    <div class="flex justify-between">
      <div>Subscription Plan</div>
      <div>{{ activeSubscription.type === 'essentials' ? 'Essentials' : 'Ultimate' }}</div>
    </div>

    <div class="flex justify-between">
      <div>Subscribed At</div>
      <div>{{ format(activeSubscription.createdAt, 'MMMM dd, yyyy') }}</div>
    </div>

    <div class="flex justify-between">
      <div>Credit Card</div>
      <div
        @click="openChangePaymentMethodDialog"
        class="flex cursor-pointer items-center gap-2 border-b border-dashed border-b-gray-400 hover:border-solid hover:border-b-blue-400 hover:text-blue-400"
      >
        <span class="uppercase">{{ user?.paymentType }}</span>
        <span>{{ user?.paymentLastFour }}</span>
        <i style="font-size: 0.825rem" class="pi pi-pencil"></i>
      </div>
    </div>

    <template v-if="activeSubscription.endsAt">
      <div class="flex justify-between">
        <div>Ends At</div>
        <div>{{ format(activeSubscription.endsAt, 'MMMM dd, yyyy') }}</div>
      </div>
      <Message severity="secondary">
        Your subscription will remain active until the end of the current month. No further charges
        will apply after that.
      </Message>
    </template>

    <div class="flex gap-4">
      <Button
        v-if="allowedUpgradeToEssentials"
        class="grow"
        outlined
        @click="downgradePlan"
        :loading="isChangePlanPending"
        >Downgrade</Button
      >
      <Button
        class="grow"
        v-if="!activeSubscription.endsAt"
        severity="danger"
        outlined
        :disabled="isPending"
        :loading="isPending"
        label="Cancel"
        @click="cancel"
      ></Button>
      <Button
        v-if="activeSubscription.endsAt"
        class="grow"
        @click="continueSubscription"
        :loading="isPendingContinueSubscription"
        label="Resume Subscription"
      ></Button>
    </div>
  </div>
</template>
