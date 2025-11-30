<template>
  <div class="toast toast-top toast-end z-1002">
    <TransitionGroup name="toast">
      <div
        v-for="toast in toasts"
        :key="toast.id"
        class="alert cursor-pointer shadow-lg"
        :class="alertClass(toast.type)"
        role="alert"
        @mouseenter="pauseToast(toast.id)"
        @mouseleave="resumeToast(toast.id)"
        @click="removeToast(toast.id)"
      >
        <div class="flex items-center gap-2">
          <component :is="iconComponent(toast.type)" class="h-6 w-6" />
          <span>{{ toast.message }}</span>
        </div>
        <button @click.stop="removeToast(toast.id)" class="btn btn-sm btn-ghost btn-circle" type="button">✕</button>
      </div>
    </TransitionGroup>
  </div>
</template>

<script setup lang="ts">
import { useToast, type ToastType } from "@/composables/useToast";
import CheckCircleIcon from "./icons/CheckCircleIcon.vue";
import HelpIcon from "./icons/HelpIcon.vue";
import ShieldIcon from "./icons/ShieldIcon.vue";
import ZapIcon from "./icons/ZapIcon.vue";

const { toasts, removeToast, pauseToast, resumeToast } = useToast();

const alertClass = (type: ToastType) => {
  switch (type) {
    case "success":
      return "alert-success";
    case "error":
      return "alert-error";
    case "warning":
      return "alert-warning";
    case "info":
      return "alert-info";
    default:
      return "";
  }
};

const iconComponent = (type: ToastType) => {
  switch (type) {
    case "success":
      return CheckCircleIcon;
    case "error":
      return ShieldIcon;
    case "warning":
      return ZapIcon;
    case "info":
      return HelpIcon;
    default:
      return HelpIcon;
  }
};
</script>

<style scoped>
/* Toast transition animations */
.toast-enter-active,
.toast-leave-active {
  transition: all 0.3s ease;
}

.toast-enter-from {
  opacity: 0;
  transform: translateX(100%);
}

.toast-leave-to {
  opacity: 0;
  transform: translateX(100%) scale(0.8);
}

.toast-move {
  transition: transform 0.3s ease;
}
</style>
