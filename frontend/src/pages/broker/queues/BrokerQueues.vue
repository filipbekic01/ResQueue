<script lang="ts" setup>
import { computed } from "vue";
import { useRoute, useRouter } from "vue-router";
import { useQueuesViewQuery } from "@/api/queues/queuesViewQuery";
import DatabaseIcon from "@/components/icons/DatabaseIcon.vue";
import Pagination from "@/components/Pagination.vue";
import { useLocalSettings } from "@/composables/useLocalSettings";
import type { QueueViewDto } from "@/dtos/queue/queueViewDto";

const route = useRoute();
const router = useRouter();

const { refetchInterval } = useLocalSettings();

const { data } = useQueuesViewQuery(refetchInterval);
const queuesView = computed(() => data.value ?? []);

const selectQueue = (queue: QueueViewDto, queueType?: number) => {
  router.push({
    name: "messages",
    params: {
      queueName: queue.queueName,
    },
    query: queueType ? { queueType: queueType.toString() } : undefined,
  });
};

// Search from URL
const search = computed({
  get: () => (route.query.search as string) ?? "",
  set: (value: string) => {
    router.replace({ query: { ...route.query, search: value || undefined } });
  },
});

// Sorting from URL
type SortField = "ready" | "errored" | "deadLettered" | "scheduled" | null;

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
  const start = (currentPage.value - 1) * pageSize;
  const end = start + pageSize;
  return filteredQueues.value.slice(start, end);
});

const getPerMinuteRate = (count: number, durationSeconds: number): string => {
  if (durationSeconds <= 0) return "-";
  const rate = (count / durationSeconds) * 60;
  if (rate === 0) return "0";
  if (rate < 0.1) return "<0.1";
  return rate.toFixed(1);
};
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
            <th
              class="bg-info/5 text-base-content/60 border-l-info/30 w-0 cursor-pointer border-l-2 text-xs font-medium whitespace-nowrap"
              @click="toggleSort('scheduled')"
            >
              Scheduled
              <span v-if="sortField === 'scheduled'" class="text-base-content">{{
                sortOrder === "asc" ? "↑" : "↓"
              }}</span>
            </th>
            <th
              class="bg-success/5 text-base-content/60 border-l-success/30 w-0 cursor-pointer border-l-2 text-xs font-medium whitespace-nowrap"
              @click="toggleSort('ready')"
            >
              Ready
              <span v-if="sortField === 'ready'" class="text-base-content">{{ sortOrder === "asc" ? "↑" : "↓" }}</span>
            </th>
            <th
              class="bg-success/3 text-base-content/60 border-l-success/30 w-0 border-l border-dashed text-xs font-medium whitespace-nowrap"
            >
              Consumed/min
            </th>
            <th
              class="bg-error/5 text-base-content/60 border-l-error/30 w-0 cursor-pointer border-l-2 text-xs font-medium whitespace-nowrap"
              @click="toggleSort('errored')"
            >
              Errored
              <span v-if="sortField === 'errored'" class="text-base-content">{{
                sortOrder === "asc" ? "↑" : "↓"
              }}</span>
            </th>
            <th
              class="bg-error/3 text-base-content/60 border-l-error/30 w-0 border-l border-dashed text-xs font-medium whitespace-nowrap"
            >
              Errored/min
            </th>
            <th
              class="bg-base-content/5 text-base-content/60 border-l-base-content/20 w-0 cursor-pointer border-l-2 text-xs font-medium whitespace-nowrap"
              @click="toggleSort('deadLettered')"
            >
              Dead Lettered
              <span v-if="sortField === 'deadLettered'" class="text-base-content">{{
                sortOrder === "asc" ? "↑" : "↓"
              }}</span>
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
            <td class="bg-info/5 border-l-info/30 border-l-2 py-2.5 text-sm" @click.stop="selectQueue(queue, 1)">
              <span :class="queue.scheduled > 0 ? 'text-info font-medium' : 'text-base-content/40'">{{
                queue.scheduled
              }}</span>
            </td>
            <td class="bg-success/5 border-l-success/30 border-l-2 py-2.5 text-sm" @click.stop="selectQueue(queue, 1)">
              <span :class="queue.ready > 0 ? 'text-success font-medium' : 'text-base-content/40'">{{
                queue.ready
              }}</span>
            </td>
            <td
              class="bg-success/3 border-l-success/30 border-l border-dashed py-2.5 text-sm"
              @click.stop="selectQueue(queue, 1)"
            >
              <span :class="queue.consumeCount > 0 ? 'text-success font-medium' : 'text-base-content/40'">
                {{ getPerMinuteRate(queue.consumeCount, queue.countDuration) }}
              </span>
            </td>
            <td class="bg-error/5 border-l-error/30 border-l-2 py-2.5 text-sm" @click.stop="selectQueue(queue, 2)">
              <span :class="queue.errored > 0 ? 'text-error font-medium' : 'text-base-content/40'">{{
                queue.errored
              }}</span>
            </td>
            <td
              class="bg-error/3 border-l-error/30 border-l border-dashed py-2.5 text-sm"
              @click.stop="selectQueue(queue, 2)"
            >
              <span :class="queue.errorCount > 0 ? 'text-error font-medium' : 'text-base-content/40'">
                {{ getPerMinuteRate(queue.errorCount, queue.countDuration) }}
              </span>
            </td>
            <td
              class="bg-base-content/5 border-l-base-content/20 border-l-2 py-2.5 text-sm"
              @click.stop="selectQueue(queue, 3)"
            >
              <span :class="queue.deadLettered > 0 ? 'text-base-content font-medium' : 'text-base-content/40'">{{
                queue.deadLettered
              }}</span>
            </td>
          </tr>
        </tbody>
      </table>

      <!-- Empty State -->
      <div v-if="paginatedQueues.length === 0" class="flex flex-1 flex-col items-center justify-center gap-4 p-8">
        <div class="bg-base-200 flex h-16 w-16 items-center justify-center rounded-full">
          <DatabaseIcon class="text-base-content/30 h-8 w-8" />
        </div>
        <div class="text-center">
          <h3 class="text-base-content text-lg font-medium">No queues found</h3>
          <p class="text-base-content/50 mt-1 text-sm">
            {{ search ? "Try adjusting your search terms." : "Queues will appear here when they are created." }}
          </p>
        </div>
      </div>
    </div>

    <!-- Pagination -->
    <div v-if="filteredQueues.length > pageSize" class="border-base-200 dark:border-base-content/10 shrink-0 border-t">
      <Pagination :total-items="filteredQueues.length" v-model:current-page="currentPage" :page-size="pageSize" />
    </div>
  </div>
</template>
