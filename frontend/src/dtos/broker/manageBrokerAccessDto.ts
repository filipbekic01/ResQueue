import type { AccessLevel } from '@/enums/accessLevel'

export interface ManageBrokerAccessDto {
  userId: string
  accessLevel?: AccessLevel | null
}
