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
  <div class="flex flex-wrap items-center justify-between gap-4 px-4 py-2.5">
    <!-- Page size selector -->
    <div v-if="showPageSizeSelector" class="flex items-center gap-2">
      <span class="text-base-content/50 text-xs">Rows per page</span>
      <select
        class="bg-base-200/50 border-base-200 focus:border-base-300 rounded-md border px-2 py-1 text-xs outline-none"
        :value="pageSize"
        @change="onPageSizeChange"
      >
        <option v-for="option in pageSizeOptions" :key="option" :value="option">
          {{ option }}
        </option>
      </select>
    </div>

    <!-- Info text -->
    <div class="text-base-content/50 text-xs">{{ startItem }}-{{ endItem }} of {{ totalItems }}</div>

    <!-- Pagination buttons -->
    <div class="flex items-center gap-1">
      <button
        class="hover:bg-base-200 text-base-content/60 hover:text-base-content flex h-7 w-7 items-center justify-center rounded-md text-sm transition-colors disabled:opacity-30 disabled:hover:bg-transparent"
        :disabled="currentPage === 1"
        @click="goToPage(currentPage - 1)"
      >
        ‹
      </button>

      <template v-for="page in visiblePages" :key="page">
        <span v-if="page === '...'" class="text-base-content/30 flex h-7 w-7 items-center justify-center text-xs"
          >...</span
        >
        <button
          v-else
          class="flex h-7 w-7 items-center justify-center rounded-md text-xs transition-colors"
          :class="{
            'bg-base-200 text-base-content font-medium': page === currentPage,
            'text-base-content/60 hover:bg-base-200 hover:text-base-content': page !== currentPage,
          }"
          @click="goToPage(page as number)"
        >
          {{ page }}
        </button>
      </template>

      <button
        class="hover:bg-base-200 text-base-content/60 hover:text-base-content flex h-7 w-7 items-center justify-center rounded-md text-sm transition-colors disabled:opacity-30 disabled:hover:bg-transparent"
        :disabled="currentPage === totalPages || totalPages === 0"
        @click="goToPage(currentPage + 1)"
      >
        ›
      </button>
    </div>
  </div>
</template>
