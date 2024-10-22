<script setup lang="ts">
import AppLayout from '@/layouts/AppLayout.vue'
import { useRoute, useRouter } from 'vue-router'

const router = useRouter()
const route = useRoute()

const updateTabValue = (a: any) => router.push({ name: a })
</script>

<template>
  <AppLayout>
    <Tabs :value="route.name?.toString() ?? ''" @update:value="updateTabValue" :pt:tabList:class="'bg-transparent'">
      <TabList>
        <Tab value="overview">Overview</Tab>
        <Tab value="topics">Topics</Tab>
        <Tab value="queues">Queues</Tab>
        <Tab value="jobs">Recurring Jobs</Tab>

        <div v-if="route.name == 'queues'" class="me-3 ms-auto flex items-center gap-2">
          <span>Auto-refresh every 5 seconds</span>
          <!-- <i class="pi pi-pencil ms-3 cursor-pointer hover:text-blue-400"></i> -->
          <Button icon="pi pi-pencil" link></Button>
        </div>
      </TabList>
    </Tabs>
    <RouterView />
  </AppLayout>
</template>
