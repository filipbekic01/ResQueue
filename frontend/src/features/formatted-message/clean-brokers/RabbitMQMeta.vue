<script lang="ts" setup>
import type { RabbitMQMessageMetaDto } from '@/dtos/messages/rabbitMQMessageMetaDto'
import FormattedMessageCleanPair from '../FormattedMessageCleanPair.vue'
import FormattedMessageDivider from '../FormattedMessageDivider.vue'
import RabbitMQHeaderValue from './RabbitMQHeaderValue.vue'

defineProps<{
  metadata: RabbitMQMessageMetaDto
}>()
</script>

<template>
  <FormattedMessageCleanPair label="Redelivered" :value="metadata.redelivered" />
  <FormattedMessageCleanPair label="Exchange" :value="metadata.exchange" />
  <FormattedMessageCleanPair label="Routing Key" :value="metadata.routingKey" />

  <FormattedMessageDivider label="Properties" />

  <FormattedMessageCleanPair label="App Id" :value="metadata.properties.appId" />
  <FormattedMessageCleanPair label="Cluster Id" :value="metadata.properties.clusterId" />
  <FormattedMessageCleanPair
    label="Content Encoding"
    :value="metadata.properties.contentEncoding"
  />
  <FormattedMessageCleanPair label="Content Type" :value="metadata.properties.contentType" />
  <FormattedMessageCleanPair label="Correlation Id" :value="metadata.properties.correlationId" />
  <FormattedMessageCleanPair label="Delivery Mode" :value="metadata.properties.deliveryMode" />
  <FormattedMessageCleanPair label="Expiration" :value="metadata.properties.expiration" />
  <FormattedMessageCleanPair label="Message Id" :value="metadata.properties.messageId" />
  <FormattedMessageCleanPair label="Priority" :value="metadata.properties.priority" />
  <FormattedMessageCleanPair label="Reply To" :value="metadata.properties.replyTo" />
  <FormattedMessageCleanPair label="Timestamp" :value="metadata.properties.timestamp" />
  <FormattedMessageCleanPair label="Type" :value="metadata.properties.type" />
  <FormattedMessageCleanPair label="User Id" :value="metadata.properties.userId" />

  <FormattedMessageDivider label="Headers" />

  <FormattedMessageCleanPair
    v-for="(value, key) in metadata.properties.headers"
    :key="key"
    :label="key"
  >
    <RabbitMQHeaderValue :header-key="key" :model-value="value" />
  </FormattedMessageCleanPair>
</template>
