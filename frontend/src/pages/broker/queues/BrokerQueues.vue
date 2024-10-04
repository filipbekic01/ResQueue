<script lang="ts" setup>
import { useBrokersQuery } from '@/api/brokers/brokersQuery'
import { useSyncBrokerMutation } from '@/api/brokers/syncBrokerMutation'
import { useUpdateBrokerMutation } from '@/api/brokers/updateBrokerMutation'
import { usePaginatedQueuesQuery } from '@/api/queues/paginatedQueuesQuery'
import { useIdentity } from '@/composables/identityComposable'
import { useRabbitMqQueues } from '@/composables/rabbitMqQueuesComposable'
import { errorToToast } from '@/utils/errorUtils'
import Button from 'primevue/button'
import Column from 'primevue/column'
import DataTable, { type DataTableSortEvent } from 'primevue/datatable'
import Paginator, { type PageState } from 'primevue/paginator'
import { useConfirm } from 'primevue/useconfirm'
import { useToast } from 'primevue/usetoast'
import { computed, ref, watch, watchEffect } from 'vue'
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

const { data: paginatedQueues, isPending } = usePaginatedQueuesQuery(
  computed(() => props.brokerId),
  pageIndex,
  computed(() => route.query.search?.toString()),
  computed(() => route.query.sortField?.toString()),
  computed(() => route.query.sortOrder?.toString()),
  computed(() => !!route.query.filtersReady)
)

const totalCountFrozen = ref(0)
watch(
  () => paginatedQueues.value,
  (v) => {
    if (v) {
      totalCountFrozen.value = v?.totalCount
    }
  },
  {
    immediate: true
  }
)

const { rabbitMqQueues } = useRabbitMqQueues(computed(() => paginatedQueues.value?.items))

const selectQueue = (data: any) => {
  router.push({
    name: 'messages',
    params: {
      brokerId: broker.value?.id,
      queueId: data.id
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

const getName = (name: string) => {
  if (access.value?.settings.queueTrimPrefix && name.startsWith(access.value?.settings.queueTrimPrefix)) {
    return name.slice(access.value?.settings.queueTrimPrefix.length)
  }

  return name
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
  <template v-else-if="rabbitMqQueues.length">
    <DataTable
      scrollable
      data-key="id"
      scroll-height="flex"
      :value="rabbitMqQueues"
      removable-sort
      class="grow overflow-auto"
      striped-rows
      :sort-field="route.query.sortField"
      :sort-order="route.query.sortOrder ? parseInt(route.query.sortOrder.toString()) : undefined"
      @sort="updateSort"
      :lazy="true"
    >
      <Column sortable field="parsed.name" header="Name" class="overflow-hidden overflow-ellipsis">
        <template #body="{ data }">
          <div
            @click="selectQueue(data)"
            class="w-0 overflow-ellipsis hover:cursor-pointer hover:border-blue-500 hover:text-blue-500"
          >
            {{ getName(data.parsed['name']) }}
          </div>
        </template>
      </Column>

      <Column sortable sort-field="totalMessages" field="parsed.messages" header="Messages" class="w-0">
        <template #body="{ data }">
          <div
            class="flex flex-nowrap items-center justify-start gap-3"
            v-tooltip.left="`${data.parsed['consumers']} consumers`"
          >
            {{ data.parsed['messages'] }}
            <div class="ms-auto flex items-center whitespace-nowrap">
              ({{ data.messages }} <i class="pi pi-inbox ms-2"></i>)
            </div>
          </div>
        </template>
      </Column>

      <Column field="parsed.type" header="Type" class="w-0">
        <template #body="{ data }">
          <Tag>{{ data.parsed['type'] }}</Tag>
        </template>
      </Column>

      <Column field="parsed.features" header="Features" class="w-0">
        <template #body="{ data }">
          <div class="flex justify-center">
            <Tag v-show="data.parsed['durable']" v-tooltip.left="'Durable'">D</Tag>
          </div>
        </template>
      </Column>
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

  <Paginator
    class="mt-auto"
    @page="changePage"
    :rows="50"
    :always-show="false"
    :total-records="totalCountFrozen"
  ></Paginator>
</template>
