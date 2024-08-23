import { useMeQuery } from '@/api/auth/meQuery'
import type { SubscriptionDto } from '@/dtos/subscriptionDto'
import { computed } from 'vue'

export function useIdentity() {
  const query = useMeQuery()

  const activeSubscription = computed<SubscriptionDto | null>(() => {
    const user = query.data.value

    if (!user || !user.subscriptions || user.subscriptions.length === 0) {
      return null
    }

    const activeSubscriptions = user.subscriptions.filter(
      (subscription) => subscription.stripeStatus === 'active'
    )

    const ultimateSubscription = activeSubscriptions.find(
      (subscription) => subscription.type === 'ultimate'
    )
    if (ultimateSubscription) {
      return ultimateSubscription
    }

    const essentialsSubscription = activeSubscriptions.find(
      (subscription) => subscription.type === 'essentials'
    )
    if (essentialsSubscription) {
      return essentialsSubscription
    }

    return null
  })

  return {
    query,
    activeSubscription
  }
}
