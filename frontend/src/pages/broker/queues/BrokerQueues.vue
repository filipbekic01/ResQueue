<script lang="ts" setup>
import { computed, ref, watchEffect } from "vue";
import { useRouter } from "vue-router";
import { useQueuesViewQuery } from "@/api/queues/queuesViewQuery";
import Pagination from "@/components/Pagination.vue";
import { useUserSettings } from "@/composables/userSettingsComposable";
import type { QueueViewDto } from "@/dtos/queue/queueViewDto";

const router = useRouter();

const { settings, updateSettings } = useUserSettings();

const { data } = useQueuesViewQuery(computed(() => settings.refetchInterval));
const queuesView = computed(() => data.value ?? []);

const selectQueue = (queue: QueueViewDto) => {
  router.push({
    name: "messages",
    params: {
      queueName: queue.queueName,
    },
  });
};

const search = ref(settings.queueSearch);

watchEffect(() => {
  updateSettings({
    ...settings,
    queueSearch: search.value,
  });
});

// Sorting
type SortField = "ready" | "errored" | "deadLettered" | "scheduled" | "locked" | null;
const sortField = ref<SortField>((settings.sortField as SortField) ?? null);
const sortOrder = ref<"asc" | "desc" | null>(
  settings.sortOrder === 1 ? "asc" : settings.sortOrder === -1 ? "desc" : null,
);

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

  updateSettings({
    ...settings,
    sortOrder: sortOrder.value === "asc" ? 1 : sortOrder.value === "desc" ? -1 : undefined,
    sortField: sortField.value ?? undefined,
  });
};

// Pagination
const currentPage = ref(1);
const pageSize = ref(20);

// Filtered, sorted and paginated data
const filteredQueues = computed(() => {
  let result = queuesView.value;

  // Filter by search
  if (search.value) {
    const searchLower = search.value.toLowerCase();
    result = result.filter((q) => q.queueName.toLowerCase().includes(searchLower));
  }

  // Sort
  if (sortField.value && sortOrder.value) {
    const field = sortField.value;
    const order = sortOrder.value === "asc" ? 1 : -1;
    result = [...result].sort((a, b) => {
      const aVal = a[field] ?? 0;
      const bVal = b[field] ?? 0;
      return (aVal - bVal) * order;
    });
  }

  return result;
});

const paginatedQueues = computed(() => {
  const start = (currentPage.value - 1) * pageSize.value;
  const end = start + pageSize.value;
  return filteredQueues.value.slice(start, end);
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
          placeholder="Search queues..."
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
      <span class="text-base-content/40 text-xs">{{ filteredQueues.length }} queues</span>
    </div>

    <!-- Table -->
    <div class="min-h-0 flex-1 overflow-auto">
      <table class="table w-full">
        <thead class="bg-base-100 sticky top-0">
          <tr class="border-base-200 dark:border-base-content/10 border-b">
            <th class="text-base-content/60 text-xs font-medium">Queue Name</th>
            <th class="text-base-content/60 w-0 text-xs font-medium whitespace-nowrap">Auto Delete</th>
            <th class="text-base-content/60 w-0 text-xs font-medium whitespace-nowrap">Max Delivery</th>
            <th
              class="text-base-content/60 w-0 cursor-pointer text-xs font-medium whitespace-nowrap"
              @click="toggleSort('ready')"
            >
              Ready
              <span v-if="sortField === 'ready'" class="text-primary">{{ sortOrder === "asc" ? "↑" : "↓" }}</span>
            </th>
            <th
              class="text-base-content/60 w-0 cursor-pointer text-xs font-medium whitespace-nowrap"
              @click="toggleSort('errored')"
            >
              Errored
              <span v-if="sortField === 'errored'" class="text-primary">{{ sortOrder === "asc" ? "↑" : "↓" }}</span>
            </th>
            <th
              class="text-base-content/60 w-0 cursor-pointer text-xs font-medium whitespace-nowrap"
              @click="toggleSort('deadLettered')"
            >
              Dead Lettered
              <span v-if="sortField === 'deadLettered'" class="text-primary">{{
                sortOrder === "asc" ? "↑" : "↓"
              }}</span>
            </th>
            <th
              class="text-base-content/60 w-0 cursor-pointer text-xs font-medium whitespace-nowrap"
              @click="toggleSort('scheduled')"
            >
              Scheduled
              <span v-if="sortField === 'scheduled'" class="text-primary">{{ sortOrder === "asc" ? "↑" : "↓" }}</span>
            </th>
            <th
              class="text-base-content/60 w-0 cursor-pointer text-xs font-medium whitespace-nowrap"
              @click="toggleSort('locked')"
            >
              Locked
              <span v-if="sortField === 'locked'" class="text-primary">{{ sortOrder === "asc" ? "↑" : "↓" }}</span>
            </th>
          </tr>
        </thead>
        <tbody>
          <tr
            v-for="queue in paginatedQueues"
            :key="queue.queueName"
            class="hover:bg-base-200/50 border-base-200 dark:border-base-content/5 cursor-pointer border-b transition-colors"
            @click="selectQueue(queue)"
          >
            <td class="text-base-content max-w-xs truncate py-2.5 text-sm font-medium">{{ queue.queueName }}</td>
            <td class="text-base-content/60 py-2.5 text-sm">
              {{ queue.queueAutoDelete ? `${queue.queueAutoDelete / 60}m` : "-" }}
            </td>
            <td class="text-base-content/60 py-2.5 text-sm">{{ queue.queueMaxDeliveryCount }}</td>
            <td class="py-2.5 text-sm">
              <span :class="queue.ready > 0 ? 'text-base-content font-medium' : 'text-base-content/40'">{{
                queue.ready
              }}</span>
            </td>
            <td class="py-2.5 text-sm">
              <span :class="queue.errored > 0 ? 'text-warning font-medium' : 'text-base-content/40'">{{
                queue.errored
              }}</span>
            </td>
            <td class="py-2.5 text-sm">
              <span :class="queue.deadLettered > 0 ? 'text-error font-medium' : 'text-base-content/40'">{{
                queue.deadLettered
              }}</span>
            </td>
            <td class="py-2.5 text-sm">
              <span :class="queue.scheduled > 0 ? 'text-info font-medium' : 'text-base-content/40'">{{
                queue.scheduled
              }}</span>
            </td>
            <td class="py-2.5 text-sm">
              <span :class="queue.locked > 0 ? 'text-base-content font-medium' : 'text-base-content/40'">{{
                queue.locked
              }}</span>
            </td>
          </tr>
          <tr v-if="paginatedQueues.length === 0">
            <td colspan="8" class="text-base-content/40 py-12 text-center text-sm">No queues found</td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- Pagination -->
    <div v-if="filteredQueues.length > pageSize" class="border-base-200 dark:border-base-content/10 shrink-0 border-t">
      <Pagination
        :total-items="filteredQueues.length"
        v-model:current-page="currentPage"
        v-model:page-size="pageSize"
      />
    </div>
  </div>
</template>
