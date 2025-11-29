<script setup lang="ts">
import { useRoute, useRouter } from "vue-router";
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
          <i class="pi pi-bars"></i> Overview
        </button>
        <button
          role="tab"
          class="tab flex gap-2"
          :class="{ 'tab-active': route.name === 'topics' }"
          @click="router.push({ name: 'topics' })"
        >
          <i class="pi pi-sitemap"></i> Topics
        </button>
        <button
          role="tab"
          class="tab flex gap-2"
          :class="{ 'tab-active': route.name === 'queues' }"
          @click="router.push({ name: 'queues' })"
        >
          <i class="pi pi-database rotate-90"></i> Queues
        </button>
      </div>

      <RouterView />
    </div>
  </AppLayout>
</template>
