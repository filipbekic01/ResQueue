<script setup lang="ts">
import { useAuthQuery } from '@/api/auth/authQuery'
import mtLogoUrlDark from '@/assets/images/masstransit-dark.svg'
import mtLogoUrl from '@/assets/images/masstransit.svg'
import type { MenuItem } from 'primevue/menuitem'
import { computed, ref } from 'vue'
import { useRoute } from 'vue-router'

const route = useRoute()

const { isSuccess, isPending, error } = useAuthQuery()

const capitalize = (value: string = '') => value.replace(/\b\w/g, (char) => char.toUpperCase())

const items = computed((): MenuItem[] => {
  var items: MenuItem[] = []

  if (route.name === 'messages') {
    items.push({
      label: 'Queues'
    })

    items.push({
      label: capitalize(route.params['queueName']?.toString())
    })
  } else {
    items.push({
      label: capitalize(route.name?.toString())
    })
  }

  return items
})
</script>

<template>
  <div v-if="!isPending && isSuccess" class="flex h-screen flex-col">
    <div class="flex items-center px-4 pb-4 pt-4 shadow">
      <div class="flex">
        <div class="flex h-14 w-14 items-center justify-center rounded-xl text-2xl">
          <img :src="mtLogoUrl" class="w-full dark:hidden" />
          <img :src="mtLogoUrlDark" class="hidden w-full dark:block" />
        </div>

        <div class="ms-4 flex flex-col justify-center">
          <div class="text-2xl font-semibold text-primary">MassTransit</div>
          <div class="flex items-center gap-2">
            <Breadcrumb style="padding: 0" :model="items" />
          </div>
        </div>
      </div>
    </div>

    <div class="flex grow flex-col overflow-auto">
      <slot></slot>
    </div>
  </div>
  <div v-else-if="!isPending && !isSuccess">{{ error?.message }}</div>
</template>
