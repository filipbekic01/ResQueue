<script lang="ts" setup>
import { useBrokersQuery } from '@/api/brokers/brokersQuery'
import { useUpdateBrokerMutation } from '@/api/brokers/updateBrokerMutation'
import { useQueuesViewQuery } from '@/api/queues/queuesViewQuery'
import { useIdentity } from '@/composables/identityComposable'
import { errorToToast } from '@/utils/errorUtils'
import Column from 'primevue/column'
import DataTable, { type DataTableSortEvent } from 'primevue/datatable'
import { useConfirm } from 'primevue/useconfirm'
import { useToast } from 'primevue/usetoast'
import { computed, ref, watchEffect } from 'vue'
import { useRoute, useRouter } from 'vue-router'

const router = useRouter()
const route = useRoute()
const toast = useToast()

const {
  query: { data: user }
} = useIdentity()

const { data: brokers } = useBrokersQuery()
const broker = computed(() => brokers.value?.find((x) => x.id === '9193dfce-5676-4b36-b7d3-7909a40696da'))
const access = computed(() => broker.value?.accessList.find((x) => x.userId == user.value?.id))

const { mutateAsync: updateBrokerAsync } = useUpdateBrokerMutation()

const pageIndex = ref(0)

const { data: queuesView, isPending } = useQueuesViewQuery(computed(() => '9193dfce-5676-4b36-b7d3-7909a40696da'))

const selectQueue = (data: any) => {
  router.push({
    name: 'messages',
    params: {
      brokerId: broker.value?.id,
      queueName: data.queueName
    }
  })
}

const updateSort = (e: DataTableSortEvent) => {
  if (broker.value && access.value) {
    updateBrokerAsync({
      broker: {
        ...broker.value,
        rabbitMQConnection: broker.value.rabbitMQConnection
          ? { ...broker.value.rabbitMQConnection, username: '', password: '' }
          : undefined,
        settings: {
          ...access.value?.settings,
          defaultQueueSortOrder: e.sortOrder ? parseInt(e.sortOrder.toString()) : -1,
          defaultQueueSortField: e.sortField ? e.sortField.toString() : undefined
        }
      },
      brokerId: broker.value.id
    }).catch((e) => toast.add(errorToToast(e)))
  }

  router.push({
    path: route.path,
    query: {
      ...route.query,
      sortField: e.sortField?.toString(),
      sortOrder: e.sortOrder ? e.sortOrder : undefined
    }
  })
}

watchEffect(() => {
  if (!broker.value || !access.value) {
    return
  }

  if (route.query.filtersReady) {
    return
  }

  let filters = {}

  if (
    !route.query.sortField ||
    (!route.query.sortOrder &&
      access.value.settings.defaultQueueSortField &&
      access.value.settings.defaultQueueSortOrder)
  ) {
    filters = {
      ...filters,
      sortField: access.value.settings.defaultQueueSortField,
      sortOrder: access.value.settings.defaultQueueSortOrder
    }
  }

  if (!route.query.search && access.value.settings.defaultQueueSearch) {
    filters = {
      ...filters,
      search: access.value.settings.defaultQueueSearch
    }
  }

  router.push({
    path: route.path,
    query: {
      ...route.query,
      ...filters,
      filtersReady: '1'
    }
  })
})
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
