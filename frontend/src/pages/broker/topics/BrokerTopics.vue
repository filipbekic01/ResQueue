<script lang="ts" setup>
import { computed } from "vue";
import { useRoute, useRouter } from "vue-router";
import { useSubscriptionsQuery } from "@/api/subscriptions/subscriptionsQuery";
import SitemapIcon from "@/components/icons/SitemapIcon.vue";
import Pagination from "@/components/Pagination.vue";
import { useLocalSettings } from "@/composables/useLocalSettings";
import type { SubscriptionDto } from "@/dtos/subscriptions/subscriptionDto";

const route = useRoute();
const router = useRouter();

const { refetchInterval } = useLocalSettings();

const { data: subscriptions } = useSubscriptionsQuery(refetchInterval);

// Search from URL
const search = computed({
  get: () => (route.query.search as string) ?? "",
  set: (value: string) => {
    router.replace({ query: { ...route.query, search: value || undefined } });
  },
});

// Sorting from URL
type SortField = keyof SubscriptionDto | null;

const sortField = computed({
  get: () => (route.query.sortField as SortField) ?? null,
  set: (value: SortField) => {
    router.replace({ query: { ...route.query, sortField: value || undefined } });
  },
});

const sortOrder = computed({
  get: () => (route.query.sortOrder as "asc" | "desc" | null) ?? null,
  set: (value: "asc" | "desc" | null) => {
    router.replace({ query: { ...route.query, sortOrder: value || undefined } });
  },
});

const toggleSort = (field: SortField) => {
  if (sortField.value === field) {
    if (sortOrder.value === "asc") {
      router.replace({ query: { ...route.query, sortOrder: "desc" } });
    } else if (sortOrder.value === "desc") {
      router.replace({ query: { ...route.query, sortField: undefined, sortOrder: undefined } });
    } else {
      router.replace({ query: { ...route.query, sortField: field, sortOrder: "asc" } });
    }
  } else {
    router.replace({ query: { ...route.query, sortField: field, sortOrder: "asc" } });
  }
};

// Pagination from URL
const currentPage = computed({
  get: () => Number(route.query.page) || 1,
  set: (value: number) => {
    router.replace({ query: { ...route.query, page: value > 1 ? value : undefined } });
  },
});
const pageSize = 20;

// Filtered, sorted and paginated data
const filteredSubscriptions = computed(() => {
  let result = subscriptions.value ?? [];

  // Filter by search
  if (search.value) {
    const searchLower = search.value.toLowerCase();
    result = result.filter((s) => s.topicName.toLowerCase().includes(searchLower));
  }

  // Sort
  if (sortField.value && sortOrder.value) {
    const field = sortField.value;
    const order = sortOrder.value === "asc" ? 1 : -1;
    result = [...result].sort((a, b) => {
      const aVal = a[field] ?? "";
      const bVal = b[field] ?? "";
      if (typeof aVal === "string" && typeof bVal === "string") {
        return aVal.localeCompare(bVal) * order;
      }
      return ((aVal as number) - (bVal as number)) * order;
    });
  }

  return result;
});

const paginatedSubscriptions = computed(() => {
  const start = (currentPage.value - 1) * pageSize;
  const end = start + pageSize;
  return filteredSubscriptions.value.slice(start, end);
});
</script>

