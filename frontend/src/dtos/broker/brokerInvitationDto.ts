export interface BrokerInvitationDto {
  id: string
  token: string
  inviteeId: string
  inviterId: string
  inviterEmail: string
  brokerName: string
  expiresAt: string
  createdAt: string
  isAccepted: boolean
  brokerId: string
}
