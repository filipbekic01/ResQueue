<script lang="ts" setup>
import { computed, ref, watchEffect } from "vue";
import { useSubscriptionsQuery } from "@/api/subscriptions/subscriptionsQuery";
import { useUserSettings } from "@/composables/userSettingsComposable";
import Pagination from "@/components/Pagination.vue";
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
  <div class="flex h-full flex-col">
    <!-- Search bar -->
    <div class="border-base-300 flex items-center gap-2 border-b px-4 py-2">
      <span class="font-semibold">Name</span>
      <div class="relative flex-1">
        <input
          v-model="search"
          type="text"
          placeholder="Search topics..."
          class="input input-bordered input-sm w-full max-w-xs"
        />
        <button
          v-if="search"
          class="text-base-content/50 hover:text-base-content absolute top-1/2 right-2 -translate-y-1/2"
          @click="search = ''"
        >
          ✕
        </button>
      </div>
    </div>

    <!-- Table -->
    <div class="grow overflow-auto">
      <table class="table-zebra table-pin-rows table w-full">
        <thead>
          <tr>
            <th>Topic Name</th>
            <th class="w-0 cursor-pointer whitespace-nowrap" @click="toggleSort('routingKey')">
              Routing Key
              <span v-if="sortField === 'routingKey'">{{ sortOrder === "asc" ? "▲" : "▼" }}</span>
            </th>
            <th class="w-0 cursor-pointer whitespace-nowrap" @click="toggleSort('destinationName')">
              Destination Name
              <span v-if="sortField === 'destinationName'">{{ sortOrder === "asc" ? "▲" : "▼" }}</span>
            </th>
            <th class="w-0 cursor-pointer whitespace-nowrap" @click="toggleSort('destinationType')">
              Destination Type
              <span v-if="sortField === 'destinationType'">{{ sortOrder === "asc" ? "▲" : "▼" }}</span>
            </th>
            <th class="w-0 cursor-pointer whitespace-nowrap" @click="toggleSort('subscriptionType')">
              Subscription Type
              <span v-if="sortField === 'subscriptionType'">{{ sortOrder === "asc" ? "▲" : "▼" }}</span>
            </th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="sub in paginatedSubscriptions" :key="`${sub.topicName}-${sub.destinationName}`">
            <td class="max-w-xs truncate">{{ sub.topicName }}</td>
            <td>{{ sub.routingKey }}</td>
            <td>{{ sub.destinationName }}</td>
            <td>{{ sub.destinationType }}</td>
            <td>{{ sub.subscriptionType }}</td>
          </tr>
          <tr v-if="paginatedSubscriptions.length === 0">
            <td colspan="5" class="text-base-content/50 text-center">No topics found</td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- Pagination -->
    <div class="border-base-300 border-t">
      <Pagination
        :total-items="filteredSubscriptions.length"
        v-model:current-page="currentPage"
        v-model:page-size="pageSize"
      />
    </div>
  </div>
</template>
