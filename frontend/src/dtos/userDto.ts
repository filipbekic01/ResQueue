import type { UserConfigDto } from './userConfigDto'

export interface UserDto {
  id: string
  email: string
  userConfig: UserConfigDto
}
