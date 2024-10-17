<script lang="ts" setup>
import pgLogoUrl from '@/assets/postgres.svg'
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
  collapsed: boolean
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
    class="flex w-full items-center rounded-lg py-2.5 text-slate-700 hover:bg-white"
    :class="[
      {
        'px-4': !collapsed,
        'justify-center': collapsed,
        'bg-white font-semibold shadow': isRoute(to),
        'font-medium': !isRoute(to)
      }
    ]"
  >
    <img
      v-if="icon === 'pi pi-postgres'"
      :src="pgLogoUrl"
      style="width: 1.2rem"
      :class="[
        'rounded p-1',
        {
          'mr-3': !collapsed,
          'bg-slate-200': true
        }
      ]"
    />
    <i
      class="text-slate-600"
      style="font-size: 1.2rem"
      v-else
      :class="[
        icon,
        {
          'mr-3': !collapsed,
          'text-slate-900': isRoute(to)
        }
      ]"
    ></i>
    <template v-if="!collapsed">
      <span>{{ label }}</span>
      <i v-if="showWarning" class="pi pi-exclamation-circle ms-auto text-orange-400"></i>
      <i v-if="shared" class="pi pi-users ms-auto"></i>
    </template>
  </RouterLink>
</template>
