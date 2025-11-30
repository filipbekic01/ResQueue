<script setup lang="ts">
import { computed, ref } from "vue";
import { useRoute, useRouter } from "vue-router";
import { useAuthQuery } from "@/api/auth/authQuery";
import mtLogoUrlDark from "@/assets/images/masstransit-dark.svg";
import mtLogoUrl from "@/assets/images/masstransit.svg";
import ChartBarIcon from "@/components/icons/ChartBarIcon.vue";
import ComputerIcon from "@/components/icons/ComputerIcon.vue";
import MoonIcon from "@/components/icons/MoonIcon.vue";
import RefreshIcon from "@/components/icons/RefreshIcon.vue";
import SunIcon from "@/components/icons/SunIcon.vue";
import { useLocalSettings } from "@/composables/useLocalSettings";
import { useTheme } from "@/composables/useTheme";

const route = useRoute();
const router = useRouter();

const { isSuccess, isPending, error } = useAuthQuery();

const capitalize = (value: string = "") => value.replace(/\b\w/g, (char) => char.toUpperCase());

const { showGraph, refetchInterval, toggleGraph, setRefetchInterval } = useLocalSettings();
const { currentTheme, cycleTheme } = useTheme();

const autoRefreshPopoverOpen = ref(false);
const refetchIntervalOptions = [
  { label: "Off", value: 0 },
  { label: "1s", value: 1000 },
  { label: "5s", value: 1000 * 5 },
  { label: "30s", value: 1000 * 30 },
  { label: "1m", value: 1000 * 60 },
  { label: "5m", value: 1000 * 60 * 5 },
];

const onRefreshIntervalChange = (interval: number) => {
  setRefetchInterval(interval);
  autoRefreshPopoverOpen.value = false;
};

interface BreadcrumbItem {
  label: string;
  command?: () => void;
}

const items = computed((): BreadcrumbItem[] => {
  const items: BreadcrumbItem[] = [];

  if (route.name === "messages") {
    items.push({
      label: "Queues",
      command: () => {
        router.push({ name: "queues" });
      },
    });

    items.push({
      label: capitalize(route.params["queueName"]?.toString()),
    });
  } else {
    items.push({
      label: capitalize(route.name?.toString()),
    });
  }

  return items;
});

const autoRefreshLabel = computed(() => {
  const option = refetchIntervalOptions.find((x) => x.value === refetchInterval.value);
  return option?.value === 0 ? "" : option?.label;
});

const isAutoRefreshActive = computed(() => refetchInterval.value > 0);

const isMessagesPage = computed(() => route.name === "messages");
const shouldShowGraph = computed(() => isMessagesPage.value && showGraph.value);
</script>

<template>
  <div v-if="!isPending && isSuccess" class="bg-base-100 flex h-screen w-full flex-col">
    <!-- Header -->
    <header class="border-base-200 dark:border-base-content/10 flex h-14 shrink-0 items-center border-b px-4">
      <!-- Logo & Brand -->
      <div class="flex items-center gap-3">
        <div class="flex h-8 w-8 items-center justify-center">
          <img :src="mtLogoUrl" class="h-full w-full object-contain dark:hidden" alt="MassTransit" />
          <img :src="mtLogoUrlDark" class="hidden h-full w-full object-contain dark:block" alt="MassTransit" />
        </div>
        <div class="flex items-center gap-2">
          <span class="text-base-content text-lg font-semibold tracking-tight">MassTransit</span>
          <span class="text-base-content/30">·</span>
          <!-- Breadcrumb -->
          <nav class="text-base-content/60 flex items-center gap-1 text-sm">
            <template v-for="(item, index) in items" :key="index">
              <span v-if="index > 0" class="text-base-content/30">/</span>
              <a
                v-if="item.command"
                @click="item.command"
                class="hover:text-base-content cursor-pointer transition-colors"
              >
                {{ item.label }}
              </a>
              <span v-else class="text-base-content">{{ item.label }}</span>
            </template>
          </nav>
        </div>
      </div>

      <!-- Right side controls -->
      <div class="ms-auto flex items-center gap-1">
        <!-- Graph Toggle - Only show on messages page -->
        <button
          v-if="isMessagesPage"
          class="btn btn-sm gap-1.5"
          :class="{ 'btn-ghost': !showGraph }"
          @click="toggleGraph"
          :title="showGraph ? 'Hide metrics graph' : 'Show metrics graph'"
        >
          <ChartBarIcon class="h-4 w-4" />
          <span class="text-xs font-medium">Metrics</span>
        </button>

        <!-- Auto Refresh Dropdown -->
        <div class="relative">
          <button
            class="btn btn-ghost btn-sm gap-1.5"
            :class="{ 'text-success': isAutoRefreshActive }"
            @click="autoRefreshPopoverOpen = !autoRefreshPopoverOpen"
            title="Auto refresh"
          >
            <RefreshIcon class="h-4 w-4" :class="{ 'animate-spin': isAutoRefreshActive }" />
            <span v-if="autoRefreshLabel" class="text-xs font-medium">{{ autoRefreshLabel }}</span>
          </button>
          <div
            v-if="autoRefreshPopoverOpen"
            class="bg-base-100 border-base-200 dark:border-base-content/10 absolute right-0 z-50 mt-2 min-w-48 rounded-lg border p-1 shadow-lg"
          >
            <div class="text-base-content/50 px-3 py-2 text-xs font-medium tracking-wider uppercase">Auto Refresh</div>
            <button
              v-for="option in refetchIntervalOptions"
              :key="option.value"
              class="hover:bg-base-200 flex w-full items-center justify-between rounded-md px-3 py-2 text-left text-sm transition-colors"
              :class="{ 'bg-base-200 text-success': refetchInterval === option.value }"
              @click="onRefreshIntervalChange(option.value)"
            >
              <span>{{ option.label }}</span>
              <span v-if="refetchInterval === option.value" class="text-success">✓</span>
            </button>
          </div>
          <div v-if="autoRefreshPopoverOpen" class="fixed inset-0 z-40" @click="autoRefreshPopoverOpen = false"></div>
        </div>

        <!-- Theme Toggle -->
        <button class="btn btn-ghost btn-sm btn-square" @click="cycleTheme" :title="`Theme: ${currentTheme}`">
          <SunIcon v-if="currentTheme === 'light'" class="h-4 w-4 opacity-60" />
          <MoonIcon v-else-if="currentTheme === 'dark'" class="h-4 w-4 opacity-60" />
          <ComputerIcon v-else class="h-4 w-4 opacity-60" />
        </button>
      </div>
    </header>

    <!-- Main Content Area -->
    <div class="flex min-h-0 flex-1 flex-col">
      <!-- Main Content -->
      <div class="flex min-h-0 min-w-0 flex-1 flex-col">
        <slot name="menu"></slot>
        <div class="flex min-h-0 flex-1 flex-col overflow-hidden">
          <slot></slot>
        </div>
      </div>

      <!-- Bottom Panel (Graph) - Only show on messages page -->
      <aside v-if="shouldShowGraph" class="border-base-200 dark:border-base-content/10 h-48 shrink-0 border-t">
        <slot name="bottom"></slot>
      </aside>
    </div>
  </div>
  <div v-else-if="!isPending && !isSuccess" class="flex h-screen items-center justify-center">
    <div class="text-error">{{ error?.message }}</div>
  </div>
</template>
