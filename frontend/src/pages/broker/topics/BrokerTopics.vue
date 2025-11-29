<script lang="ts" setup>
import { FilterMatchMode } from "@primevue/core/api";
import { computed, ref, watchEffect } from "vue";
import { useSubscriptionsQuery } from "@/api/subscriptions/subscriptionsQuery";
import { useUserSettings } from "@/composables/userSettingsComposable";

const { settings, updateSettings } = useUserSettings();

const { data: subscriptions } = useSubscriptionsQuery(computed(() => settings.refetchInterval));

const search = ref(settings.topicSearch);

watchEffect(() => {
  updateSettings({
    ...settings,
    topicSearch: search.value,
  });
});

const filters = ref({
  topicName: { value: search, matchMode: FilterMatchMode.CONTAINS },
});
</script>

<template>
  <div class="flex overflow-auto">
    <DataTable
      show-gridlines
      scrollable
      scroll-height="flex"
      :value="subscriptions"
      removable-sort
      class="rq-grid grow overflow-auto"
      striped-rows
      v-model:filters="filters"
      filter-display="menu"
    >
      <Column field="topicName" class="overflow-hidden py-0 overflow-ellipsis">
        <template #header>
          <div class="flex w-full items-center">
            <b>Name</b>
            <IconField class="ms-1 grow">
              <InputIcon class="pi pi-search" v-if="!search" />
              <InputIcon class="pi pi-times cursor-pointer" v-else @click="search = ''" />
              <InputText
                placeholder="Search anything..."
                class="dark:bg-surface-900 w-full border-0 shadow-none"
                v-model="search"
                ref="searchInputText"
              />
            </IconField>
          </div>
        </template>
      </Column>
      <Column sortable field="routingKey" header="Routing Key" class="w-[0] whitespace-nowrap"> </Column>
      <Column sortable field="destinationName" header="Destination Name" class="w-[0] whitespace-nowrap"> </Column>
      <Column sortable field="destinationType" header="Destination Type" class="w-[0] whitespace-nowrap"> </Column>
      <Column sortable field="subscriptionType" header="Subscription Type" class="w-[0] whitespace-nowrap"> </Column>
    </DataTable>
  </div>
</template>
