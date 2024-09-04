<script lang="ts" setup>
import { useBrokersQuery } from '@/api/broker/brokersQuery'
import { useDeleteBrokerMutation } from '@/api/broker/deleteBrokerMutation'
import { useTestConnectionMutation } from '@/api/broker/testConnectionRequest'
import { useUpdateBrokerMutation } from '@/api/broker/updateBrokerMutation'
import type { UpdateBrokerDto } from '@/dtos/updateBrokerDto'
import { extractErrorMessage } from '@/utils/errorUtil'
import ToggleSwitch from 'primevue/toggleswitch'
import { useConfirm } from 'primevue/useconfirm'
import { useToast } from 'primevue/usetoast'
import { computed, ref, watch } from 'vue'
import { useRouter } from 'vue-router'

const props = defineProps<{
  brokerId: string
}>()

const toast = useToast()
const router = useRouter()
const confirm = useConfirm()
const { data: brokers } = useBrokersQuery()

const { mutateAsync: updateBrokerAsync, isPending: isUpdateBrokerPending } =
  useUpdateBrokerMutation()
const { mutateAsync: testConnectionAsync, isPending: isTestConnectionPending } =
  useTestConnectionMutation()
const { mutateAsync: deleteBrokerAsync, isPending: isDeleteBrokerPending } =
  useDeleteBrokerMutation()

const broker = computed(() => brokers.value?.find((x) => x.id === props.brokerId))

const brokerEditable = ref<UpdateBrokerDto>()

watch(
  () => broker.value,
  (value) => {
    if (!value) {
      return
    }

    brokerEditable.value = {
      name: value.name,
      host: value.host,
      username: '',
      password: '',
      port: value.port,
      settings: JSON.parse(JSON.stringify(value.settings))
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

  updateBrokerAsync({ broker: brokerEditable.value, brokerId: broker.value.id }).then(() => {
    toast.add({
      severity: 'success',
      summary: 'Updated Broker Successfully',
      detail: 'Broker details are successfully updated.',
      life: 6000
    })

    updateCredentials.value = false
  })
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
        life: 6000
      })
    })
    .catch((e) => {
      toast.add({
        severity: 'error',
        summary: 'Connection Failed',
        detail: extractErrorMessage(e),
        life: 6000
      })
    })
}

const addQuickSearchModel = ref('')
const addQuickSearch = (value: string) => {
  if (!brokerEditable.value) {
    return
  }

  brokerEditable.value.settings.quickSearches.push(value)
  addQuickSearchModel.value = ''
}

const updateQuickSearches = (value: string) => {
  if (!brokerEditable.value) {
    return
  }

  brokerEditable.value.settings.quickSearches = brokerEditable.value?.settings.quickSearches.filter(
    (x) => x != value
  )
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
</script>

<template>
  <div v-if="brokerEditable" class="flex max-w-[36rem] flex-col gap-7 p-7">
    <div class="flex flex-col gap-3 rounded-xl border border-gray-200 p-5">
      <div class="text-lg font-medium">Broker Settings</div>
      <div class="flex flex-col gap-3">
        <div class="flex flex-col gap-2">
          <label for="name" class="">Name</label>
          <InputText v-model="brokerEditable.name" id="name" autocomplete="off" />
        </div>
      </div>
      <div class="flex grow flex-col gap-3">
        <div class="">Dead-Letter Queue Suffix</div>
        <div class="flex flex-col gap-3">
          <InputText
            placeholder="Set custom suffix"
            v-model="brokerEditable.settings.deadLetterQueueSuffix"
            @change="(e) => addQuickSearch((e.target as any).value)"
          ></InputText>
        </div>
      </div>
      <div class="flex grow flex-col gap-3">
        <div class="">Quick Search Suggestions</div>
        <div class="flex flex-col gap-3">
          <InputText
            placeholder="Add new option"
            v-model="addQuickSearchModel"
            @change="(e) => addQuickSearch((e.target as any).value)"
          ></InputText>
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
    <div class="flex flex-col gap-3 rounded-xl border border-gray-200 p-5">
      <div class="flex items-center gap-2 text-lg font-medium">
        Connection Details
        <ToggleSwitch v-model="updateCredentials" class="ms-auto"></ToggleSwitch>
        <label class="text-base font-normal">Credentials</label>
      </div>

      <div class="flex flex-col gap-3 rounded-xl">
        <template v-if="updateCredentials">
          <div class="flex flex-col gap-2">
            <label for="username">Username</label>
            <InputText
              placeholder="Enter username"
              v-model="brokerEditable.username"
              id="username"
              autocomplete="off"
            />
          </div>
          <div class="flex flex-col gap-2">
            <label for="password">Password</label>
            <InputText
              placeholder="*******"
              id="password"
              v-model="brokerEditable.password"
              type="password"
              autocomplete="off"
            />
          </div>
        </template>
        <div class="flex items-center gap-4">
          <div class="flex basis-1/2 flex-col gap-2">
            <label for="url">Host</label>
            <InputText id="url" v-model="brokerEditable.host" autocomplete="off" />
          </div>
          <div class="flex basis-1/2 flex-col gap-2">
            <label for="port" class="flex"
              >Port<label class="ms-auto font-normal text-slate-500"
                >80, 443, 15671...</label
              ></label
            >
            <InputNumber
              :use-grouping="false"
              id="port"
              v-model="brokerEditable.port"
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

    <div class="flex">
      <Button
        label="Delete Broker"
        icon="pi pi-trash"
        severity="danger"
        outlined
        @click="deleteBroker"
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
