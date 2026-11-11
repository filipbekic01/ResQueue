<template>
  <div class="flex h-full flex-row">
    <!-- Header (vertical on the left) -->
    <div
      class="border-base-200 dark:border-base-content/10 flex w-40 shrink-0 flex-col justify-center gap-3 border-r px-4 py-2"
    >
      <div class="flex flex-col">
        <span class="text-base-content text-sm font-medium">Queue Depth</span>
        <span class="text-base-content/50 text-xs">Last 10 minutes</span>
      </div>
      <!-- Legend -->
      <div class="flex flex-col gap-1">
        <div v-for="series in seriesList" :key="series.key" class="flex items-center gap-1.5">
          <span class="h-2 w-2 rounded-full" :class="series.dotClass"></span>
          <span class="text-base-content/60 text-xs">{{ series.label }}</span>
        </div>
      </div>
    </div>

    <!-- Graph Area -->
    <div ref="graphContainer" class="relative flex min-w-0 flex-1 items-center justify-center overflow-hidden p-4">
      <div
        v-if="depthHistory && !depthHistory.enabled"
        class="text-base-content/50 absolute inset-0 flex items-center justify-center text-xs"
      >
        Queue depth history is disabled
      </div>
      <svg
        v-else
        :width="graphWidth"
        :height="graphHeight"
        :viewBox="`0 0 ${graphWidth} ${graphHeight}`"
        preserveAspectRatio="xMidYMid meet"
        class="h-full max-h-full w-full cursor-default select-none"
      >
        <!-- Grid lines -->
        <g class="text-base-content/10">
          <line
            v-for="(tick, idx) in tickValues"
            :key="'grid-' + idx"
            :x1="padding"
            :y1="getYForValue(tick)"
            :x2="graphWidth - padding"
            :y2="getYForValue(tick)"
            stroke="currentColor"
            stroke-dasharray="2,2"
          />
        </g>

        <!-- Y-axis labels -->
        <g v-for="(tick, idx) in tickValues" :key="'tick-' + idx">
          <text
            :x="padding - 8"
            :y="getYForValue(tick)"
            text-anchor="end"
            alignment-baseline="middle"
            font-size="10"
            class="fill-base-content/40"
          >
            {{ Math.round(tick) }}
          </text>
        </g>

        <!-- Area fill (subtle) for ready -->
        <path :d="areaPathReady" class="fill-sky-500/10" />

        <!-- Line graph paths (broken where there is no sample) -->
        <path
          v-for="series in seriesList"
          :key="'line-' + series.key"
          :d="linePath(series.key)"
          :class="series.strokeClass"
          fill="none"
          stroke-width="2"
          stroke-linecap="round"
          stroke-linejoin="round"
        />

        <!-- For each data point, render the hover rectangle, circles and label -->
        <g v-for="(point, index) in graphData" :key="index">
          <!-- Invisible hover area rectangle -->
          <rect
            :x="getRectBoundaries(index).x"
            y="0"
            :width="getRectBoundaries(index).width"
            :height="graphHeight"
            :fill="hoveredIndex === index ? 'currentColor' : 'transparent'"
            :class="hoveredIndex === index ? 'text-base-content/5' : ''"
            style="cursor: pointer"
            @mouseenter="(event) => handleMouseEnter(event, point, index)"
            @mouseleave="handleMouseLeave"
          />
          <!-- Data point circles -->
          <template v-for="series in seriesList" :key="'circle-' + series.key">
            <circle
              v-if="point[series.key] !== null"
              :cx="getX(index)"
              :cy="getYForValue(point[series.key] ?? 0)"
              :r="hoveredIndex === index ? 4 : 3"
              :class="series.fillClass"
            />
          </template>
          <!-- Time label (x-axis) -->
          <text
            :x="getX(index)"
            :y="graphHeight - 4"
            class="fill-base-content/40 pointer-events-none"
            text-anchor="middle"
            font-size="10"
          >
            {{ point.time }}
          </text>
        </g>
      </svg>
    </div>

    <!-- Tooltip -->
    <div
      v-if="hoveredPoint"
      class="bg-base-300 dark:bg-base-content pointer-events-none z-50 rounded-lg px-3 py-2 text-sm shadow-lg dark:text-black"
      :style="{
        position: 'fixed',
        left: `${tooltipPosition.x + 12}px`,
        top: `${tooltipPosition.y - 60}px`,
      }"
    >
      <div class="flex flex-col gap-1">
        <template v-if="hoveredPoint.readyCount !== null">
          <div
            v-for="series in seriesList"
            :key="'tooltip-' + series.key"
            class="flex items-center justify-between gap-4"
          >
            <span class="flex items-center gap-1.5">
              <span class="h-2 w-2 rounded-full" :class="series.tooltipDotClass"></span>
              {{ series.label }}
            </span>
            <span class="font-medium">{{ hoveredPoint[series.key] }}</span>
          </div>
        </template>
        <div v-else class="text-base-content/60 dark:text-base-100/60">No sample</div>
        <div class="text-base-content/50 dark:text-base-100/50 mt-1 border-t border-current/10 pt-1 text-xs">
          {{ hoveredPoint.time }}
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { format } from "date-fns";
import { computed, onMounted, onUnmounted, ref } from "vue";
import { useQueueDepthMetricsQuery } from "@/api/queues/queueDepthMetricsQuery";
import { useLocalSettings } from "@/composables/useLocalSettings";
import type { QueueDto } from "@/dtos/queue/queueDto";

