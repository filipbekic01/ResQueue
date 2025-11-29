<template>
  <ConfirmationDialog
    :is-open="state.isOpen"
    :type="state.type"
    :title="state.title"
    :message="state.message"
    :confirm-text="state.confirmText"
    :cancel-text="state.cancelText"
    :cancelable="state.cancelable"
    @confirm="handleConfirm"
    @cancel="handleCancel"
  />
</template>

<script setup lang="ts">
import { useConfirmDialog } from "@/composables/useConfirmDialog";
import ConfirmationDialog from "./ConfirmationDialog.vue";

const { state, handleConfirm, handleCancel } = useConfirmDialog();
</script>
