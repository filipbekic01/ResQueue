<script setup lang="ts">
import { computed, ref } from "vue";
import { useRoute, useRouter } from "vue-router";
import { useAuthQuery } from "@/api/auth/authQuery";
import mtLogoUrlDark from "@/assets/images/masstransit-dark.svg";
import mtLogoUrl from "@/assets/images/masstransit.svg";
import ChevronLeftIcon from "@/components/icons/ChevronLeftIcon.vue";
import ChevronRightIcon from "@/components/icons/ChevronRightIcon.vue";
import ComputerIcon from "@/components/icons/ComputerIcon.vue";
import HourglassIcon from "@/components/icons/HourglassIcon.vue";
import MoonIcon from "@/components/icons/MoonIcon.vue";
import SunIcon from "@/components/icons/SunIcon.vue";
import { useUserSettings } from "@/composables/userSettingsComposable";
import { useTheme } from "@/composables/useTheme";

const route = useRoute();
const router = useRouter();

const { isSuccess, isPending, error } = useAuthQuery();

const capitalize = (value: string = "") => value.replace(/\b\w/g, (char) => char.toUpperCase());

const { settings, updateSettings, toggleGraph } = useUserSettings();
const { currentTheme, cycleTheme } = useTheme();

const autoRefreshPopoverOpen = ref(false);
const refetchIntervalOptions = [
  {
    label: "Never",
    value: 0,
  },
  {
    label: "1s",
    value: 1000,
  },
  {
    label: "5s",
    value: 1000 * 5,
  },
  {
    label: "30s",
    value: 1000 * 30,
  },
  {
    label: "1m",
    value: 1000 * 60,
  },
  {
    label: "5m",
    value: 1000 * 60 * 5,
  },
  {
    label: "30m",
    value: 1000 * 60 * 30,
  },
  {
    label: "1h",
    value: 1000 * 60 * 60,
  },
];

const onRefreshIntervalChange = (interval: number) => {
  updateSettings({ ...settings, refetchInterval: interval });
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
  return `${refetchIntervalOptions.find((x) => x.value === settings.refetchInterval)?.label}`;
});
</script>

<template>
  <div v-if="!isPending && isSuccess" class="flex h-screen w-full flex-col">
    <!-- Auto Refresh Dropdown -->
    <div v-if="autoRefreshPopoverOpen" class="fixed inset-0 z-40" @click="autoRefreshPopoverOpen = false"></div>

    <div class="flex">
      <div class="flex grow flex-col">
        <div class="border-base-300 dark:border-base-content/20 flex items-center border-b px-4 pt-4 pb-4">
          <div class="flex">
            <div class="flex h-14 w-14 items-center justify-center rounded-xl text-2xl">
              <img :src="mtLogoUrl" class="w-full dark:hidden" />
              <img :src="mtLogoUrlDark" class="hidden w-full dark:block" />
            </div>

            <div class="ms-4 flex flex-col justify-center">
              <div class="text-primary text-2xl font-semibold">MassTransit</div>
              <div class="flex items-center gap-2">
                <!-- Breadcrumb -->
                <div class="breadcrumbs py-0 text-sm">
                  <ul>
                    <li v-for="(item, index) in items" :key="index">
                      <a v-if="item.command" @click="item.command" class="cursor-pointer">{{ item.label }}</a>
                      <span v-else>{{ item.label }}</span>
                    </li>
                  </ul>
                </div>
              </div>
            </div>
          </div>

          <div class="my-auto ms-auto me-3 flex items-center gap-1">
            <button class="btn btn-ghost btn-sm" @click="cycleTheme" :title="`Theme: ${currentTheme}`">
              <SunIcon v-if="currentTheme === 'light'" class="h-4 w-4" />
              <MoonIcon v-else-if="currentTheme === 'dark'" class="h-4 w-4" />
              <ComputerIcon v-else class="h-4 w-4" />
            </button>

            <!-- Auto Refresh Dropdown -->
            <div class="dropdown dropdown-end">
              <button
                tabindex="0"
                class="btn btn-ghost btn-sm"
                @click="autoRefreshPopoverOpen = !autoRefreshPopoverOpen"
              >
                <HourglassIcon class="h-4 w-4" />
                {{ autoRefreshLabel }}
              </button>
              <div
                v-if="autoRefreshPopoverOpen"
                tabindex="0"
                class="dropdown-content bg-base-100 z-50 w-72 rounded-lg p-4 shadow-xl"
              >
                <div class="flex flex-col gap-2">
                  <div class="text-base-content/70 text-sm">
                    Select an interval to automatically refresh the queues and messages view. We plan to integrate a
                    real-time, socket-based system for instant updates in a future release.
                  </div>
                  <ul class="menu bg-base-100 rounded-box w-full p-0">
                    <li v-for="option in refetchIntervalOptions" :key="option.value">
                      <a
                        :class="{ active: settings.refetchInterval === option.value }"
                        @click="onRefreshIntervalChange(option.value)"
                      >
                        {{ option.label }}
                      </a>
                    </li>
                  </ul>
                </div>
              </div>
            </div>

            <button class="btn btn-ghost btn-sm" @click="toggleGraph">
              <ChevronRightIcon v-if="settings.showGraph" class="h-4 w-4" />
              <ChevronLeftIcon v-else class="h-4 w-4" />
            </button>
          </div>
        </div>
        <slot name="menu"></slot>
      </div>
      <div class="flex">
        <slot name="right"></slot>
      </div>
    </div>

    <div class="flex grow flex-col overflow-auto">
      <slot></slot>
    </div>
  </div>
  <div v-else-if="!isPending && !isSuccess">{{ error?.message }}</div>
</template>
