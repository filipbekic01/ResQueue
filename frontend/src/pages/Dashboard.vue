<script lang="ts" setup>
import { computed } from "vue";
import { useRouter } from "vue-router";
import { useAuthQuery } from "@/api/auth/authQuery";
import { useQueuesViewQuery } from "@/api/queues/queuesViewQuery";
import { useSubscriptionsQuery } from "@/api/subscriptions/subscriptionsQuery";
import DatabaseIcon from "@/components/icons/DatabaseIcon.vue";
import InformationCircleIcon from "@/components/icons/InformationCircleIcon.vue";
import SitemapIcon from "@/components/icons/SitemapIcon.vue";
import { useLocalSettings } from "@/composables/useLocalSettings";
import AppLayout from "@/layouts/AppLayout.vue";

const router = useRouter();
const { refetchInterval } = useLocalSettings();

const { data: auth } = useAuthQuery();
const { data: queuesData } = useQueuesViewQuery(refetchInterval);
const { data: subscriptionsData } = useSubscriptionsQuery(refetchInterval);

const queuesCount = computed(() => queuesData.value?.length ?? 0);
const topicsCount = computed(() => {
  const topics = new Set(subscriptionsData.value?.map((s) => s.topicName) ?? []);
  return topics.size;
});

const connectionInfo = computed(() => [
  { key: "SQL Engine", value: auth.value?.sqlEngine ?? "-" },
  { key: "Database", value: auth.value?.database ?? "-" },
  { key: "Schema", value: auth.value?.schema ?? "-" },
  { key: "Username", value: auth.value?.username ?? "-" },
  { key: "Port", value: auth.value?.port ?? "-" },
]);
</script>

