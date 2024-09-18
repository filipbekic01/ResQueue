import type { BrokerDto } from '@/dtos/broker/brokerDto'
import { AccessLevel } from '@/enums/accessLevel'

export const isBrokerOwner = (broker: BrokerDto, userId: string) =>
  broker.accessList.some((x) => x.userId === userId && x.accessLevel === AccessLevel.Owner)

export const isBrokerEditor = (broker: BrokerDto, userId: string) =>
  broker.accessList.some((x) => x.userId === userId && x.accessLevel === AccessLevel.Editor)

export const isBrokerViewer = (broker: BrokerDto, userId: string) =>
  broker.accessList.some((x) => x.userId === userId && x.accessLevel === AccessLevel.Viewer)
