<script lang="ts" setup>
import { useQueuesViewQuery } from '@/api/queues/queuesViewQuery'
import Column from 'primevue/column'
import DataTable from 'primevue/datatable'
import { useRoute, useRouter } from 'vue-router'

const router = useRouter()
const route = useRoute()

const { data: queuesView, isPending } = useQueuesViewQuery()

const selectQueue = (data: any) => {
  router.push({
    name: 'messages',
    params: {
      queueName: data.queueName
    }
  })
}
</script>

<template>
  <template v-if="isPending">
    <div class="p-5"><i class="pi pi-spinner pi-spin me-2"></i>Loading queues...</div>
  </template>
  <template v-else-if="queuesView?.length">
    <DataTable
      scrollable
      data-key="id"
      scroll-height="flex"
      :value="queuesView"
      removable-sort
      class="grow overflow-auto"
      selection-mode="single"
      striped-rows
      @row-select="(e) => selectQueue(e.data)"
    >
      <Column sortable field="queueName" header="Name" class="overflow-hidden overflow-ellipsis"> </Column>

      <Column sortable field="queueAutoDelete" header="AutoDelete" class="w-[0]">
        <template #body="{ data }">
          <div v-show="data['queueAutoDelete']" class="text-nowrap">{{ data['queueAutoDelete'] / 60 }}m</div>
          <div v-show="!data['queueAutoDelete']">-</div>
        </template>
      </Column>
      <Column sortable field="ready" header="Ready" class="w-[0]"></Column>
      <Column sortable field="scheduled" header="Scheduled" class="w-[0]"></Column>
      <Column sortable field="errored" header="Errored" class="w-[0]"></Column>
      <Column sortable field="deadLettered" header="DeadLettered" class="w-[0]"></Column>
      <Column sortable field="locked" header="Locked" class="w-[0]"></Column>
      <Column sortable field="consumeCount" header="ConsumeCount" class="w-[0]"></Column>
      <Column sortable field="errorCount" header="ErrorCount" class="w-[0]"></Column>
      <Column sortable field="deadLetterCount" header="DeadLetterCount" class="w-[0]"></Column>
      <Column sortable field="countDuration" header="CountDuration" class="w-[0]"></Column>
    </DataTable>
  </template>
  <template v-else-if="route.query.search">
    <div class="mt-24 flex grow flex-col items-center">
      <i class="pi pi-filter-slash pb-6 opacity-25" style="font-size: 2rem"></i>
      <div class="text-lg">No Results</div>
      <div class="">No queues found for given filters</div>
    </div>
  </template>
</template>
