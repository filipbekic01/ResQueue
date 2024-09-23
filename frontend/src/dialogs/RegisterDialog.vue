<script lang="ts" setup>
import { useLoginMutation } from '@/api/auth/loginMutation'
import { useRegisterMutation } from '@/api/auth/registerMutation'
import { useIdentity } from '@/composables/identityComposable'
import { useStripe } from '@/composables/stripeComposable'
import router from '@/router'
import { errorToToast } from '@/utils/errorUtils'
import type { DynamicDialogOptions } from 'primevue/dynamicdialogoptions'
import Message from 'primevue/message'
import { useToast } from 'primevue/usetoast'
import { inject, onMounted, ref, watchEffect, type Ref } from 'vue'

const { mutateAsync: registerAsync } = useRegisterMutation()
const toast = useToast()

const {
  query: { refetch }
} = useIdentity()

const { mutateAsync: loginAsync } = useLoginMutation()

const email = ref('')
const password = ref('')
const coupon = ref('')

const passwordType = ref('password')

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

const togglePasswordType = () =>
  (passwordType.value = passwordType.value == 'password' ? 'text' : 'password')

const paymentMethodId = ref<string>()
const isRegisterLoading = ref(false)
const register = async () => {
  if (!stripe.value || !cardElement.value) {
    return
  }

  if (isRegisterLoading.value) {
    return
  }

  isRegisterLoading.value = true

  // Get payment method
  if (dialogRef?.value.data.plan) {
    if (!stripe.value || !cardElement.value) {
      isRegisterLoading.value = false
      return
    }

    try {
      const { error, paymentMethod } = await stripe.value.createPaymentMethod({
        type: 'card',
        card: cardElement.value,
        billing_details: {
          email: email.value
        }
      })

      if (!paymentMethod || error) {
        toast.add({
          severity: 'error',
          summary: 'Registration Problem',
          detail: 'Payment processing error.',
          life: 3000
        })
        isRegisterLoading.value = false
        return
      }

      paymentMethodId.value = paymentMethod.id
    } catch (e) {
      isRegisterLoading.value = false
      toast.add(errorToToast(e))
    }
  }

  // Call register endpoint
  registerAsync({
    email: email.value,
    password: password.value,
    paymentMethodId: paymentMethodId.value,
    plan: dialogRef?.value.data.plan,
    coupon: coupon.value
  })
    .then(() => {
      loginAsync({
        email: email.value,
        password: password.value
      })
        .then(() => {
          refetch().then(() => {
            dialogRef?.value.close()
            router.push({
              name: 'app'
            })
          })
        })
        .catch((e) => toast.add(errorToToast(e)))
    })
    .catch((e) => {
      console.log(e)
      toast.add(errorToToast(e))
    })
    .finally(() => {
      isRegisterLoading.value = false
    })
}
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
        dialogRef.data.plan === 'essentials' ? '$9.99/mo' : '$14.99/mo'
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

  <div v-if="dialogRef?.data.plan" class="my-4 flex flex-col gap-4">
    <label for="coupon" class="white flex items-center font-semibold">Coupon</label>
    <InputText
      id="coupon"
      placeholder="Enter coupon"
      v-model="coupon"
      class="flex-auto"
      autocomplete="off"
    />
  </div>

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
