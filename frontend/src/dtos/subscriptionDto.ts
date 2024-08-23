import type { SubscriptionItemDto } from './subscriptionItemDto'

export interface SubscriptionDto {
  type: string
  stripeId: string
  stripeStatus: string
  stripePrice: string
  quantity: number
  trialEndsAt?: string
  endsAt: string
  createdAt: string
  updatedAt: string
  subscriptionItems: SubscriptionItemDto[]
}
