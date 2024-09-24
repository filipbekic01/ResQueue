<script lang="ts" setup>
import { useChangeCardMutation } from '@/api/stripe/changeCardMutation'
import { useIdentity } from '@/composables/identityComposable'
import { useStripe } from '@/composables/stripeComposable'
import { errorToToast } from '@/utils/errorUtils'
import Button from 'primevue/button'
import type { DynamicDialogOptions } from 'primevue/dynamicdialogoptions'
import { useToast } from 'primevue/usetoast'
import { inject, onMounted, ref, watchEffect, type Ref } from 'vue'

const dialogRef = inject<Ref<DynamicDialogOptions>>('dialogRef')

const {
  query: { data: user }
} = useIdentity()

const toast = useToast()

const { stripe, cardElement, mountCreditCardElement } = useStripe()
const { mutateAsync: changeCardAsync } = useChangeCardMutation()

// Wait for #card-element to mount
onMounted(() => {
  // Wait for modal animation to finish
  setTimeout(() => {
    // Mount credit card
    watchEffect(() => {
      if (stripe.value && !cardElement.value) {
        mountCreditCardElement()
      }
    })
  }, 500)
})

const isLoading = ref(false)

const updateCreditCard = async () => {
  if (!stripe.value || !cardElement.value) {
    return
  }

  if (isLoading.value) {
    return
  }

  isLoading.value = true

  try {
    const { error, paymentMethod } = await stripe.value.createPaymentMethod({
      type: 'card',
      card: cardElement.value,
      billing_details: {
        email: user.value?.email
      }
    })

    if (!paymentMethod || error) {
      isLoading.value = false
      alert('Invalid payment method')
      return
    }

    changeCardAsync({
      paymentMethodId: paymentMethod.id
    })
      .then(() => dialogRef?.value.close())
      .catch((e) => toast.add(errorToToast(e)))
  } catch (e) {
    console.error(e)
    isLoading.value = false
  }
}
</script>

<template>
  <div class="flex w-[28rem] flex-col">
    <div class="rounded-md border border-slate-300 p-3" id="card-element"></div>
    <Button class="mt-4" label="Update Credit Card" @click="updateCreditCard" :loading="isLoading"></Button>
  </div>
</template>
