<script setup lang="ts">
import { useUpdateUserMutation } from '@/api/auth/updateUserMutation'
import { useBrokersQuery } from '@/api/brokers/brokersQuery'
import { useIdentity } from '@/composables/identityComposable'
import CreateBrokerDialog from '@/dialogs/CreateBrokerDialog.vue'
import type { BrokerDto } from '@/dtos/broker/brokerDto'
import { useHead } from '@unhead/vue'
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

const { mutateAsync: updateUserAsync } = useUpdateUserMutation()

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
    },
    shared: false
  }
])

const brokerRoutes = computed<ResqueueRoute[]>(() => {
  let id = 0

  const routes =
    brokers.value?.map((broker) => ({
      id: ++id,
      label: broker.name ?? '',
      icon: `pi pi-${broker.system}`,
      shared: broker.createdByUserId !== user.value?.id,
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

useHead({
  title: 'ResQueue',
  meta: [
    { name: 'robots', content: 'noindex, nofollow' } // Prevents the page from being indexed
  ]
})

const toggleCollapse = () => {
  if (!user.value) {
    return
  }

  updateUserAsync({
    fullName: user.value.fullName,
    settings: {
      ...user.value.settings,
      collapseSidebar: !user.value.settings.collapseSidebar
    }
  })
}
</script>

<template>
  <div class="flex h-screen gap-2 bg-gray-100 p-2" v-if="user">
    <div
      class="flex shrink-0 flex-col"
      :class="[
        {
          'w-72 basis-72': !user.settings.collapseSidebar,
          '': user.settings.collapseSidebar
        }
      ]"
    >
      <div
        class="flex flex-row items-center gap-3 border-b border-slate-200 px-2 py-3"
        :class="[
          {
            'me-3 ms-2': !user.settings.collapseSidebar
          }
        ]"
      >
        <RouterLink :to="{ name: 'home' }">
          <div class="flex grow items-center justify-end rounded-lg bg-black p-2.5">
            <i class="pi pi-database rotate-90 text-white" style="font-size: 1.5rem"></i>
          </div>
        </RouterLink>
        <div
          v-if="!user.settings.collapseSidebar"
          class="flex grow flex-col overflow-hidden leading-5"
        >
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
      <div
        class="mb-1 mt-4 flex grow flex-col gap-2"
        :class="[
          {
            'me-3 ms-2': !user.settings.collapseSidebar,
            'mx-2.5': user.settings.collapseSidebar
          }
        ]"
      >
        <SidebarRouterLink
          v-for="staticRoute in staticRoutes"
          :key="staticRoute.id"
          v-bind="staticRoute"
          :shared="staticRoute.shared"
          :collapsed="user.settings.collapseSidebar"
        />

        <div class="my-2 border-b border-slate-200"></div>

        <SidebarRouterLink
          v-for="brokerRoute in brokerRoutes"
          :key="brokerRoute.id"
          v-bind="brokerRoute"
          :shared="brokerRoute.shared"
          :collapsed="user.settings.collapseSidebar"
        />

        <div
          class="flex gap-3"
          :class="[
            {
              'flex-col-reverse': user.settings.collapseSidebar,
              'flex-row': !user.settings.collapseSidebar,
              'mt-auto': brokers?.length
            }
          ]"
        >
          <Button
            @click="openCreateBrokerDialog()"
            icon="pi pi-plus"
            class="grow"
            :label="`${!user.settings.collapseSidebar ? 'Add Broker' : ''}`"
          ></Button>
          <Button
            outlined
            @click="toggleCollapse"
            :icon="`pi pi-angle-double-${user.settings.collapseSidebar ? 'right' : 'left'}`"
          ></Button>
        </div>
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
  <template v-else>
    <!-- loading... -->
  </template>
</template>
