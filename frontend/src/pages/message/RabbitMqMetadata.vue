<script lang="ts" setup>
import type { RabbitmqMessageMetadataDto } from '@/dtos/rabbitmqMessageMetadataDto'
import RabbitMqHeaderValue from './RabbitMqHeaderValue.vue'

defineProps<{
  metadata: RabbitmqMessageMetadataDto
}>()
</script>

<template>
  <div class="flex even:bg-white">
    <div class="basis-96 ps-5">Redelivered</div>
    <div class="text-gray-500">{{ metadata.redelivered }}</div>
  </div>

  <div class="flex even:bg-white">
    <div class="shrink-0 basis-96 ps-5">Exchange</div>
    <div class="text-gray-500">{{ metadata.exchange }}</div>
  </div>

  <div class="flex even:bg-white">
    <div class="w-96 shrink-0 basis-96 ps-5">Routing Key</div>
    <div class="text-gray-500">{{ metadata.routingKey }}</div>
  </div>

  <div class="flex flex-col even:bg-white">
    <div class="ps-5">Properties</div>
  </div>

  <div class="flex even:bg-white">
    <div class="shrink-0 basis-96 ps-7">App Id</div>
    <div class="text-gray-500">{{ metadata.properties.appId }}</div>
  </div>

  <div class="flex even:bg-white">
    <div class="shrink-0 basis-96 ps-7">Cluster Id</div>
    <div class="text-gray-500">{{ metadata.properties.clusterId }}</div>
  </div>

  <div class="flex even:bg-white">
    <div class="shrink-0 basis-96 ps-7">Content Encoding</div>
    <div class="text-gray-500">{{ metadata.properties.contentEncoding }}</div>
  </div>

  <div class="flex even:bg-white">
    <div class="shrink-0 basis-96 ps-7">Content Type</div>
    <div class="text-gray-500">{{ metadata.properties.contentType }}</div>
  </div>

  <div class="flex even:bg-white">
    <div class="shrink-0 basis-96 ps-7">Correlation Id</div>
    <div class="text-gray-500">{{ metadata.properties.correlationId }}</div>
  </div>

  <div class="flex even:bg-white">
    <div class="shrink-0 basis-96 ps-7">Delivery Mode (TODO: convert to string)</div>
    <div class="text-gray-500">{{ metadata.properties.deliveryMode }}</div>
  </div>

  <div class="flex even:bg-white">
    <div class="shrink-0 basis-96 ps-7">Expiration</div>
    <div class="text-gray-500">{{ metadata.properties.expiration }}</div>
  </div>

  <div class="flex flex-col even:bg-white">
    <div class="px-7">Headers</div>
  </div>

  <div v-for="(value, key) in metadata.properties.headers" :key="key" class="flex even:bg-white">
    <div class="shrink-0 basis-96 ps-9">{{ key }}</div>
    <div class="text-gray-500">
      <RabbitMqHeaderValue :model-value="value" />
    </div>
  </div>

  <div class="flex even:bg-white">
    <div class="shrink-0 basis-96 ps-7">Message Id</div>
    <div class="text-gray-500">{{ metadata.properties.messageId }}</div>
  </div>

  <div class="flex even:bg-white">
    <div class="shrink-0 basis-96 ps-7">Priority</div>
    <div class="text-gray-500">{{ metadata.properties.priority }}</div>
  </div>

  <div class="flex even:bg-white">
    <div class="shrink-0 basis-96 ps-7">Reply To</div>
    <div class="text-gray-500">{{ metadata.properties.replyTo }}</div>
  </div>

  <div class="flex even:bg-white">
    <div class="shrink-0 basis-96 ps-7">Timestamp</div>
    <div class="text-gray-500">{{ metadata.properties.timestamp }}</div>
  </div>

  <div class="flex even:bg-white">
    <div class="shrink-0 basis-96 ps-7">Type</div>
    <div class="text-gray-500">{{ metadata.properties.type }}</div>
  </div>

  <div class="flex even:bg-white">
    <div class="shrink-0 basis-96 ps-7">User Id</div>
    <div class="text-gray-500">{{ metadata.properties.userId }}</div>
  </div>
</template>
