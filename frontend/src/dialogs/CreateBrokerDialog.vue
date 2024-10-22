<script setup lang="ts">
import { useCreateBrokerMutation } from '@/api/brokers/createBrokerMutation'
import { useTestConnectionMutation } from '@/api/brokers/testConnectionRequest'
import type { CreateBrokerDto } from '@/dtos/broker/createBrokerDto'
import { errorToToast } from '@/utils/errorUtils'
import type { DynamicDialogOptions } from 'primevue/dynamicdialogoptions'
import { useToast } from 'primevue/usetoast'
import { inject, reactive, type Ref } from 'vue'

const toast = useToast()

const { mutateAsync: createBrokerAsync, isPending: isCreateBrokerPending } = useCreateBrokerMutation()
const { mutateAsync: testConnectionAsync, isPending: isTestConnectionPending } = useTestConnectionMutation()

const dialogRef = inject<Ref<DynamicDialogOptions>>('dialogRef')

const newBroker = reactive<CreateBrokerDto>({
  name: 'md-local',
  rabbitMQConnection: {
    username: 'rabbitmq',
    password: 'rabbitmq',
    managementPort: 15671,
    managementTls: true,
    amqpPort: 5671,
    amqpTls: true,
    host: 'localhost',
    vHost: '/'
  },
  postgresConnection: {
    username: 'postgres',
    password: 'postgres',
    database: 'sandbox',
    port: 5431,
    host: 'localhost'
  }
})

const createBroker = () => {
  createBrokerAsync(newBroker)
    .then((data) => {
      dialogRef?.value.close(data)
    })
    .catch((e) => toast.add(errorToToast(e)))
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
    .catch((e) => toast.add(errorToToast(e)))
}
</script>

<template>
  <div class="mb-5 flex flex-col gap-4">
    <div class="flex grow gap-2">
      <div class="flex grow flex-col gap-2">
        <label for="name" class="font-semibold">Name</label>
        <InputText v-model="newBroker.name" id="name" autocomplete="off" />
      </div>
    </div>
    <template v-if="newBroker.postgresConnection">
      <div class="flex gap-3">
        <div class="flex grow flex-col gap-2">
          <label for="postgresConnection.username" class="font-semibold">Username</label>
          <InputText
            v-model="newBroker.postgresConnection.username"
            id="postgresConnection.username"
            autocomplete="off"
          />
        </div>
        <div class="flex grow flex-col gap-2">
          <label for="postgresConnection.password" class="font-semibold">Password</label>
          <InputText
            id="postgresConnection.password"
            v-model="newBroker.postgresConnection.password"
            type="password"
            autocomplete="off"
          />
        </div>
      </div>

      <div class="flex gap-3">
        <div class="flex grow basis-2/3 flex-col gap-2">
          <label for="postgresConnection.host" class="font-semibold">Host</label>
          <InputText id="postgresConnection.host" v-model="newBroker.postgresConnection.host" autocomplete="off" />
        </div>
        <div class="flex basis-1/3 flex-col gap-2">
          <label for="postgresConnection.managementPort" class="flex font-semibold">Port</label>
          <InputNumber
            :use-grouping="false"
            id="postgresConnection.managementPort"
            v-model="newBroker.postgresConnection.port"
            type="password"
            autocomplete="off"
          />
        </div>
        <!-- <div class="flex basis-1/2 flex-col gap-2">
          <label for="vhost" class="font-semibold">V-Host</label>
          <InputText id="vhost" v-model="newBroker.rabbitMQConnection.vHost" autocomplete="off" />
        </div> -->
      </div>

      <!-- <div class="flex gap-3">
        <div class="flex grow flex-col gap-2">
          <label for="rabbitMQConnection.managementPort" class="flex font-semibold"
            >API Port
            <span class="ms-auto flex items-center gap-3">
              TLS
              <InputSwitch
                :use-grouping="false"
                id="rabbitMQConnection.managementTls"
                v-model="newBroker.rabbitMQConnection.managementTls"
                type="password"
                autocomplete="off" /></span
          ></label>
          <InputNumber
            :use-grouping="false"
            id="rabbitMQConnection.managementPort"
            v-model="newBroker.rabbitMQConnection.managementPort"
            type="password"
            autocomplete="off"
          />
        </div>

        <div class="flex grow flex-col gap-2">
          <label for="rabbitMQConnection.amqpPort" class="flex font-semibold"
            >AMQP Port

            <span class="ms-auto flex items-center gap-3">
              TLS
              <InputSwitch
                :use-grouping="false"
                id="rabbitMQConnection.amqpTls"
                v-model="newBroker.rabbitMQConnection.amqpTls"
                type="password"
                autocomplete="off"
              />
            </span>
          </label>
          <InputNumber
            :use-grouping="false"
            id="rabbitMQConnection.amqpPort"
            v-model="newBroker.rabbitMQConnection.amqpPort"
            type="password"
            autocomplete="off"
          />
        </div>
      </div> -->
    </template>
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
