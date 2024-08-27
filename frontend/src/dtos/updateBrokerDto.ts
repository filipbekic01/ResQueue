import type { BrokerSettingsDto } from './brokerSettingsDto'

export interface UpdateBrokerDto {
  name: string
  username: string
  password: string
  port: number
  host: string
  settings: BrokerSettingsDto
}
