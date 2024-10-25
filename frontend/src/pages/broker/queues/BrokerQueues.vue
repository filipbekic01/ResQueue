<script lang="ts" setup>
import { useQueuesViewQuery } from '@/api/queues/queuesViewQuery'
import { FilterMatchMode } from '@primevue/core/api'
import Column from 'primevue/column'
import DataTable from 'primevue/datatable'
import { computed, ref } from 'vue'
import { useRouter } from 'vue-router'

const router = useRouter()

const { data } = useQueuesViewQuery()
const queuesView = computed(() => data.value ?? [])

const selectQueue = (data: any) => {
  router.push({
    name: 'messages',
    params: {
      queueName: data.queueName
    }
  })
}

const filters = ref({
  queueName: { value: null, matchMode: FilterMatchMode.CONTAINS }
})
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
      @row-select="(e) => selectQueue(e.data)"
    >
      <Column sortable field="queueName" header="Name" class="overflow-hidden overflow-ellipsis">
        <template #body="{ data }">
          {{ data.queueName }}
        </template>
        <template #filter="{ filterModel, filterCallback }">
          <InputText
            v-model="filterModel.value"
            type="text"
            @input="filterCallback()"
            placeholder="Search by queue name"
          />
        </template>
      </Column>
      <Column sortable field="ready" header="Ready" class="w-[0]"> </Column>
      <Column sortable field="errored" header="Errored" class="w-[0]"></Column>
      <Column sortable field="deadLettered" header="Dead Lettered" class="w-[0] whitespace-nowrap"></Column>
      <Column sortable field="scheduled" header="Scheduled" class="w-[0]"></Column>
      <Column sortable field="locked" header="Locked" class="w-[0]"></Column>
      <Column sortable field="queueAutoDelete" header="Auto Delete" class="w-[0] whitespace-nowrap">
        <template #body="{ data }">
          <div v-show="data['queueAutoDelete']" class="text-nowrap">{{ data['queueAutoDelete'] / 60 }}m</div>
          <div v-show="!data['queueAutoDelete']">-</div>
        </template>
      </Column>
    </DataTable>
  </div>
</template>