interface DepthDataPoint {
  time: string;
  readyCount: number | null;
  errorCount: number | null;
  deadLetterCount: number | null;
}

type SeriesKey = "readyCount" | "errorCount" | "deadLetterCount";

const seriesList: {
  key: SeriesKey;
  label: string;
  dotClass: string;
  tooltipDotClass: string;
  strokeClass: string;
  fillClass: string;
}[] = [
  {
    key: "readyCount",
    label: "Ready",
    dotClass: "bg-sky-500",
    tooltipDotClass: "bg-sky-500",
    strokeClass: "stroke-sky-500",
    fillClass: "fill-sky-500",
  },
  {
    key: "errorCount",
    label: "Errors",
    dotClass: "bg-red-500",
    tooltipDotClass: "bg-red-500",
    strokeClass: "stroke-red-500",
    fillClass: "fill-red-500",
  },
  {
    key: "deadLetterCount",
    label: "Dead",
    dotClass: "bg-base-content/40",
    tooltipDotClass: "bg-base-content/40 dark:bg-base-100/40",
    strokeClass: "stroke-base-content/40",
    fillClass: "fill-base-content/40",
  },
];

const { refetchInterval } = useLocalSettings();

// === Tooltip & Hover ===
const hoveredPoint = ref<DepthDataPoint | null>(null);
const hoveredIndex = ref<number | null>(null);
const tooltipPosition = ref({ x: 0, y: 0 });

const handleMouseEnter = (event: MouseEvent, point: DepthDataPoint, index: number) => {
  hoveredPoint.value = point;
  hoveredIndex.value = index;
  tooltipPosition.value = { x: event.clientX - 125, y: event.clientY };
};

const handleMouseLeave = () => {
  hoveredPoint.value = null;
  hoveredIndex.value = null;
};

// === Props & Data ===
const props = defineProps<{
  queue: QueueDto;
}>();

const { data: depthHistory } = useQueueDepthMetricsQuery(
  computed(() => props.queue.id),
  refetchInterval,
);

// Re-evaluate the timeline periodically so it keeps moving even without new data.
const timer = ref(Date.now());
const interval = setInterval(() => {
  timer.value = Date.now();
}, 5000);

onUnmounted(() => {
  clearInterval(interval);
});

