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
  host: 'localhost'
})

const createBroker = () => {
  createBrokerAsync(newBroker).then(() => {
    dialogRef?.value.close()
  })
}
</script>

<template>
  <div class="flex flex-col gap-3">
    <div class="flex grow flex-col items-start gap-2">
      <label for="name" class="font-semibold">Name</label>
      <InputText v-model="newBroker.name" id="name" autocomplete="off" />
    </div>
    <div class="flex flex-col gap-2">
      <label for="username" class="font-semibold">Username</label>
      <InputText v-model="newBroker.username" id="username" autocomplete="off" />
    </div>
    <div class="flex flex-col gap-2">
      <label for="password" class="font-semibold">Password</label>
      <InputText id="password" v-model="newBroker.password" type="password" autocomplete="off" />
    </div>
    <div class="flex items-center gap-4">
      <div class="flex basis-2/3 flex-col gap-2">
        <label for="url" class="font-semibold">Host</label>
        <InputText id="url" v-model="newBroker.host" autocomplete="off" />
      </div>
      <div class="flex basis-1/3 flex-col gap-2">
        <label for="port" class="font-semibold">Port</label>
        <InputNumber
          :use-grouping="false"
          id="port"
          v-model="newBroker.port"
          type="password"
          autocomplete="off"
        />
      </div>
    </div>
  </div>
  <div class="flex justify-end gap-2">
    <Button type="button" label="Cancel" severity="secondary" @click="dialogRef?.close()"></Button>
    <Button type="button" label="Create broker" @click="createBroker"></Button>
  </div>
</template>
