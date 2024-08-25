<script lang="ts" setup>
import type { RabbitmqMessageMetadataDto } from '@/dtos/rabbitmqMessageMetadataDto'

defineProps<{
  metadata: RabbitmqMessageMetadataDto
}>()
</script>

<template>
  <div class="flex even:bg-white">
    <div class="w-96 shrink-0 basis-96 px-2">Redelivered</div>
    <div class="text-gray-500">{{ metadata.redelivered }}</div>
  </div>

  <div class="flex even:bg-white">
    <div class="w-96 shrink-0 basis-96 px-2">Exchange</div>
    <div class="text-gray-500">{{ metadata.exchange }}</div>
  </div>

  <div class="flex even:bg-white">
    <div class="w-96 shrink-0 basis-96 px-2">Routing Key</div>
    <div class="text-gray-500">{{ metadata.routingKey }}</div>
  </div>

  <div class="flex flex-col even:bg-white">
    <div class="px-2">Properties</div>

    <div class="flex ps-3 even:bg-white">
      <div class="w-96 shrink-0 basis-96 px-2">App Id</div>
      <div class="text-gray-500">{{ metadata.properties.appId }}</div>
    </div>

    <div class="flex ps-3 even:bg-white">
      <div class="w-96 shrink-0 basis-96 px-2">Cluster Id</div>
      <div class="text-gray-500">{{ metadata.properties.clusterId }}</div>
    </div>

    <div class="flex ps-3 even:bg-white">
      <div class="w-96 shrink-0 basis-96 px-2">Content Encoding</div>
      <div class="text-gray-500">{{ metadata.properties.contentEncoding }}</div>
    </div>

    <div class="flex ps-3 even:bg-white">
      <div class="w-96 shrink-0 basis-96 px-2">Content Type</div>
      <div class="text-gray-500">{{ metadata.properties.contentType }}</div>
    </div>

    <div class="flex ps-3 even:bg-white">
      <div class="w-96 shrink-0 basis-96 px-2">Correlation Id</div>
      <div class="text-gray-500">{{ metadata.properties.correlationId }}</div>
    </div>

    <div class="flex ps-3 even:bg-white">
      <div class="w-96 shrink-0 basis-96 px-2">Delivery Mode (TODO: convert to string)</div>
      <div class="text-gray-500">{{ metadata.properties.deliveryMode }}</div>
    </div>

    <div class="flex ps-3 even:bg-white">
      <div class="w-96 shrink-0 basis-96 px-2">Expiration</div>
      <div class="text-gray-500">{{ metadata.properties.expiration }}</div>
    </div>

    <div class="flex ps-3 flex-col even:bg-white">
      <div class="px-2">Headers</div>
      <div v-for="(value, key) in metadata.properties.headers" :key="key" class="flex ps-3  even:bg-white">
        <div class="w-96 shrink-0 basis-96">{{ key }}</div>
        <div class="text-gray-500">{{ value }}</div>
      </div>
    </div>
<!-- 
    <div class="flex ps-3 even:bg-white">
      <div class="px-2">Headers</div>
      <div class="bg-white rounded-lg">
        <div
          v-for="(hdrV, hKey) in metadata.properties.headers"
          :key="hKey"
          class="even:bg-slate-100/50 px-2 rounded"
        >
          <div class="flex">
            <div class="">{{ hKey }}</div>
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
    </div> -->

    <div class="flex ps-3 even:bg-white">
      <div class="w-96 shrink-0 basis-96 px-2">Message Id</div>
      <div class="text-gray-500">{{ metadata.properties.messageId }}</div>
    </div>

    <div class="flex ps-3 even:bg-white">
      <div class="w-96 shrink-0 basis-96 px-2">Priority</div>
      <div class="text-gray-500">{{ metadata.properties.priority }}</div>
    </div>

    <div class="flex ps-3 even:bg-white">
      <div class="w-96 shrink-0 basis-96 px-2">Reply To</div>
      <div class="text-gray-500">{{ metadata.properties.replyTo }}</div>
    </div>

    <div class="flex ps-3 even:bg-white">
      <div class="w-96 shrink-0 basis-96 px-2">Timestamp</div>
      <div class="text-gray-500">{{ metadata.properties.timestamp }}</div>
    </div>

    <div class="flex ps-3 even:bg-white">
      <div class="w-96 shrink-0 basis-96 px-2">Type</div>
      <div class="text-gray-500">{{ metadata.properties.type }}</div>
    </div>

    <div class="flex ps-3 even:bg-white">
      <div class="w-96 shrink-0 basis-96 px-2">User Id</div>
      <div class="text-gray-500">{{ metadata.properties.userId }}</div>
    </div>
  </div>
</template>
