import { useMeQuery } from '@/api/auth/meQuery'
import type { SubscriptionDto } from '@/dtos/users/subscriptionDto'
import { computed } from 'vue'

export function useIdentity() {
  const query = useMeQuery()

  const activeSubscription = computed<SubscriptionDto | null>(() =>
    query.data.value?.subscription?.stripeStatus === 'active'
      ? query.data.value?.subscription
      : null
  )

  const allowedUpgradeToUltimate = computed(
    () =>
      query.data.value?.subscription?.type === 'essentials' && !query.data.value.subscription.endsAt
  )

  const allowedUpgradeToEssentials = computed(
    () =>
      query.data.value?.subscription?.type === 'ultimate' && !query.data.value.subscription.endsAt
  )

  return {
    query,
    activeSubscription,
    allowedUpgradeToUltimate,
    allowedUpgradeToEssentials
  }
}
