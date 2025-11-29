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
    <div class="flex flex-col overflow-auto">
      <div role="tablist" class="tabs tabs-border">
        <button
          role="tab"
          class="tab flex gap-2"
          :class="{ 'tab-active': route.name === 'overview' }"
          @click="router.push({ name: 'overview' })"
        >
          <BarsIcon class="h-4 w-4" /> Overview
        </button>
        <button
          role="tab"
          class="tab flex gap-2"
          :class="{ 'tab-active': route.name === 'topics' }"
          @click="router.push({ name: 'topics' })"
        >
          <SitemapIcon class="h-4 w-4" /> Topics
        </button>
        <button
          role="tab"
          class="tab flex gap-2"
          :class="{ 'tab-active': route.name === 'queues' }"
          @click="router.push({ name: 'queues' })"
        >
          <DatabaseIcon class="h-4 w-4 rotate-90" /> Queues
        </button>
      </div>

      <RouterView />
    </div>
  </AppLayout>
</template>
