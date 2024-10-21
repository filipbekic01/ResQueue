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
      </TabList>
    </Tabs>
    <RouterView />
  </AppLayout>
</template>
