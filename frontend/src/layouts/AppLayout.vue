<script setup lang="ts">
import { useLogoutMutation } from '@/api/auth/logoutMutation'
import { useBrokersQuery } from '@/api/broker/brokersQuery'
import { useDeleteBrokerMutation } from '@/api/broker/deleteBrokerMutation'
import { useIdentity } from '@/composables/identityComposable'
import { RouterLink, useRouter } from 'vue-router'
import { useConfirm } from 'primevue/useconfirm'
import CreateBrokerDialog from '@/dialogs/CreateBrokerDialog.vue'

const { user } = useIdentity()
const { mutateAsync: logoutAsync } = useLogoutMutation()
const { mutateAsync: deleteBrokerAsync } = useDeleteBrokerMutation()
const { data: brokers } = useBrokersQuery()
const confirm = useConfirm()
const router = useRouter()

import { useDialog } from 'primevue/usedialog'
import Button from 'primevue/button'

const dialog = useDialog()

const logout = () => {
  logoutAsync().then(() => {
    user.value = undefined
    router.push({ name: 'home' })
  })
}

const openCreateBrokerDialog = () => {
  dialog.open(CreateBrokerDialog, {})
}

const deleteBroker = (event: any, id: string) => {
  confirm.require({
    target: event.currentTarget,
    message: 'Do you want to delete this broker?',
    icon: 'pi pi-info-circle',
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
      deleteBrokerAsync(id)
    },
    reject: () => {}
  })
}

const openBroker = (id: string) => {
  router.push({
    name: 'broker',
    params: {
      brokerId: id
    }
  })
}

// todo: do it properly
setTimeout(() => {
  if (!user.value) {
    router.push({ name: 'home' })
  }
}, 1500)
</script>

<template>
  <div class="flex h-screen" v-if="user">
    <div class="basis-64 w-64 shrink-0 h-screen flex">
      <div class="flex flex-col rounded-lg grow">
        <RouterLink
          :to="{ name: 'app' }"
          class="w-full flex flex-col items-start py-2 px-4 text-slate-700 bg-white shadow rounded-lg ms-1 mt-3"
        >
          <!-- <i class="pi pi-home mr-3"></i> -->
          <span class="font-bold">Filip Bekic</span>
          <span>filip1994sm@gmail.com</span>
        </RouterLink>

        <nav class="flex flex-col w-full px-2">
          <RouterLink
            :to="{ name: 'app' }"
            class="w-full flex items-center py-2 px-4 text-slate-700 hover:bg-white hover:shadow rounded-lg"
          >
            <i class="pi pi-home mr-3"></i>
            <span>Dashboard</span>
          </RouterLink>
          <RouterLink
            v-for="broker in brokers"
            :key="broker.id"
            to="/page2"
            class="w-full flex items-center py-2 px-4 text-slate-700 hover:bg-white hover:shadow rounded-lg"
          >
            <i class="pi pi-calendar mr-3"></i>
            <span>{{ broker.name }}</span>
          </RouterLink>
          <RouterLink
            to="/page3"
            class="w-full flex items-center py-2 px-4 text-slate-700 hover:bg-white hover:shadow rounded-lg"
          >
            <i class="pi pi-users mr-3"></i>
            <span>Page 3</span>
          </RouterLink>
        </nav>

        <div class="mt-auto mb-6 flex flex-col items-center">
          <RouterLink
            to="/page3"
            class="w-full flex items-center py-2 px-4 text-slate-700 hover:bg-white hover:shadow rounded-lg"
          >
            <i class="pi pi-users mr-3"></i>
            <span>Help</span> </RouterLink
          ><RouterLink
            to="/page3"
            class="w-full flex items-center py-2 px-4 text-slate-700 hover:bg-white hover:shadow rounded-lg"
          >
            <i class="pi pi-users mr-3"></i>
            <span>Settings</span>
          </RouterLink>
        </div>
      </div>
    </div>
    <!-- <div class="bg-slate-300 flex flex-col">
      <div class="flex px-2">
        <div class="text-orange-700">RabbitMQ</div>
      </div>
      <div v-for="broker in brokers" :key="broker.id" class="mt-2">
        <div class="flex px-2">
          <div class="cursor-pointer" @click="openBroker(broker.id)">{{ broker.name }}</div>
          <Button
            size="small"
            class="ms-auto"
            @click="(e) => deleteBroker(e, broker.id)"
            severity="danger"
            >Delete</Button
          >
        </div>
      </div>
      <Button size="small" class="mx-2 mt-2" @click="openCreateBrokerDialog()">Create</Button>
      ----------------------- Archive?
      <div class="border-t flex flex-col mt-auto">
        <div class="p-2 ms-auto text-slate-400">{{ user!.email }}</div>
        <div class="p-2 cursor-pointer" @click="logout">Log out</div>
      </div>
    </div> -->
    <div class="grow flex overflow-auto p-3">
      <div class="bg-white flex flex-col grow rounded-lg shadow">
        <slot></slot>
      </div>
    </div>
  </div>
  <template v-else> Loading...</template>
</template>
