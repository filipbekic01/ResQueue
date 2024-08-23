<script lang="ts" setup>
import { useCancelSubscriptionMutation } from '@/api/stripe/cancelSubscriptionMutation'
import { useIdentity } from '@/composables/identityComposable'
import { extractErrorMessage } from '@/utils/errorUtil'
import Button from 'primevue/button'
import type { DynamicDialogOptions } from 'primevue/dynamicdialogoptions'
import { useToast } from 'primevue/usetoast'
import { inject, ref, type Ref } from 'vue'

const dialogRef = inject<Ref<DynamicDialogOptions>>('dialogRef')

const toast = useToast()

const {
  query: { data: user }
} = useIdentity()

const { mutateAsync: cancelSubscriptionAsync, isPending } = useCancelSubscriptionMutation()

const protect = ref('')

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

    <label for="password" class="font-semibold white flex items-center border-t pt-3"
      >Enter "{{ user?.subscriptionPlan }}" to enable cancel button</label
    >
    <InputText v-model="protect" placeholder="What's the plan?" type="text"></InputText>
    <Button
      severity="danger"
      :disabled="protect !== user?.subscriptionPlan || isPending"
      :loading="isPending"
      label="Cancel Subscription"
      @click="cancel"
    ></Button>
  </div>
</template>
