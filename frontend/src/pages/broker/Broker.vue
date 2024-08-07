<script setup lang="ts">
import { useBrokersQuery } from '@/api/broker/brokersQuery'
import { useSyncBrokerMutation } from '@/api/broker/syncBrokerMutation'
import { useExchangesQuery } from '@/api/exchanges/exchangesQuery'
import { useQueuesQuery } from '@/api/queues/queuesQuery'
import AppLayout from '@/layouts/AppLayout.vue'
import DataTable from 'primevue/datatable'
import { useConfirm } from 'primevue/useconfirm'
import { useToast } from 'primevue/usetoast'
import { computed } from 'vue'
import { formatDistanceToNow, format } from 'date-fns'
import { useRouter } from 'vue-router'

const props = defineProps<{
  brokerId: string
}>()

const { mutateAsync: syncBrokerAsync, isPending: isPendingSyncBroker } = useSyncBrokerMutation()

const router = useRouter()
const { data: brokers } = useBrokersQuery()
const broker = computed(() => brokers.value?.find((x) => x.id === props.brokerId))
const { data: queues } = useQueuesQuery(computed(() => broker.value?.id))
const { data: exchanges } = useExchangesQuery(computed(() => broker.value?.id))

const confirm = useConfirm()
const toast = useToast()

const rabbitMqQueues = computed(
  () =>
    queues.value?.map((q) => {
      return {
        _id: q.id,
        ...JSON.parse(q.rawData)
      }
    }) ?? []
)

const rabbitMqExchanges = computed(
  () =>
    exchanges.value?.map((q) => {
      return {
        _id: q.id,
        ...JSON.parse(q.rawData)
      }
    }) ?? []
)

const syncBroker = (event: any) => {
  confirm.require({
    target: event.currentTarget,
    message: 'Do you want to sync the broker?',
    icon: 'pi pi-info-circle',
    rejectProps: {
      label: 'Cancel',
      severity: 'secondary',
      outlined: true
    },
    acceptProps: {
      label: 'Sync Broker',
      severity: ''
    },
    accept: () => {
      if (!broker.value) {
        return
      }

      syncBrokerAsync(broker.value?.id).then(() => {
        toast.add({
          severity: 'info',
          summary: 'Sync Completed!',
          detail: `Broker ${broker.value?.name} synced!`,
          life: 3000
        })
      })
    },
    reject: () => {}
  })
}

const selectQueue = (data: any) => {
  // RabbitMQ/QueueDto missing type
  router.push({
    name: 'messages',
    params: {
      brokerId: broker.value?.id,
      queueId: data._id
    }
  })
}
</script>

<template>
  <AppLayout>
    <template v-if="broker">
      <div class="flex p-4">
        <div
          class="w-24 h-24 rounded bg-orange-400 items-center justify-center flex text-2xl text-white"
        >
          RMQ
        </div>
        <div class="flex flex-col ps-2">
          <div>Name: {{ broker.name }}</div>
          <div>URL: {{ broker.url }}</div>
          <div>Port: {{ broker.port }}</div>

          <div
            v-if="!isPendingSyncBroker"
            class="text-blue-400 hover:text-blue-300 cursor-pointer"
            @click="(e) => syncBroker(e)"
          >
            sync
          </div>
          <a v-else>syncing...</a>
        </div>
        <div class="ms-auto text-end">
          created on {{ format(broker.createdAt, 'MM/dd/yyyy') }}
          <br />
          updated {{ formatDistanceToNow(broker.updatedAt) }} ago
          <br />
          <template v-if="broker.syncedAt">
            synced {{ formatDistanceToNow(broker.syncedAt) ?? 'never' }} ago </template
          ><template v-else>never synced</template>
          <br />
          framework: {{ broker.framework ?? 'none' }}
        </div>
      </div>
      <Tabs value="1" class="overflow-auto">
        <TabList>
          <Tab value="0">Exchanges ({{ rabbitMqExchanges?.length }})</Tab>
          <Tab value="1">Queues ({{ rabbitMqQueues?.length }})</Tab>
        </TabList>
        <TabPanels class="overflow-auto">
          <TabPanel value="0">
            <p class="m-0">
              <DataTable :value="rabbitMqExchanges">
                <Column field="name" header="Name"></Column>
              </DataTable>
            </p>
          </TabPanel>
          <TabPanel value="1">
            <p class="m-0">
              <DataTable
                :value="rabbitMqQueues"
                selection-mode="single"
                @row-select="(e) => selectQueue(e.data)"
              >
                <Column field="vhost" header="VHost"></Column>
                <Column field="name" header="Name">
                  <template #body="{ data }">
                    ...{{ data.name.slice(-20) }} ({{ data.messages }})</template
                  >
                </Column>

                <Column field="type" header="Type"></Column>
                <Column field="" header="Features">
                  <template #body="{ data }">
                    <Tag v-show="data.durable" severity="info">D</Tag>
                  </template>
                </Column>
              </DataTable>
            </p>
          </TabPanel>
        </TabPanels>
      </Tabs>
    </template>
    <template v-else> loading... </template>
  </AppLayout>
</template>
