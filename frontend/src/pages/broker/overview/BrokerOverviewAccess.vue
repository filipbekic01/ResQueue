<script lang="ts" setup>
import { useBrokerInvitationsQuery } from '@/api/brokers/brokerInvitationsQuery'
import { useCreateBrokerInvitationMutation } from '@/api/brokers/createBrokerInvitationMutation'
import { useExpireBrokerInvitationMutation } from '@/api/brokers/expireBrokerInvitationMutation'
import { useManageBrokerAccessMutation } from '@/api/brokers/manageBrokerAccessMutation'
import { useUsersBasicQuery } from '@/api/users/usersBasicQuery'
import { useIdentity } from '@/composables/identityComposable'
import type { BrokerAccessDto } from '@/dtos/broker/brokerAccessDto'
import type { BrokerDto } from '@/dtos/broker/brokerDto'
import type { BrokerInvitationDto } from '@/dtos/broker/brokerInvitationDto'
import { AccessLevel } from '@/enums/accessLevel'
import { errorToToast } from '@/utils/errorUtils'
import { formatDistance } from 'date-fns'
import DataTable from 'primevue/datatable'
import InputText from 'primevue/inputtext'
import SelectButton from 'primevue/selectbutton'
import { useConfirm } from 'primevue/useconfirm'
import { useToast } from 'primevue/usetoast'
import { computed, ref } from 'vue'
import { useRouter } from 'vue-router'

const props = defineProps<{
  broker: BrokerDto
}>()

const toast = useToast()
const email = ref('')
const confirm = useConfirm()
const router = useRouter()

const {
  activeSubscription,
  query: { data: user }
} = useIdentity()
const { mutateAsync: createBrokerInvitationAsync, isPending } = useCreateBrokerInvitationMutation()
const { mutateAsync: manageBrokerAccessAsync, isPending: isPendingManageBrokerAccess } =
  useManageBrokerAccessMutation()
const { data: brokerInvitations } = useBrokerInvitationsQuery(computed(() => props.broker.id))
const { data: usersBasic } = useUsersBasicQuery(
  computed(
    () =>
      brokerInvitations.value
        ?.map((y) => y.inviteeId)
        .concat(props.broker.accessList.map((y) => y.userId)) ?? []
  )
)

const { mutateAsync: expireBrokerInvitationAsync } = useExpireBrokerInvitationMutation()

const createBrokerInvitation = () => {
  if (activeSubscription.value?.type !== 'ultimate') {
    confirm.require({
      header: 'Upgrade Required',
      message: `You must upgrade account to Ultimate plan for this feature.`,
      rejectProps: {
        label: 'Close',
        severity: 'secondary',
        outlined: true
      },
      acceptProps: {
        label: 'Upgrade'
      },
      accept: () => {
        router.push({
          name: 'app'
        })
      },
      reject: () => {}
    })

    return
  }

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

      email.value = ''
    })
    .catch((e) => toast.add(errorToToast(e)))
}

const accessLevels = [
  {
    label: 'Owner',
    value: AccessLevel.Owner.toString()
  },
  {
    label: 'Editor',
    value: AccessLevel.Editor.toString()
  },
  {
    label: 'Viewer',
    value: AccessLevel.Viewer.toString()
  }
]

const changeAccessLevel = (brokerAccess: BrokerAccessDto, accessLevel?: AccessLevel) => {
  if (!accessLevel) {
    return
  }

  confirm.require({
    header: 'Change Access Level',
    message: `Do you really want to change ${getUserName(brokerAccess)?.email} access level to ${accessLevel}?`,
    icon: 'pi pi-info-circle',
    rejectProps: {
      label: 'Cancel',
      severity: 'secondary',
      outlined: true
    },
    acceptProps: {
      label: 'Change'
    },
    accept: () => {
      manageBrokerAccessAsync({
        brokerId: props.broker.id,
        userId: brokerAccess.userId,
        accessLevel
      }).catch((e) => toast.add(errorToToast(e)))
    },
    reject: () => {}
  })
}

