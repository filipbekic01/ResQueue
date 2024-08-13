<script lang="ts" setup>
import { useBrokersQuery } from '@/api/broker/brokersQuery'
import Column from 'primevue/column'
import DataTable from 'primevue/datatable'
import { computed } from 'vue'
import { useRouter } from 'vue-router'
import { useQueues } from '@/composables/queuesComposable'

const props = defineProps<{
  brokerId: string
  filter: string
}>()

const router = useRouter()

const { data: brokers } = useBrokersQuery()
const broker = computed(() => brokers.value?.find((x) => x.id === props.brokerId))

const { formattedQueues } = useQueues(props.brokerId)
const filteredFormattedQueues = computed(() =>
  formattedQueues.value.filter((x) => x.rawData.toLowerCase().includes(props.filter))
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
</script>
<template>
  <DataTable
    scrollable
    scroll-height="flex"
    :virtual-scroller-options="{
      itemSize: 46
    }"
    sort-field="parsed.messages"
    :sort-order="-1"
    class="grow"
    :value="filteredFormattedQueues"
    removable-sort
    selection-mode="single"
    @row-select="(e) => selectQueue(e.data)"
  >
    <Column sortable field="parsed.vhost" header="VHost" class="w-[10%]"></Column>
    <Column sortable field="name" header="Name" class="w-[60%]">
      <template #body="{ data }">
        <span class="overflow-hidden whitespace-nowrap">{{ data.parsed['name'] }}</span>
      </template>
    </Column>

    <Column sortable field="messages" header="Messages" class="w-[10%]">
      <template #body="{ data }">
        <div class="flex gap-2" v-tooltip.left="'Messages in queue and locally.'">
          <div class="flex gap-1 items-center">
            <i class="text-xs text-emerald-500 pi pi-caret-up"></i>{{ data.parsed['messages'] }}
          </div>
          <div class="flex gap-1 items-center">
            <i class="text-xs text-red-500 pi pi-caret-down"></i>{{ data.parsed['messages'] }}
          </div>
        </div>
      </template>
    </Column>
    <Column sortable field="type" header="Type" class="w-[10%]"></Column>
    <Column sortable field="" header="Features" class="w-[10%]">
      <template #body="{ data }">
        <Tag v-show="data.parsed['durable']" severity="info">D</Tag>
      </template>
    </Column>
  </DataTable>
</template>
