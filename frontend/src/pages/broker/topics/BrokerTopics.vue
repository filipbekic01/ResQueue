<script lang="ts" setup>
import { useSubscriptionsQuery } from '@/api/subscriptions/subscriptionsQuery'
import { useUserSettings } from '@/composables/userSettingsComposable'
import { FilterMatchMode } from '@primevue/core/api'
import { computed, ref } from 'vue'

const { settings } = useUserSettings()

const { data: subscriptions } = useSubscriptionsQuery(computed(() => settings.refetchInterval))

const filters = ref({
  topicName: { value: null, matchMode: FilterMatchMode.CONTAINS },
})
</script>

<template>
  <div>
    <!-- 
        :sort-field="settings.sortField"
        :sort-order="settings.sortOrder" 
        @row-select="(e) => selectQueue(e.data)"
        @sort="onSort"
        selection-mode="single"
      -->
    <DataTable
      scrollable
      data-key="topicName"
      scroll-height="flex"
      :value="subscriptions"
      removable-sort
      class="grow overflow-auto"
      striped-rows
      v-model:filters="filters"
      filter-display="menu"
    >
      <Column
        sortable
        field="topicName"
        header="Topic Name"
        class="overflow-hidden overflow-ellipsis"
      >
        <template #filter="{ filterModel, filterCallback }">
          <InputText
            v-model="filterModel.value"
            type="text"
            @input="filterCallback()"
            placeholder="Search by topic name"
          />
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
