<script lang="ts" setup>
import { useRabbitMqMessage } from '@/composables/rabbitMqMessageComposable'
import type { MessageDto } from '@/dtos/messageDto'
import { computed, ref } from 'vue'
import { formatDistanceToNow } from 'date-fns'
import SelectButton from 'primevue/selectbutton'

const props = defineProps<{
  message: MessageDto
}>()

const options = ref(['Formatted', 'Raw View'])
const optValue = ref('Formatted')

const { rabbitMqMessage } = useRabbitMqMessage(computed(() => props.message))
</script>

<template>
  <div class="mx-5 my-3 rounded-lg">
    <!-- <div class="font-semibold bg-gray-100 rounded-lg px-2 mb-1">Message</div> -->
    <div class="mb-4 flex items-center">
      <div>
        <div class="text-2xl">Message</div>
        <div class="text-gray-500">
          Pulled {{ formatDistanceToNow(message.createdAt) }} • Updated
          {{ message.updatedAt ? formatDistanceToNow(message.updatedAt) : 'never' }}
        </div>
      </div>

      <SelectButton class="ms-auto" v-model="optValue" :options="options" aria-labelledby="basic" />
    </div>
    <div
      v-for="(value, key) in rabbitMqMessage?.parsed"
      :key="key"
      class="even:bg-gray-100/50 rounded"
    >
      <div v-if="key === 'properties'" class="bg-white rounded-lg">
        <div class="font-semibold bg-gray-100 my-1 px-2 rounded-lg">properties</div>
        <div
          v-for="(pValue, pKey) in rabbitMqMessage?.parsed.properties"
          :key="pKey"
          class="even:bg-gray-100/50 rounded ms-4"
        >
          <div v-if="pKey === 'headers'" class="bg-white rounded-lg">
            <div class="font-semibold bg-gray-100 my-1 px-2 rounded-lg">headers</div>
            <div
              v-for="(hdrV, hKey) in pValue"
              :key="hKey"
              class="even:bg-gray-100/50 px-2 rounded ms-4"
            >
              <div class="flex">
                <div class="w-96 shrink-0 basis-96">{{ hKey }}</div>
                <div class="text-gray-500">
                  <template v-if="hKey === 'MT-Fault-Message'"> ... </template>
                  <template v-else-if="hKey === 'MT-Fault-StackTrace'"> ... </template>
                  <template v-else>
                    {{ hdrV }}
                  </template>
                </div>
              </div>
            </div>
          </div>
          <div v-else class="flex">
            <div class="w-96 shrink-0 basis-96 px-2">{{ pKey }}</div>
            <div class="text-gray-500">{{ pValue }}</div>
          </div>
        </div>
      </div>
      <div v-else class="flex">
        <div class="w-96 shrink-0 basis-96 px-2">{{ key }}</div>
        <div class="text-gray-500">{{ value }}</div>
      </div>
    </div>
  </div>
</template>
