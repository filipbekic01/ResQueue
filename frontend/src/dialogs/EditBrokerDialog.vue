<script setup lang="ts">
import { useCreateBrokerMutation } from '@/api/broker/createBrokerMutation'
import type { CreateBrokerDto } from '@/dtos/createBrokerDto'
import type { DynamicDialogOptions } from 'primevue/dynamicdialogoptions'
import { inject, reactive, type Ref } from 'vue'

const { mutateAsync: createBrokerAsync } = useCreateBrokerMutation()

const dialogRef = inject<Ref<DynamicDialogOptions>>('dialogRef')

const newBroker = reactive<CreateBrokerDto>({
  name: 'md-local',
  username: 'rabbitmq',
  password: 'rabbitmq',
  port: 15671,
  host: 'https://localhost'
})

const createBroker = () => {
  createBrokerAsync(newBroker).then(() => {
    dialogRef?.value.close()
  })
}
</script>

<template>
  <div class="mb-4 flex items-center gap-4">
    <label for="name" class="w-24 font-semibold">Name</label>
    <InputText v-model="newBroker.name" id="name" class="flex-auto" autocomplete="off" />
  </div>
  <div class="mb-4 flex items-center gap-4">
    <label for="username" class="w-24 font-semibold">Username</label>
    <InputText v-model="newBroker.username" id="username" class="flex-auto" autocomplete="off" />
  </div>
  <div class="mb-8 flex items-center gap-4">
    <label for="password" class="w-24 font-semibold">Password</label>
    <InputText
      id="password"
      v-model="newBroker.password"
      class="flex-auto"
      type="password"
      autocomplete="off"
    />
  </div>
  <div class="mb-8 flex items-center gap-4">
    <label for="url" class="w-24 font-semibold">URL</label>
    <InputText id="url" v-model="newBroker.host" class="flex-auto" autocomplete="off" />
  </div>
  <div class="mb-8 flex items-center gap-4">
    <label for="port" class="w-24 font-semibold">Port</label>
    <InputNumber
      :use-grouping="false"
      id="port"
      v-model="newBroker.port"
      class="flex-auto"
      type="password"
      autocomplete="off"
    />
  </div>
  <div class="flex justify-end gap-2">
    <Button type="button" label="Cancel" severity="secondary" @click="dialogRef?.close()"></Button>
    <Button type="button" label="Create broker" @click="createBroker"></Button>
  </div>
</template>
