<script setup lang="ts">
import { useBrokersQuery } from '@/api/broker/brokersQuery'
import AppLayout from '@/layouts/AppLayout.vue'
import Button from 'primevue/button'
import { computed } from 'vue'

const props = defineProps<{
  id: string
}>()

const { data: brokers } = useBrokersQuery()

const broker = computed(() => brokers.value?.find((x) => x.id === props.id))
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
          <Button size="small">Edit</Button>
        </div>
      </div>
      <Tabs value="0">
        <TabList>
          <Tab value="0">Overview</Tab>
          <Tab value="1">Exchanges (0)</Tab>
          <Tab value="2">Subscribers (0)</Tab>
          <Tab value="3">Queues (0)</Tab>
        </TabList>
        <TabPanels>
          <TabPanel value="0">
            <p class="m-0">
              Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor
              incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud
              exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure
              dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur.
              Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt
              mollit anim id est laborum.
            </p>
          </TabPanel>
          <TabPanel value="1">
            <p class="m-0">
              Sed ut perspiciatis unde omnis iste natus error sit voluptatem accusantium doloremque
              laudantium, totam rem aperiam, eaque ipsa quae ab illo inventore veritatis et quasi
              architecto beatae vitae dicta sunt explicabo. Nemo enim ipsam voluptatem quia voluptas
              sit aspernatur aut odit aut fugit, sed quia consequuntur magni dolores eos qui ratione
              voluptatem sequi nesciunt. Consectetur, adipisci velit, sed quia non numquam eius
              modi.
            </p>
          </TabPanel>
          <TabPanel value="2">
            <p class="m-0">
              At vero eos et accusamus et iusto odio dignissimos ducimus qui blanditiis praesentium
              voluptatum deleniti atque corrupti quos dolores et quas molestias excepturi sint
              occaecati cupiditate non provident, similique sunt in culpa qui officia deserunt
              mollitia animi, id est laborum et dolorum fuga. Et harum quidem rerum facilis est et
              expedita distinctio. Nam libero tempore, cum soluta nobis est eligendi optio cumque
              nihil impedit quo minus.
            </p>
          </TabPanel>
        </TabPanels>
      </Tabs>
    </template>
    <template v-else> loading... </template>
  </AppLayout>
</template>
