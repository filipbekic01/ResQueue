<script lang="ts" setup>
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
  <div class="overflow-x-auto">
    <table class="table-zebra table w-auto">
      <thead>
        <tr>
          <th class="whitespace-nowrap">Property</th>
          <th>Value</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="item in data" :key="item.key">
          <td class="font-medium whitespace-nowrap">{{ item.key }}</td>
          <td>{{ item.value }}</td>
        </tr>
      </tbody>
    </table>
  </div>
  <div role="alert" class="alert m-4 w-[40rem]">
    <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" class="h-6 w-6 shrink-0 stroke-current">
      <path
        stroke-linecap="round"
        stroke-linejoin="round"
        stroke-width="2"
        d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"
      ></path>
    </svg>
    <span>
      Learn more about SQL transport and configuration in
      <a
        href="https://masstransit.io/documentation/transports/sql"
        target="_blank"
        class="text-blue-500 hover:text-blue-400"
        >official documentation</a
      >.
    </span>
  </div>
</template>
