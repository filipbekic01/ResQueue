<script setup lang="ts">
import { useAuthQuery } from '@/api/auth/authQuery'
import mtLogoUrlDark from '@/assets/images/masstransit-dark.svg'
import mtLogoUrl from '@/assets/images/masstransit.svg'
import { useUserSettings } from '@/composables/userSettingsComposable'
import Listbox from 'primevue/listbox'
import type { MenuItem } from 'primevue/menuitem'
import { computed, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import Graph from './Graph.vue'

const route = useRoute()
const router = useRouter()

const { isSuccess, isPending, error } = useAuthQuery()

const capitalize = (value: string = '') => value.replace(/\b\w/g, (char) => char.toUpperCase())

const { settings, updateSettings, toggleDarkMode, toggleGraph } = useUserSettings()

const autoRefreshPopover = ref()
const refetchIntervalOptions = [
  {
    label: 'Never',
    value: 0,
  },
  {
    label: '1s',
    value: 1000,
  },
  {
    label: '5s',
    value: 1000 * 5,
  },
  {
    label: '30s',
    value: 1000 * 30,
  },
  {
    label: '1m',
    value: 1000 * 60,
  },
  {
    label: '5m',
    value: 1000 * 60 * 5,
  },
  {
    label: '30m',
    value: 1000 * 60 * 30,
  },
  {
    label: '1h',
    value: 1000 * 60 * 60,
  },
]

const onRefreshIntervalChange = (interval: number) => {
  updateSettings({ ...settings, refetchInterval: interval })
  autoRefreshPopover.value.hide()
}

const items = computed((): MenuItem[] => {
  const items: MenuItem[] = []

  if (route.name === 'messages') {
    items.push({
      label: 'Queues',
      command: () => {
        router.push({ name: 'queues' })
      },
    })

    items.push({
      label: capitalize(route.params['queueName']?.toString()),
    })
  } else {
    items.push({
      label: capitalize(route.name?.toString()),
    })
  }

  return items
})

const autoRefreshLabel = computed(() => {
  return `${refetchIntervalOptions.find((x) => x.value === settings.refetchInterval)?.label}`
})
</script>

<template>
  <div v-if="!isPending && isSuccess" class="flex h-screen w-full flex-col">
    <Popover ref="autoRefreshPopover">
      <div class="flex w-72 flex-col gap-2">
        <div>
          Select an interval to automatically refresh the queues and messages view. We plan to
          integrate a real-time, socket-based system for instant updates in a future release.
        </div>
        <Listbox
          :options="refetchIntervalOptions"
          :model-value="settings.refetchInterval"
          @update:model-value="onRefreshIntervalChange"
          option-value="value"
          option-label="label"
        ></Listbox>
      </div>
    </Popover>

    <div class="flex">
      <div class="flex grow flex-col">
        <div class="flex items-center border-b px-4 pb-4 pt-4 dark:border-b-surface-700">
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

          <div class="my-auto me-3 ms-auto items-center">
            <Button @click="toggleDarkMode" icon="pi pi-palette" text class="ms-1"></Button>
            <Button
              @click="(e) => autoRefreshPopover.toggle(e)"
              :label="autoRefreshLabel"
              icon="pi pi-hourglass "
              text
            ></Button>
            <Button
              @click="toggleGraph"
              :icon="`pi pi-angle-${settings.showGraph ? 'right' : 'left'}`"
              text
              class="ms-1"
            ></Button>
          </div>
        </div>
        <slot name="menu"></slot>
      </div>
      <div class="flex">
        <slot name="right"></slot>
      </div>
    </div>

    <div class="flex grow flex-col overflow-auto">
      <slot></slot>
    </div>
  </div>
  <div v-else-if="!isPending && !isSuccess">{{ error?.message }}</div>
</template>
