import type { AccessLevel } from '@/enums/accessLevel'

export interface ManageBrokerAccessDto {
  brokerId: string
  userId: string
  accessLevel?: AccessLevel | null
}
