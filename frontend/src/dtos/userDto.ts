import type { SubscriptionDto } from './subscriptionDto'
import type { UserConfigDto } from './userConfigDto'

export interface UserDto {
  id: string
  fullName: string
  email: string
  userConfig: UserConfigDto
  emailConfirmed: boolean
  stripeId?: string | null
  paymentType?: string | null
  paymentLastFour?: string | null
  subscriptions: SubscriptionDto[]
}
