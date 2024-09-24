<script lang="ts" setup>
import { useBrokersQuery } from '@/api/brokers/brokersQuery'
import { useDeleteBrokerMutation } from '@/api/brokers/deleteBrokerMutation'
import { useManageBrokerAccessMutation } from '@/api/brokers/manageBrokerAccessMutation'
import { useTestConnectionMutation } from '@/api/brokers/testConnectionRequest'
import { useUpdateBrokerMutation } from '@/api/brokers/updateBrokerMutation'
import { useIdentity } from '@/composables/identityComposable'
import type { UpdateBrokerDto } from '@/dtos/broker/updateBrokerDto'
import { isBrokerAgent, isBrokerOwner } from '@/utils/brokerUtils'
import { errorToToast } from '@/utils/errorUtils'
import ToggleSwitch from 'primevue/toggleswitch'
import { useConfirm } from 'primevue/useconfirm'
import { useToast } from 'primevue/usetoast'
import { computed, ref, watch } from 'vue'
import { useRouter } from 'vue-router'
import BrokerOverviewAccess from './BrokerOverviewAccess.vue'

const props = defineProps<{
  brokerId: string
}>()

const toast = useToast()
const router = useRouter()
const confirm = useConfirm()
const { data: brokers } = useBrokersQuery()

const {
  query: { data: user }
} = useIdentity()
const { mutateAsync: updateBrokerAsync, isPending: isUpdateBrokerPending } = useUpdateBrokerMutation()
const { mutateAsync: testConnectionAsync, isPending: isTestConnectionPending } = useTestConnectionMutation()
const { mutateAsync: deleteBrokerAsync, isPending: isDeleteBrokerPending } = useDeleteBrokerMutation()
const { mutateAsync: manageBrokerAccessAsync } = useManageBrokerAccessMutation()

const broker = computed(() => brokers.value?.find((x) => x.id === props.brokerId))

const brokerEditable = ref<UpdateBrokerDto>()

watch(
  () => broker.value,
  (value) => {
    if (!value || !user.value) {
      return
    }

    brokerEditable.value = {
      name: value.name,
      rabbitMQConnection: value.rabbitMQConnection
        ? {
            username: '',
            password: '',
            managementPort: value.rabbitMQConnection.managementPort,
            managementTls: value.rabbitMQConnection.managementTls,
            amqpPort: value.rabbitMQConnection.amqpPort,
            amqpTls: value.rabbitMQConnection.amqpTls,
            host: value.rabbitMQConnection.host,
            vHost: value.rabbitMQConnection.vHost
          }
        : undefined,
      settings: JSON.parse(JSON.stringify(value.accessList.find((x) => x.userId === user.value?.id)?.settings))
    }
  },
  {
    immediate: true
  }
)

const updateBroker = () => {
  if (!broker.value || !brokerEditable.value) {
    return
  }

  const brokerToUpdate = JSON.parse(JSON.stringify(brokerEditable.value))

  if (!updateCredentials.value) {
    brokerToUpdate.value = ''
    brokerToUpdate.value = ''
  }

  updateBrokerAsync({ broker: brokerEditable.value, brokerId: broker.value.id })
    .then(() => {
      toast.add({
        severity: 'success',
        summary: 'Updated Broker Successfully',
        detail: 'Broker details are successfully updated.',
        life: 3000
      })

      updateCredentials.value = false
    })
    .catch((e) => toast.add(errorToToast(e)))
}

const testConnection = () => {
  if (!brokerEditable.value) {
    return
  }

  testConnectionAsync(brokerEditable.value)
    .then(() => {
      toast.add({
        severity: 'success',
        summary: 'Connection Successful',
        detail: 'The broker connection was established successfully.',
        life: 3000
      })
    })
    .catch((e) => toast.add(errorToToast(e)))
}

const addQuickSearchModel = ref('')
const addQuickSearch = () => {
  if (!brokerEditable.value) {
    return
  }

  brokerEditable.value.settings.quickSearches.push(addQuickSearchModel.value)
  addQuickSearchModel.value = ''
}

const updateQuickSearches = (value: string) => {
  if (!brokerEditable.value) {
    return
  }

  brokerEditable.value.settings.quickSearches = brokerEditable.value?.settings.quickSearches.filter((x) => x != value)
}

const updateCredentials = ref(false)

