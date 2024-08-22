<script lang="ts" setup>
import { useRabbitMqMessage } from '@/composables/rabbitMqMessageComposable'
import type { MessageDto } from '@/dtos/messageDto'
import { formatDistanceToNow } from 'date-fns'
import SelectButton from 'primevue/selectbutton'
import { computed, ref } from 'vue'

const props = defineProps<{
  message: MessageDto
}>()

const options = ref(['Formatted', 'Raw View'])
const optValue = ref('Formatted')

const { rabbitMqMessage } = useRabbitMqMessage(computed(() => props.message))
</script>

<template>
  <div class="mx-5 my-3 rounded-lg">
    <!-- <div class="font-semibold bg-slate-100 rounded-lg px-2 mb-1">Message</div> -->
    <div class="mb-4 flex items-center">
      <div>
        <div class="text-2xl">Message</div>
        <div class="text-slate-500">
          Pulled {{ formatDistanceToNow(message.createdAt) }} • Updated
          {{ message.updatedAt ? formatDistanceToNow(message.updatedAt) : 'never' }}
        </div>
      </div>

      <SelectButton class="ms-auto" v-model="optValue" :options="options" aria-labelledby="basic" />
    </div>
    <div v-if="message.rabbitmqMetadata" class="bg-gray-100/50 rounded">
      <div class="font-semibold my-1 px-2 rounded-lg">Metadata</div>
      <div class="flex">
        <div class="w-96 shrink-0 basis-96 px-2">Redelivered</div>
        <div class="text-gray-500">{{ message.rabbitmqMetadata.redelivered }}</div>
      </div>

      <div class="flex">
        <div class="w-96 shrink-0 basis-96 px-2">Exchange</div>
        <div class="text-gray-500">{{ message.rabbitmqMetadata.exchange }}</div>
      </div>

      <div class="flex">
        <div class="w-96 shrink-0 basis-96 px-2">Routing Key</div>
        <div class="text-gray-500">{{ message.rabbitmqMetadata.routingKey }}</div>
      </div>

      <div class="flex flex-col">
        <div class="px-2">Properties</div>
        <div class="flex ps-3">
          <div class="w-96 shrink-0 basis-96 px-2">App Id</div>
          <div class="text-gray-500">{{ message.rabbitmqMetadata.properties.appId }}</div>
        </div>

        <div class="flex ps-3">
          <div class="w-96 shrink-0 basis-96 px-2">Cluster Id</div>
          <div class="text-gray-500">{{ message.rabbitmqMetadata.properties.clusterId }}</div>
        </div>

        <div class="flex ps-3">
          <div class="w-96 shrink-0 basis-96 px-2">Content Encoding</div>
          <div class="text-gray-500">{{ message.rabbitmqMetadata.properties.contentEncoding }}</div>
        </div>

        <div class="flex ps-3">
          <div class="w-96 shrink-0 basis-96 px-2">Content Type</div>
          <div class="text-gray-500">{{ message.rabbitmqMetadata.properties.contentType }}</div>
        </div>

        <div class="flex ps-3">
          <div class="w-96 shrink-0 basis-96 px-2">Correlation Id</div>
          <div class="text-gray-500">{{ message.rabbitmqMetadata.properties.correlationId }}</div>
        </div>

        <div class="flex ps-3">
          <div class="w-96 shrink-0 basis-96 px-2">Delivery Mode (TODO: convert to string)</div>
          <div class="text-gray-500">{{ message.rabbitmqMetadata.properties.deliveryMode }}</div>
        </div>

        <div class="flex ps-3">
          <div class="w-96 shrink-0 basis-96 px-2">Expiration</div>
          <div class="text-gray-500">{{ message.rabbitmqMetadata.properties.expiration }}</div>
        </div>

        <div class="flex ps-3">
          <div class="w-96 shrink-0 basis-96 px-2">Headers</div>
          <div class="bg-white rounded-lg">
            <div
              v-for="(hdrV, hKey) in message.rabbitmqMetadata.properties.headers"
              :key="hKey"
              class="even:bg-slate-100/50 px-2 rounded ms-4"
            >
              <div class="flex">
                <div class="w-96 shrink-0 basis-96">{{ hKey }}</div>
                <div class="text-slate-500">
                  <template v-if="hKey === 'MT-Fault-Message'"> ... </template>
                  <template v-else-if="hKey === 'MT-Fault-StackTrace'"> ... </template>
                  <template v-else>
                    {{ hdrV }}
                  </template>
                </div>
              </div>
            </div>
          </div>
        </div>

        <div class="flex ps-3">
          <div class="w-96 shrink-0 basis-96 px-2">Message Id</div>
          <div class="text-gray-500">{{ message.rabbitmqMetadata.properties.messageId }}</div>
        </div>

        <div class="flex ps-3">
          <div class="w-96 shrink-0 basis-96 px-2">Priority</div>
          <div class="text-gray-500">{{ message.rabbitmqMetadata.properties.priority }}</div>
        </div>

        <div class="flex ps-3">
          <div class="w-96 shrink-0 basis-96 px-2">Reply To</div>
          <div class="text-gray-500">{{ message.rabbitmqMetadata.properties.replyTo }}</div>
        </div>

        <div class="flex ps-3">
          <div class="w-96 shrink-0 basis-96 px-2">Timestamp</div>
          <div class="text-gray-500">{{ message.rabbitmqMetadata.properties.timestamp }}</div>
        </div>

        <div class="flex ps-3">
          <div class="w-96 shrink-0 basis-96 px-2">Type</div>
          <div class="text-gray-500">{{ message.rabbitmqMetadata.properties.type }}</div>
        </div>

        <div class="flex ps-3">
          <div class="w-96 shrink-0 basis-96 px-2">User Id</div>
          <div class="text-gray-500">{{ message.rabbitmqMetadata.properties.userId }}</div>
        </div>
      </div>
    </div>

    <div class="bg-gray-100/50 rounded">
      <div class="font-semibold my-1 rounded-lg">Body</div>
      <div class="text-gray-500">{{ message.body }}</div>
    </div>
  </div>
</template>
