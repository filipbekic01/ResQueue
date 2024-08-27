<script setup lang="ts">
import { useBrokersQuery } from '@/api/broker/brokersQuery'
import { useIdentity } from '@/composables/identityComposable'
import { RouterLink, useRoute, useRouter } from 'vue-router'
import CreateBrokerDialog from '@/dialogs/CreateBrokerDialog.vue'
import { useDialog } from 'primevue/usedialog'
import Button from 'primevue/button'
import { computed } from 'vue'
import type { ResqueueRoute } from './SidebarRouterLink.vue'
import SidebarRouterLink from './SidebarRouterLink.vue'
import { useToast } from 'primevue/usetoast'

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

const router = useRouter()
const route = useRoute()
const toast = useToast()
const dialog = useDialog()

const openCreateBrokerDialog = () => {
  dialog.open(CreateBrokerDialog, {
    props: {
      header: 'Add Broker',
      style: {
        width: '30rem'
      },
      modal: true
    }
  })
}

const staticRoutes = computed<ResqueueRoute[]>(() => [
  {
    id: 0,
    label: 'Control Panel',
    icon: 'pi pi-th-large',
    to: {
      name: 'app'
    }
  }
  // {
  //   id: 0,
  //   label: 'Updates',
  //   icon: 'pi pi-history',
  //   to: {
  //     name: 'updates'
  //   }
  // }
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

const openFullNameEditPage = () => {
  if (route.name === 'app') {
    toast.add({
      severity: 'secondary',
      summary: 'Set Full Name',
      detail: 'Please provide your full name below.',
      life: 6000
    })
  } else {
    router.push({ name: 'app' })
  }
}
</script>

<template>
  <div class="flex h-screen gap-2 p-2 bg-gray-100" v-if="user">
    <div class="basis-72 w-72 shrink-0 flex flex-col">
      <div class="flex flex-row gap-3 items-center py-3 px-2 ms-2 me-3 border-b border-slate-200">
        <RouterLink :to="{ name: 'home' }">
          <div class="grow flex items-center justify-end bg-black p-2.5 rounded-lg">
            <i class="pi pi-database text-white rotate-90" style="font-size: 1.5rem"></i>
          </div>
        </RouterLink>
        <div class="flex flex-col grow leading-5 overflow-hidden">
          <span class="font-bold" v-if="user.fullName">{{ user.fullName }}</span>
          <div v-else class="overflow-hidden flex">
            <span
              @click="openFullNameEditPage"
              class="whitespace-nowrap overflow-ellipsis overflow-hidden border-dashed border-slate-500 font-bold text-blue-500 cursor-pointer hover:text-blue-400 hover:border-blue-500 hover:border-solid"
              >Set full name<i class="pi pi-pencil ms-2" style="font-size: 0.85rem"></i
            ></span>
          </div>
          <span class="overflow-ellipsis overflow-hidden whitespace-nowrap">{{ user.email }}</span>
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
