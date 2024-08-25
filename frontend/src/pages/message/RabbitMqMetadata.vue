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
    <div class="ps-7">Properties</div>
  </div>

  <div class="flex even:bg-white">
    <div class="shrink-0 basis-96 ps-9">App Id</div>
    <div class="text-gray-500">{{ metadata.properties.appId }}</div>
  </div>

  <div class="flex even:bg-white">
    <div class="shrink-0 basis-96 ps-9">Cluster Id</div>
    <div class="text-gray-500">{{ metadata.properties.clusterId }}</div>
  </div>

  <div class="flex even:bg-white">
    <div class="shrink-0 basis-96 ps-9">Content Encoding</div>
    <div class="text-gray-500">{{ metadata.properties.contentEncoding }}</div>
  </div>

  <div class="flex even:bg-white">
    <div class="shrink-0 basis-96 ps-9">Content Type</div>
    <div class="text-gray-500">{{ metadata.properties.contentType }}</div>
  </div>

  <div class="flex even:bg-white">
    <div class="shrink-0 basis-96 ps-9">Correlation Id</div>
    <div class="text-gray-500">{{ metadata.properties.correlationId }}</div>
  </div>

  <div class="flex even:bg-white">
    <div class="shrink-0 basis-96 ps-9">Delivery Mode (TODO: convert to string)</div>
    <div class="text-gray-500">{{ metadata.properties.deliveryMode }}</div>
  </div>

  <div class="flex even:bg-white">
    <div class="shrink-0 basis-96 ps-9">Expiration</div>
    <div class="text-gray-500">{{ metadata.properties.expiration }}</div>
  </div>

  <div class="flex flex-col even:bg-white">
    <div class="px-9">Headers</div>
  </div>

  <div v-for="(value, key) in metadata.properties.headers" :key="key" class="flex even:bg-white">
    <div class="shrink-0 basis-96 ps-11">{{ key }}</div>
    <div class="text-gray-500">
      <RabbitMqHeaderValue :model-value="value" />
    </div>
  </div>

  <div class="flex even:bg-white">
    <div class="shrink-0 basis-96 ps-9">Message Id</div>
    <div class="text-gray-500">{{ metadata.properties.messageId }}</div>
  </div>

  <div class="flex even:bg-white">
    <div class="shrink-0 basis-96 ps-9">Priority</div>
    <div class="text-gray-500">{{ metadata.properties.priority }}</div>
  </div>

  <div class="flex even:bg-white">
    <div class="shrink-0 basis-96 ps-9">Reply To</div>
    <div class="text-gray-500">{{ metadata.properties.replyTo }}</div>
  </div>

  <div class="flex even:bg-white">
    <div class="shrink-0 basis-96 ps-9">Timestamp</div>
    <div class="text-gray-500">{{ metadata.properties.timestamp }}</div>
  </div>

  <div class="flex even:bg-white">
    <div class="shrink-0 basis-96 ps-9">Type</div>
    <div class="text-gray-500">{{ metadata.properties.type }}</div>
  </div>

  <div class="flex even:bg-white">
    <div class="shrink-0 basis-96 ps-9">User Id</div>
    <div class="text-gray-500">{{ metadata.properties.userId }}</div>
  </div>
</template>
