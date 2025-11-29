<script setup lang="ts">
import { useRoute, useRouter } from "vue-router";
import BarsIcon from "@/components/icons/BarsIcon.vue";
import DatabaseIcon from "@/components/icons/DatabaseIcon.vue";
import SitemapIcon from "@/components/icons/SitemapIcon.vue";
import AppLayout from "@/layouts/AppLayout.vue";

const router = useRouter();
const route = useRoute();
</script>

<template>
  <AppLayout>
    <div class="flex min-h-0 flex-1 flex-col">
      <!-- Navigation Tabs -->
      <div class="border-base-200 dark:border-base-content/10 flex gap-1 border-b px-4 py-2">
        <button
          class="text-base-content/60 hover:text-base-content hover:bg-base-200 flex cursor-pointer items-center gap-2 rounded-md px-3 py-1.5 text-sm font-medium transition-colors"
          :class="{
            'bg-base-200 text-base-content!': route.name === 'overview',
          }"
          @click="router.push({ name: 'overview' })"
        >
          <BarsIcon class="h-4 w-4" /> Overview
        </button>
        <button
          class="text-base-content/60 hover:text-base-content hover:bg-base-200 flex cursor-pointer items-center gap-2 rounded-md px-3 py-1.5 text-sm font-medium transition-colors"
          :class="{
            'bg-base-200 text-base-content!': route.name === 'topics',
          }"
          @click="router.push({ name: 'topics' })"
        >
          <SitemapIcon class="h-4 w-4" /> Topics
        </button>
        <button
          class="text-base-content/60 hover:text-base-content hover:bg-base-200 flex cursor-pointer items-center gap-2 rounded-md px-3 py-1.5 text-sm font-medium transition-colors"
          :class="{
            'bg-base-200 text-base-content!': route.name === 'queues',
          }"
          @click="router.push({ name: 'queues' })"
        >
          <DatabaseIcon class="h-4 w-4 rotate-90" /> Queues
        </button>
      </div>

      <RouterView />
    </div>
  </AppLayout>
</template>
