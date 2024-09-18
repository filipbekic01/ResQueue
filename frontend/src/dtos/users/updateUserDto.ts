import type { UserSettingsDto } from './userSettings'

export interface UpdateUserDto {
  fullName?: string
  settings: UserSettingsDto
}
