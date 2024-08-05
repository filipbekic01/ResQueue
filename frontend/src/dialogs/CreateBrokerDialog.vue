<script setup lang="ts">
import type { CreateBrokerDto } from '@/dtos/createBrokerDto'
import { inject, reactive, type Ref } from 'vue'
import type { DynamicDialogOptions } from 'primevue/dynamicdialogoptions'
import { useCreateBrokerMutation } from '@/api/broker/createBrokerMutation'

const { mutateAsync: createBrokerAsync } = useCreateBrokerMutation()

const dialogRef = inject<Ref<DynamicDialogOptions>>('dialogRef')

const newBroker = reactive<CreateBrokerDto>({
  name: 'local',
  username: 'rabbitmq',
  password: 'rabbitmq',
  port: 15671,
  url: 'https://localhost'
})

const createBroker = () => {
  createBrokerAsync(newBroker).then(() => {
    dialogRef?.value.close()
  })
}
</script>

<template>
  <div class="flex items-center gap-4 mb-4">
    <label for="name" class="font-semibold w-24">Name</label>
    <InputText v-model="newBroker.name" id="name" class="flex-auto" autocomplete="off" />
  </div>
  <div class="flex items-center gap-4 mb-4">
    <label for="username" class="font-semibold w-24">Username</label>
    <InputText v-model="newBroker.username" id="username" class="flex-auto" autocomplete="off" />
  </div>
  <div class="flex items-center gap-4 mb-8">
    <label for="password" class="font-semibold w-24">Password</label>
    <InputText
      id="password"
      v-model="newBroker.password"
      class="flex-auto"
      type="password"
      autocomplete="off"
    />
  </div>
  <div class="flex items-center gap-4 mb-8">
    <label for="url" class="font-semibold w-24">URL</label>
    <InputText id="url" v-model="newBroker.url" class="flex-auto" autocomplete="off" />
  </div>
  <div class="flex items-center gap-4 mb-8">
    <label for="port" class="font-semibold w-24">Port</label>
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
