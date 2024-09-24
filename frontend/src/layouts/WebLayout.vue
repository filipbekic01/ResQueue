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
  <div class="flex min-h-screen flex-col">
    <div class="center sticky top-0 z-10 items-center gap-2 border-b bg-white">
      <div class="mx-auto flex w-[1024px]">
        <RouterLink :to="{ name: 'home' }" class="flex grow-0 basis-1/3 items-center py-3">
          <div class="flex items-center justify-end rounded-lg bg-black p-2">
            <i class="pi pi-database rotate-90 text-white" style="font-size: 1.5rem"></i>
          </div>
          <div class="px-2 text-xl font-semibold">ResQueue</div>
        </RouterLink>
        <div class="-mb-px flex grow basis-1/3 items-center justify-center gap-3">
          <RouterLink
            :class="[
              'flex h-full items-center border-b px-2 text-lg',
              {
                'border-slate-200': !isRoute({ name: 'home' }),
                'border-slate-700': isRoute({ name: 'home' })
              }
            ]"
            :to="{ name: 'home' }"
            >Home</RouterLink
          >
          <RouterLink
            :class="[
              'flex h-full items-center border-b px-2 text-lg',
              {
                'border-slate-200': !isRoute({ name: 'pricing' }),
                'border-slate-700': isRoute({ name: 'pricing' })
              }
            ]"
            :to="{ name: 'pricing' }"
            >Pricing</RouterLink
          >
          <RouterLink
            :class="[
              'flex h-full items-center border-b px-2 text-lg',
              {
                'border-slate-200': !isRoute({ name: 'support' }),
                'border-slate-700': isRoute({ name: 'support' })
              }
            ]"
            :to="{ name: 'support' }"
            >Support</RouterLink
          >
        </div>
        <div class="flex basis-1/3 items-center justify-end gap-3 py-3">
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
              class="cursor-pointer p-2"
              label="Login"
              icon="pi pi-sign-in"
              text
            ></Button>
            <Button
              @click="router.push({ name: 'pricing' })"
              class="cursor-pointer p-2"
              label="Free Registration"
              icon="pi pi-user-plus"
              :disabled="route.name === 'pricing'"
              :text="route.name === 'pricing'"
            ></Button>
          </template>
        </div>
      </div>
    </div>
    <div class="mx-auto w-[1024px] grow px-8">
      <slot></slot>
      <div class="mt-16 border-t py-5 text-center text-slate-500">Republic of Serbia, Novi Sad &copy; 2024</div>
    </div>
  </div>
</template>
