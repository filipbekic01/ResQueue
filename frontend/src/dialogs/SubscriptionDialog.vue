<script lang="ts" setup>
import { useCancelSubscriptionMutation } from '@/api/stripe/cancelSubscriptionMutation'
import { useIdentity } from '@/composables/identityComposable'
import { extractErrorMessage } from '@/utils/errorUtil'
import Button from 'primevue/button'
import type { DynamicDialogOptions } from 'primevue/dynamicdialogoptions'
import { useToast } from 'primevue/usetoast'
import { inject, type Ref } from 'vue'

const dialogRef = inject<Ref<DynamicDialogOptions>>('dialogRef')

const toast = useToast()

const {
  query: { data: user }
} = useIdentity()

const { mutateAsync: cancelSubscriptionAsync } = useCancelSubscriptionMutation()

const cancel = () => {
  cancelSubscriptionAsync()
    .then(() => {
      dialogRef?.value.close()

      toast.add({
        severity: 'warn',
        summary: 'Subscription Cancelled',
        detail: 'Subscription is successfully cancelled',
        life: 6000
      })
    })
    .catch((e) => {
      toast.add({
        severity: 'error',
        summary: 'Cancellation Failed',
        detail: extractErrorMessage(e),
        life: 6000
      })
    })
}
</script>

<template>
  <div class="flex flex-col gap-3">
    <div class="flex flex-col">
      <div class="font-bold">Subscription ID</div>
      <div>{{ user?.subscriptionId }}</div>
    </div>
    <div class="flex flex-col">
      <div class="font-bold">Subscription Plan</div>
      <div>{{ user?.subscriptionPlan }}</div>
    </div>
    <Button
      class="mt-3"
      severity="danger"
      outlined
      label="Cancel Subscription"
      @click="cancel"
    ></Button>
  </div>
</template>
