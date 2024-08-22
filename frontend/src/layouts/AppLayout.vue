<script setup lang="ts">
import { useBrokersQuery } from '@/api/broker/brokersQuery'
import { useIdentity } from '@/composables/identityComposable'
import { RouterLink } from 'vue-router'
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

const {
  query: { data: user }
} = useIdentity()
const { data: brokers } = useBrokersQuery()

const dialog = useDialog()

const openCreateBrokerDialog = () => {
  dialog.open(CreateBrokerDialog, {})
}

const staticRoutes = computed<ResqueueRoute[]>(() => [
  {
    id: 0,
    label: 'Dashboard',
    icon: 'pi pi-th-large',
    to: {
      name: 'app'
    }
  }
])

const brokerRoutes = computed<ResqueueRoute[]>(() => {
  let id = 0

  const routes =
    brokers.value?.map((broker) => ({
      id: ++id,
      label: broker.name ?? '',
      icon: 'pi pi-sort',
      to: {
        name: 'queues',
        params: {
          brokerId: broker.id
        }
      }
    })) ?? []

  return routes
})
</script>

<template>
  <div class="flex h-screen gap-2 p-2 bg-gray-100" v-if="user">
    <div class="basis-70 w-70 basis-70 shrink-0 flex flex-col">
      <div class="flex flex-row gap-3 items-center py-3 px-2 ms-2 me-3 border-b border-slate-200">
        <RouterLink :to="{ name: 'home' }">
          <div class="grow flex items-center justify-end bg-black p-2.5 rounded-lg">
            <i class="pi pi-database text-white rotate-90" style="font-size: 1.5rem"></i>
          </div>
        </RouterLink>
        <div class="flex flex-col grow">
          <span class="font-bold"> Filip Bekic</span>
          <span>{{ user.email }}</span>
        </div>
      </div>

      <div class="flex flex-col ms-2 me-3 gap-2 mt-4 grow mb-1">
        <SidebarRouterLink
          v-for="staticRoute in staticRoutes"
          :key="staticRoute.id"
          v-bind="staticRoute"
        />

        <div class="border-b border-slate-200 my-2"></div>

        <SidebarRouterLink
          v-for="brokerRoute in brokerRoutes"
          :key="brokerRoute.id"
          v-bind="brokerRoute"
        />

        <Button
          @click="openCreateBrokerDialog()"
          icon="pi pi-plus"
          :class="[
            {
              'mt-auto': brokers?.length
            }
          ]"
          label="Add Broker"
        ></Button>
      </div>
    </div>

    <div class="bg-white flex flex-col grow rounded-2xl border border-slate-200 overflow-auto">
      <div class="border-b px-4 py-3" v-if="!hideHeader">
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
</template>