const deleteBroker = () => {
  confirm.require({
    header: `Delete Broker`,
    message: `Do you want to delete ${broker.value?.name} broker?`,
    icon: 'pi pi-exclamation-circle',
    rejectProps: {
      label: 'Cancel',
      severity: 'secondary',
      outlined: true
    },
    acceptProps: {
      label: 'Delete',
      severity: 'danger'
    },
    accept: () => {
      if (!broker.value) {
        return
      }

      deleteBrokerAsync(broker.value.id).then(() => {
        router.push({
          name: 'app'
        })
      })
    },
    reject: () => {}
  })
}

const leaveBroker = () => {
  confirm.require({
    header: `Leave Broker`,
    message: `Do you want to leave ${broker.value?.name} broker?`,
    icon: 'pi pi-exclamation-circle',
    rejectProps: {
      label: 'Cancel',
      severity: 'secondary',
      outlined: true
    },
    acceptProps: {
      label: 'Leave Broker',
      severity: 'danger'
    },
    accept: () => {
      if (!user.value || !broker.value) {
        return
      }

      manageBrokerAccessAsync({
        brokerId: broker.value.id,
        userId: user.value.id,
        accessLevel: null
      })
        .then(() => {
          router.push({
            name: 'app'
          })
        })
        .catch((e) => toast.add(errorToToast(e)))
    },
    reject: () => {}
  })
}
</script>

