<script lang="ts" setup>
import { useAuthQuery } from '@/api/auth/authQuery'
import { computed } from 'vue'

const { data: auth } = useAuthQuery()
const data = computed(() => {
  return [
    { key: 'SQL Engine', value: auth.value?.sqlEngine },
    { key: 'Database', value: auth.value?.database },
    { key: 'Schema', value: auth.value?.schema },
    { key: 'Username', value: auth.value?.username },
    { key: 'Port', value: auth.value?.port },
  ]
})
</script>

<template>
  <DataTable :value="data" striped-rows>
    <Column header="Property" field="key" class="w-0 text-nowrap"></Column>
    <Column header="Value" field="value"></Column>
  </DataTable>
</template>
