import type { SubscriptionDto } from './subscriptionDto'
import type { UserSettingsDto } from './userSettings'

export interface UserDto {
  id: string
  fullName: string
  email: string
  settings: UserSettingsDto
  emailConfirmed: boolean
  stripeId?: string | null
  paymentType?: string | null
  paymentLastFour?: string | null
  subscriptions: SubscriptionDto[]
}
