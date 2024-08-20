<script setup lang="ts">
import { useBrokersQuery } from '@/api/broker/brokersQuery'
import { useSyncBrokerMutation } from '@/api/broker/syncBrokerMutation'
import AppLayout from '@/layouts/AppLayout.vue'
import { useConfirm } from 'primevue/useconfirm'
import { useToast } from 'primevue/usetoast'
import { computed } from 'vue'
import { formatDistanceToNow } from 'date-fns'
import Select from 'primevue/select'
import { useRoute, useRouter } from 'vue-router'

const props = defineProps<{
  brokerId: string
}>()

const confirm = useConfirm()
const toast = useToast()
const router = useRouter()
const route = useRoute()

const { mutateAsync: syncBrokerAsync, isPending: isPendingSyncBroker } = useSyncBrokerMutation()

const { data: brokers } = useBrokersQuery()
const broker = computed(() => brokers.value?.find((x) => x.id === props.brokerId))

const applySearch = (value: string) => {
  router.push({
    path: route.path,
    query: {
      ...route.query,
      search: value ? value : undefined
    }
  })
}

const syncBroker = () => {
  confirm.require({
    message:
      'Do you really want to sync with remote broker? You can turn off this dialog in settings.',
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

const updateTabValue = (a: any) => router.push({ name: a })
</script>

<template>
  <AppLayout hide-header>
    <template v-if="broker">
      <div class="flex p-4">
        <div
          class="w-24 h-24 rounded-2xl bg-[#FF6600] items-center justify-center flex text-2xl text-white"
        >
          <img src="/rmq.svg" class="w-16" />
        </div>

        <div class="flex flex-col ps-3">
          <div class="font-bold text-3xl">{{ broker.name }}</div>
          <div class="flex gap-2 text-gray-500 mt-auto items-center">
            <i
              class="pi pi-sync"
              :class="[
                {
                  'pi pi-spin': isPendingSyncBroker
                }
              ]"
              style="font-size: 0.8rem"
            ></i>
            <div v-if="!isPendingSyncBroker">
              <template v-if="broker.syncedAt">
                Synced
                <span class="underline hover:text-blue-500 cursor-pointer" @click="syncBroker">{{
                  formatDistanceToNow(broker.syncedAt)
                }}</span>
                ago
              </template>
              <template v-else>
                <span class="underline hover:text-blue-500 cursor-pointer" @click="syncBroker">
                  Click to sync broker
                </span>
              </template>
            </div>
            <div v-else>Syncing...</div>
          </div>
        </div>
      </div>
      <Tabs :value="route.name?.toString() ?? ''" @update:value="updateTabValue">
        <TabList>
          <Tab value="overview">Overview</Tab>
          <Tab value="topics">Topics</Tab>
          <Tab value="queues">Queues</Tab>
          <div class="flex px-3 gap-3 grow items-center">
            <div class="ms-auto text-gray-500">Filters</div>

            <i
              v-if="route.query.search"
              class="pi pi-times me-1 text-gray-400 cursor-pointer hover:text-gray-600"
              @click="applySearch('')"
            ></i>
            <i v-else class="pi pi-filter me-1 text-gray-400"></i>
            <InputText
              class="max-w-96"
              placeholder="Search"
              icon="pi pi-search"
              :value="route.query.search"
              @change="(e) => applySearch((e.target as any)?.value)"
            ></InputText>

            <ButtonGroup>
              <Button label="error" outlined @click="applySearch('error')"></Button>
              <Button label="fail" outlined @click="applySearch('fail')"></Button>
              <Button label="dead" outlined @click="applySearch('dead')"></Button>
            </ButtonGroup>
            <Select></Select>
          </div>
        </TabList>
      </Tabs>

      <RouterView />
    </template>
    <template v-else> loading... </template>
  </AppLayout>
</template>
