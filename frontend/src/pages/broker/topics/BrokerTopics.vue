<script lang="ts" setup>
import Message from 'primevue/message'
</script>

<template>
  <div class="flex grow flex-col">
    <Message class="m-5" severity="secondary"
      >Topics can be selected for publishing or requeuing messages through other pages. A visual representation of
      broker topology will be included in a future ResQueue release.</Message
    >
    <DataTable
      scrollable
      scroll-height="flex"
      class="grow border-t"
      :virtual-scroller-options="{
        itemSize: 46
      }"
    >
      <Column field="parsed.name" header="Name"></Column>
    </DataTable>
  </div>
</template>
