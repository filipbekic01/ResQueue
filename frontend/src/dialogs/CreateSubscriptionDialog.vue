<script lang="ts" setup>
import { onMounted, ref } from 'vue'
import { loadStripe, type Stripe, type StripeCardElement } from '@stripe/stripe-js'
import { useCreateSubscriptionMutation } from '@/api/stripe/createSubscriptionMutation'

const subscribeAsync = async () => {
  if (!stripe || !cardElement) {
    return
  }

  const { error, paymentMethod } = await stripe.createPaymentMethod({
    type: 'card',
    card: cardElement,
    billing_details: {
      email: 'customer@example.com' // Replace with the customer's email
    }
  })

  console.log(error, paymentMethod)
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
  <div>
    <div id="card-element"></div>
    <Button @click="subscribeAsync" label="Subscribe" severity="success" class="mt-5"></Button>
  </div>
</template>