<template>
  <div v-if="brokerEditable" class="flex max-w-[70rem] flex-col gap-7 p-7">
    <div
      v-if="broker && user && !isBrokerAgent(broker, user?.id)"
      class="flex grow flex-col gap-3 rounded-xl border border-gray-200 p-5"
    >
      <div class="text-lg font-medium">Common Settings</div>
      <div class="flex flex-col gap-3">
        <div class="flex flex-col gap-2">
          <label for="name" class="">Name</label>
          <InputText v-model="brokerEditable.name" id="name" autocomplete="off" />
        </div>
      </div>
    </div>
    <div v-if="broker && user" class="flex flex-row gap-7">
      <div class="flex w-1/2 grow basis-1/2 flex-col gap-3 rounded-xl border border-gray-200 p-5">
        <div class="text-lg font-medium">Personal Settings</div>
        <div class="flex grow flex-col gap-3">
          <div class="flex items-center">
            Hide Queue Name Prefix
            <i
              v-tooltip="
                'Enter a common prefix to hide it in the table, making queue names shorter and easier to navigate. For example, hiding a repeating namespace can reduce clutter.'
              "
              class="pi pi-question-circle me-2 ms-auto cursor-pointer text-gray-400"
            ></i>
          </div>
          <div class="flex flex-col gap-3">
            <InputText placeholder="Enter prefix to hide" v-model="brokerEditable.settings.queueTrimPrefix"></InputText>
          </div>
        </div>
        <div class="flex grow flex-col gap-3">
          <div class="flex items-center">
            Dead-Letter Queue Suffix
            <i
              v-tooltip="
                'Used for various features, including the automatic discovery of topic (exchange) destinations.'
              "
              class="pi pi-question-circle me-2 ms-auto cursor-pointer text-gray-400"
            ></i>
          </div>
          <div class="flex flex-col gap-3">
            <InputText
              placeholder="Set custom suffix"
              v-model="brokerEditable.settings.deadLetterQueueSuffix"
            ></InputText>
          </div>
        </div>
        <div class="flex grow flex-col gap-3">
          <div class="flex items-center">
            Quick Search Suggestions
            <i
              v-tooltip="'Helps you quickly search through queues.'"
              class="pi pi-question-circle me-2 ms-auto cursor-pointer text-gray-400"
            ></i>
          </div>
          <div class="flex flex-col gap-3">
            <div class="flex grow gap-3">
              <InputText placeholder="Add new option" class="grow" v-model="addQuickSearchModel"></InputText>
              <Button icon="pi pi-plus" outlined @click="addQuickSearch"></Button>
            </div>
            <ButtonGroup>
              <Button
                v-for="qs in brokerEditable.settings.quickSearches"
                :key="qs"
                @click="updateQuickSearches(qs)"
                :label="qs"
                v-tooltip.top="'Click to remove'"
                outlined
              ></Button>
            </ButtonGroup>
          </div>
        </div>
      </div>

      <div
        v-if="brokerEditable.rabbitMQConnection && !isBrokerAgent(broker, user.id)"
        class="flex w-1/2 grow basis-1/2 flex-col gap-3 rounded-xl border border-gray-200 p-5"
      >
        <div class="flex items-center gap-2 text-lg font-medium">
          Connection Details
          <ToggleSwitch v-model="updateCredentials" class="ms-auto"></ToggleSwitch>
          <label class="text-base font-normal">Credentials</label>
        </div>

        <div class="flex flex-col gap-3 rounded-xl">
          <div class="flex gap-3" v-if="updateCredentials">
            <div class="flex grow flex-col gap-2">
              <label for="username">Username</label>
              <InputText
                placeholder="Enter username"
                v-model="brokerEditable.rabbitMQConnection.username"
                id="username"
                autocomplete="off"
              />
            </div>
            <div class="flex grow flex-col gap-2">
              <label for="password">Password</label>
              <InputText
                placeholder="*******"
                id="password"
                v-model="brokerEditable.rabbitMQConnection.password"
                type="password"
                autocomplete="off"
              />
            </div>
          </div>

          <div class="flex items-center gap-4">
            <div class="flex grow flex-col gap-2">
              <label for="url">Host</label>
              <InputText id="url" v-model="brokerEditable.rabbitMQConnection.host" autocomplete="off" />
            </div>
            <div class="flex grow flex-col gap-2">
              <label for="vhost">V-Host</label>
              <InputText id="vhost" v-model="brokerEditable.rabbitMQConnection.vHost" autocomplete="off" />
            </div>
          </div>

          <div class="flex gap-3">
            <div class="flex grow flex-col gap-2">
              <label for="rabbitMQConnection.managementPort" class="flex"
                >API Port
                <span class="ms-auto flex items-center gap-3">
                  TLS
                  <InputSwitch
                    :use-grouping="false"
                    id="rabbitMQConnection.managementTls"
                    v-model="brokerEditable.rabbitMQConnection.managementTls"
                    type="password"
                    autocomplete="off" /></span
              ></label>
              <InputNumber
                :use-grouping="false"
                id="rabbitMQConnection.managementPort"
                v-model="brokerEditable.rabbitMQConnection.managementPort"
                type="password"
                autocomplete="off"
              />
            </div>

            <div class="flex grow flex-col gap-2">
              <label for="rabbitMQConnection.amqpPort" class="flex"
                >AMQP Port

                <span class="ms-auto flex items-center gap-3">
                  TLS
                  <InputSwitch
                    :use-grouping="false"
                    id="rabbitMQConnection.amqpTls"
                    v-model="brokerEditable.rabbitMQConnection.amqpTls"
                    type="password"
                    autocomplete="off"
                  />
                </span>
              </label>
              <InputNumber
                :use-grouping="false"
                id="rabbitMQConnection.amqpPort"
                v-model="brokerEditable.rabbitMQConnection.amqpPort"
                type="password"
                autocomplete="off"
              />
            </div>
          </div>
        </div>
        <Button
          v-if="updateCredentials"
          type="button"
          label="Test Connection"
          icon="pi pi-arrow-right-arrow-left"
          severity="secondary"
          :loading="isTestConnectionPending"
          @click="testConnection"
        ></Button>
      </div>
    </div>
    <div v-else>
      <div class="flex w-1/2 grow basis-1/2 flex-col gap-3 rounded-xl border border-gray-200 p-5">
        <div class="text-lg font-medium">Broker Access Limited</div>
        You have "Agent" permissions, which grant access only to the queues topics and queues page. Access to the broker
        overview is restricted to "Owner" and "Manager" levels.
      </div>
      <Button
        outlined
        label="Leave Broker"
        severity="danger"
        icon="pi pi-times"
        class="mt-4"
        @click="leaveBroker"
      ></Button>
    </div>

    <div v-if="broker && user && isBrokerOwner(broker, user.id)">
      <BrokerOverviewAccess :broker="broker" />
    </div>

    <div v-if="broker && user" class="flex max-w-[70rem]">
      <Button
        v-if="isBrokerOwner(broker, user.id)"
        label="Delete Broker"
        icon="pi pi-trash"
        severity="danger"
        outlined
        @click="deleteBroker"
        :loading="isDeleteBrokerPending"
      ></Button>
      <Button
        v-else
        label="Leave Broker"
        icon="pi pi-times"
        severity="danger"
        outlined
        @click="leaveBroker"
        :loading="isDeleteBrokerPending"
      ></Button>

      <Button
        label="Update Broker"
        icon="pi pi-arrow-right"
        icon-pos="right"
        class="ms-auto"
        @click="updateBroker"
        :loading="isUpdateBrokerPending"
      ></Button>
    </div>
  </div>
  <div v-else>loading...</div>
</template>
