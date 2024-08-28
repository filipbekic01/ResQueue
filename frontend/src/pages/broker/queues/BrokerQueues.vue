<script lang="ts" setup>
import { useBrokersQuery } from '@/api/broker/brokersQuery'
import { useFavoriteQueueMutation } from '@/api/queues/favoriteQueueMutation'
import { usePaginatedQueuesQuery } from '@/api/queues/paginatedQueuesQuery'
import { useRabbitMqQueues } from '@/composables/rabbitMqQueuesComposable'
import type { QueueDto } from '@/dtos/queueDto'
import Button from 'primevue/button'
import Column from 'primevue/column'
import DataTable, { type DataTableSortEvent } from 'primevue/datatable'
import Paginator, { type PageState } from 'primevue/paginator'
import { computed, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'

const props = defineProps<{
  brokerId: string
}>()

const router = useRouter()
const route = useRoute()

const { data: brokers } = useBrokersQuery()
const broker = computed(() => brokers.value?.find((x) => x.id === props.brokerId))
const { mutateAsync: favoriteQueueAsync } = useFavoriteQueueMutation()

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
      scroll-height="flex"
      :value="rabbitMqQueues"
      removable-sort
      class="grow overflow-auto"
      :sort-field="route.query.sortField"
      :sort-order="route.query.sortOrder ? parseInt(route.query.sortOrder.toString()) : undefined"
      @sort="updateSort"
    >
      <Column field="favorite" header="" class="w-[0%] overflow-hidden overflow-ellipsis">
        <template #body="{ data }">
          <i
            class="pi pi-thumbtack cursor-pointer"
            :class="[
              {
                'text-slate-300': !data.isFavorite,
                'text-slate-700': data.isFavorite
              }
            ]"
            @click="toggleFavorite(data)"
          ></i>
        </template>
      </Column>

      <Column sortable field="name" header="Name" class="w-[60%] overflow-hidden overflow-ellipsis">
        <template #body="{ data }">
          <span
            @click="selectQueue(data)"
            class="hover:cursor-pointer hover:border-blue-500 hover:text-blue-500"
            >{{ data.parsed['name'] }}</span
          >
        </template>
      </Column>

      <Column field="parsed.consumers" header="Cons" class="w-[0%]">
        <template #body="{ data }">
          <div class="text-end">
            {{ data.parsed['consumers'] }}
          </div>
        </template>
      </Column>

      <Column field="pulled" header="Pld" class="w-[0%]">
        <template #body="{ data }">
          <div class="flex items-center gap-1">
            <i class="pi pi-inbox text-xs text-slate-500"></i>{{ data.parsed['messages'] }}
          </div>
        </template>
      </Column>

      <Column sortable sort-field="messages" field="parsed.messages" header="Msgs" class="w-[0%]">
        <template #body="{ data }">
          <div class="flex items-center gap-1">
            <i class="pi pi-arrow-down text-xs text-emerald-500"></i>{{ data.parsed['messages'] }}
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
  <template v-else>
    <div class="mt-24 flex grow flex-col items-center">
      <img src="/ebox.svg" class="w-72 pb-5 opacity-50" />
      <div class="text-lg">No Queues</div>
      <div class="">Make sure you sync the broker.</div>
      <Button label="Sync" icon="pi pi-sync" class="mt-3"></Button>
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
