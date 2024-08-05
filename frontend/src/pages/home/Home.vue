<script lang="ts" setup>
import { useIdentity } from '@/composables/identityComposable'
import WebLayout from '@/layouts/WebLayout.vue'
import Button from 'primevue/button'

const { user } = useIdentity()
</script>

<template>
  <WebLayout>
    <div class="text-center">
      <div class="text-2xl mx-auto mt-10 text-center">Rescue queue loads</div>
      <div class="mx-auto mt-10 text-center">resqueue</div>

      <Button v-if="!user" class="mt-4">Register</Button>
      <RouterLink v-else :to="{ name: 'app' }"><Button class="mt-4">Open App</Button></RouterLink>
    </div>
    <div class="flex gap-4 mt-16">
      <div class="bg-gray-200 h-48 flex-1 text-gray-700 rounded-lg p-2">
        Lorem Ipsum!<br />
        Lorem ipsum dolor sit amet, consectetur adipiscing elit. In fringilla ligula at erat tempor
        dictum. Lorem ipsum dolor sit amet, consectetur adipiscing elit. Etiam non viverra dolor.
      </div>
      <div class="bg-gray-200 h-48 flex-1 text-gray-700 rounded-lg p-2">
        Lorem Ipsum!<br />
        Lorem ipsum dolor sit amet, consectetur adipiscing elit. In fringilla ligula at erat tempor
        dictum. Lorem ipsum dolor sit amet, consectetur adipiscing elit. Etiam non viverra dolor.
      </div>
      <div class="bg-gray-200 h-48 flex-1 text-gray-700 rounded-lg p-2">
        Lorem Ipsum!<br />
        Lorem ipsum dolor sit amet, consectetur adipiscing elit. In fringilla ligula at erat tempor
        dictum. Lorem ipsum dolor sit amet, consectetur adipiscing elit. Etiam non viverra dolor.
      </div>
    </div>
    <div class="flex gap-4 mt-4">
      <div class="bg-gray-200 h-48 flex-1 text-gray-700 rounded-lg p-2">
        Lorem Ipsum!<br />
        Lorem ipsum dolor sit amet, consectetur adipiscing elit. In fringilla ligula at erat tempor
        dictum. Lorem ipsum dolor sit amet, consectetur adipiscing elit. Etiam non viverra dolor.
      </div>
      <div class="bg-gray-200 h-48 flex-1 text-gray-700 rounded-lg p-2">
        Lorem Ipsum!<br />
        Lorem ipsum dolor sit amet, consectetur adipiscing elit. In fringilla ligula at erat tempor
        dictum. Lorem ipsum dolor sit amet, consectetur adipiscing elit. Etiam non viverra dolor.
      </div>
      <div class="bg-gray-200 h-48 flex-1 text-gray-700 rounded-lg p-2">
        Lorem Ipsum!<br />
        Lorem ipsum dolor sit amet, consectetur adipiscing elit. In fringilla ligula at erat tempor
        dictum. Lorem ipsum dolor sit amet, consectetur adipiscing elit. Etiam non viverra dolor.
      </div>
    </div>
  </WebLayout>
</template>
