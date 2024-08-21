<script lang="ts" setup>
import { useLoginMutation } from '@/api/auth/loginMutation'
import { useMeQuery } from '@/api/auth/meQuery'
import { useRegisterMutation } from '@/api/auth/registerMutation'
import { useIdentity } from '@/composables/identityComposable'
import {
  loadStripe,
  type PaymentMethod,
  type Stripe,
  type StripeCardElement
} from '@stripe/stripe-js'
import type { DynamicDialogOptions } from 'primevue/dynamicdialogoptions'
import Message from 'primevue/message'
import { inject, onMounted, ref, type Ref } from 'vue'

const { user } = useIdentity()
const { mutateAsync: registerAsync, isPending } = useRegisterMutation()
const { refetch } = useMeQuery()

const { mutateAsync: loginAsync } = useLoginMutation()

const email = ref('filip1994sm@gmail.com')
const password = ref('Password1!')

const passwordType = ref('password')

let stripe: Stripe | null = null
let cardElement: StripeCardElement | null = null

const dialogRef = inject<Ref<DynamicDialogOptions>>('dialogRef')

const togglePasswordType = () =>
  (passwordType.value = passwordType.value == 'password' ? 'text' : 'password')

const register = async () => {
  let pm: PaymentMethod | undefined = undefined

  if (dialogRef?.value.plan) {
    if (!stripe || !cardElement) {
      alert('Payment initialization error.')
      return
    }

    const { error, paymentMethod } = await stripe.createPaymentMethod({
      type: 'card',
      card: cardElement,
      billing_details: {
        email: email.value
      }
    })

    if (!paymentMethod || error) {
      alert('Payment processing error.')
      return
    }

    pm = paymentMethod
  }

  registerAsync({
    email: email.value,
    password: password.value,
    paymentMethodId: pm?.id,
    plan: dialogRef?.value.data.plan
  }).then(() => {
    loginAsync({
      email: email.value,
      password: password.value
    }).then(() => {
      refetch().then((x) => {
        user.value = x.data
        dialogRef?.value.close()
      })
    })
  })
}

const loadStripeAsync = async () => {
  stripe = await loadStripe(
    'pk_test_51PpxV4KE6sxW2owau69f5U6Fzf5uMnUIgo9gB8WxnCkaix9pcxn9yGr1erlTZjPt2ec7I8X42eKKmNpCgNoaDxw300PVRqNQUe'
  )

  if (!stripe) {
    alert('Failed to load payment processor.')
    return
  }

  const elements = stripe.elements()
  cardElement = elements.create('card', {
    style: {
      base: {}
    }
  })
  cardElement.mount('#card-element')
}

onMounted(() => {
  loadStripeAsync()
})
</script>

<template>
  <div class="flex flex-col gap-4 mb-4">
    <label for="email" class="font-semibold white">E-Mail Address</label>
    <InputText
      v-model="email"
      placeholder="E-mail address"
      id="email"
      class="flex-auto"
      type="email"
      autocomplete="off"
    />
  </div>
  <div class="flex flex-col gap-4 mb-4">
    <label for="password" class="font-semibold white flex items-center"
      >Password <i class="pi pi-eye ms-2 cursor-pointer" @click="togglePasswordType"></i
    ></label>
    <InputText
      id="password"
      placeholder="********"
      v-model="password"
      class="flex-auto"
      :type="passwordType"
      autocomplete="off"
    />
  </div>
  <div v-if="dialogRef?.data.plan" class="flex flex-col gap-4">
    <label for="card-element" class="font-semibold white flex items-center">Credit Card </label>
    <div class="border border-slate-300 p-3 rounded-md" id="card-element"></div>
    <Message severity="secondary" pt:text:class="flex grow gap-2 "
      >Plan:
      <span class="">{{ dialogRef.data.plan === 'essentials' ? 'Essentials' : 'Ultimate' }}</span
      ><span class="ms-auto">{{
        dialogRef.data.plan === 'essentials' ? '$7.99/mo' : '$19.99/mo'
      }}</span></Message
    >
  </div>
  <Message severity="secondary" v-else
    >Registering for a free account limits features.<br /><span
      class="border-b border-gray-400 hover:border-gray-800 border-dashed cursor-pointer"
      @click="dialogRef?.close()"
      >Upgrade to unlock full access.</span
    ></Message
  >

  <div class="flex justify-end gap-2 mt-8">
    <Button
      type="button"
      label="Register"
      icon="pi pi-arrow-right"
      icon-pos="right"
      :loading="isPending"
      @click="register"
    ></Button>
  </div>
</template>
