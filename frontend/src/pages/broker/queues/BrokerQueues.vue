<script lang="ts" setup>
import { useQueuesViewQuery } from '@/api/queues/queuesViewQuery'
import { useUserSettings } from '@/composables/userSettingsComposable'
import { FilterMatchMode } from '@primevue/core/api'
import Column from 'primevue/column'
import DataTable, { type DataTableSortEvent } from 'primevue/datatable'
import IconField from 'primevue/iconfield'
import InputIcon from 'primevue/inputicon'
import InputText from 'primevue/inputtext'
import { computed, ref, watchEffect } from 'vue'
import { useRouter } from 'vue-router'

const router = useRouter()

const { settings, updateSettings } = useUserSettings()

const { data } = useQueuesViewQuery(computed(() => settings.refetchInterval))
const queuesView = computed(() => data.value ?? [])

const selectQueue = (data: any) => {
  router.push({
    name: 'messages',
    params: {
      queueName: data.queueName,
    },
  })
}

const search = ref(settings.queueSearch)

watchEffect(() => {
  updateSettings({
    ...settings,
    queueSearch: search.value,
  })
})

const filters = ref({
  queueName: { value: search, matchMode: FilterMatchMode.CONTAINS },
})

const onSort = (e: DataTableSortEvent) => {
  updateSettings({
    ...settings,
    sortOrder: e.sortOrder === null || e.sortOrder === undefined ? undefined : e.sortOrder,
    sortField: e.sortField ? e.sortField.toString() : undefined,
  })
}
</script>

<template>
  <div>
    <DataTable
      scrollable
      data-key="queueName"
      scroll-height="flex"
      :value="queuesView"
      removable-sort
      class="grow overflow-auto"
      selection-mode="single"
      striped-rows
      v-model:filters="filters"
      filter-display="menu"
      :sort-field="settings.sortField"
      :sort-order="settings.sortOrder"
      @row-select="(e) => selectQueue(e.data)"
      @sort="onSort"
    >
      <Column field="queueName" class="overflow-hidden overflow-ellipsis py-0">
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
      <Column field="queueAutoDelete" header="Auto Delete" class="w-[0] whitespace-nowrap">
        <template #body="{ data }">
          <div v-show="data['queueAutoDelete']" class="text-nowrap">
            {{ data['queueAutoDelete'] / 60 }}m
          </div>
          <div v-show="!data['queueAutoDelete']">-</div>
        </template>
      </Column>
      <Column
        field="queueMaxDeliveryCount"
        header="Max Delivery"
        class="w-0 whitespace-nowrap"
      ></Column>
      <Column
        sortable
        field="ready"
        header="Ready"
        class="w-[0] bg-surface-200/25 dark:bg-surface-800/20"
      >
      </Column>
      <Column
        sortable
        field="errored"
        header="Errored"
        class="w-[0] bg-surface-200/25 dark:bg-surface-800/20"
      ></Column>
      <Column
        sortable
        field="deadLettered"
        header="Dead Lettered"
        class="w-[0] whitespace-nowrap bg-surface-200/25 dark:bg-surface-800/20"
      ></Column>
      <Column
        sortable
        field="scheduled"
        header="Scheduled"
        class="w-[0] bg-surface-200/25 dark:bg-surface-800/20"
      ></Column>
      <Column
        sortable
        field="locked"
        header="Locked"
        class="w-[0] bg-surface-200/25 dark:bg-surface-800/20"
      ></Column>
    </DataTable>
  </div>
</template>
