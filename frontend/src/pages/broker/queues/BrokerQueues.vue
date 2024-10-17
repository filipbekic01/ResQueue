<script lang="ts" setup>
import { useBrokersQuery } from '@/api/brokers/brokersQuery'
import { useSyncBrokerMutation } from '@/api/brokers/syncBrokerMutation'
import { useUpdateBrokerMutation } from '@/api/brokers/updateBrokerMutation'
import { useQueuesQuery } from '@/api/queues/queuesQuery'
import { useIdentity } from '@/composables/identityComposable'
import { errorToToast } from '@/utils/errorUtils'
import Button from 'primevue/button'
import Column from 'primevue/column'
import DataTable, { type DataTableSortEvent } from 'primevue/datatable'
import { type PageState } from 'primevue/paginator'
import { useConfirm } from 'primevue/useconfirm'
import { useToast } from 'primevue/usetoast'
import { computed, ref, watchEffect } from 'vue'
import { useRoute, useRouter } from 'vue-router'

const props = defineProps<{
  brokerId: string
}>()

const router = useRouter()
const route = useRoute()
const toast = useToast()
const confirm = useConfirm()

const {
  query: { data: user }
} = useIdentity()

const { data: brokers } = useBrokersQuery()
const broker = computed(() => brokers.value?.find((x) => x.id === props.brokerId))
const access = computed(() => broker.value?.accessList.find((x) => x.userId == user.value?.id))
const { mutateAsync: syncBrokerAsync, isPending: isPendingSyncBroker } = useSyncBrokerMutation()

const { mutateAsync: updateBrokerAsync } = useUpdateBrokerMutation()

const syncBroker = () => {
  if (!user.value?.settings.showSyncConfirmDialogs) {
    syncBrokerRequest()
    return
  }

  confirm.require({
    message: 'Do you really want to sync with remote broker? You can turn off this dialog on dashboard.',
    icon: 'pi pi-info-circle',
    header: 'Sync Broker',
    rejectProps: {
      label: 'Cancel',
      severity: 'secondary',
      outlined: true
    },
    acceptProps: {
      label: 'Sync Broker',
      severity: ''
    },
    accept: () => syncBrokerRequest(),
    reject: () => {}
  })
}

const syncBrokerRequest = () => {
  if (!broker.value) {
    return
  }

  syncBrokerAsync(broker.value?.id).then(() => {
    toast.add({
      severity: 'success',
      summary: 'Sync Completed!',
      detail: `Broker ${broker.value?.name} synced!`,
      life: 3000
    })
  })
}

const pageIndex = ref(0)
const changePage = (e: PageState) => {
  pageIndex.value = e.page
}

const { data: paginatedQueues, isPending } = useQueuesQuery(
  computed(() => props.brokerId),
  pageIndex,
  computed(() => route.query.search?.toString()),
  computed(() => route.query.sortField?.toString()),
  computed(() => route.query.sortOrder?.toString()),
  computed(() => !!route.query.filtersReady)
)

// const { rabbitMqQueues } = useRabbitMqQueues(computed(() => paginatedQueues.value?.items))

const selectQueue = (data: any) => {
  router.push({
    name: 'messages',
    params: {
      brokerId: broker.value?.id,
      queueId: data.queueName
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
  <template v-else-if="paginatedQueues?.length">
    <DataTable
      scrollable
      data-key="id"
      scroll-height="flex"
      :value="paginatedQueues"
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
  <template v-else>
    <div class="mt-24 flex grow flex-col items-center">
      <i class="pi pi-arrow-right-arrow-left pb-6 opacity-25" style="font-size: 2rem"></i>
      <div class="text-lg">No Queues</div>
      <div class="">Make sure you sync the broker</div>
      <Button :loading="isPendingSyncBroker" @click="syncBroker" label="Sync" icon="pi pi-sync" class="mt-3"></Button>
    </div>
  </template>
</template>
