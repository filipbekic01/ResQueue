<script lang="ts" setup>
import { useBrokersQuery } from '@/api/broker/brokersQuery'
import { useTestConnectionMutation } from '@/api/broker/testConnectionRequest'
import { useUpdateBrokerMutation } from '@/api/broker/updateBrokerMutation'
import type { UpdateBrokerDto } from '@/dtos/updateBrokerDto'
import { extractErrorMessage } from '@/utils/errorUtil'
import ToggleSwitch from 'primevue/toggleswitch'
import { useToast } from 'primevue/usetoast'
import { computed, ref, watch } from 'vue'

const props = defineProps<{
  brokerId: string
}>()

const toast = useToast()
const { data: brokers } = useBrokersQuery()

const { mutateAsync: updateBrokerAsync, isPending: isUpdateBrokerPending } =
  useUpdateBrokerMutation()
const { mutateAsync: testConnectionAsync, isPending: isTestConnectionPending } =
  useTestConnectionMutation()

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
</script>

<template>
  <div v-if="brokerEditable" class="p-5 flex flex-col max-w-[36rem] gap-5">
    <div class="pt-3 rounded-xl border-gray-200 border p-5 flex flex-col gap-3">
      <div class="font-semibold text-lg">Broker Settings</div>
      <div class="flex flex-col gap-3">
        <div class="flex flex-col gap-2">
          <label for="name" class="">Name</label>
          <InputText v-model="brokerEditable.name" id="name" autocomplete="off" />
        </div>
      </div>
      <div class="flex flex-col gap-3 grow">
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
      <div class="font-semibold flex text-lg items-center gap-2">
        Connection Details
        <ToggleSwitch v-model="updateCredentials" class="ms-auto"></ToggleSwitch>
        <label class="text-base font-normal">Credentials</label>
      </div>

      <div class="flex flex-col rounded-xl gap-3">
        <div class="flex flex-col gap-2">
          <label for="username">Username</label>
          <InputText
            placeholder="Enter username"
            v-model="brokerEditable.username"
            id="username"
            autocomplete="off"
            :disabled="!updateCredentials"
          />
        </div>
        <div class="flex flex-col gap-2">
          <label for="password">Password</label>
          <InputText
            placeholder="*******"
            id="password"
            v-model="brokerEditable.password"
            :disabled="!updateCredentials"
            type="password"
            autocomplete="off"
          />
        </div>
        <div class="flex items-center gap-4">
          <div class="flex basis-1/2 flex-col gap-2">
            <label for="url">Host</label>
            <InputText id="url" v-model="brokerEditable.host" autocomplete="off" />
          </div>
          <div class="flex basis-1/2 flex-col gap-2">
            <label for="port" class="flex"
              >Port<label class="ms-auto text-slate-500 font-normal"
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

    <div class="flex gap-2 mt-2">
      <Button
        type="button"
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
