<script setup lang="ts">
import { onBeforeMount } from "vue";
import { RouterView } from "vue-router";
import ConfirmationDialogContainer from "./components/ConfirmationDialogContainer.vue";
import ToastContainer from "./components/ToastContainer.vue";
import { useTheme } from "./composables/useTheme";

const { init } = useTheme();

onBeforeMount(() => {
  init();
});
</script>

<template>
  <ConfirmationDialogContainer />
  <ToastContainer />
  <RouterView />
</template>
