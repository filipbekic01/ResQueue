import type { UserConfigDto } from './userConfigDto'

export interface UserDto {
  id: string
  email: string
  userConfig: UserConfigDto
  emailConfirmed: boolean
  isSubscribed: boolean
  subscriptionId: string
  subscriptionPlan: string
}
