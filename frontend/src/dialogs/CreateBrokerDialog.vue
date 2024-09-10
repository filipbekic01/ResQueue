<script setup lang="ts">
import { useCreateBrokerMutation } from '@/api/broker/createBrokerMutation'
import { useTestConnectionMutation } from '@/api/broker/testConnectionRequest'
import type { CreateBrokerDto } from '@/dtos/createBrokerDto'
import { extractErrorMessage } from '@/utils/errorUtils'
import type { DynamicDialogOptions } from 'primevue/dynamicdialogoptions'
import { useToast } from 'primevue/usetoast'
import { inject, reactive, type Ref } from 'vue'

const toast = useToast()

const { mutateAsync: createBrokerAsync, isPending: isCreateBrokerPending } =
  useCreateBrokerMutation()
const { mutateAsync: testConnectionAsync, isPending: isTestConnectionPending } =
  useTestConnectionMutation()

const dialogRef = inject<Ref<DynamicDialogOptions>>('dialogRef')

const newBroker = reactive<CreateBrokerDto>({
  name: 'md-local',
  username: 'rabbitmq',
  password: 'rabbitmq',
  port: 15671,
  host: 'localhost',
  vhost: '/'
})

const createBroker = () => {
  createBrokerAsync(newBroker).then((data) => {
    dialogRef?.value.close(data)
  })
}

const testConnection = () => {
  testConnectionAsync(newBroker)
    .then(() => {
      toast.add({
        severity: 'success',
        summary: 'Connection Successful',
        detail: 'The broker connection was established successfully.',
        life: 3000
      })
    })
    .catch((e) => {
      toast.add({
        severity: 'error',
        summary: 'Connection Failed',
        detail: extractErrorMessage(e),
        life: 3000
      })
    })
}
</script>

<template>
  <div class="mb-5 flex flex-col gap-4">
    <div class="flex grow gap-2">
      <div class="flex grow flex-col gap-2">
        <label for="name" class="font-semibold">Name</label>
        <InputText v-model="newBroker.name" id="name" autocomplete="off" />
      </div>
      <div class="flex grow flex-col gap-2">
        <label for="broker" class="font-semibold">Broker</label>
        <InputText :model-value="'RabbitMQ'" disabled id="broker" autocomplete="off" />
      </div>
    </div>
    <!-- <div class="flex flex-col border border-slate-200 rounded-xl p-4 gap-3"> -->
    <div class="flex flex-col gap-2">
      <label for="username" class="font-semibold">Username</label>
      <InputText v-model="newBroker.username" id="username" autocomplete="off" />
    </div>
    <div class="flex flex-col gap-2">
      <label for="password" class="font-semibold">Password</label>
      <InputText id="password" v-model="newBroker.password" type="password" autocomplete="off" />
    </div>
    <div class="flex flex-col gap-2">
      <label for="url" class="font-semibold">Host</label>
      <InputText id="url" v-model="newBroker.host" autocomplete="off" />
    </div>
    <div class="flex items-center gap-4">
      <div class="flex basis-1/2 flex-col gap-2">
        <label for="vhost" class="font-semibold">V-Host</label>
        <InputText id="vhost" v-model="newBroker.vhost" autocomplete="off" />
      </div>
      <div class="flex basis-1/2 flex-col gap-2">
        <label for="port" class="flex font-semibold"
          >Port<span class="ms-auto font-normal text-slate-500">80, 443, 15671...</span></label
        >
        <InputNumber
          :use-grouping="false"
          id="port"
          v-model="newBroker.port"
          type="password"
          autocomplete="off"
        />
      </div>
      <!-- </div> -->
    </div>
  </div>
  <div class="flex gap-2">
    <Button
      type="button"
      class="grow"
      label="Test Connection"
      icon="pi pi-arrow-right-arrow-left"
      severity="secondary"
      :loading="isTestConnectionPending"
      @click="testConnection"
    ></Button>
    <Button
      type="button"
      label="Add Broker"
      icon="pi pi-arrow-right"
      icon-pos="right"
      @click="createBroker"
      :loading="isCreateBrokerPending"
    ></Button>
  </div>
</template>
