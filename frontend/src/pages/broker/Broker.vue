<script setup lang="ts">
import { useBrokersQuery } from '@/api/broker/brokersQuery'
import { useSyncBrokerMutation } from '@/api/broker/syncBrokerMutation'
import { useQueuesQuery } from '@/api/queues/queuesQuery'
import AppLayout from '@/layouts/AppLayout.vue'
import Button from 'primevue/button'
import { computed } from 'vue'

const props = defineProps<{
  id: string
}>()

const { mutateAsync: syncBrokerAsync } = useSyncBrokerMutation()

const { data: brokers } = useBrokersQuery()
const broker = computed(() => brokers.value?.find((x) => x.id === props.id))
const brokerId = computed(() => broker.value?.id)
const { data: queues } = useQueuesQuery(brokerId)

const syncBroker = () => {
  if (!broker.value) {
    return
  }

  syncBrokerAsync(broker.value.id).then(() => {
    console.log('synced!')
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
          R.MQ
        </div>
        <div class="flex flex-col ps-2">
          <div>Name: {{ broker.name }}</div>
          <div>URL: {{ broker.url }}</div>
          <div>Port: {{ broker.port }}</div>
          <Button size="small" @click="syncBroker">sync</Button>
        </div>
      </div>
      <Tabs value="0">
        <TabList>
          <Tab value="0">Overview</Tab>
        </TabList>
        <TabPanels>
          <TabPanel value="0">
            <p class="m-0">
              {{ queues }}
            </p>
          </TabPanel>
        </TabPanels>
      </Tabs>
    </template>
    <template v-else> loading... </template>
  </AppLayout>
</template>
