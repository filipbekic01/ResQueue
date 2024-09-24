<script lang="ts" setup>
import { useForgotPasswordMutation } from '@/api/auth/forgotPassword'
import { useResetPasswordMutation } from '@/api/auth/resetPassword'
import { errorToToast } from '@/utils/errorUtils'
import { useHead } from '@unhead/vue'
import { useToast } from 'primevue/usetoast'
import { ref } from 'vue'
import { useRouter } from 'vue-router'

const toast = useToast()
const router = useRouter()

const email = ref('')
const resetCode = ref('')
const newPassword = ref('')

const { mutateAsync: forgotPasswordAsync } = useForgotPasswordMutation()
const { mutateAsync: resetPasswordAsync, isPending: isResetPasswordPending } = useResetPasswordMutation()

const isLoading = ref(false)
const isEmailSent = ref(false)
const forgotPassword = async () => {
  if (isLoading.value) {
    return
  }

  isLoading.value = true

  forgotPasswordAsync({
    email: email.value
  })
    .then(() => {
      toast.add({
        severity: 'success',
        summary: 'Recovery Process Initiated',
        detail: 'A recovery email has been successfully sent.',
        life: 3000
      })

      isEmailSent.value = true
    })
    .catch((e) => toast.add(errorToToast(e)))
    .finally(() => {
      isLoading.value = false
    })
}

const resetPassword = () => {
  if (!resetCode.value || !newPassword.value) {
    toast.add({
      severity: 'error',
      summary: 'Input Missing',
      detail: 'Reset code and new password must be present.',
      life: 3000
    })
    return
  }

  resetPasswordAsync({
    email: email.value,
    resetCode: resetCode.value,
    newPassword: newPassword.value
  })
    .then(() => {
      router.push({ name: 'login' })
    })
    .catch((e) => toast.add(errorToToast(e)))
}

useHead({
  title: 'ResQueue',
  meta: [
    { name: 'robots', content: 'noindex, nofollow' } // Prevents the page from being indexed
  ]
})
</script>

<template>
  <div class="mt-16 flex h-screen grow flex-col items-center">
    <RouterLink :to="{ name: 'home' }" class="mb-4 flex items-center py-3">
      <div class="flex items-center justify-end rounded-lg bg-black p-2">
        <i class="pi pi-database rotate-90 text-white" style="font-size: 1.5rem"></i>
      </div>
      <div class="px-2 text-2xl font-semibold">
        ResQueue
        <span class="font-normal text-gray-500"><i class="pi pi-angle-right"></i> Recovery</span>
      </div>
    </RouterLink>
    <div class="flex w-96 flex-col rounded-xl border bg-white p-8 shadow-md">
      <div class="mb-4 flex flex-col gap-4">
        <label for="email" class="white w-24 font-semibold">E-Mail</label>
        <InputText
          v-model="email"
          :disabled="isEmailSent"
          id="email"
          class="flex-auto"
          type="email"
          autocomplete="off"
        />
      </div>

      <div class="mb-4 flex flex-col gap-4" v-if="isEmailSent">
        <label for="resetCode" class="white w-24 font-semibold">Reset Code</label>
        <InputText
          v-model="resetCode"
          id="resetCode"
          placeholder="******"
          class="flex-auto"
          type="text"
          autocomplete="off"
        />
      </div>

      <div class="mb-4 flex flex-col gap-4" v-if="resetCode.length">
        <label for="newPassword" class="w-24 whitespace-nowrap font-semibold">New Password</label>
        <InputText
          v-model="newPassword"
          id="newPassword"
          class="flex-auto"
          type="password"
          autocomplete="off"
          placeholder="******"
        />
      </div>

      <div class="flex flex-col gap-2">
        <Button
          v-if="!isEmailSent"
          type="button"
          label="Send Recovery E-Mail"
          @click="forgotPassword"
          :loading="isLoading"
          icon="pi pi-send"
          icon-pos="right"
        ></Button>

        <Button
          v-else
          type="button"
          label="Reset Password"
          @click="resetPassword"
          icon="pi pi-refresh"
          icon-pos="right"
          :loading="isResetPasswordPending"
        ></Button>
      </div>
    </div>
  </div>
</template>
