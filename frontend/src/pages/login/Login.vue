<script lang="ts" setup>
import { useLoginMutation } from '@/api/auth/loginMutation'
import { useIdentity } from '@/composables/identityComposable'
import { useToast } from 'primevue/usetoast'
import { ref, watch } from 'vue'
import { useRouter } from 'vue-router'

const toast = useToast()

const email = ref('filip1994sm@gmail.com')
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
      if (e.response?.data?.detail == 'LockedOut') {
        toast.add({
          severity: 'error',
          summary: 'Login Failed',
          detail: 'Too many attempts, try a bit later.',
          life: 6000
        })
      } else {
        toast.add({
          severity: 'error',
          summary: 'Login Failed',
          detail: 'Failed attempt to login.',
          life: 6000
        })
      }

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
  <div class="flex flex-col grow items-center mt-16 h-screen">
    <RouterLink :to="{ name: 'home' }" class="flex items-center py-3 mb-4">
      <div class="flex items-center justify-end bg-black p-2 rounded-lg">
        <i class="pi pi-database text-white rotate-90" style="font-size: 1.5rem"></i>
      </div>
      <div class="text-2xl font-semibold px-2">
        ResQueue
        <span class="text-gray-500 font-normal"><i class="pi pi-angle-right"></i> Login</span>
      </div>
    </RouterLink>
    <div class="flex flex-col bg-white shadow border rounded-lg p-8 w-96">
      <div class="flex flex-col gap-4 mb-4">
        <label for="username" class="font-semibold w-24 white">E-Mail</label>
        <InputText
          v-model="email"
          id="username"
          class="flex-auto"
          type="email"
          autocomplete="off"
        />
      </div>
      <div class="flex flex-col gap-4 mb-8">
        <label for="email" class="font-semibold w-24 white">Password</label>
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
        <div class="flex justify-center mt-3 text-gray-400">
          <RouterLink :to="'forgot-password'" class="hover:text-blue-500"
            >Forgot account password?</RouterLink
          >
        </div>
      </div>
    </div>
  </div>
</template>
