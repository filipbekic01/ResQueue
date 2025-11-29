<template>
  <div class="flex h-full flex-row">
    <!-- Header (now vertical on the left) -->
    <div class="border-base-200 dark:border-base-content/10 flex w-40 shrink-0 flex-col justify-center gap-3 border-r px-4 py-2">
      <div class="flex flex-col">
        <span class="text-base-content text-sm font-medium">Queue Metrics</span>
        <span class="text-base-content/50 text-xs">Last 10 minutes</span>
      </div>
      <!-- Legend -->
      <div class="flex flex-col gap-1">
        <div class="flex items-center gap-1.5">
          <span class="h-2 w-2 rounded-full bg-emerald-500"></span>
          <span class="text-base-content/60 text-xs">Consumed</span>
        </div>
        <div class="flex items-center gap-1.5">
          <span class="h-2 w-2 rounded-full bg-red-500"></span>
          <span class="text-base-content/60 text-xs">Errors</span>
        </div>
        <div class="flex items-center gap-1.5">
          <span class="bg-base-content/40 h-2 w-2 rounded-full"></span>
          <span class="text-base-content/60 text-xs">Dead</span>
        </div>
      </div>
    </div>

    <!-- Graph Area -->
    <div ref="graphContainer" class="flex min-w-0 flex-1 items-center justify-center overflow-hidden p-4">
      <svg :width="graphWidth" :height="graphHeight" :viewBox="`0 0 ${graphWidth} ${graphHeight}`" preserveAspectRatio="xMidYMid meet" class="h-full w-full max-h-full cursor-default select-none">
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

        <!-- Area fills (subtle) -->
        <path :d="areaPathConsume" class="fill-emerald-500/10" />
        <path :d="areaPathError" class="fill-red-500/10" />

        <!-- Line graph paths -->
        <path
          :d="linePathConsume"
          class="stroke-emerald-500"
          fill="none"
          stroke-width="2"
          stroke-linecap="round"
          stroke-linejoin="round"
        />
        <path
          :d="linePathError"
          class="stroke-red-500"
          fill="none"
          stroke-width="2"
          stroke-linecap="round"
          stroke-linejoin="round"
        />
        <path
          :d="linePathDeadLetter"
          class="stroke-base-content/40"
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
          <circle
            :cx="padding + index * pointSpacing"
            :cy="getY(point, point.consumeCount)"
            :r="hoveredIndex === index ? 4 : 3"
            class="fill-emerald-500"
          />
          <circle
            :cx="padding + index * pointSpacing"
            :cy="getY(point, point.errorCount)"
            :r="hoveredIndex === index ? 4 : 3"
            class="fill-red-500"
          />
          <circle
            :cx="padding + index * pointSpacing"
            :cy="getY(point, point.deadLetterCount)"
            :r="hoveredIndex === index ? 4 : 3"
            class="fill-base-content/40"
          />
          <!-- Time label (x-axis) -->
          <text
            :x="padding + index * pointSpacing"
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
        <div class="flex items-center justify-between gap-4">
          <span class="flex items-center gap-1.5">
            <span class="h-2 w-2 rounded-full bg-emerald-500"></span>
            Consumed
          </span>
          <span class="font-medium">{{ hoveredPoint.consumeCount }}</span>
        </div>
        <div class="flex items-center justify-between gap-4">
          <span class="flex items-center gap-1.5">
            <span class="h-2 w-2 rounded-full bg-red-500"></span>
            Errors
          </span>
          <span class="font-medium">{{ hoveredPoint.errorCount }}</span>
        </div>
        <div class="flex items-center justify-between gap-4">
          <span class="flex items-center gap-1.5">
            <span class="bg-base-content/40 dark:bg-base-100/40 h-2 w-2 rounded-full"></span>
            Dead Letter
          </span>
          <span class="font-medium">{{ hoveredPoint.deadLetterCount }}</span>
        </div>
        <div class="text-base-content/50 dark:text-base-100/50 mt-1 border-t border-current/10 pt-1 text-xs">
          {{ hoveredPoint.time }}
        </div>
      </div>
    </div>
  </div>
</template>

<script lang="ts">
export interface DataPoint {
  time: string;
  consumeCount: number;
  errorCount: number;
  deadLetterCount: number;
}
</script>

<script setup lang="ts">
import { format } from "date-fns";
import { computed, onMounted, onUnmounted, ref } from "vue";
import { useQueueMetricsQuery } from "@/api/queues/queueMetricsQuery";
import { useUserSettings } from "@/composables/userSettingsComposable";
import type { QueueDto } from "@/dtos/queue/queueDto";

const { settings } = useUserSettings();

// === Tooltip & Hover Reactive Variables ===
const hoveredPoint = ref<DataPoint | null>(null);
const hoveredIndex = ref<number | null>(null);
const tooltipPosition = ref({ x: 0, y: 0 });

const handleMouseEnter = (event: MouseEvent, point: DataPoint, index?: number) => {
  hoveredPoint.value = point;
  if (typeof index === "number") {
    hoveredIndex.value = index;
  }
  tooltipPosition.value = { x: event.clientX - 125, y: event.clientY };
};

const handleMouseLeave = () => {
  hoveredPoint.value = null;
  hoveredIndex.value = null;
};

// === Props & Metrics Data ===
const props = defineProps<{
  queue: QueueDto;
}>();

const { data: metrics } = useQueueMetricsQuery(
  computed(() => props.queue.id),
  computed(() => settings.refetchInterval),
);

// --- Add a timer to force updates every minute ---
// Create a reactive variable that updates every minute.
const timer = ref(Date.now());
const interval = setInterval(() => {
  timer.value = Date.now();
}, 5000); // 60_000 ms = 1 minute

// Clean up the interval when the component is unmounted.
onUnmounted(() => {
  clearInterval(interval);
});

