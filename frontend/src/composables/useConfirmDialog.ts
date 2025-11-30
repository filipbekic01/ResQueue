import { reactive } from "vue";
import type { ConfirmDialogType } from "../components/ConfirmationDialog.vue";

export interface ConfirmDialogOptions {
  type?: ConfirmDialogType;
  title?: string;
  message: string;
  confirmText?: string;
  cancelText?: string;
  cancelable?: boolean;
}

interface ConfirmDialogState {
  isOpen: boolean;
  type: ConfirmDialogType;
  title: string;
  message: string;
  confirmText: string;
  cancelText: string;
  cancelable: boolean;
  resolve?: (value: boolean) => void;
}

const state = reactive<ConfirmDialogState>({
  isOpen: false,
  type: "info",
  title: "Confirm",
  message: "",
  confirmText: "Confirm",
  cancelText: "Cancel",
  cancelable: true,
});

export function useConfirmDialog() {
  const confirm = (options: ConfirmDialogOptions): Promise<boolean> => {
    return new Promise((resolve) => {
      state.isOpen = true;
      state.type = options.type || "info";
      state.title = options.title || "Confirm";
      state.message = options.message;
      state.confirmText = options.confirmText || "Confirm";
      state.cancelText = options.cancelText || "Cancel";
      state.cancelable = options.cancelable !== undefined ? options.cancelable : true;
      state.resolve = resolve;
    });
  };

  const handleConfirm = () => {
    state.isOpen = false;
    state.resolve?.(true);
    state.resolve = undefined;
  };

  const handleCancel = () => {
    state.isOpen = false;
    state.resolve?.(false);
    state.resolve = undefined;
  };

  // Convenience methods
  const confirmDelete = (message: string, title = "Confirm Delete"): Promise<boolean> => {
    return confirm({
      type: "error",
      title,
      message,
      confirmText: "Delete",
      cancelText: "Cancel",
    });
  };

  const confirmAction = (message: string, title = "Confirm Action"): Promise<boolean> => {
    return confirm({
      type: "warning",
      title,
      message,
      confirmText: "Proceed",
      cancelText: "Cancel",
    });
  };

  const showSuccess = (message: string, title = "Success"): Promise<boolean> => {
    return confirm({
      type: "success",
      title,
      message,
      confirmText: "OK",
      cancelable: false,
    });
  };

  const showError = (message: string, title = "Error"): Promise<boolean> => {
    return confirm({
      type: "error",
      title,
      message,
      confirmText: "OK",
      cancelable: false,
    });
  };

  const showInfo = (message: string, title = "Information"): Promise<boolean> => {
    return confirm({
      type: "info",
      title,
      message,
      confirmText: "OK",
      cancelable: false,
    });
  };

  return {
    state,
    confirm,
    handleConfirm,
    handleCancel,
    confirmDelete,
    confirmAction,
    showSuccess,
    showError,
    showInfo,
  };
}
