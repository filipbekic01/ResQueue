<script setup lang="ts">
import { useIdentity } from '@/composables/identityComposable'
import { RouterLink, useRoute, useRouter, type RouteLocationAsRelativeGeneric } from 'vue-router'

const router = useRouter()

const {
  query: { data: user }
} = useIdentity()
const route = useRoute()

const goToLogin = () => router.push({ name: 'login' })

const isRoute = (to: RouteLocationAsRelativeGeneric) => route.name == to.name
</script>

<template>
  <div class="h-screen bg-[url('/vert.svg')] flex flex-col">
    <div class="gap-2 items-center center border-b border-b-100 bg-white sticky top-0">
      <div class="w-[1024px] mx-auto flex">
        <RouterLink :to="{ name: 'home' }" class="flex items-center py-3 basis-1/3">
          <div class="flex items-center justify-end bg-black p-2 rounded-lg">
            <i class="pi pi-database text-white rotate-90" style="font-size: 1.5rem"></i>
          </div>
          <div class="text-xl font-semibold px-2">ResQueue</div>
        </RouterLink>
        <div class="basis-1/3 flex gap-3 items-center grow justify-center -mb-px">
          <RouterLink
            :class="[
              'text-lg px-2 h-full flex items-center border-b',
              {
                'border-slate-200': !isRoute({ name: 'home' }),
                'border-slate-500': isRoute({ name: 'home' })
              }
            ]"
            :to="{ name: 'home' }"
            >Home</RouterLink
          >
          <RouterLink
            :class="[
              'text-lg px-2 h-full flex items-center border-b',
              {
                'border-slate-200': !isRoute({ name: 'pricing' }),
                'border-slate-500': isRoute({ name: 'pricing' })
              }
            ]"
            :to="{ name: 'pricing' }"
            >Pricing</RouterLink
          >
          <RouterLink
            :class="[
              'text-lg px-2 h-full flex items-center border-b',
              {
                'border-slate-200': !isRoute({ name: 'support' }),
                'border-slate-500': isRoute({ name: 'support' })
              }
            ]"
            :to="{ name: 'support' }"
            >Support</RouterLink
          >
        </div>
        <div class="basis-1/3 flex justify-end gap-3 py-3 items-center">
          <template v-if="user">
            <Button
              @click="router.push({ name: 'app' })"
              label="Dashboard"
              icon="pi pi-arrow-right"
              icon-pos="right"
            ></Button>
          </template>
          <template v-else>
            <Button
              outlined
              @click="goToLogin"
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
              :disabled="route.name === 'pricing'"
              :text="route.name === 'pricing'"
            ></Button>
          </template>
        </div>
      </div>
    </div>
    <div class="w-[1024px] grow mx-auto px-8">
      <slot></slot>
    </div>
    <div class="bg-white border-t border-slate-100 mt-16 py-4 text-center">
      Developed in Republic of Serbia, Novi Sad
    </div>
  </div>
</template>
