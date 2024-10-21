<script setup lang="ts">
import mtLogoUrl from '@/assets/masstransit.svg'
import type { MenuItem } from 'primevue/menuitem'
import { computed, ref } from 'vue'
import { useRoute } from 'vue-router'

const route = useRoute()

const home = ref({
  icon: 'pi pi-home'
})

const capitalize = (value: string = '') => value.replace(/\b\w/g, (char) => char.toUpperCase())

const items = computed((): MenuItem[] => {
  var x: MenuItem[] = []

  if (route.name === 'messages') {
    x.push({
      label: 'Queues'
    })

    x.push({
      label: capitalize(route.params['queueName']?.toString())
    })
  } else {
    x.push({
      label: capitalize(route.name?.toString())
    })
  }

  return x
})
</script>

<template>
  <div class="flex h-screen flex-col dark:bg-zinc-950">
    <div class="flex items-center px-4 pb-2 pt-4">
      <div class="flex">
        <div class="flex h-14 w-14 items-center justify-center rounded-xl text-2xl text-white">
          <img :src="mtLogoUrl" class="w-full" />
        </div>

        <div class="flex flex-col justify-center ps-3">
          <div class="text-2xl font-semibold">MassTransit</div>
          <div class="flex items-center gap-2 text-slate-500">
            <Breadcrumb style="padding: 0" :home="home" :model="items" />
          </div>
        </div>
      </div>
    </div>

    <div class="flex grow flex-col overflow-auto">
      <slot></slot>
    </div>
  </div>
</template>
