<script setup lang="ts">
import { usePublishNewMessageMutation } from '@/api/messages/publishNewMessageMutation'
import RabbitMqHeadersInput from '@/components/RabbitMqHeadersInput.vue'
import { useExchanges } from '@/composables/exchangesComposable'
import type { NewMessageDto } from '@/dtos/newMessageDto'
import { extractErrorMessage } from '@/utils/errorUtils'
import type { DynamicDialogOptions } from 'primevue/dynamicdialogoptions'
import Textarea from 'primevue/textarea'
import { useToast } from 'primevue/usetoast'
import { computed, inject, reactive, ref, watch, type Ref } from 'vue'

const toast = useToast()

const { mutateAsync: publishNewMessageAsync, isPending: isPublishNewMessagePending } =
  usePublishNewMessageMutation()

const dialogRef = inject<Ref<DynamicDialogOptions>>('dialogRef')
const { formattedExchanges } = useExchanges(computed(() => dialogRef?.value.data.broker.id))

const newMessage = reactive<Omit<NewMessageDto, 'brokerId' | 'body'>>({
  bodyEncoding: 'json',
  rabbitmqMetadata: {
    exchange: '',
    routingKey: '',
    properties: {
      deliveryMode: 2
    }
  }
})

const body = ref('')

const encodingOptions = [
  {
    label: 'JSON',
    value: 'json'
  },
  {
    label: 'Base64',
    value: 'base64'
  }
]

const publishMessage = () => {
  const brokerId = dialogRef?.value.data.broker.id
  if (!brokerId) {
    return
  }

  publishNewMessageAsync({
    brokerId,
    ...newMessage,
    body: newMessage.bodyEncoding === 'json' ? JSON.parse(body.value) : body
  })
    .then(() => {
      toast.add({
        severity: 'success',
        summary: 'Publish Completed',
        detail: 'Messages published to exchange ...',
        life: 3000
      })
    })
    .catch((e) => {
      toast.add({
        severity: 'error',
        summary: 'Publish Failed',
        detail: extractErrorMessage(e),
        life: 3000
      })
    })
}

watch(
  () => formattedExchanges.value,
  (v) => {
    if (!newMessage.rabbitmqMetadata) {
      return
    }

    if (!v || newMessage.rabbitmqMetadata.exchange) {
      return
    }

    const name = dialogRef?.value.data.queue.parsed.name.replace(
      dialogRef?.value.data.broker.settings.deadLetterQueueSuffix ?? '',
      ''
    )

    newMessage.rabbitmqMetadata.exchange =
      formattedExchanges.value.find((x) => x.parsed.name == name)?.parsed.name ??
      formattedExchanges.value[0] ??
      ''
  },
  {
    immediate: true
  }
)
</script>

