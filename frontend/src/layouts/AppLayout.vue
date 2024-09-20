<script setup lang="ts">
import { useBrokersQuery } from '@/api/brokers/brokersQuery'
import { useIdentity } from '@/composables/identityComposable'
import CreateBrokerDialog from '@/dialogs/CreateBrokerDialog.vue'
import type { BrokerDto } from '@/dtos/broker/brokerDto'
import Button from 'primevue/button'
import { useDialog } from 'primevue/usedialog'
import { useToast } from 'primevue/usetoast'
import { computed } from 'vue'
import { RouterLink, useRoute, useRouter } from 'vue-router'
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

const router = useRouter()
const route = useRoute()
const toast = useToast()
const dialog = useDialog()

const openCreateBrokerDialog = () => {
  dialog.open(CreateBrokerDialog, {
    props: {
      header: 'Add RabbitMQ Broker',
      style: {
        width: '32rem'
      },
      modal: true,
      draggable: false
    },
    onClose: (opts) => {
      if (!opts?.data) {
        return
      }

      var broker: BrokerDto = opts.data

      router.push({
        name: 'queues',
        params: {
          brokerId: broker.id
        }
      })
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
])

const brokerRoutes = computed<ResqueueRoute[]>(() => {
  let id = 0

  const routes =
    brokers.value?.map((broker) => ({
      id: ++id,
      label: broker.name ?? '',
      icon: `pi pi-${broker.system}`,
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
      life: 3000
    })
  } else {
    router.push({ name: 'app' })
  }
}
</script>

<template>
  <div class="flex h-screen gap-2 bg-gray-100 p-2" v-if="user">
    <div class="flex w-72 shrink-0 basis-72 flex-col">
      <div class="me-3 ms-2 flex flex-row items-center gap-3 border-b border-slate-200 px-2 py-3">
        <RouterLink :to="{ name: 'home' }">
          <div class="flex grow items-center justify-end rounded-lg bg-black p-2.5">
            <i class="pi pi-database rotate-90 text-white" style="font-size: 1.5rem"></i>
          </div>
        </RouterLink>
        <div class="flex grow flex-col overflow-hidden leading-5">
          <span class="font-bold" v-if="user.fullName">{{ user.fullName }}</span>
          <div v-else class="flex overflow-hidden">
            <span
              @click="openFullNameEditPage"
              class="cursor-pointer overflow-hidden overflow-ellipsis whitespace-nowrap border-dashed border-slate-500 font-bold text-blue-500 hover:border-solid hover:border-blue-500 hover:text-blue-400"
              >Set full name<i class="pi pi-pencil ms-2" style="font-size: 0.85rem"></i
            ></span>
          </div>
          <span class="overflow-hidden overflow-ellipsis whitespace-nowrap">{{ user.email }}</span>
        </div>
      </div>

      <div class="mb-1 me-3 ms-2 mt-4 flex grow flex-col gap-2">
        <SidebarRouterLink
          v-for="staticRoute in staticRoutes"
          :key="staticRoute.id"
          v-bind="staticRoute"
        />

        <div class="my-2 border-b border-slate-200"></div>

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

    <div class="flex grow flex-col overflow-hidden rounded-2xl border border-slate-200 bg-white">
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
          <slot name="append"></slot>
        </div>
      </div>
      <div class="flex grow flex-col overflow-auto">
        <slot></slot>
      </div>
    </div>
  </div>
  <template v-else> Loading...</template>
</template>
