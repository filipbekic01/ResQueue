<script setup lang="ts">
import AppLayout from '@/layouts/AppLayout.vue'
import { useRoute, useRouter } from 'vue-router'

const router = useRouter()
const route = useRoute()
</script>

<template>
  <AppLayout>
    <div class="flex flex-col overflow-auto">
      <Tabs
        :value="route.name?.toString() ?? ''"
        @update:value="(a: any) => router.push({ name: a })"
      >
        <TabList>
          <Tab value="overview" class="flex gap-2"><i class="pi pi-bars"></i> Overview</Tab>
          <Tab value="topics" class="flex gap-2"><i class="pi pi-sitemap"></i> Topics</Tab>
          <Tab value="queues" class="flex gap-2"
            ><i class="pi pi-database rotate-90"></i> Queues</Tab
          >
        </TabList>
      </Tabs>

      <RouterView />
    </div>
  </AppLayout>
</template>
