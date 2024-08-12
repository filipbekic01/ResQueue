<script setup lang="ts">
import { useBrokersQuery } from '@/api/broker/brokersQuery'
import { useSyncBrokerMutation } from '@/api/broker/syncBrokerMutation'
import { useExchangesQuery } from '@/api/exchanges/exchangesQuery'
import { useQueuesQuery } from '@/api/queues/queuesQuery'
import AppLayout from '@/layouts/AppLayout.vue'
import DataTable from 'primevue/datatable'
import { useConfirm } from 'primevue/useconfirm'
import { useToast } from 'primevue/usetoast'
import { computed, ref } from 'vue'
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
    queues.value
      ?.filter((x) => x.rawData.toLowerCase().includes(search.value.toLowerCase()))
      .map((q) => {
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

const selectedTab = ref('2')

const search = ref('')

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

const breadcrumbs = computed(() => {
  return [
    {
      label: 'RQ'
    },
    { label: `${broker.value?.name}` },
    { label: `queue ?` },
    { label: `messages` }
  ]
})
</script>

<template>
  <AppLayout>
    <template #title>
      {{ broker?.name }}
    </template>
    <template #description>
      <Breadcrumb :model="breadcrumbs" style="padding: 0" />
    </template>
    <template v-if="broker">
      <div class="flex p-4">
        <div
          class="w-24 h-24 rounded-2xl bg-orange-400 items-center justify-center flex text-2xl text-white"
        >
          RMQ
        </div>

        <div class="flex flex-col ps-2">
          <div class="font-bold">RabbitMQ</div>
          <div>{{ broker.url }}:{{ broker.port }}</div>
          <div class="text-gray-500 flex gap-3">
            <div>Created {{ formatDistanceToNow(broker.createdAt) }} ago</div>
          </div>
          <div class="flex gap-2">
            <Badge severity="secondary" class="mt-auto">
              <div class="flex items-center gap-2">
                <i class="pi pi-tags" style="font-size: 0.8rem"></i>
                <b>Framework</b>
                {{ broker.framework ? 'MassTransit' : 'none' }}
              </div>
            </Badge>
            <Badge severity="secondary" class="mt-auto">
              <div class="flex items-center gap-2">
                <i class="pi pi-tags" style="font-size: 0.8rem"></i>
                <b>Version</b>
                <a>3.16.2</a>
              </div>
            </Badge>
          </div>
        </div>
        <div class="ms-auto text-end flex items-center gap-3">
          <template v-if="broker.syncedAt">
            synced {{ formatDistanceToNow(broker.syncedAt) ?? 'never' }} ago </template
          ><template v-else>never synced</template>
          <Button
            :loading="isPendingSyncBroker"
            @click="(e) => syncBroker(e)"
            label="Synchronize"
          ></Button>
        </div>
      </div>
      <Tabs v-model:value="selectedTab" class="overflow-auto grow">
        <TabList>
          <Tab value="0">Overview</Tab>
          <Tab value="1">Topics ({{ rabbitMqExchanges?.length }})</Tab>
          <Tab value="2"
            >Queues (<template v-if="queues?.length != rabbitMqQueues?.length"
              >{{ rabbitMqQueues?.length }} of </template
            >{{ queues?.length }})</Tab
          >
          <div class="flex items-center grow px-3">
            <InputText
              class="max-w-96 grow ms-auto"
              placeholder="Search"
              icon="pi pi-search"
              v-model="search"
            ></InputText>
          </div>
        </TabList>
        <TabPanels class="flex overflow-auto grow" style="padding: 0">
          <TabPanel value="0" class="overflow-auto flex grow"> settings </TabPanel>
          <TabPanel value="1" class="overflow-auto flex grow">
            <DataTable
              v-if="selectedTab == '1'"
              :value="rabbitMqExchanges"
              scrollable
              scroll-height="flex"
              class="grow"
              :virtual-scroller-options="{
                itemSize: 46
              }"
            >
              <Column field="name" header="Name"></Column>
            </DataTable>
          </TabPanel>
          <TabPanel value="2" class="overflow-auto flex grow">
            <template v-if="selectedTab == '2'">
              <DataTable
                scrollable
                scroll-height="flex"
                :virtual-scroller-options="{
                  itemSize: 46
                }"
                sort-field="messages"
                :sort-order="-1"
                class="grow"
                :value="rabbitMqQueues"
                selection-mode="single"
                @row-select="(e) => selectQueue(e.data)"
              >
                <Column sortable field="vhost" header="VHost" class="w-[10%]"></Column>
                <Column sortable field="name" header="Name" class="w-[60%]">
                  <template #body="{ data }">
                    <span class="overflow-hidden whitespace-nowrap">{{ data.name }}</span>
                  </template>
                </Column>

                <Column sortable field="messages" header="Messages" class="w-[10%]">
                  <template #body="{ data }">
                    <div class="flex gap-2" v-tooltip.left="'Messages in queue and locally.'">
                      <div class="flex gap-1 items-center">
                        <i class="text-xs text-emerald-500 pi pi-caret-up"></i>{{ data.messages }}
                      </div>
                      <div class="flex gap-1 items-center">
                        <i class="text-xs text-red-500 pi pi-caret-down"></i>{{ data.messages }}
                      </div>
                    </div>
                  </template>
                </Column>
                <Column sortable field="type" header="Type" class="w-[10%]"></Column>
                <Column sortable field="" header="Features" class="w-[10%]">
                  <template #body="{ data }">
                    <Tag v-show="data.durable" severity="info">D</Tag>
                  </template>
                </Column>
              </DataTable>
            </template>
          </TabPanel>
        </TabPanels>
      </Tabs>
    </template>
    <template v-else> loading... </template>
  </AppLayout>
</template>
