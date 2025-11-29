<template>
  <Teleport to="body">
    <!-- Backdrop -->
    <Transition
      appear
      enter-active-class="transition-opacity duration-200 ease-out"
      leave-active-class="transition-opacity duration-150 ease-in"
      enter-from-class="opacity-0"
      enter-to-class="opacity-100"
      leave-from-class="opacity-100"
      leave-to-class="opacity-0"
    >
      <div
        v-if="isVisible && backdrop"
        class="fixed inset-0 z-1000 bg-black/30"
        :class="backdropBlurClass"
        @click="handleBackdropClick"
      ></div>
    </Transition>

    <!-- Modal Dialog -->
    <Transition
      appear
      enter-active-class="transition-all duration-200 ease-out"
      leave-active-class="transition-all duration-150 ease-in"
      enter-from-class="opacity-0 scale-95"
      enter-to-class="opacity-100 scale-100"
      leave-from-class="opacity-100 scale-100"
      leave-to-class="opacity-0 scale-95"
      @after-leave="handleAfterLeave"
    >
      <div
        v-if="isVisible"
        class="fixed inset-0 z-1001 flex items-center justify-center"
        :class="fullscreen ? 'p-[2vh]' : 'p-4'"
        @mousedown.self="handleMouseDown"
        @click.self="handleBackdropClick"
      >
        <div
          ref="modalRef"
          class="bg-base-100 relative flex flex-col rounded-lg shadow-xl"
          :class="sizeClass"
          role="dialog"
          aria-modal="true"
          :aria-labelledby="title ? `${id}-title` : undefined"
        >
          <!-- Close button (top right) -->
          <button
            v-if="showClose && closable"
            class="btn btn-sm btn-circle btn-ghost absolute top-2 right-2 z-10"
            @click="handleClose"
            type="button"
            aria-label="Close"
          >
            ✕
          </button>

          <!-- Title -->
          <h3 v-if="title" :id="`${id}-title`" class="mb-4 shrink-0 px-6 pt-6 text-lg font-bold">
            {{ title }}
          </h3>

          <!-- Content Slot (scrollable) -->
          <div class="min-h-0 flex-1 overflow-y-auto px-6" :class="{ 'pt-6': !title, 'pb-6': !$slots.actions }">
            <slot></slot>
          </div>

          <!-- Actions Slot (fixed at bottom) -->
          <div v-if="$slots.actions" class="modal-action shrink-0 px-6 pb-6">
            <slot name="actions"></slot>
          </div>
        </div>
      </div>
    </Transition>
  </Teleport>
</template>

<script setup lang="ts">
import { computed, onMounted, onUnmounted, ref } from "vue";

interface Props {
  id?: string;
  title?: string;
  size?: "sm" | "md" | "lg" | "full";
  fullscreen?: boolean;
  backdrop?: boolean;
  showClose?: boolean;
  backdropBlur?: "none" | "xs" | "sm" | "md" | "lg" | "xl";
  closable?: boolean;
}

interface Emits {
  close: [];
  hidden: [];
}

const props = withDefaults(defineProps<Props>(), {
  id: "modal",
  title: undefined,
  size: "md",
  fullscreen: false,
  backdrop: true,
  showClose: true,
  backdropBlur: "none",
  closable: true,
});

const emit = defineEmits<Emits>();

// Visibility state
const isVisible = ref(false);

// Modal ref
const modalRef = ref<HTMLElement | null>(null);

// Track if mousedown happened on backdrop
const mouseDownOnBackdrop = ref(false);

// Size classes
const sizeClass = computed(() => {
  if (props.fullscreen) {
    return "w-full h-full rounded-lg";
  }

  return {
    sm: "w-full max-w-sm max-h-[90vh]",
    md: "w-full max-w-2xl max-h-[90vh]",
    lg: "w-full max-w-5xl max-h-[90vh]",
    full: "w-[95vw] max-w-7xl h-[90vh]",
  }[props.size];
});

// Backdrop blur class
const backdropBlurClass = computed(() => {
  return {
    none: "",
    xs: "backdrop-blur-xs",
    sm: "backdrop-blur-sm",
    md: "backdrop-blur-md",
    lg: "backdrop-blur-lg",
    xl: "backdrop-blur-xl",
  }[props.backdropBlur];
});

const handleClose = () => {
  isVisible.value = false;
  emit("close");
};

const handleAfterLeave = () => {
  emit("hidden");
};

const handleMouseDown = () => {
  mouseDownOnBackdrop.value = true;
};

const handleBackdropClick = () => {
  if (props.backdrop && props.closable && mouseDownOnBackdrop.value) {
    handleClose();
  }
  mouseDownOnBackdrop.value = false;
};

// Handle ESC key press
const handleEscKey = (event: KeyboardEvent) => {
  if (event.key === "Escape" && isVisible.value && props.closable) {
    handleClose();
  }
};

// Show modal on mount
onMounted(() => {
  if (typeof document !== "undefined") {
    document.addEventListener("keydown", handleEscKey);

    // Prevent body scroll when modal is open
    const scrollbarWidth = window.innerWidth - document.documentElement.clientWidth;
    document.body.style.overflow = "hidden";
    if (scrollbarWidth > 0) {
      document.body.style.paddingRight = `${scrollbarWidth}px`;
    }

    // Show modal - transition will handle the animation
    isVisible.value = true;
  }
});

// Cleanup on unmount
onUnmounted(() => {
  if (typeof document !== "undefined") {
    document.removeEventListener("keydown", handleEscKey);
    document.body.style.overflow = "";
    document.body.style.paddingRight = "";
  }
});
</script>
