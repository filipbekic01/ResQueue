<script lang="ts" setup>
import { computed, ref, watchEffect } from "vue";
import { useSubscriptionsQuery } from "@/api/subscriptions/subscriptionsQuery";
import Pagination from "@/components/Pagination.vue";
import { useUserSettings } from "@/composables/userSettingsComposable";
import type { SubscriptionDto } from "@/dtos/subscriptions/subscriptionDto";

const { settings, updateSettings } = useUserSettings();

const { data: subscriptions } = useSubscriptionsQuery(computed(() => settings.refetchInterval));

const search = ref(settings.topicSearch);

watchEffect(() => {
  updateSettings({
    ...settings,
    topicSearch: search.value,
  });
});

// Sorting
type SortField = keyof SubscriptionDto | null;
const sortField = ref<SortField>(null);
const sortOrder = ref<"asc" | "desc" | null>(null);

const toggleSort = (field: SortField) => {
  if (sortField.value === field) {
    if (sortOrder.value === "asc") {
      sortOrder.value = "desc";
    } else if (sortOrder.value === "desc") {
      sortField.value = null;
      sortOrder.value = null;
    } else {
      sortOrder.value = "asc";
    }
  } else {
    sortField.value = field;
    sortOrder.value = "asc";
  }
};

// Pagination
const currentPage = ref(1);
const pageSize = ref(20);

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
  const start = (currentPage.value - 1) * pageSize.value;
  const end = start + pageSize.value;
  return filteredSubscriptions.value.slice(start, end);
});

// Reset to page 1 when search changes
watchEffect(() => {
  if (search.value !== undefined) {
    currentPage.value = 1;
  }
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
              <span v-if="sortField === 'routingKey'" class="text-primary">{{ sortOrder === "asc" ? "↑" : "↓" }}</span>
            </th>
            <th
              class="text-base-content/60 w-0 cursor-pointer text-xs font-medium whitespace-nowrap"
              @click="toggleSort('destinationName')"
            >
              Destination Name
              <span v-if="sortField === 'destinationName'" class="text-primary">{{
                sortOrder === "asc" ? "↑" : "↓"
              }}</span>
            </th>
            <th
              class="text-base-content/60 w-0 cursor-pointer text-xs font-medium whitespace-nowrap"
              @click="toggleSort('destinationType')"
            >
              Destination Type
              <span v-if="sortField === 'destinationType'" class="text-primary">{{
                sortOrder === "asc" ? "↑" : "↓"
              }}</span>
            </th>
            <th
              class="text-base-content/60 w-0 cursor-pointer text-xs font-medium whitespace-nowrap"
              @click="toggleSort('subscriptionType')"
            >
              Subscription Type
              <span v-if="sortField === 'subscriptionType'" class="text-primary">{{
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
          <tr v-if="paginatedSubscriptions.length === 0">
            <td colspan="5" class="text-base-content/40 py-12 text-center text-sm">No topics found</td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- Pagination -->
    <div
      v-if="filteredSubscriptions.length > pageSize"
      class="border-base-200 dark:border-base-content/10 shrink-0 border-t"
    >
      <Pagination
        :total-items="filteredSubscriptions.length"
        v-model:current-page="currentPage"
        v-model:page-size="pageSize"
      />
    </div>
  </div>
</template>
