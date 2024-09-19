import type { BrokerDto } from '@/dtos/broker/brokerDto'
import { AccessLevel } from '@/enums/accessLevel'

export const isBrokerOwner = (broker: BrokerDto, userId: string) =>
  broker.accessList.some((x) => x.userId === userId && x.accessLevel === AccessLevel.Owner)

export const isBrokerManager = (broker: BrokerDto, userId: string) =>
  broker.accessList.some((x) => x.userId === userId && x.accessLevel === AccessLevel.Manager)

export const isBrokerAgent = (broker: BrokerDto, userId: string) =>
  broker.accessList.some((x) => x.userId === userId && x.accessLevel === AccessLevel.Agent)