// === Graph Data Computation ===
// Now include the timer variable in your computed dependency so that it updates every minute.
const graphData = computed<DataPoint[]>(() => {
  // Access timer.value to establish a dependency.
  const _ = timer.value;

  const data: DataPoint[] = [];
  const now = new Date();

  for (let i = 0; i < 10; i++) {
    const pastTime = new Date(now.getTime() - i * 60000);
    const timeString = format(pastTime, "hh:mm");

    const minuteMetric = metrics.value?.find(
      (x) => format(x.startTime, "yyyy-MM-dd|HH:mm") === format(pastTime, "yyyy-MM-dd|HH:mm"),
    );

    data.push({
      time: timeString,
      consumeCount: minuteMetric?.consumeCount ?? 0,
      errorCount: minuteMetric?.errorCount ?? 0,
      deadLetterCount: minuteMetric?.deadLetterCount ?? 0,
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
    containerWidth.value = graphContainer.value.clientWidth - 32; // subtract padding
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
  if (resizeObserver) {
    resizeObserver.disconnect();
  }
});

const graphWidth = computed(() => Math.max(containerWidth.value, 200));
const graphHeight = computed(() => Math.max(containerHeight.value, 100));
const padding = 30;
const innerWidth = computed(() => graphWidth.value - 2 * padding);
const innerHeight = computed(() => graphHeight.value - 2 * padding);

// Compute the maximum messages value; if all values are zero, default to 1.
const maxMessages = computed(() => {
  const maxVal = Math.max(...graphData.value.map((d) => Math.max(d.consumeCount, d.errorCount, d.deadLetterCount)));
  return maxVal === 0 ? 1 : maxVal;
});

// Calculate horizontal spacing between points.
const pointSpacing = computed(() => {
  return graphData.value.length > 1 ? innerWidth.value / (graphData.value.length - 1) : innerWidth.value;
});

// Helper function: get y coordinate for a given data point count.
const getY = (point: DataPoint, count: number): number => {
  return graphHeight.value - padding - (count / maxMessages.value) * innerHeight.value;
};

// --- New Helper for Y-axis ticks ---
const getYForValue = (value: number): number => {
  return graphHeight.value - padding - (value / maxMessages.value) * innerHeight.value;
};

// Computed tick values for the y-axis.
const tickValues = computed(() => {
  const maxVal = maxMessages.value;
  return [maxVal, (1 / 2) * maxVal, 0];
});

// --- SVG Paths for the three lines ---
const linePathConsume = computed(() => {
  if (!graphData.value.length) return "";
  return graphData.value
    .map((point, index) => {
      const x = padding + index * pointSpacing.value;
      const y = getY(point, point.consumeCount);
      return index === 0 ? `M ${x} ${y}` : `L ${x} ${y}`;
    })
    .join(" ");
});

const linePathError = computed(() => {
  if (!graphData.value.length) return "";
  return graphData.value
    .map((point, index) => {
      const x = padding + index * pointSpacing.value;
      const y = getY(point, point.errorCount);
      return index === 0 ? `M ${x} ${y}` : `L ${x} ${y}`;
    })
    .join(" ");
});

const linePathDeadLetter = computed(() => {
  if (!graphData.value.length) return "";
  return graphData.value
    .map((point, index) => {
      const x = padding + index * pointSpacing.value;
      const y = getY(point, point.deadLetterCount);
      return index === 0 ? `M ${x} ${y}` : `L ${x} ${y}`;
    })
    .join(" ");
});

// --- SVG Area Paths for subtle fills ---
const areaPathConsume = computed(() => {
  if (!graphData.value.length) return "";
  const linePath = graphData.value
    .map((point, index) => {
      const x = padding + index * pointSpacing.value;
      const y = getY(point, point.consumeCount);
      return index === 0 ? `M ${x} ${y}` : `L ${x} ${y}`;
    })
    .join(" ");
  const lastX = padding + (graphData.value.length - 1) * pointSpacing.value;
  const baseY = graphHeight.value - padding;
  return `${linePath} L ${lastX} ${baseY} L ${padding} ${baseY} Z`;
});

const areaPathError = computed(() => {
  if (!graphData.value.length) return "";
  const linePath = graphData.value
    .map((point, index) => {
      const x = padding + index * pointSpacing.value;
      const y = getY(point, point.errorCount);
      return index === 0 ? `M ${x} ${y}` : `L ${x} ${y}`;
    })
    .join(" ");
  const lastX = padding + (graphData.value.length - 1) * pointSpacing.value;
  const baseY = graphHeight.value - padding;
  return `${linePath} L ${lastX} ${baseY} L ${padding} ${baseY} Z`;
});

// Helper Function to Compute the Hover-Rectangle Boundaries.
const getRectBoundaries = (index: number): { x: number; width: number } => {
  const n = graphData.value.length;
  const ps = pointSpacing.value;
  const center = padding + index * ps;
  let rectX = 0;
  let rectWidth = 0;

  if (index === 0) {
    rectX = padding;
    if (n > 1) {
      const nextCenter = padding + (index + 1) * ps;
      rectWidth = (center + nextCenter) / 2 - rectX;
    } else {
      rectWidth = innerWidth.value;
    }
  } else if (index === n - 1) {
    const prevCenter = padding + (index - 1) * ps;
    rectX = (prevCenter + center) / 2;
    rectWidth = padding + (n - 1) * ps - rectX;
  } else {
    const prevCenter = padding + (index - 1) * ps;
    const nextCenter = padding + (index + 1) * ps;
    rectX = (prevCenter + center) / 2;
    const rectRight = (center + nextCenter) / 2;
    rectWidth = rectRight - rectX;
  }
  return { x: rectX, width: rectWidth };
};
</script>
