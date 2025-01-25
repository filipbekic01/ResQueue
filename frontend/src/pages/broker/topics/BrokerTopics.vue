<script lang="ts" setup>
import { useSubscriptionsQuery } from '@/api/subscriptions/subscriptionsQuery'
import { useUserSettings } from '@/composables/userSettingsComposable'
import { FilterMatchMode } from '@primevue/core/api'
import { computed, ref, watchEffect } from 'vue'

const { settings, updateSettings } = useUserSettings()

const { data: subscriptions } = useSubscriptionsQuery(computed(() => settings.refetchInterval))

const search = ref(settings.topicSearch)

watchEffect(() => {
  updateSettings({
    ...settings,
    topicSearch: search.value,
  })
})

const filters = ref({
  topicName: { value: search, matchMode: FilterMatchMode.CONTAINS },
})
</script>

<template>
  <div class="flex overflow-auto">
    <DataTable
      scrollable
      scroll-height="flex"
      :value="subscriptions"
      removable-sort
      class="grow overflow-auto"
      striped-rows
      v-model:filters="filters"
      filter-display="menu"
    >
      <Column field="topicName" class="overflow-hidden overflow-ellipsis py-0">
        <template #header>
          <div class="flex w-full items-center">
            <b>Name</b>
            <IconField class="ms-1 grow">
              <InputIcon class="pi pi-search" v-if="!search" />
              <InputIcon class="pi pi-times cursor-pointer" v-else @click="search = ''" />
              <InputText
                placeholder="Search anything..."
                class="w-full border-0 shadow-none dark:bg-surface-900"
                v-model="search"
                ref="searchInputText"
              />
            </IconField>
          </div>
        </template>
      </Column>
      <Column sortable field="routingKey" header="Routing Key" class="w-[0] whitespace-nowrap">
      </Column>
      <Column
        sortable
        field="destinationName"
        header="Destination Name"
        class="w-[0] whitespace-nowrap"
      >
      </Column>
      <Column
        sortable
        field="destinationType"
        header="Destination Type"
        class="w-[0] whitespace-nowrap"
      >
      </Column>
      <Column
        sortable
        field="subscriptionType"
        header="Subscription Type"
        class="w-[0] whitespace-nowrap"
      >
      </Column>
    </DataTable>
  </div>
</template>
