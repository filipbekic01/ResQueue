<script lang="ts" setup>
import { useBrokersQuery } from '@/api/broker/brokersQuery'
import Column from 'primevue/column'
import DataTable from 'primevue/datatable'
import { computed, ref } from 'vue'
import { useRouter } from 'vue-router'
import { useQueues } from '@/composables/queuesComposable'
import Paginator, { type PageState } from 'primevue/paginator'

const emit = defineEmits<{
  (e: 'request-sync'): void
}>()

const props = defineProps<{
  brokerId: string
  filter: string
}>()

const router = useRouter()

const { data: brokers } = useBrokersQuery()
const broker = computed(() => brokers.value?.find((x) => x.id === props.brokerId))

const pageIndex = ref(0)

const {
  query: { data: queues, isPending: isLoading },
  formattedQueues
} = useQueues(
  computed(() => props.brokerId),
  pageIndex
)

const selectQueue = (data: any) => {
  router.push({
    name: 'messages',
    params: {
      brokerId: broker.value?.id,
      queueId: data.id
    }
  })
}

const changePage = (e: PageState) => {
  pageIndex.value = e.page
}
</script>
<template>
  <template v-if="isLoading">
    <div class="p-5"><i class="pi pi-spinner pi-spin me-2"></i>Loading queues...</div>
  </template>
  <template v-else-if="formattedQueues.length">
    <DataTable
      scrollable
      data-key="id"
      scroll-height="flex"
      :value="formattedQueues"
      class="grow overflow-auto"
    >
      <Column sortable field="name" header="Name" class="w-[60%] overflow-ellipsis overflow-hidden">
        <template #body="{ data }">
          <span
            @click="selectQueue(data)"
            class="border-b border-gray-600 border-dashed hover:cursor-pointer hover:border-blue-500 hover:text-blue-500"
            >{{ data.parsed['name'] }}</span
          >
        </template>
      </Column>

      <Column sortable field="parsed.messages" header="Messages" class="w-[0%]">
        <template #body="{ data }">
          <div class="flex gap-1 items-center">
            <i class="text-xs text-emerald-500 pi pi-cloud-upload"></i>{{ data.parsed['messages'] }}
          </div>
        </template>
      </Column>

      <Column sortable field="asb" header="Synced" class="w-[0%]">
        <template #body="{ data }">
          <div class="flex gap-1 items-center">
            <i class="text-xs text-gray-500 pi pi-cloud-download"></i>{{ data.parsed['messages'] }}
          </div>
        </template>
      </Column>

      <Column field="parsed.type" header="Type" class="w-[0%]"> </Column>

      <Column field="parsed.features" header="Features" class="w-[0%]">
        <template #body="{ data }">
          <Tag v-show="data.parsed['durable']" severity="info">D</Tag>
        </template>
      </Column>
    </DataTable>
    <Paginator @page="changePage" :rows="50" :total-records="queues?.totalCount"></Paginator>
  </template>
  <template v-else>
    <div class="flex items-center flex-col mt-24 grow">
      <img src="/ebox.svg" class="w-72 opacity-50 pb-5" />
      <div class="text-lg">No Queues</div>
      <div class="">Make sure you sync the broker.</div>
      <Button @click="emit('request-sync')" label="Sync" icon="pi pi-sync" class="mt-3"></Button>
    </div>
  </template>
</template>
