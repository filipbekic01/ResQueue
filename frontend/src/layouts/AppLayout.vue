<script setup lang="ts">
import mtLogoUrl from '@/assets/masstransit.svg'
import { computed, ref } from 'vue'
import { useRoute } from 'vue-router'

const route = useRoute()

const home = ref({
  icon: 'pi pi-home'
})

const items = computed(() => {
  var x = []

  if (route.name === 'messages') {
    x.push({
      label: 'queues'
    })

    x.push({
      label: route.params['queueName']
    })
  } else {
    x.push({
      label: route.name
    })
  }

  return x
})
</script>

<template>
  <div class="flex px-4 pb-2 pt-4">
    <div class="flex h-14 w-14 items-center justify-center rounded-xl text-2xl text-white">
      <img :src="mtLogoUrl" class="w-full" />
    </div>

    <div class="flex flex-col justify-center ps-3">
      <div class="text-2xl font-semibold">MassTransit</div>
      <div class="flex items-center gap-2 text-slate-500">
        <Breadcrumb style="padding: 0" :home="home" :model="items" />
      </div>
    </div>

    <div class="ms-auto flex flex-col">
      <div>host_info</div>
      <div>app_version</div>
    </div>
  </div>

  <div class="flex h-screen grow flex-col overflow-hidden">
    <div class="flex grow flex-col overflow-auto">
      <slot></slot>
    </div>
  </div>
</template>
