<script lang="ts" setup>
import { computed } from "vue";

const props = withDefaults(
  defineProps<{
    totalItems: number;
    currentPage: number;
    pageSize: number;
    pageSizeOptions?: number[];
    showPageSizeSelector?: boolean;
  }>(),
  {
    pageSizeOptions: () => [10, 20, 50, 100],
    showPageSizeSelector: true,
  },
);

const emit = defineEmits<{
  (e: "update:currentPage", page: number): void;
  (e: "update:pageSize", size: number): void;
}>();

const totalPages = computed(() => Math.ceil(props.totalItems / props.pageSize));

const startItem = computed(() => {
  if (props.totalItems === 0) return 0;
  return (props.currentPage - 1) * props.pageSize + 1;
});

const endItem = computed(() => {
  return Math.min(props.currentPage * props.pageSize, props.totalItems);
});

const visiblePages = computed(() => {
  const pages: (number | string)[] = [];
  const total = totalPages.value;
  const current = props.currentPage;

  if (total <= 7) {
    for (let i = 1; i <= total; i++) {
      pages.push(i);
    }
  } else {
    pages.push(1);

    if (current > 3) {
      pages.push("...");
    }

    const start = Math.max(2, current - 1);
    const end = Math.min(total - 1, current + 1);

    for (let i = start; i <= end; i++) {
      pages.push(i);
    }

    if (current < total - 2) {
      pages.push("...");
    }

    pages.push(total);
  }

  return pages;
});

const goToPage = (page: number) => {
  if (page >= 1 && page <= totalPages.value) {
    emit("update:currentPage", page);
  }
};

const onPageSizeChange = (event: Event) => {
  const target = event.target as HTMLSelectElement;
  emit("update:pageSize", Number(target.value));
  emit("update:currentPage", 1);
};
</script>

<template>
  <div class="flex flex-wrap items-center justify-between gap-4 px-2 py-3">
    <!-- Page size selector -->
    <div v-if="showPageSizeSelector" class="flex items-center gap-2">
      <span class="text-base-content/70 text-sm">Rows per page:</span>
      <select class="select select-bordered select-sm" :value="pageSize" @change="onPageSizeChange">
        <option v-for="option in pageSizeOptions" :key="option" :value="option">
          {{ option }}
        </option>
      </select>
    </div>

    <!-- Info text -->
    <div class="text-base-content/70 text-sm">Showing {{ startItem }} to {{ endItem }} of {{ totalItems }} entries</div>

    <!-- Pagination buttons -->
    <div class="join">
      <button class="btn btn-sm join-item" :disabled="currentPage === 1" @click="goToPage(currentPage - 1)">«</button>

      <template v-for="page in visiblePages" :key="page">
        <button v-if="page === '...'" class="btn btn-sm join-item btn-disabled">...</button>
        <button
          v-else
          class="btn btn-sm join-item"
          :class="{ 'btn-active': page === currentPage }"
          @click="goToPage(page as number)"
        >
          {{ page }}
        </button>
      </template>

      <button
        class="btn btn-sm join-item"
        :disabled="currentPage === totalPages || totalPages === 0"
        @click="goToPage(currentPage + 1)"
      >
        »
      </button>
    </div>
  </div>
</template>
