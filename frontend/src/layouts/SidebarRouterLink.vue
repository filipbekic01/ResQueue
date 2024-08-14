<script lang="ts" setup>
import { useRoute, type RouteLocationAsRelativeGeneric } from 'vue-router'

export interface ResqueueRoute {
  id: number
  label: string
  icon: string
  to: RouteLocationAsRelativeGeneric
}

defineProps<{
  id: number
  label: string
  icon: string
  to: RouteLocationAsRelativeGeneric
}>()

const route = useRoute()

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
</script>

<template>
  <RouterLink
    :key="id"
    :to="to"
    class="w-full flex items-center py-2 px-4 text-gray-700 rounded-lg hover:bg-white"
    :class="[
      {
        'bg-white shadow font-semibold': isRoute(to),
        'font-medium': !isRoute(to)
      }
    ]"
  >
    <i class="mr-3" :class="icon"></i>
    <span>{{ label }}</span>
  </RouterLink>
</template>
