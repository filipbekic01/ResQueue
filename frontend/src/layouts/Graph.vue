<template>
  <div class="ms-5 h-full grow border-e px-3">
    <div class="graph-container">
      <canvas ref="canvas"></canvas>
    </div>
  </div>
</template>

<script lang="ts">
export interface DataPoint {
  time: string
  messages: number
}
</script>

<script setup lang="ts">
import { useQueueMetricsQuery } from '@/api/queues/queueMetricsQuery'
import type { QueueDto } from '@/dtos/queue/queueDto'
import { format } from 'date-fns'
import { computed, onMounted, ref, watch, watchEffect } from 'vue'

const props = defineProps<{
  queue: QueueDto
}>()

const { data: metrics } = useQueueMetricsQuery(computed(() => props.queue.id))

watchEffect(() => {
  console.log('metrics', metrics.value)
})

//   { time: '11:05', messages: 50 },
const graphData = computed((): DataPoint[] => {
  // if (!metrics.value || !metrics.value.length) {
  //   return [] as DataPoint[]
  // }

  // return metrics.value.map((metric) => ({
  //   time: metric.dateTime,
  //   messages: metric.errorCount,
  // }))

  const data = []
  const now = new Date()
  for (let i = 0; i < 10; i++) {
    const pastTime = new Date(now.getTime() - i * 60000)
    const timeString = format(pastTime, 'hh:mm')
    data.push({ time: timeString, messages: 0 })
  }

  return data
})

// Watch for changes in the data prop
watch(
  () => graphData,
  () => {
    drawGraph()
  },
  { deep: true },
)

// Canvas reference
const canvas = ref<HTMLCanvasElement | null>(null)

// Function to draw the graph
const drawGraph = () => {
  const theCanvas = canvas.value
  if (!theCanvas) {
    return
  }

  const ctx = theCanvas.getContext('2d')
  if (!ctx) {
    return
  }

  // Set canvas resolution based on devicePixelRatio
  const dpr = window.devicePixelRatio || 1
  const width = 500
  const height = 49
  theCanvas.width = width * dpr
  theCanvas.height = height * dpr
  theCanvas.style.width = `${width}px`
  theCanvas.style.height = `${height}px`
  ctx.scale(dpr, dpr)

  // Clear canvas
  ctx.clearRect(0, 0, theCanvas.width, theCanvas.height)

  // Styling
  const padding = 20
  const graphWidth = width - 2 * padding
  const graphHeight = height - 2 * padding

  // Data
  const maxMessages = Math.max(...graphData.value.map((d) => d.messages))
  const pointSpacing = graphWidth / (graphData.value.length - 1)

  // Draw axis
  // ctx.strokeStyle = '#ccc'
  // ctx.beginPath()
  // ctx.moveTo(padding, height - padding)
  // ctx.lineTo(padding, padding)
  // ctx.lineTo(width - padding, height - padding)
  // ctx.stroke()

  // Draw line
  ctx.strokeStyle = 'black'
  ctx.lineWidth = 1
  ctx.beginPath()
  graphData.value.forEach((point, index) => {
    const x = padding + index * pointSpacing
    const y = height - padding - (point.messages / maxMessages) * graphHeight
    if (index === 0) {
      ctx.moveTo(x, y)
    } else {
      ctx.lineTo(x, y)
    }
  })
  ctx.stroke()

  // Draw points and labels
  graphData.value.forEach((point, index) => {
    const x = padding + index * pointSpacing
    const y = height - padding - (point.messages / maxMessages) * graphHeight

    // Draw point
    ctx.fillStyle = 'black'
    ctx.beginPath()
    ctx.arc(x, y, 2, 0, Math.PI * 2)
    ctx.fill()

    // Labels
    ctx.fillStyle = 'black'
    ctx.textAlign = 'center'
    ctx.font = '12px Arial'
    ctx.fillText(point.time, x, height - padding + 15)
    ctx.fillText(point.messages.toString(), x, y - 10)
  })
}

// Lifecycle hooks
onMounted(() => {
  drawGraph()
})
</script>
