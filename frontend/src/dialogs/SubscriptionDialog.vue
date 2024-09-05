<script lang="ts" setup>
import { useCancelSubscriptionMutation } from '@/api/stripe/cancelSubscriptionMutation'
import { useIdentity } from '@/composables/identityComposable'
import { extractErrorMessage } from '@/utils/errorUtils'
import { format } from 'date-fns'
import Button from 'primevue/button'
import type { DynamicDialogOptions } from 'primevue/dynamicdialogoptions'
import Message from 'primevue/message'
import { useToast } from 'primevue/usetoast'
import { inject, ref, type Ref } from 'vue'

const dialogRef = inject<Ref<DynamicDialogOptions>>('dialogRef')

const toast = useToast()

const { activeSubscription } = useIdentity()

const { mutateAsync: cancelSubscriptionAsync, isPending } = useCancelSubscriptionMutation()

const protect = ref('')

const cancel = () => {
  if (!activeSubscription.value) {
    return
  }

  cancelSubscriptionAsync({ subscriptionId: activeSubscription.value.stripeId })
    .then(() => {
      dialogRef?.value.close()

      toast.add({
        severity: 'warn',
        summary: 'Subscription Cancelled',
        detail: 'Subscription is successfully cancelled',
        life: 3000
      })
    })
    .catch((e) => {
      toast.add({
        severity: 'error',
        summary: 'Cancellation Failed',
        detail: extractErrorMessage(e),
        life: 3000
      })
    })
}
</script>

<template>
  <div v-if="activeSubscription" class="flex flex-col gap-3">
    <div class="flex flex-col">
      <div class="font-bold">Subscription Plan</div>
      <div>{{ activeSubscription.type === 'essentials' ? 'Essentials' : 'Ultimate' }}</div>
      <div class="text-gray-400">{{ activeSubscription.stripeId }}</div>
    </div>

    <div class="flex flex-col">
      <div class="font-bold">Started At</div>
      <div>{{ format(activeSubscription.createdAt, 'MMMM dd, yyyy') }}</div>
    </div>

    <div class="flex flex-col" v-if="activeSubscription.endsAt">
      <div class="font-bold">Ends At</div>
      <div>{{ format(activeSubscription.endsAt, 'MMMM dd, yyyy') }}</div>
      <Message class="mt-3" severity="info">
        Your subscription will remain active until the end of the current month. No further charges
        will apply after that.
      </Message>
    </div>
    <div v-else>The subscription is billed on a monthly basis.</div>

    <label for="password" class="white flex items-center border-t pt-3 font-semibold"
      >Enter "{{ activeSubscription.type }}" to enable cancel button</label
    >
    <InputText v-model="protect" placeholder="What's the plan?" type="text"></InputText>
    <Button
      severity="danger"
      :disabled="protect !== activeSubscription.type || isPending"
      :loading="isPending"
      label="Cancel Subscription"
      @click="cancel"
    ></Button>
  </div>
  <div v-else>No active subscriptions.</div>
</template>
