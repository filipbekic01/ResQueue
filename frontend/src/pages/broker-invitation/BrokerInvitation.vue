<script lang="ts" setup>
import { useAcceptBrokerInvitationMutation } from '@/api/broker/acceptBrokerInvitationMutation'
import { useBrokerInvitationQuery } from '@/api/broker/brokerInvitationQuery'
import { computed } from 'vue'
import { useRoute } from 'vue-router'

const route = useRoute()

const { mutateAsync: acceptBrokerInvitationAsync } = useAcceptBrokerInvitationMutation()
const { data } = useBrokerInvitationQuery(computed(() => route.query.token?.toString()))

const acceptBrokerInvitation = () => {
  if (!route.query.token) {
    return
  }

  acceptBrokerInvitationAsync({
    token: route.query.token.toString()
  })
    .then(() => {
      // ...
    })
    .catch(() => {
      // ...
    })
}
</script>

<template>
  <div class="mt-16 flex h-screen grow flex-col items-center">
    <RouterLink :to="{ name: 'home' }" class="mb-4 flex items-center py-3">
      <div class="flex items-center justify-end rounded-lg bg-black p-2">
        <i class="pi pi-database rotate-90 text-white" style="font-size: 1.5rem"></i>
      </div>
      <div class="px-2 text-2xl font-semibold">
        ResQueue
        <span class="font-normal text-gray-500"
          ><i class="pi pi-angle-right"></i> Broker Invitation</span
        >
      </div>
    </RouterLink>
    <div class="flex w-96 flex-col gap-6 rounded-xl border bg-white p-8 text-center shadow-md">
      User {{ data?.invitedBy }} invited you to join and collaborate on a broker team.

      <div class="text-xl font-semibold">{{ data?.brokerName }}</div>

      <Button @click="acceptBrokerInvitation" label="Accept Invitation"></Button>
    </div>
  </div>
</template>
