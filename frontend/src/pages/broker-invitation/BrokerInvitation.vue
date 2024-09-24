<script lang="ts" setup>
import { useAcceptBrokerInvitationMutation } from '@/api/brokers/acceptBrokerInvitationMutation'
import { useBrokerInvitationQuery } from '@/api/brokers/brokerInvitationQuery'
import { useIdentity } from '@/composables/identityComposable'
import { errorToToast } from '@/utils/errorUtils'
import { useHead } from '@unhead/vue'
import { differenceInSeconds } from 'date-fns'
import { useToast } from 'primevue/usetoast'
import { computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'

const route = useRoute()
const router = useRouter()
const toast = useToast()

const {
  query: { data: user }
} = useIdentity()
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
      router.push({
        name: 'app'
      })
    })
    .catch((e) => toast.add(errorToToast(e)))
}

useHead({
  title: 'Broker Invitation - ResQueue',
  meta: [
    { name: 'robots', content: 'noindex, nofollow' } // Prevents the page from being indexed
  ]
})
</script>

<template>
  <div class="mt-16 flex h-screen grow flex-col items-center">
    <RouterLink :to="{ name: 'home' }" class="mb-4 flex items-center py-3">
      <div class="flex items-center justify-end rounded-lg bg-black p-2">
        <i class="pi pi-database rotate-90 text-white" style="font-size: 1.5rem"></i>
      </div>
      <div class="px-2 text-2xl font-semibold">
        ResQueue
        <span class="font-normal text-gray-500"><i class="pi pi-angle-right"></i> Broker Invitation</span>
      </div>
    </RouterLink>

    <div
      v-if="
        data &&
        user &&
        user?.id === data?.inviteeId &&
        !data?.isAccepted &&
        differenceInSeconds(data?.expiresAt, new Date()) > 0
      "
      class="flex w-96 flex-col gap-6 rounded-xl border bg-white p-8 text-center shadow-md"
    >
      User {{ data?.inviterEmail }} invited you to join and collaborate on a broker team.

      <div class="text-xl font-semibold">{{ data?.brokerName }}</div>

      <Button @click="acceptBrokerInvitation" label="Accept Invitation"></Button>
    </div>
    <div v-else class="flex w-96 flex-col gap-6 rounded-xl border bg-white p-8 text-center shadow-md">
      Access to this link is restricted to invited users who are logged in. It appears you don't have the necessary
      permissions to view this content.
    </div>
  </div>
</template>
