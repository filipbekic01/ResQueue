<script setup lang="ts">
import type { CreateBrokerDto } from '@/dtos/createBrokerDto'
import { inject, reactive, ref, type Ref } from 'vue'
import type { DynamicDialogOptions } from 'primevue/dynamicdialogoptions'
import { useCreateBrokerMutation } from '@/api/broker/createBrokerMutation'

const { mutateAsync: createBrokerAsync } = useCreateBrokerMutation()

const dialogRef = inject<Ref<DynamicDialogOptions>>('dialogRef')

const frameworks = ref([
  { name: 'None', code: '' },
  { name: 'MassTransit', code: 'masstransit' }
])

const newBroker = reactive<CreateBrokerDto>({
  name: 'local',
  username: 'rabbitmq',
  password: 'rabbitmq',
  port: 15671,
  host: 'localhost',
  framework: ''
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
    <label for="url" class="font-semibold w-24">Host</label>
    <InputText id="url" v-model="newBroker.host" class="flex-auto" autocomplete="off" />
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
  <div class="flex items-center gap-4 mb-8">
    <label for="port" class="font-semibold w-24">Framework (optional)</label>
    <Select
      v-model="newBroker.framework"
      :options="frameworks"
      optionLabel="name"
      option-value="code"
      placeholder="Select a Framework"
      class="w-full md:w-56"
    ></Select>
  </div>

  <div class="flex justify-end gap-2">
    <Button type="button" label="Cancel" severity="secondary" @click="dialogRef?.close()"></Button>
    <Button type="button" label="Create broker" @click="createBroker"></Button>
  </div>
</template>
