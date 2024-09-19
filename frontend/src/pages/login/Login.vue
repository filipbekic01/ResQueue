<script lang="ts" setup>
import { useLoginMutation } from '@/api/auth/loginMutation'
import { useIdentity } from '@/composables/identityComposable'
import { errorToToast } from '@/utils/errorUtils'
import { useToast } from 'primevue/usetoast'
import { ref, watch } from 'vue'
import { useRouter } from 'vue-router'

const toast = useToast()

const email = ref('filip@gmail.com')
const password = ref('Password1!')

const router = useRouter()

const { mutateAsync: loginAsync } = useLoginMutation()
const {
  query: { data: user, refetch }
} = useIdentity()

const isLoading = ref(false)
const login = async (email: string, password: string) => {
  if (isLoading.value) {
    return
  }

  isLoading.value = true

  loginAsync({
    email,
    password
  })
    .then(() => {
      refetch().then(() => {
        router.push({
          name: 'app'
        })
      })
    })
    .catch((e) => {
      toast.add(errorToToast(e))
      isLoading.value = false
    })
}

watch(
  () => user.value,
  (value) => {
    if (value) {
      router.push({
        name: 'app'
      })
    }
  },
  {
    immediate: true
  }
)
</script>

<template>
  <div class="mt-16 flex h-screen grow flex-col items-center">
    <RouterLink :to="{ name: 'home' }" class="mb-4 flex items-center py-3">
      <div class="flex items-center justify-end rounded-lg bg-black p-2">
        <i class="pi pi-database rotate-90 text-white" style="font-size: 1.5rem"></i>
      </div>
      <div class="px-2 text-2xl font-semibold">
        ResQueue
        <span class="font-normal text-gray-500"><i class="pi pi-angle-right"></i> Login</span>
      </div>
    </RouterLink>
    <div class="flex w-96 flex-col rounded-xl border bg-white p-8 shadow-md">
      <div class="mb-4 flex flex-col gap-4">
        <label for="username" class="white w-24 font-semibold">E-Mail</label>
        <InputText
          v-model="email"
          id="username"
          class="flex-auto"
          type="email"
          autocomplete="off"
        />
      </div>
      <div class="mb-8 flex flex-col gap-4">
        <label for="email" class="white w-24 font-semibold">Password</label>
        <InputText
          id="email"
          v-model="password"
          class="flex-auto"
          type="password"
          autocomplete="off"
        />
      </div>
      <div class="flex flex-col gap-2">
        <Button
          type="button"
          label="Log In"
          @click="login(email, password)"
          :loading="isLoading"
          icon="pi pi-arrow-right"
          icon-pos="right"
        ></Button>
        <div class="mt-3 flex justify-center text-gray-400">
          <RouterLink :to="'forgot-password'" class="hover:text-blue-500"
            >Forgot account password?</RouterLink
          >
        </div>
      </div>
    </div>
  </div>
</template>
