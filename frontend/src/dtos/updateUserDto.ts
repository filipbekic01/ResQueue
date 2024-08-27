import type { UserConfigDto } from './userConfigDto'

export interface UpdateUserDto {
  fullName?: string
  config: UserConfigDto
}
