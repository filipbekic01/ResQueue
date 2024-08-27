import type { BrokerSettingsDto } from './brokerSettingsDto'

export interface BrokerDto {
  id: string
  system: string
  name: string
  port: number
  host: string
  settings: BrokerSettingsDto
  createdAt: string
  updatedAt: string
  syncedAt: string
  deletedAt: string
}