// === Graph Data ===
// Same per-minute timeline as the Queue Metrics graph. Depth is a snapshot, so for each minute we take
// the latest sample captured within that minute; minutes without a sample stay empty (not zero).
const graphData = computed<DepthDataPoint[]>(() => {
  const latestByMinute = new Map<
    string,
    { capturedAt: number; readyCount: number; errorCount: number; deadLetterCount: number }
  >();
  for (const item of depthHistory.value?.items ?? []) {
    const capturedAt = new Date(item.capturedAt).getTime();
    const key = format(capturedAt, "yyyy-MM-dd|HH:mm");
    const existing = latestByMinute.get(key);
    if (!existing || existing.capturedAt < capturedAt) {
      latestByMinute.set(key, {
        capturedAt,
        readyCount: item.readyCount,
        errorCount: item.errorCount,
        deadLetterCount: item.deadLetterCount,
      });
    }
  }

  const data: DepthDataPoint[] = [];
  // Depends on the timer so the timeline keeps moving even without new data.
  const now = new Date(timer.value);

  for (let i = 0; i < 10; i++) {
    const pastTime = new Date(now.getTime() - i * 60000);
    const sample = latestByMinute.get(format(pastTime, "yyyy-MM-dd|HH:mm"));

    data.push({
      time: format(pastTime, "hh:mm"),
      readyCount: sample?.readyCount ?? null,
      errorCount: sample?.errorCount ?? null,
      deadLetterCount: sample?.deadLetterCount ?? null,
    });
  }

  return data.reverse();
});

// === Responsive Graph Configuration ===
const graphContainer = ref<HTMLElement | null>(null);
const containerWidth = ref(800);
const containerHeight = ref(140);

const updateDimensions = () => {
  if (graphContainer.value) {
    containerWidth.value = graphContainer.value.clientWidth - 32;
    containerHeight.value = graphContainer.value.clientHeight - 32;
  }
};

let resizeObserver: ResizeObserver | null = null;

onMounted(() => {
  updateDimensions();
  if (graphContainer.value) {
    resizeObserver = new ResizeObserver(updateDimensions);
    resizeObserver.observe(graphContainer.value);
  }
});

onUnmounted(() => {
  resizeObserver?.disconnect();
});

const graphWidth = computed(() => Math.max(containerWidth.value, 200));
const graphHeight = computed(() => Math.max(containerHeight.value, 100));
const padding = 30;
const innerWidth = computed(() => graphWidth.value - 2 * padding);
const innerHeight = computed(() => graphHeight.value - 2 * padding);

const maxValue = computed(() => {
  const values = graphData.value.flatMap((d) => seriesList.map((s) => d[s.key] ?? 0));
  const maxVal = Math.max(0, ...values);
  return maxVal === 0 ? 1 : maxVal;
});

const pointSpacing = computed(() =>
  graphData.value.length > 1 ? innerWidth.value / (graphData.value.length - 1) : innerWidth.value,
);

const getX = (index: number): number => padding + index * pointSpacing.value;

const getYForValue = (value: number): number =>
  graphHeight.value - padding - (value / maxValue.value) * innerHeight.value;

const tickValues = computed(() => [maxValue.value, maxValue.value / 2, 0]);

// Splits a series into contiguous runs of points that have samples.
const segments = (key: SeriesKey): { x: number; y: number }[][] => {
  const result: { x: number; y: number }[][] = [];
  let current: { x: number; y: number }[] = [];

  graphData.value.forEach((point, index) => {
    const value = point[key];
    if (value === null) {
      if (current.length) result.push(current);
      current = [];
    } else {
      current.push({ x: getX(index), y: getYForValue(value) });
    }
  });

  if (current.length) result.push(current);
  return result;
};

const linePath = (key: SeriesKey): string =>
  segments(key)
    .map((segment) => segment.map((p, i) => `${i === 0 ? "M" : "L"} ${p.x} ${p.y}`).join(" "))
    .join(" ");

const areaPathReady = computed(() => {
  const baseY = graphHeight.value - padding;
  return segments("readyCount")
    .filter((segment) => segment.length > 1)
    .map((segment) => {
      const line = segment.map((p, i) => `${i === 0 ? "M" : "L"} ${p.x} ${p.y}`).join(" ");
      const first = segment[0]!;
      const last = segment[segment.length - 1]!;
      return `${line} L ${last.x} ${baseY} L ${first.x} ${baseY} Z`;
    })
    .join(" ");
});

// Hover rectangle boundaries (midpoints between neighbouring points).
const getRectBoundaries = (index: number): { x: number; width: number } => {
  const n = graphData.value.length;
  const center = getX(index);
  const left = index === 0 ? padding : (getX(index - 1) + center) / 2;
  const right = index === n - 1 ? getX(n - 1) : (center + getX(index + 1)) / 2;
  return { x: left, width: n > 1 ? right - left : innerWidth.value };
};
</script>
