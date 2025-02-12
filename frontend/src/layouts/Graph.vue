<template>
  <div>
    <!-- SVG element with fixed dimensions -->
    <svg
      :width="width"
      :height="height"
      :viewBox="`0 0 ${width} ${height}`"
      class="cursor-default select-none"
    >
      <!-- Y-axis (vertical line) and tick marks -->
      <line :x1="padding" :y1="padding" :x2="padding" :y2="height - padding" stroke="gray" />
      <g v-for="(tick, idx) in tickValues" :key="'tick-' + idx">
        <line
          :x1="padding - 3"
          :y1="getYForValue(tick)"
          :x2="padding"
          :y2="getYForValue(tick)"
          :stroke="settings.darkMode ? 'gray' : 'black'"
        />
        <text
          :x="padding - 5"
          :y="getYForValue(tick)"
          text-anchor="end"
          alignment-baseline="middle"
          font-size="10"
          :fill="settings.darkMode ? 'gray' : 'black'"
        >
          {{ Math.round(tick) }}
        </text>
      </g>

      <!-- Line graph paths -->
      <path :d="linePathConsume" stroke="green" fill="none" stroke-width="1" />
      <path :d="linePathError" stroke="red" fill="none" stroke-width="1" />
      <path
        :d="linePathDeadLetter"
        :stroke="settings.darkMode ? 'gray' : 'black'"
        fill="none"
        stroke-width="1"
      />

      <!-- For each data point, render the hover rectangle, circles and label -->
      <g v-for="(point, index) in graphData" :key="index">
        <!-- Invisible hover area rectangle (with blue fill on hover) -->
        <rect
          :x="getRectBoundaries(index).x"
          y="0"
          :width="getRectBoundaries(index).width"
          :height="height"
          :fill="hoveredIndex === index ? 'rgba(0, 0, 0, 0.1)' : 'transparent'"
          style="cursor: pointer"
          @mouseenter="(event) => handleMouseEnter(event, point, index)"
          @mouseleave="handleMouseLeave"
        />
        <!-- Data point circles -->
        <circle
          :cx="padding + index * pointSpacing"
          :cy="getY(point, point.consumeCount)"
          r="2"
          fill="green"
        />
        <circle
          :cx="padding + index * pointSpacing"
          :cy="getY(point, point.errorCount)"
          r="2"
          fill="red"
        />
        <circle
          :cx="padding + index * pointSpacing"
          :cy="getY(point, point.deadLetterCount)"
          r="2"
          :fill="settings.darkMode ? 'gray' : 'black'"
        />
        <!-- Time label (x-axis) -->
        <text
          :x="padding + index * pointSpacing"
          :y="height - padding + 15"
          :fill="settings.darkMode ? 'gray' : 'black'"
          class="pointer-events-none"
          text-anchor="middle"
          font-size="12"
        >
          {{ point.time }}
        </text>
      </g>
    </svg>

    <!-- Tooltip -->
    <div
      v-if="hoveredPoint"
      class="z-50 rounded-lg bg-black/80 px-3 py-2 text-sm text-white dark:bg-black"
      :style="{
        position: 'fixed',
        left: `${tooltipPosition.x + 10}px`,
        top: `${tooltipPosition.y + 10}px`,
      }"
    >
      <p><strong>Consume:</strong> {{ hoveredPoint.consumeCount }}</p>
      <p><strong>Error:</strong> {{ hoveredPoint.errorCount }}</p>
      <p><strong>Dead Letter:</strong> {{ hoveredPoint.deadLetterCount }}</p>
      <p class="mt-2"><strong>Time:</strong> {{ hoveredPoint.time }}</p>
    </div>
  </div>
</template>

<script lang="ts">
export interface DataPoint {
  time: string
  consumeCount: number
  errorCount: number
  deadLetterCount: number
}
</script>

<script setup lang="ts">
import { useQueueMetricsQuery } from '@/api/queues/queueMetricsQuery'
import { useUserSettings } from '@/composables/userSettingsComposable'
import type { QueueDto } from '@/dtos/queue/queueDto'
import { format } from 'date-fns'
import { computed, ref } from 'vue'

const { settings } = useUserSettings()

// === Tooltip & Hover Reactive Variables ===
const hoveredPoint = ref<DataPoint | null>(null)
const hoveredIndex = ref<number | null>(null)
const tooltipPosition = ref({ x: 0, y: 0 })

const handleMouseEnter = (event: MouseEvent, point: DataPoint, index?: number) => {
  hoveredPoint.value = point
  if (typeof index === 'number') {
    hoveredIndex.value = index
  }
  tooltipPosition.value = { x: event.clientX - 125, y: event.clientY }
}

