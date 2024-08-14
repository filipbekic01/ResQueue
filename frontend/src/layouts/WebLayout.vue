<script setup lang="ts">
import { useLoginMutation } from '@/api/auth/loginMutation'
import { useLogoutMutation } from '@/api/auth/logoutMutation'
import { useMeQuery } from '@/api/auth/meQuery'
import { useRegisterMutation } from '@/api/auth/registerMutation'
import { useIdentity } from '@/composables/identityComposable'
import Dialog from 'primevue/dialog'
import { ref } from 'vue'

const { user } = useIdentity()
const { refetch } = useMeQuery()

const { mutateAsync: loginAsync } = useLoginMutation()
const { mutateAsync: logoutAsync } = useLogoutMutation()
const { mutateAsync: registerAsync } = useRegisterMutation()

const showLogin = ref(false)
const showRegister = ref(false)

const email = ref('filip1994sm@gmail.com')
const password = ref('Password1!')

const regEmail = ref('filip1994sm@gmail.com')
const regPassword = ref('Password1!')
const regPassword1 = ref('Password1!')

const login = (email: string, password: string) => {
  loginAsync({
    email,
    password
  }).then(() => {
    refetch().then((x) => {
      user.value = x.data

      showLogin.value = false
      showRegister.value = false
    })
  })
}

const logout = () => {
  logoutAsync().then(() => {
    user.value = undefined
  })
}

const register = () => {
  if (regPassword.value != regPassword1.value) {
    return
  }

  registerAsync({
    email: regEmail.value,
    password: regPassword.value
  }).then(() => {
    login(regEmail.value, regPassword.value)
  })
}
</script>

<template>
  <Dialog
    v-model:visible="showLogin"
    modal
    pt:mask:class="backdrop-grayscale"
    header="Login"
    :style="{ width: '25rem' }"
  >
    <div class="flex items-center gap-4 mb-4">
      <label for="username" class="font-semibold w-24">E-Mail</label>
      <InputText v-model="email" id="username" class="flex-auto" type="email" autocomplete="off" />
    </div>
    <div class="flex items-center gap-4 mb-8">
      <label for="email" class="font-semibold w-24">Password</label>
      <InputText
        id="email"
        v-model="password"
        class="flex-auto"
        type="password"
        autocomplete="off"
      />
    </div>
    <div class="flex justify-end gap-2">
      <Button type="button" label="Cancel" severity="secondary" @click="showLogin = false"></Button>
      <Button type="button" label="Log in" @click="login(email, password)"></Button>
    </div>
  </Dialog>

  <Dialog
    v-model:visible="showRegister"
    modal
    pt:mask:class="backdrop-grayscale"
    header="Create broker"
    :style="{ width: '25rem' }"
  >
    <div class="flex items-center gap-4 mb-4">
      <label for="username" class="font-semibold w-24">E-Mail</label>
      <InputText
        v-model="regEmail"
        id="username"
        class="flex-auto"
        type="email"
        autocomplete="off"
      />
    </div>
    <div class="flex items-center gap-4 mb-8">
      <label for="email" class="font-semibold w-24">Password</label>
      <InputText
        id="email"
        v-model="regPassword"
        class="flex-auto"
        type="password"
        autocomplete="off"
      />
    </div>
    <div class="flex items-center gap-4 mb-8">
      <label for="email" class="font-semibold w-24">Password Again</label>
      <InputText
        id="email"
        v-model="regPassword1"
        class="flex-auto"
        type="password"
        autocomplete="off"
      />
    </div>
    <div class="flex justify-end gap-2">
      <Button
        type="button"
        label="Cancel"
        severity="secondary"
        @click="showRegister = false"
      ></Button>
      <Button type="button" label="Register" @click="register"></Button>
    </div>
  </Dialog>

  <div class="h-screen">
    <div class="max-w-[1024px] mx-auto">
      <div class="flex gap-2 py-3 items-center">
        <div class="flex items-center justify-end bg-black p-2.5 rounded-lg">
          <i class="pi pi-database text-white rotate-90" style="font-size: 1.5rem"></i>
        </div>
        <RouterLink class="text-xl font-semibold p-2" :to="{ name: 'home' }">Resqueue</RouterLink>
        <div class="flex gap-3 ms-2 items-center grow justify-center">
          <RouterLink class="text-lg p-2" :to="{ name: 'home' }">Home</RouterLink>
          <RouterLink class="text-lg p-2" :to="{ name: 'home' }">Pricing</RouterLink>
          <RouterLink class="text-lg p-2" :to="{ name: 'home' }">Support</RouterLink>
        </div>
        <div class="ms-auto flex text-lg gap-3">
          <template v-if="user">
            <div class="p-2 text-gray-400">{{ user.email }}</div>
            <div class="p-2 cursor-pointer" @click="logout">Log out</div>
          </template>
          <template v-else>
            <Button outlined @click="showLogin = true" class="p-2 cursor-pointer">Log in</Button>
            <Button @click="showRegister = true" class="p-2 cursor-pointer">Register</Button>
          </template>
        </div>
      </div>
      <slot></slot>
      <div class="border-t mt-16 text-gray-600 py-4 text-center">
        Developed by
        <a
          href="https://www.linkedin.com/in/filipbekic01/"
          target="_blank"
          class="hover:text-blue-500 border-b border-gray-400 border-dashed"
          >Filip Bekic</a
        >
        — Republic of Serbia, Novi Sad
      </div>
    </div>
  </div>
</template>
