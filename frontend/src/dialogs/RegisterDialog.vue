<script lang="ts" setup>
import { useLoginMutation } from '@/api/auth/loginMutation'
import { useRegisterMutation } from '@/api/auth/registerMutation'
import { useIdentity } from '@/composables/identityComposable'
import router from '@/router'
import { extractErrorMessage } from '@/utils/errorUtil'
import { loadStripe, type Stripe, type StripeCardElement } from '@stripe/stripe-js'
import type { DynamicDialogOptions } from 'primevue/dynamicdialogoptions'
import Message from 'primevue/message'
import { useToast } from 'primevue/usetoast'
import { inject, onMounted, ref, type Ref } from 'vue'

const { mutateAsync: registerAsync } = useRegisterMutation()
const toast = useToast()

const {
  query: { refetch }
} = useIdentity()

const { mutateAsync: loginAsync } = useLoginMutation()

const email = ref('filip1994sm@gmail.com')
const password = ref('Password1!')

const passwordType = ref('password')

let stripe: Stripe | null = null
let cardElement: StripeCardElement | null = null

const dialogRef = inject<Ref<DynamicDialogOptions>>('dialogRef')

const togglePasswordType = () =>
  (passwordType.value = passwordType.value == 'password' ? 'text' : 'password')

const paymentMethodId = ref<string>()
const isRegisterLoading = ref(false)
const register = async () => {
  if (isRegisterLoading.value) {
    return
  }

  isRegisterLoading.value = true

  if (dialogRef?.value.data.plan) {
    if (!stripe || !cardElement) {
      toast.add({
        severity: 'error',
        summary: 'Registration Problem',
        detail: 'Payment initialization error.',
        life: 6000
      })
      isRegisterLoading.value = false
      return
    }

    try {
      const { error, paymentMethod } = await stripe.createPaymentMethod({
        type: 'card',
        card: cardElement,
        billing_details: {
          email: email.value
        }
      })

      if (!paymentMethod || error) {
        toast.add({
          severity: 'error',
          summary: 'Registration Problem',
          detail: 'Payment processing error.',
          life: 6000
        })
        isRegisterLoading.value = false
        return
      }

      paymentMethodId.value = paymentMethod.id
    } catch (e) {
      isRegisterLoading.value = false
      toast.add({
        severity: 'error',
        summary: 'Registration Problem',
        detail: extractErrorMessage(e),
        life: 6000
      })
    }
  }

  registerAsync({
    email: email.value,
    password: password.value,
    paymentMethodId: paymentMethodId.value,
    plan: dialogRef?.value.data.plan
  })
    .then(() => {
      loginAsync({
        email: email.value,
        password: password.value
      }).then(() => {
        refetch().then(() => {
          dialogRef?.value.close()
          router.push({
            name: 'app'
          })
        })
      })
    })
    .catch((e) => {
      toast.add({
        severity: 'error',
        summary: 'Registration Problem',
        detail: extractErrorMessage(e),
        life: 6000
      })
    })
    .finally(() => {
      isRegisterLoading.value = false
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
  isRegisterLoading.value = true
  setTimeout(() => {
    if (dialogRef?.value.data.plan) {
      loadStripeAsync().finally(() => {
        isRegisterLoading.value = false
      })
    } else {
      isRegisterLoading.value = false
    }
  }, 500)
})
</script>

<template>
  <div class="mb-4 flex flex-col gap-4">
    <label for="email" class="white font-semibold">E-Mail Address</label>
    <InputText
      v-model="email"
      placeholder="E-mail address"
      id="email"
      class="flex-auto"
      type="email"
      autocomplete="off"
    />
  </div>
  <div class="mb-4 flex flex-col gap-4">
    <label for="password" class="white flex items-center font-semibold"
      >Password
      <i
        class="pi ms-2 cursor-pointer"
        :class="[
          {
            'pi-eye': passwordType === 'text',
            'pi-eye-slash': passwordType === 'password'
          }
        ]"
        @click="togglePasswordType"
      ></i>
    </label>
    <InputText
      id="password"
      placeholder="********"
      v-model="password"
      class="flex-auto"
      :type="passwordType"
      autocomplete="off"
    />
  </div>
  <div
    v-if="dialogRef?.data.plan"
    class="flex flex-col gap-4 rounded-xl border border-slate-300 p-4"
  >
    <label for="card-element" class="white flex items-center font-semibold"> Credit Card </label>
    <div class="rounded-md border border-slate-300 p-3" id="card-element"></div>
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
      class="cursor-pointer border-b border-dashed border-slate-400 hover:border-slate-800"
      @click="dialogRef?.close()"
      >Upgrade to unlock full access.</span
    ></Message
  >

  <div class="mt-8 flex flex-col items-center gap-2">
    <Button
      type="button"
      label="Register"
      icon="pi pi-arrow-right"
      icon-pos="right"
      class="w-3/4"
      :loading="isRegisterLoading"
      @click="register"
    ></Button>
    <div class="text-gray-500">Let's build something great together!</div>
  </div>
</template>
