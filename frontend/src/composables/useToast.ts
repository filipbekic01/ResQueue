import { reactive } from "vue";

export type ToastType = "success" | "error" | "warning" | "info";

export interface Toast {
  id: string;
  message: string;
  type: ToastType;
  duration: number;
  timer?: number;
  remainingTime?: number;
  startTime?: number;
}

interface ToastState {
  toasts: Toast[];
}

const state = reactive<ToastState>({
  toasts: [],
});

let toastIdCounter = 0;

export function useToast() {
  const addToast = (message: string, type: ToastType = "info", duration = 3000) => {
    const id = `toast-${++toastIdCounter}`;
    const toast: Toast = {
      id,
      message,
      type,
      duration,
      remainingTime: duration,
      startTime: Date.now(),
    };

    state.toasts.push(toast);

    // Auto-remove toast after duration
    const timer = setTimeout(() => {
      removeToast(id);
    }, duration);

    toast.timer = timer as unknown as number;

    return id;
  };

  const removeToast = (id: string) => {
    const index = state.toasts.findIndex((t) => t.id === id);
    if (index > -1) {
      const toast = state.toasts[index];
      if (toast?.timer) {
        clearTimeout(toast.timer);
      }
      state.toasts.splice(index, 1);
    }
  };

  const pauseToast = (id: string) => {
    const toast = state.toasts.find((t) => t.id === id);
    if (toast && toast.timer && toast.startTime !== undefined && toast.remainingTime !== undefined) {
      clearTimeout(toast.timer);
      toast.remainingTime -= Date.now() - toast.startTime;
      toast.timer = undefined;
    }
  };

  const resumeToast = (id: string) => {
    const toast = state.toasts.find((t) => t.id === id);
    if (toast && !toast.timer && toast.remainingTime !== undefined) {
      toast.startTime = Date.now();
      const timer = setTimeout(() => {
        removeToast(id);
      }, toast.remainingTime);
      toast.timer = timer as unknown as number;
    }
  };

  // Convenience methods
  const success = (message: string, duration = 3000) => addToast(message, "success", duration);
  const error = (message: string, duration = 5000) => addToast(message, "error", duration);
  const warning = (message: string, duration = 4000) => addToast(message, "warning", duration);
  const info = (message: string, duration = 3000) => addToast(message, "info", duration);

  return {
    toasts: state.toasts,
    addToast,
    removeToast,
    pauseToast,
    resumeToast,
    success,
    error,
    warning,
    info,
  };
}