<template>
  <div class="flex min-h-0 flex-1 flex-col">
    <!-- Search bar -->
    <div class="border-base-200 dark:border-base-content/10 flex shrink-0 items-center gap-3 border-b px-4 py-2">
      <div class="relative flex-1">
        <input
          v-model="search"
          type="text"
          placeholder="Search topics..."
          class="bg-base-200/50 focus:bg-base-100 border-base-200 focus:border-base-300 w-full max-w-sm rounded-lg border px-3 py-1.5 text-sm transition-colors outline-none"
        />
        <button
          v-if="search"
          class="text-base-content/40 hover:text-base-content absolute top-1/2 right-2.5 -translate-y-1/2 cursor-pointer transition-colors"
          @click="search = ''"
        >
          ✕
        </button>
      </div>
      <span class="text-base-content/40 text-xs">{{ filteredSubscriptions.length }} topics</span>
    </div>

    <!-- Table -->
    <div class="min-h-0 flex-1 overflow-auto">
      <table class="table w-full">
        <thead class="bg-base-100 sticky top-0">
          <tr class="border-base-200 dark:border-base-content/10 border-b">
            <th class="text-base-content/60 text-xs font-medium">Topic Name</th>
            <th
              class="text-base-content/60 w-0 cursor-pointer text-xs font-medium whitespace-nowrap"
              @click="toggleSort('routingKey')"
            >
              Routing Key
              <span v-if="sortField === 'routingKey'" class="text-base-content">{{
                sortOrder === "asc" ? "↑" : "↓"
              }}</span>
            </th>
            <th
              class="text-base-content/60 w-0 cursor-pointer text-xs font-medium whitespace-nowrap"
              @click="toggleSort('destinationName')"
            >
              Destination Name
              <span v-if="sortField === 'destinationName'" class="text-base-content">{{
                sortOrder === "asc" ? "↑" : "↓"
              }}</span>
            </th>
            <th
              class="text-base-content/60 w-0 cursor-pointer text-xs font-medium whitespace-nowrap"
              @click="toggleSort('destinationType')"
            >
              Destination Type
              <span v-if="sortField === 'destinationType'" class="text-base-content">{{
                sortOrder === "asc" ? "↑" : "↓"
              }}</span>
            </th>
            <th
              class="text-base-content/60 w-0 cursor-pointer text-xs font-medium whitespace-nowrap"
              @click="toggleSort('subscriptionType')"
            >
              Subscription Type
              <span v-if="sortField === 'subscriptionType'" class="text-base-content">{{
                sortOrder === "asc" ? "↑" : "↓"
              }}</span>
            </th>
          </tr>
        </thead>
        <tbody>
          <tr
            v-for="sub in paginatedSubscriptions"
            :key="`${sub.topicName}-${sub.destinationName}`"
            class="border-base-200 dark:border-base-content/5 border-b transition-colors"
          >
            <td class="text-base-content max-w-xs truncate py-2.5 text-sm font-medium">{{ sub.topicName }}</td>
            <td class="text-base-content/60 py-2.5 text-sm">{{ sub.routingKey || "-" }}</td>
            <td class="text-base-content/60 py-2.5 text-sm">{{ sub.destinationName }}</td>
            <td class="text-base-content/60 py-2.5 text-sm">{{ sub.destinationType }}</td>
            <td class="text-base-content/60 py-2.5 text-sm">{{ sub.subscriptionType }}</td>
          </tr>
        </tbody>
      </table>

      <!-- Empty State -->
      <div
        v-if="paginatedSubscriptions.length === 0"
        class="flex flex-1 flex-col items-center justify-center gap-4 p-8"
      >
        <div class="bg-base-200 flex h-16 w-16 items-center justify-center rounded-full">
          <SitemapIcon class="text-base-content/30 h-8 w-8" />
        </div>
        <div class="text-center">
          <h3 class="text-base-content text-lg font-medium">No topics found</h3>
          <p class="text-base-content/50 mt-1 text-sm">
            {{
              search ? "Try adjusting your search terms." : "Topics will appear here when subscriptions are created."
            }}
          </p>
        </div>
      </div>
    </div>

    <!-- Pagination -->
    <div
      v-if="filteredSubscriptions.length > pageSize"
      class="border-base-200 dark:border-base-content/10 shrink-0 border-t"
    >
      <Pagination
        :total-items="filteredSubscriptions.length"
        v-model:current-page="currentPage"
        :page-size="pageSize"
      />
    </div>
  </div>
</template>
