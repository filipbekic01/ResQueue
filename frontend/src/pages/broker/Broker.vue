<script setup lang="ts">
import { useBrokersQuery } from '@/api/brokers/brokersQuery'
import { useSyncBrokerMutation } from '@/api/brokers/syncBrokerMutation'
import { useUpdateBrokerMutation } from '@/api/brokers/updateBrokerMutation'
import { useIdentity } from '@/composables/identityComposable'
import Avatars from '@/features/avatars/Avatars.vue'
import AppLayout from '@/layouts/AppLayout.vue'
import { isBrokerAgent } from '@/utils/brokerUtils'
import { errorToToast } from '@/utils/errorUtils'
import { formatDistanceToNow } from 'date-fns'
import { useConfirm } from 'primevue/useconfirm'
import { useToast } from 'primevue/usetoast'
import { computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'

const props = defineProps<{
  brokerId: string
}>()

const confirm = useConfirm()
const toast = useToast()
const router = useRouter()
const route = useRoute()

const {
  query: { data: user }
} = useIdentity()

const { mutateAsync: syncBrokerAsync, isPending: isPendingSyncBroker } = useSyncBrokerMutation()

const { data: brokers } = useBrokersQuery()
const broker = computed(() => brokers.value?.find((x) => x.id === props.brokerId))
const access = computed(() => broker.value?.accessList.find((x) => x.userId === user.value?.id))
const { mutateAsync: updateBrokerAsync } = useUpdateBrokerMutation()

const applySearch = (value: string) => {
  if (broker.value && access.value && value !== access.value.settings.defaultQueueSearch) {
    updateBrokerAsync({
      broker: {
        ...broker.value,
        rabbitMQConnection: broker.value.rabbitMQConnection
          ? { ...broker.value.rabbitMQConnection, username: '', password: '' }
          : undefined,
        settings: {
          ...access.value.settings,
          defaultQueueSearch: value
        }
      },
      brokerId: broker.value.id
    }).catch((e) => toast.add(errorToToast(e)))
  }

  router.push({
    path: route.path,
    query: {
      ...route.query,
      search: value ? value : undefined
    }
  })
}

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

const updateTabValue = (a: any) => router.push({ name: a })
</script>

<template>
  <AppLayout hide-header>
    <template v-if="broker">
      <div class="flex px-4 pb-2 pt-4">
        <div
          class="flex h-20 w-20 items-center justify-center rounded-2xl bg-[#FF6600] text-2xl text-white"
        >
          <img src="/rmq.svg" class="w-14" />
        </div>

        <div class="flex flex-col justify-center ps-3">
          <div class="font-semibold">RabbitMQ</div>
          <div class="text-2xl font-bold">{{ broker.name }}</div>
          <div class="flex items-center gap-2 text-slate-500">
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
                <span class="cursor-pointer underline hover:text-blue-500" @click="syncBroker">{{
                  formatDistanceToNow(broker.syncedAt)
                }}</span>
                ago
              </template>
              <template v-else>
                <span class="cursor-pointer underline hover:text-blue-500" @click="syncBroker">
                  Click to sync broker
                </span>
              </template>
            </div>
            <div v-else>Syncing...</div>
          </div>
        </div>
        <div class="ms-auto">
          <Avatars :user-ids="broker.accessList.map((x) => x.userId)" />
        </div>
      </div>
      <Tabs :value="route.name?.toString() ?? ''" @update:value="updateTabValue">
        <TabList>
          <Tab value="overview">Overview</Tab>
          <Tab value="topics">Topics</Tab>
          <Tab value="queues">Queues</Tab>
          <div v-if="route.name === 'queues'" class="flex grow items-center gap-3 px-3">
            <!-- <div class="ms-auto text-slate-600">Filters</div> -->
            <div class="ms-auto"></div>
            <i
              v-if="route.query.search"
              class="pi pi-times me-1 cursor-pointer text-slate-400 hover:text-slate-600"
              @click="applySearch('')"
            ></i>
            <i v-else class="pi pi-search me-1 text-slate-400"></i>
            <InputText
              class="max-w-96"
              placeholder="Search"
              icon="pi pi-search"
              :value="route.query.search"
              @change="(e) => applySearch((e.target as any)?.value)"
            ></InputText>

            <ButtonGroup>
              <Button
                v-for="qs in access?.settings.quickSearches"
                outlined
                :key="qs"
                :label="qs"
                @click="applySearch(qs)"
              ></Button>
            </ButtonGroup>
          </div>
        </TabList>
      </Tabs>

      <RouterView />
    </template>
    <template v-else> loading... </template>
  </AppLayout>
</template>
