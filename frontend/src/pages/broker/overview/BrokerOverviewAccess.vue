<script lang="ts" setup>
import { useCreateBrokerInvitationMutation } from '@/api/broker/createBrokerInvitationMutation'
import type { BrokerDto } from '@/dtos/brokerDto'
import { extractErrorMessage } from '@/utils/errorUtils'
import InputText from 'primevue/inputtext'
import { useToast } from 'primevue/usetoast'
import { ref } from 'vue'

const props = defineProps<{
  broker: BrokerDto
}>()

const toast = useToast()

const email = ref('filip1994sm@gmail.com')

const { mutateAsync: createBrokerInvitationAsync, isPending } = useCreateBrokerInvitationMutation()

const createBrokerInvitation = () => {
  if (!email.value.includes('@')) {
    toast.add({
      severity: 'error',
      summary: 'Invalid E-Mail',
      detail: 'Invalid e-mail address format.',
      life: 3000
    })
    return
  }

  createBrokerInvitationAsync({
    email: email.value,
    brokerId: props.broker.id
  })
    .then(() => {
      toast.add({
        severity: 'success',
        summary: 'Invitation Sent',
        detail: 'Invitation successfully sent.',
        life: 3000
      })
    })
    .catch((e) => {
      toast.add({
        severity: 'error',
        summary: 'Invitation Failed',
        detail: extractErrorMessage(e),
        life: 3000
      })
    })
}
</script>

<template>
  <div class="rounded-xl border p-5">
    <div class="text-lg font-medium">Access List</div>
    <div v-for="acc in broker.accessList" :key="acc.userId">{{ acc }}</div>
    <hr class="my-3" />
    <div class="flex gap-3">
      <InputText v-model="email" placeholder="Enter user e-mail..."></InputText>
      <Button
        :loading="isPending"
        label="Send Invitation"
        icon="pi pi-send"
        @click="createBrokerInvitation"
      ></Button>
    </div>
    <div class="mt-2 text-sm text-gray-500">
      User must be registered in order to accept the invite.
    </div>
  </div>
</template>
