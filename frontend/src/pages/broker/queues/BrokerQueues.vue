<script lang="ts" setup>
import { useBrokersQuery } from '@/api/broker/brokersQuery'
import { useSyncBrokerMutation } from '@/api/broker/syncBrokerMutation'
import { useFavoriteQueueMutation } from '@/api/queues/favoriteQueueMutation'
import { usePaginatedQueuesQuery } from '@/api/queues/paginatedQueuesQuery'
import { useIdentity } from '@/composables/identityComposable'
import { useRabbitMqQueues } from '@/composables/rabbitMqQueuesComposable'
import type { QueueDto } from '@/dtos/queueDto'
import Button from 'primevue/button'
import Column from 'primevue/column'
import DataTable, { type DataTableSortEvent } from 'primevue/datatable'
import Paginator, { type PageState } from 'primevue/paginator'
import { useConfirm } from 'primevue/useconfirm'
import { useToast } from 'primevue/usetoast'
import { computed, ref, watch } from 'vue'
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
const { mutateAsync: favoriteQueueAsync } = useFavoriteQueueMutation()
const { mutateAsync: syncBrokerAsync, isPending: isPendingSyncBroker } = useSyncBrokerMutation()

const syncBroker = () => {
  if (!user.value?.settings.showSyncConfirmDialogs) {
    syncBrokerRequest()
    return
  }

  confirm.require({
    message:
      'Do you really want to sync with remote broker? You can turn off this dialog on dashboard.',
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
  computed(() => route.query.sortOrder?.toString())
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
  router.push({
    path: route.path,
    query: {
      ...route.query,
      sortField: e.sortField?.toString(),
      sortOrder: e.sortOrder ? e.sortOrder : undefined
    }
  })
}

const toggleFavorite = (data: QueueDto) => {
  favoriteQueueAsync({
    queueId: data.id,
    dto: {
      isFavorite: !data.isFavorite
    }
  })
}
</script>

<template>
  <template v-if="isPending">
    <div class="p-5"><i class="pi pi-spinner pi-spin me-2"></i>Loading queues...</div>
  </template>
  <template v-else-if="rabbitMqQueues.length">
    <DataTable
      scrollable
      data-key="id"
      size="small"
      scroll-height="flex"
      :value="rabbitMqQueues"
      removable-sort
      class="grow overflow-auto"
      :sort-field="route.query.sortField"
      :sort-order="route.query.sortOrder ? parseInt(route.query.sortOrder.toString()) : undefined"
      @sort="updateSort"
    >
      <Column field="favorite" header="" class="w-[0%]">
        <template #body="{ data }">
          <Button text size="small" @click="toggleFavorite(data)"
            ><i
              class="pi pi-thumbtack"
              :class="[
                {
                  'rotate-12 text-slate-400': !data.isFavorite,
                  'text-slate-700': data.isFavorite
                }
              ]"
            ></i
          ></Button>
        </template>
      </Column>

      <Column field="pulled" header="Inbox" class="w-[0%]">
        <template #body="{ data }">
          {{ data.parsed['messages'] }}
        </template>
      </Column>

      <Column sortable field="name" header="Name" class="max-w-[0]">
        <template #body="{ data }">
          <div
            @click="selectQueue(data)"
            :title="data.parsed['name']"
            class="overflow-hidden overflow-ellipsis hover:cursor-pointer hover:border-blue-500 hover:text-blue-500"
            :class="[
              {
                'text-gray-500': !data.parsed['messages']
              }
            ]"
          >
            {{ data.parsed['name'] }}
          </div>
        </template>
      </Column>

      <Column
        sortable
        sort-field="messages"
        field="parsed.messages"
        header="Messages"
        class="w-[0%]"
      >
        <template #body="{ data }">
          <div
            class="flex items-center justify-start gap-3"
            v-tooltip.left="`${data.parsed['consumers']} consumers`"
          >
            <i
              class="pi pi-circle-fill text-xs"
              :class="[
                {
                  'text-emerald-400': data.parsed['consumers'],
                  'text-gray-400': !data.parsed['consumers']
                }
              ]"
              style="font-size: 0.625rem"
            ></i>
            {{ data.parsed['messages'] }}
          </div>
        </template>
      </Column>

      <Column field="parsed.type" header="Type" class="w-[0%]"> </Column>

      <Column field="parsed.features" header="Features" class="w-[0%]">
        <template #body="{ data }">
          <div v-show="data.parsed['durable']" severity="info">D</div>
        </template>
      </Column>
    </DataTable>
  </template>
  <template v-else-if="route.query.search">
    <div class="mt-24 flex grow flex-col items-center">
      <!-- <img src="/ebox.svg" class="w-72 pb-5 opacity-50" /> -->
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
      <Button
        :loading="isPendingSyncBroker"
        @click="syncBroker"
        label="Sync"
        icon="pi pi-sync"
        class="mt-3"
      ></Button>
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