const handleMouseLeave = () => {
  hoveredPoint.value = null
  hoveredIndex.value = null
}

// === Props & Metrics Data ===
const props = defineProps<{
  queue: QueueDto
}>()

const { data: metrics } = useQueueMetricsQuery(computed(() => props.queue.id))

const graphData = computed<DataPoint[]>(() => {
  const data: DataPoint[] = []
  const now = new Date()

  for (let i = 0; i < 10; i++) {
    const pastTime = new Date(now.getTime() - i * 60000)
    const timeString = format(pastTime, 'hh:mm')

    const minuteMetric = metrics.value?.find(
      (x) => format(x.startTime, 'yyyy-MM-dd|HH:mm') == format(pastTime, 'yyyy-MM-dd|HH:mm'),
    )

    data.push({
      time: timeString,
      consumeCount: minuteMetric?.consumeCount ?? 0,
      errorCount: minuteMetric?.errorCount ?? 0,
      deadLetterCount: minuteMetric?.deadLetterCount ?? 0,
    })
  }

  return data.reverse()
})

// === Graph Configuration ===
const width = 500
const height = 120
const padding = 20
const graphWidth = width - 2 * padding
const graphHeight = height - 2 * padding

// Compute the maximum messages value; if all values are zero, default to 1.
const maxMessages = computed(() => {
  const maxVal = Math.max(
    ...graphData.value.map((d) => Math.max(d.consumeCount, d.errorCount, d.deadLetterCount)),
  )
  return maxVal === 0 ? 1 : maxVal
})

// Calculate horizontal spacing between points.
const pointSpacing = computed(() => {
  return graphData.value.length > 1 ? graphWidth / (graphData.value.length - 1) : graphWidth
})

// Helper function: get y coordinate for a given data point count.
const getY = (point: DataPoint, count: number): number => {
  return height - padding - (count / maxMessages.value) * graphHeight
}

// --- New Helper for Y-axis ticks ---
// For a given value v, calculate its y position on the axis.
const getYForValue = (value: number): number => {
  return height - padding - (value / maxMessages.value) * graphHeight
}

// Computed tick values for the y-axis.
// They are: max, ¾·max, ½·max, ¼·max, and 0.
const tickValues = computed(() => {
  const maxVal = maxMessages.value
  return [maxVal, (3 / 4) * maxVal, (1 / 2) * maxVal, (1 / 4) * maxVal, 0]
})

// --- SVG Paths for the three lines ---
const linePathConsume = computed(() => {
  if (!graphData.value.length) return ''
  return graphData.value
    .map((point, index) => {
      const x = padding + index * pointSpacing.value
      const y = getY(point, point.consumeCount)
      return index === 0 ? `M ${x} ${y}` : `L ${x} ${y}`
    })
    .join(' ')
})

const linePathError = computed(() => {
  if (!graphData.value.length) return ''
  return graphData.value
    .map((point, index) => {
      const x = padding + index * pointSpacing.value
      const y = getY(point, point.errorCount)
      return index === 0 ? `M ${x} ${y}` : `L ${x} ${y}`
    })
    .join(' ')
})

const linePathDeadLetter = computed(() => {
  if (!graphData.value.length) return ''
  return graphData.value
    .map((point, index) => {
      const x = padding + index * pointSpacing.value
      const y = getY(point, point.deadLetterCount)
      return index === 0 ? `M ${x} ${y}` : `L ${x} ${y}`
    })
    .join(' ')
})

// Helper Function to Compute the Hover-Rectangle Boundaries.
const getRectBoundaries = (index: number): { x: number; width: number } => {
  const n = graphData.value.length
  const ps = pointSpacing.value
  const center = padding + index * ps
  let rectX = 0
  let rectWidth = 0

  if (index === 0) {
    rectX = padding
    if (n > 1) {
      const nextCenter = padding + (index + 1) * ps
      rectWidth = (center + nextCenter) / 2 - rectX
    } else {
      rectWidth = graphWidth
    }
  } else if (index === n - 1) {
    const prevCenter = padding + (index - 1) * ps
    rectX = (prevCenter + center) / 2
    rectWidth = padding + (n - 1) * ps - rectX
  } else {
    const prevCenter = padding + (index - 1) * ps
    const nextCenter = padding + (index + 1) * ps
    rectX = (prevCenter + center) / 2
    const rectRight = (center + nextCenter) / 2
    rectWidth = rectRight - rectX
  }
  return { x: rectX, width: rectWidth }
}
</script>
