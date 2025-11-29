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
  <div class="flex h-full flex-col">
    <!-- Search bar -->
    <div class="border-base-300 flex items-center gap-2 border-b px-4 py-2">
      <span class="font-semibold">Name</span>
      <div class="relative flex-1">
        <input
          v-model="search"
          type="text"
          placeholder="Search queues..."
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
            <th>Queue Name</th>
            <th class="w-0 whitespace-nowrap">Auto Delete</th>
            <th class="w-0 whitespace-nowrap">Max Delivery</th>
            <th class="w-0 cursor-pointer whitespace-nowrap" @click="toggleSort('ready')">
              Ready
              <span v-if="sortField === 'ready'">{{ sortOrder === "asc" ? "▲" : "▼" }}</span>
            </th>
            <th class="w-0 cursor-pointer whitespace-nowrap" @click="toggleSort('errored')">
              Errored
              <span v-if="sortField === 'errored'">{{ sortOrder === "asc" ? "▲" : "▼" }}</span>
            </th>
            <th class="w-0 cursor-pointer whitespace-nowrap" @click="toggleSort('deadLettered')">
              Dead Lettered
              <span v-if="sortField === 'deadLettered'">{{ sortOrder === "asc" ? "▲" : "▼" }}</span>
            </th>
            <th class="w-0 cursor-pointer whitespace-nowrap" @click="toggleSort('scheduled')">
              Scheduled
              <span v-if="sortField === 'scheduled'">{{ sortOrder === "asc" ? "▲" : "▼" }}</span>
            </th>
            <th class="w-0 cursor-pointer whitespace-nowrap" @click="toggleSort('locked')">
              Locked
              <span v-if="sortField === 'locked'">{{ sortOrder === "asc" ? "▲" : "▼" }}</span>
            </th>
          </tr>
        </thead>
        <tbody>
          <tr
            v-for="queue in paginatedQueues"
            :key="queue.queueName"
            class="hover cursor-pointer"
            @click="selectQueue(queue)"
          >
            <td class="max-w-xs truncate">{{ queue.queueName }}</td>
            <td>{{ queue.queueAutoDelete ? `${queue.queueAutoDelete / 60}m` : "-" }}</td>
            <td>{{ queue.queueMaxDeliveryCount }}</td>
            <td>{{ queue.ready }}</td>
            <td>{{ queue.errored }}</td>
            <td>{{ queue.deadLettered }}</td>
            <td>{{ queue.scheduled }}</td>
            <td>{{ queue.locked }}</td>
          </tr>
          <tr v-if="paginatedQueues.length === 0">
            <td colspan="8" class="text-base-content/50 text-center">No queues found</td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- Pagination -->
    <div class="border-base-300 border-t">
      <Pagination
        :total-items="filteredQueues.length"
        v-model:current-page="currentPage"
        v-model:page-size="pageSize"
      />
    </div>
  </div>
</template>
