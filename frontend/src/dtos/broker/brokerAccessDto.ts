import type { AccessLevel } from '@/enums/accessLevel'

export interface BrokerAccessDto {
  userId: string
  accessLevel: AccessLevel
}