<template>
  <AppLayout>
    <div class="flex min-h-0 flex-1 flex-col overflow-auto">
      <div class="mx-auto w-full max-w-4xl p-6">
        <!-- Header -->
        <div class="mb-8">
          <h1 class="text-base-content text-2xl font-semibold">Dashboard</h1>
          <p class="text-base-content/60 mt-1 text-sm">Manage your message broker queues and topics</p>
        </div>

        <!-- Navigation Tiles -->
        <div class="mb-8 grid gap-4 sm:grid-cols-2">
          <!-- Queues Tile -->
          <button
            class="bg-base-100 border-base-200 hover:border-base-300 dark:border-base-content/10 dark:hover:border-base-content/20 group flex cursor-pointer flex-col rounded-xl border p-6 text-left transition-all hover:shadow-md"
            @click="router.push({ name: 'queues' })"
          >
            <div class="bg-primary/10 mb-4 flex h-12 w-12 items-center justify-center rounded-lg">
              <DatabaseIcon class="text-primary h-6 w-6 rotate-90" />
            </div>
            <h2 class="text-base-content text-lg font-semibold">Queues</h2>
            <p class="text-base-content/60 mt-1 text-sm">Browse and manage message queues</p>
            <div class="mt-4 flex items-center gap-2">
              <span class="bg-base-200 text-base-content rounded-full px-2.5 py-0.5 text-xs font-medium">
                {{ queuesCount }} {{ queuesCount === 1 ? "queue" : "queues" }}
              </span>
            </div>
          </button>

          <!-- Topics Tile -->
          <button
            class="bg-base-100 border-base-200 hover:border-base-300 dark:border-base-content/10 dark:hover:border-base-content/20 group flex cursor-pointer flex-col rounded-xl border p-6 text-left transition-all hover:shadow-md"
            @click="router.push({ name: 'topics' })"
          >
            <div class="bg-secondary/10 mb-4 flex h-12 w-12 items-center justify-center rounded-lg">
              <SitemapIcon class="text-secondary h-6 w-6" />
            </div>
            <h2 class="text-base-content text-lg font-semibold">Topics</h2>
            <p class="text-base-content/60 mt-1 text-sm">View topic subscriptions and routing</p>
            <div class="mt-4 flex items-center gap-2">
              <span class="bg-base-200 text-base-content rounded-full px-2.5 py-0.5 text-xs font-medium">
                {{ topicsCount }} {{ topicsCount === 1 ? "topic" : "topics" }}
              </span>
            </div>
          </button>
        </div>

        <!-- Connection Info Card -->
        <div class="bg-base-100 border-base-200 dark:border-base-content/10 mb-8 rounded-xl border">
          <div class="border-base-200 dark:border-base-content/10 flex items-center gap-3 border-b px-6 py-4">
            <div class="bg-info/10 flex h-8 w-8 items-center justify-center rounded-lg">
              <InformationCircleIcon class="text-info h-4 w-4" />
            </div>
            <h3 class="text-base-content font-medium">Connection Details</h3>
          </div>
          <div class="p-6">
            <div class="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
              <div v-for="item in connectionInfo" :key="item.key" class="flex flex-col">
                <span class="text-base-content/50 text-xs font-medium tracking-wider uppercase">{{ item.key }}</span>
                <span class="text-base-content mt-1 text-sm font-medium">{{ item.value }}</span>
              </div>
            </div>
          </div>
        </div>

        <!-- Documentation Links -->
        <div class="mb-8">
          <h3 class="text-base-content/60 mb-3 text-xs font-medium tracking-wider uppercase">Documentation</h3>
          <div class="grid gap-3 sm:grid-cols-2">
            <a
              href="https://masstransit.io/documentation/transports/sql"
              target="_blank"
              class="bg-base-100 border-base-200 hover:border-base-300 dark:border-base-content/10 dark:hover:border-base-content/20 flex items-center gap-3 rounded-lg border p-4 transition-all hover:shadow-sm"
            >
              <div class="bg-base-200 flex h-10 w-10 shrink-0 items-center justify-center rounded-lg">
                <svg
                  class="text-base-content/60 h-5 w-5"
                  fill="none"
                  viewBox="0 0 24 24"
                  stroke="currentColor"
                  stroke-width="1.5"
                >
                  <path
                    stroke-linecap="round"
                    stroke-linejoin="round"
                    d="M12 6.042A8.967 8.967 0 006 3.75c-1.052 0-2.062.18-3 .512v14.25A8.987 8.987 0 016 18c2.305 0 4.408.867 6 2.292m0-14.25a8.966 8.966 0 016-2.292c1.052 0 2.062.18 3 .512v14.25A8.987 8.987 0 0018 18a8.967 8.967 0 00-6 2.292m0-14.25v14.25"
                  />
                </svg>
              </div>
              <div class="min-w-0 flex-1">
                <div class="text-base-content text-sm font-medium">SQL Transport</div>
                <div class="text-base-content/50 truncate text-xs">Configuration & setup guide</div>
              </div>
              <svg
                class="text-base-content/30 h-4 w-4 shrink-0"
                fill="none"
                viewBox="0 0 24 24"
                stroke="currentColor"
                stroke-width="2"
              >
                <path
                  stroke-linecap="round"
                  stroke-linejoin="round"
                  d="M10 6H6a2 2 0 00-2 2v10a2 2 0 002 2h10a2 2 0 002-2v-4M14 4h6m0 0v6m0-6L10 14"
                />
              </svg>
            </a>
            <a
              href="https://masstransit.io/documentation/concepts"
              target="_blank"
              class="bg-base-100 border-base-200 hover:border-base-300 dark:border-base-content/10 dark:hover:border-base-content/20 flex items-center gap-3 rounded-lg border p-4 transition-all hover:shadow-sm"
            >
              <div class="bg-base-200 flex h-10 w-10 shrink-0 items-center justify-center rounded-lg">
                <svg
                  class="text-base-content/60 h-5 w-5"
                  fill="none"
                  viewBox="0 0 24 24"
                  stroke="currentColor"
                  stroke-width="1.5"
                >
                  <path
                    stroke-linecap="round"
                    stroke-linejoin="round"
                    d="M19.5 14.25v-2.625a3.375 3.375 0 00-3.375-3.375h-1.5A1.125 1.125 0 0113.5 7.125v-1.5a3.375 3.375 0 00-3.375-3.375H8.25m0 12.75h7.5m-7.5 3H12M10.5 2.25H5.625c-.621 0-1.125.504-1.125 1.125v17.25c0 .621.504 1.125 1.125 1.125h12.75c.621 0 1.125-.504 1.125-1.125V11.25a9 9 0 00-9-9z"
                  />
                </svg>
              </div>
              <div class="min-w-0 flex-1">
                <div class="text-base-content text-sm font-medium">MassTransit Docs</div>
                <div class="text-base-content/50 truncate text-xs">Full documentation</div>
              </div>
              <svg
                class="text-base-content/30 h-4 w-4 shrink-0"
                fill="none"
                viewBox="0 0 24 24"
                stroke="currentColor"
                stroke-width="2"
              >
                <path
                  stroke-linecap="round"
                  stroke-linejoin="round"
                  d="M10 6H6a2 2 0 00-2 2v10a2 2 0 002 2h10a2 2 0 002-2v-4M14 4h6m0 0v6m0-6L10 14"
                />
              </svg>
            </a>
          </div>
        </div>

        <!-- GitHub Link -->
        <div class="mt-6 text-center">
          <a
            href="https://github.com/filipbekic01/resqueue"
            target="_blank"
            class="text-base-content/40 hover:text-base-content/60 inline-flex items-center gap-1.5 text-xs transition-colors"
          >
            <svg class="h-4 w-4" fill="currentColor" viewBox="0 0 24 24">
              <path
                d="M12 0c-6.626 0-12 5.373-12 12 0 5.302 3.438 9.8 8.207 11.387.599.111.793-.261.793-.577v-2.234c-3.338.726-4.033-1.416-4.033-1.416-.546-1.387-1.333-1.756-1.333-1.756-1.089-.745.083-.729.083-.729 1.205.084 1.839 1.237 1.839 1.237 1.07 1.834 2.807 1.304 3.492.997.107-.775.418-1.305.762-1.604-2.665-.305-5.467-1.334-5.467-5.931 0-1.311.469-2.381 1.236-3.221-.124-.303-.535-1.524.117-3.176 0 0 1.008-.322 3.301 1.23.957-.266 1.983-.399 3.003-.404 1.02.005 2.047.138 3.006.404 2.291-1.552 3.297-1.23 3.297-1.23.653 1.653.242 2.874.118 3.176.77.84 1.235 1.911 1.235 3.221 0 4.609-2.807 5.624-5.479 5.921.43.372.823 1.102.823 2.222v3.293c0 .319.192.694.801.576 4.765-1.589 8.199-6.086 8.199-11.386 0-6.627-5.373-12-12-12z"
              />
            </svg>
            ResQueue on GitHub
          </a>
        </div>
      </div>
    </div>
  </AppLayout>
</template>
