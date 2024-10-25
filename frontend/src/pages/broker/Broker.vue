<script setup lang="ts">
import AppLayout from '@/layouts/AppLayout.vue'
import { useRoute, useRouter } from 'vue-router'

const router = useRouter()
const route = useRoute()
</script>

<template>
  <AppLayout>
    <Tabs :value="route.name?.toString() ?? ''" @update:value="(a: any) => router.push({ name: a })">
      <TabList>
        <Tab disabled value="overview">Overview</Tab>
        <Tab disabled value="topics">Topics</Tab>
        <Tab value="queues">Queues</Tab>
        <Tab disabled value="jobs">Recurring Jobs</Tab>
      </TabList>
    </Tabs>

    <RouterView />
  </AppLayout>
</template>
