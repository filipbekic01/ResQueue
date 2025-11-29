<script lang="ts" setup>
import Message from "primevue/message";
import { computed } from "vue";
import { useAuthQuery } from "@/api/auth/authQuery";

const { data: auth } = useAuthQuery();
const data = computed(() => {
  return [
    { key: "SQL Engine", value: auth.value?.sqlEngine },
    { key: "Database", value: auth.value?.database },
    { key: "Schema", value: auth.value?.schema },
    { key: "Username", value: auth.value?.username },
    { key: "Password", value: "*****" },
    { key: "Port", value: auth.value?.port },
  ];
});
</script>

<template>
  <DataTable show-gridlines :value="data" striped-rows class="rq-grid">
    <Column header="Property" field="key" class="w-0 text-nowrap"></Column>
    <Column header="Value" field="value"></Column>
  </DataTable>
  <Message severity="secondary" class="m-4 w-[40rem]">
    Learn more about SQL transport and configuration in
    <a
      href="https://masstransit.io/documentation/transports/sql"
      target="_blank"
      class="text-blue-500 hover:text-blue-400"
      >official documentation</a
    >.</Message
  >
</template>
