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
      striped-rows
      :sort-field="route.query.sortField"
      :sort-order="route.query.sortOrder ? parseInt(route.query.sortOrder.toString()) : undefined"
      @sort="updateSort"
    >
      <Column sortable field="queue_name" header="Name" class="overflow-hidden overflow-ellipsis">
        <template #body="{ data }">
          <div
            @click="selectQueue(data)"
            class="w-0 overflow-ellipsis whitespace-nowrap hover:cursor-pointer hover:border-blue-500 hover:text-blue-500"
          >
            {{ data['queueName'] }}
          </div>
        </template>
      </Column>

      <Column sortable field="queueAutoDelete" header="AutoDelete" class="w-[0]"></Column>
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
