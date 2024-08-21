<script setup lang="ts">
import { useLoginMutation } from '@/api/auth/loginMutation'
import { useLogoutMutation } from '@/api/auth/logoutMutation'
import { useMeQuery } from '@/api/auth/meQuery'
import { useIdentity } from '@/composables/identityComposable'
import Dialog from 'primevue/dialog'
import { ref } from 'vue'
import { RouterLink, useRoute, useRouter, type RouteLocationAsRelativeGeneric } from 'vue-router'

const router = useRouter()

const { user } = useIdentity()
const { refetch } = useMeQuery()
const route = useRoute()

const { mutateAsync: loginAsync } = useLoginMutation()
const { mutateAsync: logoutAsync } = useLogoutMutation()

const showLogin = ref(false)
const showRegister = ref(false)

const email = ref('filip1994sm@gmail.com')
const password = ref('Password1!')

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

const isRoute = (to: RouteLocationAsRelativeGeneric) => route.name == to.name
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
      <label for="username" class="font-semibold w-24 white">E-Mail</label>
      <InputText v-model="email" id="username" class="flex-auto" type="email" autocomplete="off" />
    </div>
    <div class="flex items-center gap-4 mb-8">
      <label for="email" class="font-semibold w-24 white">Password</label>
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
      <Button type="button" label="Log In" @click="login(email, password)"></Button>
    </div>
  </Dialog>

  <div class="h-screen bg-[url('/vert.svg')] flex flex-col">
    <div class="gap-2 items-center center border-b border-b-100 bg-white sticky top-0">
      <div class="w-[1024px] mx-auto flex items-">
        <div class="basis-1/3 flex items-center py-3">
          <div class="flex items-center justify-end bg-black p-2.5 rounded-lg">
            <i class="pi pi-database text-white rotate-90" style="font-size: 1.5rem"></i>
          </div>
          <RouterLink class="text-xl font-semibold px-2" :to="{ name: 'home' }"
            >ResQueue</RouterLink
          >
        </div>
        <div class="basis-1/3 flex gap-3 items-center grow justify-center -mb-px">
          <RouterLink
            :class="[
              'text-lg px-2 h-full flex items-center border-b',
              {
                'border-gray-200': !isRoute({ name: 'home' }),
                'border-gray-500': isRoute({ name: 'home' })
              }
            ]"
            :to="{ name: 'home' }"
            >Home</RouterLink
          >
          <RouterLink
            :class="[
              'text-lg px-2 h-full flex items-center border-b',
              {
                'border-gray-200': !isRoute({ name: 'pricing' }),
                'border-gray-500': isRoute({ name: 'pricing' })
              }
            ]"
            :to="{ name: 'pricing' }"
            >Pricing</RouterLink
          >
          <RouterLink
            :class="[
              'text-lg px-2 h-full flex items-center border-b',
              {
                'border-gray-200': !isRoute({ name: 'support' }),
                'border-gray-500': isRoute({ name: 'support' })
              }
            ]"
            :to="{ name: 'support' }"
            >Support</RouterLink
          >
        </div>
        <div class="basis-1/3 flex justify-end gap-3 py-3 items-center">
          <template v-if="user">
            <Button @click="logout" label="Logout" icon="pi pi-sign-out" text></Button>
          </template>
          <template v-else>
            <Button
              outlined
              @click="showLogin = true"
              class="p-2 cursor-pointer"
              label="Login"
              icon="pi pi-sign-in"
              text
            ></Button>
            <Button
              @click="router.push({ name: 'pricing' })"
              class="p-2 cursor-pointer"
              label="Free Registration"
              icon="pi pi-user-plus"
            ></Button>
          </template>
        </div>
      </div>
    </div>
    <div class="w-[1024px] grow mx-auto px-8">
      <slot></slot>
    </div>
    <div class="bg-white border-t border-gray-100 mt-16 py-4 text-center">
      Developed in Republic of Serbia, Novi Sad
    </div>
  </div>
</template>
