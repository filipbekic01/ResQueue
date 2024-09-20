<script lang="ts" setup>
import { useIdentity } from '@/composables/identityComposable'
import { computed } from 'vue'
import { useRoute, type RouteLocationAsRelativeGeneric } from 'vue-router'

export interface ResqueueRoute {
  id: number
  label: string
  icon: string
  to: RouteLocationAsRelativeGeneric
  shared: boolean
}

const props = defineProps<{
  id: number
  label: string
  icon: string
  to: RouteLocationAsRelativeGeneric
  shared: boolean
}>()

const route = useRoute()
const {
  activeSubscription,
  query: { data: user }
} = useIdentity()

const isRoute = (to: RouteLocationAsRelativeGeneric) => {
  if (route.path?.toString().startsWith('/app/broker')) {
    if (route.params?.brokerId === to.params?.brokerId) {
      return true
    } else {
      return false
    }
  }

  return route.name === to.name
}

const showWarning = computed(() => {
  if (props.label === 'Control Panel') {
    if (!user.value?.emailConfirmed) {
      return true
    }

    if (!activeSubscription) {
      return true
    }
  }

  return false
})
</script>

<template>
  <RouterLink
    :key="id"
    :to="to"
    class="flex w-full items-center rounded-lg px-4 py-2.5 text-slate-700 hover:bg-white"
    :class="[
      {
        'bg-white font-semibold shadow': isRoute(to),
        'font-medium': !isRoute(to)
      }
    ]"
  >
    <img
      v-if="icon === 'pi pi-rabbitmq'"
      src="/rmq.svg"
      style="width: 1.125rem"
      :class="[
        'mr-3 rounded p-1',
        {
          'bg-slate-900': isRoute(to),
          'bg-slate-600': !isRoute(to)
        }
      ]"
    />
    <i
      class="mr-3 text-slate-600"
      style="font-size: 1.125rem"
      v-else
      :class="[
        icon,
        {
          'text-slate-900': isRoute(to)
        }
      ]"
    ></i>
    <span>{{ label }}</span>
    <i v-if="showWarning" class="pi pi-exclamation-circle ms-auto text-orange-400"></i>
    <i v-if="shared" class="pi pi-users ms-auto"></i>
    <!-- <span v-if="shared" class="ms-auto text-xs uppercase">owner</span> -->
  </RouterLink>
</template>
