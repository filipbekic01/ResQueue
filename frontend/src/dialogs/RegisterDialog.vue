<script lang="ts" setup>
import { useLoginMutation } from '@/api/auth/loginMutation'
import { useMeQuery } from '@/api/auth/meQuery'
import { useRegisterMutation } from '@/api/auth/registerMutation'
import { useCreateSubscriptionMutation } from '@/api/stripe/createSubscriptionMutation'
import { useIdentity } from '@/composables/identityComposable'
import { loadStripe, type Stripe, type StripeCardElement } from '@stripe/stripe-js'
import type { DynamicDialogOptions } from 'primevue/dynamicdialogoptions'
import { inject, onMounted, ref, type Ref } from 'vue'

const { user } = useIdentity()
const { mutateAsync: registerAsync } = useRegisterMutation()
const { refetch } = useMeQuery()

const { mutateAsync: loginAsync } = useLoginMutation()
const { mutateAsync: createSubscriptionAsync } = useCreateSubscriptionMutation()

const email = ref('filip1994sm@gmail.com')
const password = ref('Password1!')

const passwordType = ref('password')

let stripe: Stripe | null = null
let cardElement: StripeCardElement | null = null

const dialogRef = inject<Ref<DynamicDialogOptions>>('dialogRef')

const togglePasswordType = () =>
  (passwordType.value = passwordType.value == 'password' ? 'text' : 'password')

const register = async () => {
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

  registerAsync({
    email: email.value,
    password: password.value,
    paymentMethodId: paymentMethod.id,
    priceId: dialogRef?.value.data.priceId
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
    return
  }

  const elements = stripe.elements()

  cardElement = elements.create('card')
  cardElement.mount('#card-element')
}

onMounted(() => {
  loadStripeAsync()
})
</script>

<template>
  <div class="flex flex-col gap-4 mb-4">
    <label for="email" class="font-semibold white">E-Mail Address</label>
    <InputText v-model="email" id="email" class="flex-auto" type="email" autocomplete="off" />
  </div>
  <div class="flex flex-col gap-4 mb-8">
    <label for="password" class="font-semibold white flex items-center"
      >Password <i class="pi pi-eye ms-2 cursor-pointer" @click="togglePasswordType"></i
    ></label>
    <InputText
      id="password"
      v-model="password"
      class="flex-auto"
      :type="passwordType"
      autocomplete="off"
    />
  </div>
  <div class="flex justify-end gap-2">
    <Button type="button" label="Cancel" severity="secondary" tabindex="-1"></Button>
    <Button type="button" label="Register" @click="register"></Button>
  </div>
</template>
