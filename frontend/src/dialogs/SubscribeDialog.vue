<script lang="ts" setup>
import { useSubscribeMutation } from '@/api/stripe/subscribeMutation'
import { useIdentity } from '@/composables/identityComposable'
import { useStripe } from '@/composables/stripeComposable'
import router from '@/router'
import { errorToToast } from '@/utils/errorUtils'
import type { DynamicDialogOptions } from 'primevue/dynamicdialogoptions'
import Message from 'primevue/message'
import { useToast } from 'primevue/usetoast'
import { inject, onMounted, ref, watchEffect, type Ref } from 'vue'

const toast = useToast()

const {
  query: { data: user }
} = useIdentity()

const { mutateAsync: subscribeAsync } = useSubscribeMutation()
const { stripe, cardElement, mountCreditCardElement } = useStripe()

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

const dialogRef = inject<Ref<DynamicDialogOptions>>('dialogRef')

const paymentMethodId = ref<string>()
const coupon = ref('')

const isLoading = ref(false)

const subscribe = async () => {
  if (!stripe.value || !cardElement.value) {
    return
  }

  if (isLoading.value) {
    return
  }

  isLoading.value = true

  if (dialogRef?.value.data.plan) {
    if (!stripe || !cardElement) {
      toast.add({
        severity: 'error',
        summary: 'Subscription Problem',
        detail: 'Payment initialization error.',
        life: 3000
      })
      isLoading.value = false
      return
    }

    try {
      const { error, paymentMethod } = await stripe.value.createPaymentMethod({
        type: 'card',
        card: cardElement.value,
        billing_details: {
          email: user.value?.email
        }
      })
      if (!paymentMethod || error) {
        toast.add({
          severity: 'error',
          summary: 'Registration Problem',
          detail: 'Payment processing error.',
          life: 3000
        })
        isLoading.value = false
        return
      }
      paymentMethodId.value = paymentMethod.id
    } catch (e) {
      isLoading.value = false
      toast.add({
        severity: 'error',
        summary: 'Registration Problem',
        detail: e,
        life: 3000
      })
    }
  }

  subscribeAsync({
    paymentMethodId: paymentMethodId.value ?? '',
    plan: dialogRef?.value.data.plan,
    coupon: coupon.value
  })
    .then(() => {
      dialogRef?.value.close()
      setTimeout(() => {
        router.push({
          name: 'app'
        })
      }, 500)
    })
    .catch((e) => toast.add(errorToToast(e)))
}
</script>

<template>
  <div class="mb-4 flex flex-col gap-2">
    <label for="email" class="font-medium">Account E-Mail</label>
    <InputText
      :model-value="user?.email"
      placeholder="E-mail address"
      id="email"
      class="flex-auto"
      type="email"
      :disabled="true"
      autocomplete="off"
    />
  </div>
  <div class="flex flex-col gap-4 rounded-xl border border-slate-300 p-4">
    <label for="card-element" class="white flex items-center font-medium"> Credit Card </label>
    <div class="rounded-md border border-slate-300 p-3" id="card-element"></div>
    <Message severity="secondary" pt:text:class="flex grow gap-2 "
      >Plan: <span class="">{{ dialogRef?.data.plan === 'essentials' ? 'Essentials' : 'Ultimate' }}</span
      ><span class="ms-auto">{{ dialogRef?.data.plan === 'essentials' ? '$9.99/mo' : '$14.99/mo' }}</span></Message
    >
  </div>

  <div v-if="dialogRef?.data.plan" class="my-4 flex flex-col gap-4">
    <label for="coupon" class="white flex items-center font-semibold">Coupon</label>
    <InputText id="coupon" placeholder="Enter coupon" v-model="coupon" class="flex-auto" autocomplete="off" />
  </div>

  <div class="mt-8 flex flex-col items-center gap-2">
    <Button
      type="button"
      label="Subscribe"
      icon="pi pi-arrow-right"
      icon-pos="right"
      class="w-3/4"
      :loading="isLoading"
      @click="subscribe"
    ></Button>
  </div>
</template>