const removeAccessLevel = (brokerAccess: BrokerAccessDto) => {
  confirm.require({
    header: 'Remove Access',
    message: `Do you really want to remove ${getUserName(brokerAccess)?.email} access?`,
    icon: 'pi pi-info-circle',
    rejectProps: {
      label: 'Cancel',
      severity: 'secondary',
      outlined: true
    },
    acceptProps: {
      label: 'Remove',
      severity: 'danger'
    },
    accept: () => {
      manageBrokerAccessAsync({
        brokerId: props.broker.id,
        userId: brokerAccess.userId,
        accessLevel: undefined
      }).catch((e) => toast.add(errorToToast(e)))
    },
    reject: () => {}
  })
}

const expireBrokerInvitation = (brokerInvitation: BrokerInvitationDto) => {
  confirm.require({
    header: 'Remove Access',
    message: `Do you really want to remove ${getUserName1(brokerInvitation)?.email} invitation?`,
    icon: 'pi pi-info-circle',
    rejectProps: {
      label: 'Cancel',
      severity: 'secondary',
      outlined: true
    },
    acceptProps: {
      label: 'Remove',
      severity: 'danger'
    },
    accept: () => {
      expireBrokerInvitationAsync({
        id: brokerInvitation.id
      }).catch((e) => toast.add(errorToToast(e)))
    },
    reject: () => {}
  })
}

const getUserName = (data: BrokerAccessDto) => usersBasic.value?.find((x) => x.id === data.userId)
const getUserName1 = (data: BrokerInvitationDto) =>
  usersBasic.value?.find((x) => x.id === data.inviteeId)

const copyDirectLink = (data: BrokerInvitationDto) => {
  navigator.clipboard.writeText(`http://localhost:5173/app/broker-invitation?token=${data.token}`)
}
</script>

<template>
  <div class="flex flex-col rounded-xl border p-5">
    <div class="text-lg font-medium">Collaborators</div>
    <DataTable :value="broker.accessList" v-if="broker.accessList.length && usersBasic?.length">
      <Column field="userId">
        <template #body="{ data }">
          {{ getUserName(data)?.email }} {{ getUserName(data)?.fullName }}
        </template>
      </Column>

      <Column field="accessLevel" class="w-0">
        <template #body="{ data }">
          <SelectButton
            :disabled="data.userId === user?.id"
            :model-value="data.accessLevel"
            :options="accessLevels"
            @update:model-value="(al) => changeAccessLevel(data, al)"
            option-value="value"
            option-label="label"
          >
          </SelectButton>
        </template>
      </Column>
      <Column header="" field="actions" class="w-0">
        <template #body="{ data }">
          <Button
            outlined
            size="small"
            icon="pi pi-times"
            @click="removeAccessLevel(data)"
          ></Button>
        </template>
      </Column>
    </DataTable>

    <DataTable :value="brokerInvitations" v-if="brokerInvitations?.length && usersBasic?.length">
      <Column field="userId" header="Pending Invitations">
        <template #body="{ data }">
          <div class="flex flex-nowrap">
            {{ getUserName1(data)?.email }}
            <span
              class="ms-3 flex cursor-pointer items-center text-blue-500 hover:text-blue-400"
              @click="copyDirectLink(data)"
              ><i class="pi pi-copy me-1"></i>Copy direct link</span
            >
          </div>
        </template>
      </Column>
      <Column header="" field="actions" class="w-0">
        <template #body="{ data }">
          <div class="whitespace-nowrap">
            Expires in {{ formatDistance(new Date(), data.expiresAt) }}
          </div>
        </template>
      </Column>
      <Column header="" field="actions" class="w-0">
        <template #body="{ data }">
          <Button
            outlined
            size="small"
            icon="pi pi-times"
            @click="expireBrokerInvitation(data)"
          ></Button>
        </template>
      </Column>
    </DataTable>

    <div class="mt-5 items-center">
      <div class="flex gap-3">
        <InputText class="w-72" v-model="email" placeholder="Enter user e-mail..."></InputText>
        <Button
          :loading="isPending"
          label="Send Invitation"
          icon-pos="right"
          icon="pi pi-send"
          outlined
          @click="createBrokerInvitation"
        ></Button>
      </div>
      <div class="mt-3">Registration is required to accept the invitation.</div>
    </div>
  </div>
</template>
