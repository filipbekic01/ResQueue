<script lang="ts" setup>
import { useExchanges } from '@/composables/exchangesComposable'
import { computed } from 'vue'

const props = defineProps<{
  brokerId: string
}>()

const { formattedExchanges } = useExchanges(computed(() => props.brokerId))
</script>

<template>
  <DataTable
    :value="formattedExchanges"
    scrollable
    scroll-height="flex"
    class="grow"
    :virtual-scroller-options="{
      itemSize: 46
    }"
  >
    <Column field="parsed.name" header="Name"></Column>
  </DataTable>
</template>
