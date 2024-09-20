import type { AccessLevel } from '@/enums/accessLevel'
import type { BrokerSettingsDto } from './brokerSettingsDto'

export interface BrokerAccessDto {
  userId: string
  settings: BrokerSettingsDto
  accessLevel: AccessLevel
}
