<script lang="ts" setup>
import { useIdentity } from '@/composables/identityComposable'
import { computed } from 'vue'
import { useRoute, type RouteLocationAsRelativeGeneric } from 'vue-router'

export interface ResqueueRoute {
  id: number
  label: string
  icon: string
  to: RouteLocationAsRelativeGeneric
}

const props = defineProps<{
  id: number
  label: string
  icon: string
  to: RouteLocationAsRelativeGeneric
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
    class="w-full flex items-center py-2.5 px-4 text-slate-700 rounded-lg hover:bg-white"
    :class="[
      {
        'bg-white shadow font-semibold': isRoute(to),
        'font-medium': !isRoute(to)
      }
    ]"
  >
    <i
      class="mr-3 text-slate-600"
      style="font-size: 1.125rem"
      :class="[
        icon,
        {
          'text-slate-900': isRoute(to)
        }
      ]"
    ></i>
    <span>{{ label }}</span>
    <i v-if="showWarning" class="ms-auto text-orange-400 pi pi-exclamation-circle"></i>
  </RouterLink>
</template>
