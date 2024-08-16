<script setup lang="ts">
import { useLogoutMutation } from '@/api/auth/logoutMutation'
import { useBrokersQuery } from '@/api/broker/brokersQuery'
import { useDeleteBrokerMutation } from '@/api/broker/deleteBrokerMutation'
import { useIdentity } from '@/composables/identityComposable'
import { RouterLink, useRouter } from 'vue-router'
import { useConfirm } from 'primevue/useconfirm'
import CreateBrokerDialog from '@/dialogs/CreateBrokerDialog.vue'
import { useDialog } from 'primevue/usedialog'
import Button from 'primevue/button'
import { computed } from 'vue'
import type { ResqueueRoute } from './SidebarRouterLink.vue'
import SidebarRouterLink from './SidebarRouterLink.vue'

withDefaults(
  defineProps<{
    hideHeader?: boolean
  }>(),
  {
    hideHeader: false
  }
)

const { user } = useIdentity()
const { mutateAsync: logoutAsync } = useLogoutMutation()
const { mutateAsync: deleteBrokerAsync } = useDeleteBrokerMutation()
const { data: brokers } = useBrokersQuery()
const confirm = useConfirm()
const router = useRouter()

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

const staticRoutes = computed<ResqueueRoute[]>(() => [
  {
    id: 0,
    label: 'Dashboard',
    icon: 'pi pi-home',
    to: {
      name: 'app'
    }
  },
  {
    id: 1,
    label: 'Settings',
    icon: 'pi pi-cog',
    to: {
      name: 'settings'
    }
  }
])

const brokerRoutes = computed<ResqueueRoute[]>(() => {
  let id = 0

  const routes =
    brokers.value?.map((broker) => ({
      id: ++id,
      label: broker.name ?? '',
      icon: 'pi pi-box',
      to: {
        name: 'broker',
        params: {
          brokerId: broker.id
        }
      }
    })) ?? []

  return routes
})
</script>

<template>
  <div class="flex h-screen gap-2 p-2" v-if="user">
    <div class="basis-70 w-7basis-70 shrink-0 flex flex-col">
      <RouterLink
        :to="{ name: 'app' }"
        class="flex flex-row gap-3 items-center py-3 px-2 ms-2 me-3 border-b border-gray-200"
      >
        <div class="grow flex items-center justify-end bg-black p-2.5 rounded-lg">
          <i class="pi pi-database text-white rotate-90" style="font-size: 1.5rem"></i>
        </div>
        <div class="flex flex-col grow">
          <span class="font-bold"> Filip Bekic</span>
          <span>{{ user.email }}</span>
        </div>
      </RouterLink>

      <div class="flex flex-col ms-2 me-3 gap-2 mt-4 grow mb-1">
        <SidebarRouterLink
          v-for="staticRoute in staticRoutes"
          :key="staticRoute.id"
          v-bind="staticRoute"
        />

        <div class="border-b border-gray-200 my-2"></div>
        <!-- <InputText placeholder="Search" variant="outlined"></InputText> -->

        <SidebarRouterLink
          v-for="brokerRoute in brokerRoutes"
          :key="brokerRoute.id"
          v-bind="brokerRoute"
        />

        <Button
          :class="[
            {
              'mt-auto': brokers?.length
            }
          ]"
          @click="openCreateBrokerDialog()"
          icon="pi pi-plus"
          label="Add Broker"
        ></Button>
      </div>
    </div>

    <div class="bg-white flex flex-col grow rounded-2xl border border-gray-200 overflow-auto">
      <div class="border-b px-5 py-3" v-if="!hideHeader">
        <div class="flex gap-2">
          <div><slot name="prepend"></slot></div>
          <div class="flex-col">
            <div class="font-bold">
              <slot name="title"></slot>
            </div>
            <div>
              <slot name="description"></slot>
            </div>
          </div>
        </div>
      </div>
      <slot></slot>
    </div>
  </div>
  <template v-else> Loading...</template>
  <!-- <div class="bg-gray-300 flex flex-col">
      <div class="flex px-2">
        <div class="text-orange-700">RabbitMQ</div>
      </div>
      <div v-for="broker in brokers" :key="broker.id" class="mt-2">
        <div class="flex px-2">
          <Button
            size="small"
            class="ms-auto"
            @click="(e) => deleteBroker(e, broker.id)"
            severity="danger"
            >Delete</Button
          >
        </div>
      </div>
      
      ----------------------- Archive?
      <div class="border-t flex flex-col mt-auto">
        <div class="p-2 ms-auto text-gray-400">{{ user!.email }}</div>
        <div class="p-2 cursor-pointer" @click="logout">Log out</div>
      </div>
    </div> -->
</template>
