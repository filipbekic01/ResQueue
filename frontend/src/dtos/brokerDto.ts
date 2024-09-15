import type { BrokerAccessDto } from './broker/brokerAccessDto'
import type { BrokerSettingsDto } from './brokerSettingsDto'

export interface BrokerDto {
  id: string
  accessList: BrokerAccessDto[]
  system: string
  name: string
  port: number
  host: string
  vHost: string
  settings: BrokerSettingsDto
  createdAt: string
  updatedAt: string
  syncedAt: string
  deletedAt: string
}
