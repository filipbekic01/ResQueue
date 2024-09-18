import { loadStripe, type Stripe, type StripeCardElement } from '@stripe/stripe-js'
import { ref } from 'vue'

const stripe = ref<Stripe | null>(null)
const isLoading = ref(false)
const loadStripeAsync = async () => {
  if (isLoading.value) {
    return
  }

  isLoading.value = true

  try {
    stripe.value = await loadStripe(
      'pk_test_51PpxV4KE6sxW2owau69f5U6Fzf5uMnUIgo9gB8WxnCkaix9pcxn9yGr1erlTZjPt2ec7I8X42eKKmNpCgNoaDxw300PVRqNQUe'
    )

    if (!stripe.value) {
      console.error('Failed method loadStripeAsync')
      return
    }
  } catch (e) {
    console.error(e)
  } finally {
    isLoading.value = false
  }
}

export function useStripe() {
  const cardElement = ref<StripeCardElement | null>(null)

  const mountCreditCardElement = () => {
    if (!stripe.value) {
      return
    }

    const elements = stripe.value.elements()
    cardElement.value = elements.create('card', {
      style: {
        base: {
          // Customize styles here
        }
      }
    })

    cardElement.value.mount('#card-element')
  }

  if (!stripe.value) {
    loadStripeAsync()
  }

  return {
    stripe,
    cardElement,
    mountCreditCardElement
  }
}