<template>
  <div class="mb-5 flex flex-col gap-4">
    <template v-if="newMessage.rabbitmqMetadata">
      <label class="font-semibold">Excahnge</label>
      <Select
        v-model="newMessage.rabbitmqMetadata.exchange"
        :options="formattedExchanges"
        optionLabel="parsed.name"
        optionValue="parsed.name"
        placeholder="Select an exchange"
        filter
        severity="danger"
        :virtualScrollerOptions="{ itemSize: 38, style: 'width:900px' }"
      ></Select>

      <div class="flex flex-col gap-2">
        <label class="font-semibold">Properties</label>

        <div class="ms-4 flex flex-col gap-2">
          <div class="flex items-center gap-2">
            <label for="appId" class="w-36 font-semibold">App Id:</label>
            <InputText
              v-model="newMessage.rabbitmqMetadata.properties.appId"
              id="appId"
              autocomplete="off"
              class="flex-1"
            />
          </div>

          <div class="flex items-center gap-2">
            <label for="clusterId" class="w-36 font-semibold">Cluster Id:</label>
            <InputText
              v-model="newMessage.rabbitmqMetadata.properties.clusterId"
              id="clusterId"
              autocomplete="off"
              class="flex-1"
            />
          </div>

          <div class="flex items-center gap-2">
            <label for="contentEncoding" class="w-36 font-semibold">Content Encoding:</label>
            <InputText
              v-model="newMessage.rabbitmqMetadata.properties.contentEncoding"
              id="contentEncoding"
              autocomplete="off"
              class="flex-1"
            />
          </div>

          <div class="flex items-center gap-2">
            <label for="contentType" class="w-36 font-semibold">Content Type:</label>
            <InputText
              v-model="newMessage.rabbitmqMetadata.properties.contentType"
              id="contentType"
              autocomplete="off"
              class="flex-1"
            />
          </div>

          <div class="flex items-center gap-2">
            <label for="correlationId" class="w-36 font-semibold">Correlation Id:</label>
            <InputText
              v-model="newMessage.rabbitmqMetadata.properties.correlationId"
              id="correlationId"
              autocomplete="off"
              class="flex-1"
            />
          </div>

          <div class="flex items-center gap-2">
            <label class="w-36 font-semibold">Delivery Mode:</label>
            <Select
              v-model="newMessage.rabbitmqMetadata.properties.deliveryMode"
              :options="[1, 2]"
              class="flex-1"
            />
          </div>

          <div class="flex items-center gap-2">
            <label for="expiration" class="w-36 font-semibold">Expiration:</label>
            <InputText
              v-model="newMessage.rabbitmqMetadata.properties.expiration"
              id="expiration"
              autocomplete="off"
              class="flex-1"
            />
          </div>

          <div class="flex items-center gap-2">
            <label for="headers" class="w-36 font-semibold">Headers:</label>
            <RabbitMqHeadersInput
              v-model="newMessage.rabbitmqMetadata.properties.headers"
              id="headers"
              autocomplete="off"
              class="flex-1"
            />
          </div>

          <div class="flex items-center gap-2">
            <label for="messageId" class="w-36 font-semibold">Message Id:</label>
            <InputText
              v-model="newMessage.rabbitmqMetadata.properties.messageId"
              id="messageId"
              autocomplete="off"
              class="flex-1"
            />
          </div>

          <div class="flex items-center gap-2">
            <label for="priority" class="w-36 font-semibold">Priority:</label>
            <InputNumber
              v-model="newMessage.rabbitmqMetadata.properties.priority"
              id="priority"
              autocomplete="off"
              class="flex-1"
            />
          </div>

          <div class="flex items-center gap-2">
            <label for="replyTo" class="w-36 font-semibold">Reply To:</label>
            <InputText
              v-model="newMessage.rabbitmqMetadata.properties.replyTo"
              id="replyTo"
              autocomplete="off"
              class="flex-1"
            />
          </div>

          <div class="flex items-center gap-2">
            <label for="timestamp" class="w-36 font-semibold">Timestamp:</label>
            <InputNumber
              v-model="newMessage.rabbitmqMetadata.properties.timestamp"
              id="timestamp"
              autocomplete="off"
              class="flex-1"
            />
          </div>

          <div class="flex items-center gap-2">
            <label for="type" class="w-36 font-semibold">Type:</label>
            <InputText
              v-model="newMessage.rabbitmqMetadata.properties.type"
              id="type"
              autocomplete="off"
              class="flex-1"
            />
          </div>

          <div class="flex items-center gap-2">
            <label for="userId" class="w-36 font-semibold">User Id:</label>
            <InputText
              v-model="newMessage.rabbitmqMetadata.properties.userId"
              id="userId"
              autocomplete="off"
              class="flex-1"
            />
          </div>
        </div>
      </div>
    </template>

    <label class="font-semibold">Format</label>
    <Select
      v-model="newMessage.bodyEncoding"
      :options="encodingOptions"
      optionLabel="label"
      optionValue="value"
      placeholder="Select a format"
      class="w-72"
      severity="danger"
    ></Select>

    <div class="flex flex-col gap-2">
      <label for="body" class="font-semibold">Body</label>
      <Textarea v-model="body" id="body" autocomplete="off" />
    </div>
  </div>
  <div class="flex gap-2">
    <Button
      type="button"
      class="grow"
      label="Publish message"
      icon="pi pi-send"
      severity="secondary"
      :loading="isPublishNewMessagePending"
      @click="publishMessage"
    ></Button>
  </div>
</template>
