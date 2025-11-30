<template>
  <Modal v-if="isOpen" :title="title" size="sm" :closable="cancelable" :show-close="cancelable" @close="handleCancel">
    <!-- Icon at top -->
    <div class="mb-4 flex items-start gap-3">
      <div class="mt-0.5 rounded-lg p-2" :class="iconBackgroundClass">
        <component :is="iconComponent" :class="iconColorClass" class="h-5 w-5" />
      </div>
      <div class="flex-1">
        <p class="text-base-content text-sm leading-relaxed">
          {{ message }}
        </p>
      </div>
    </div>

    <template #actions>
      <div class="flex gap-3">
        <button v-if="cancelable" type="button" class="btn btn-ghost" @click="handleCancel">
          {{ cancelText }}
        </button>
        <button type="button" class="btn" :class="confirmButtonClass" @click="handleConfirm" autofocus>
          {{ confirmText }}
        </button>
      </div>
    </template>
  </Modal>
</template>

<script setup lang="ts">
import { computed } from "vue";
import CheckCircleIcon from "./icons/CheckCircleIcon.vue";
import ExclamationCircleIcon from "./icons/ExclamationCircleIcon.vue";
import InformationCircleIcon from "./icons/InformationCircleIcon.vue";
import XCircleIcon from "./icons/XCircleIcon.vue";
import Modal from "./Modal.vue";

export type ConfirmDialogType = "success" | "error" | "warning" | "info";

interface Props {
  isOpen: boolean;
  type?: ConfirmDialogType;
  title?: string;
  message: string;
  confirmText?: string;
  cancelText?: string;
  cancelable?: boolean;
}

interface Emits {
  confirm: [];
  cancel: [];
}

const props = withDefaults(defineProps<Props>(), {
  type: "info",
  title: "Confirm",
  confirmText: "Confirm",
  cancelText: "Cancel",
  cancelable: true,
});

const emit = defineEmits<Emits>();

const iconComponent = computed(() => {
  return {
    success: CheckCircleIcon,
    error: XCircleIcon,
    warning: ExclamationCircleIcon,
    info: InformationCircleIcon,
  }[props.type];
});

const iconBackgroundClass = computed(() => {
  return {
    success: "bg-success/10",
    error: "bg-error/10",
    warning: "bg-warning/10",
    info: "bg-info/10",
  }[props.type];
});

const iconColorClass = computed(() => {
  return {
    success: "text-success",
    error: "text-error",
    warning: "text-warning",
    info: "text-info",
  }[props.type];
});

const confirmButtonClass = computed(() => {
  return {
    success: "btn-success",
    error: "btn-error",
    warning: "btn-warning",
    info: "btn-info",
  }[props.type];
});

const handleConfirm = () => {
  emit("confirm");
};

const handleCancel = () => {
  emit("cancel");
};
</script>
