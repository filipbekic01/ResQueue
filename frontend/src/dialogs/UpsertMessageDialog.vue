<script setup lang="ts">
import { useCreateMessageMutation } from '@/api/messages/createMessageMutation'
import { useUpdateMessageMutation } from '@/api/messages/updateMessageMutation'
import RabbitMqHeadersInput from '@/components/RabbitMqHeadersInput.vue'
import type { UpsertMessageDto } from '@/dtos/upsertMessageDto'
import { extractErrorMessage } from '@/utils/errorUtils'
import type { DynamicDialogOptions } from 'primevue/dynamicdialogoptions'
import Textarea from 'primevue/textarea'
import { useToast } from 'primevue/usetoast'
import { computed, inject, reactive, ref, type Ref } from 'vue'

const toast = useToast()

const { mutateAsync: createMessageAsync, isPending: isCreateMessagePending } =
  useCreateMessageMutation()

const { mutateAsync: updateMessageAsync, isPending: isUpdateMessagePending } =
  useUpdateMessageMutation()

const dialogRef = inject<Ref<DynamicDialogOptions>>('dialogRef')

const originalMessage = computed(() => dialogRef?.value.data.message)

const newMessage = reactive<Omit<UpsertMessageDto, 'brokerId' | 'body' | 'queueId'>>({
  bodyEncoding: 'json',
  rabbitmqMetadata: {
    routingKey: '',
    properties: {
      deliveryMode: 2
    }
  }
})

const body = ref('')

if (originalMessage.value) {
  const theMsg = JSON.parse(JSON.stringify(originalMessage.value))

  newMessage.rabbitmqMetadata = theMsg.rabbitmqMetadata
  newMessage.bodyEncoding = theMsg.bodyEncoding

  if (newMessage.bodyEncoding.toLowerCase() === 'json') {
    body.value = JSON.stringify(theMsg.body, null, 4)
  } else {
    body.value = theMsg.body
  }
}

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

const submit = () => {
  const brokerId = dialogRef?.value.data.broker.id
  if (!brokerId) {
    throw new Error('brokerId is null.')
  }

  const queueId = dialogRef?.value.data.queue.id
  if (!queueId) {
    throw new Error('queueId is null.')
  }

  const dto = {
    brokerId,
    queueId,
    ...newMessage,
    body: newMessage.bodyEncoding === 'json' ? JSON.parse(body.value) : body
  }

  ;(originalMessage.value
    ? updateMessageAsync({ id: originalMessage.value.id, dto })
    : createMessageAsync(dto)
  )
    .then(() => dialogRef.value?.close())
    .catch((e) => {
      toast.add({
        severity: 'error',
        summary: 'Failed',
        detail: extractErrorMessage(e),
        life: 3000
      })
    })
}

const selectedTab = ref('body')
</script>

<template>
  <template v-if="newMessage.rabbitmqMetadata">
    <div class="flex w-[44rem] flex-col gap-4">
      <div class="flex flex-col gap-4 overflow-auto">
        <div class="flex flex-col gap-2">
          <Tabs v-model:value="selectedTab">
            <TabList>
              <Tab value="properties">Properties</Tab>
              <Tab value="headers">Headers</Tab>
              <Tab value="body">Body</Tab>
            </TabList>
          </Tabs>

          <template v-if="selectedTab === 'properties'">
            <div class="ms-4 flex flex-col gap-2">
              <div class="flex items-center gap-2">
                <label for="appId" class="w-44 text-end">App Id:</label>
                <InputText
                  v-model="newMessage.rabbitmqMetadata.properties.appId"
                  id="appId"
                  autocomplete="off"
                  class="flex-1"
                />
              </div>

              <div class="flex items-center gap-2">
                <label for="clusterId" class="w-44 text-end">Cluster Id:</label>
                <InputText
                  v-model="newMessage.rabbitmqMetadata.properties.clusterId"
                  id="clusterId"
                  autocomplete="off"
                  class="flex-1"
                />
              </div>

              <div class="flex items-center gap-2">
                <label for="contentEncoding" class="w-44 text-end">Content Encoding:</label>
                <InputText
                  v-model="newMessage.rabbitmqMetadata.properties.contentEncoding"
                  id="contentEncoding"
                  autocomplete="off"
                  class="flex-1"
                />
              </div>

              <div class="flex items-center gap-2">
                <label for="contentType" class="w-44 text-end">Content Type:</label>
                <InputText
                  v-model="newMessage.rabbitmqMetadata.properties.contentType"
                  id="contentType"
                  autocomplete="off"
                  class="flex-1"
                />
              </div>

              <div class="flex items-center gap-2">
                <label for="correlationId" class="w-44 text-end">Correlation Id:</label>
                <InputText
                  v-model="newMessage.rabbitmqMetadata.properties.correlationId"
                  id="correlationId"
                  autocomplete="off"
                  class="flex-1"
                />
              </div>

              <div class="flex items-center gap-2">
                <label class="w-44 text-end">Delivery Mode:</label>
                <Select
                  v-model="newMessage.rabbitmqMetadata.properties.deliveryMode"
                  :options="[1, 2]"
                  class="flex-1"
                />
              </div>

              <div class="flex items-center gap-2">
                <label for="expiration" class="w-44 text-end">Expiration:</label>
                <InputText
                  v-model="newMessage.rabbitmqMetadata.properties.expiration"
                  id="expiration"
                  autocomplete="off"
                  class="flex-1"
                />
              </div>

              <div class="flex items-center gap-2">
                <label for="messageId" class="w-44 text-end">Message Id:</label>
                <InputText
                  v-model="newMessage.rabbitmqMetadata.properties.messageId"
                  id="messageId"
                  autocomplete="off"
                  class="flex-1"
                />
              </div>

              <div class="flex items-center gap-2">
                <label for="priority" class="w-44 text-end">Priority:</label>
                <InputNumber
                  v-model="newMessage.rabbitmqMetadata.properties.priority"
                  id="priority"
                  autocomplete="off"
                  class="flex-1"
                />
              </div>

              <div class="flex items-center gap-2">
                <label for="replyTo" class="w-44 text-end">Reply To:</label>
                <InputText
                  v-model="newMessage.rabbitmqMetadata.properties.replyTo"
                  id="replyTo"
                  autocomplete="off"
                  class="flex-1"
                />
              </div>

              <div class="flex items-center gap-2">
                <label for="timestamp" class="w-44 text-end">Timestamp:</label>
                <InputNumber
                  v-model="newMessage.rabbitmqMetadata.properties.timestamp"
                  id="timestamp"
                  autocomplete="off"
                  class="flex-1"
                />
              </div>

              <div class="flex items-center gap-2">
                <label for="type" class="w-44 text-end">Type:</label>
                <InputText
                  v-model="newMessage.rabbitmqMetadata.properties.type"
                  id="type"
                  autocomplete="off"
                  class="flex-1"
                />
              </div>

              <div class="flex items-center gap-2">
                <label for="userId" class="w-44 text-end">User Id:</label>
                <InputText
                  v-model="newMessage.rabbitmqMetadata.properties.userId"
                  id="userId"
                  autocomplete="off"
                  class="flex-1"
                />
              </div>
            </div>
          </template>
          <template v-if="selectedTab === 'headers'">
            <RabbitMqHeadersInput
              v-model="newMessage.rabbitmqMetadata.properties.headers"
              id="headers"
              autocomplete="off"
              class="flex-1"
            />
          </template>
          <template v-if="selectedTab === 'body'">
            <Textarea
              v-model="body"
              id="body"
              placeholder="Enter body content..."
              class="min-h-[35rem] w-full"
              autocomplete="off"
            ></Textarea>
          </template>
        </div>
      </div>

      <div class="overflow-hiddens item-start flex gap-2">
        <Select
          v-model="newMessage.bodyEncoding"
          :options="encodingOptions"
          optionLabel="label"
          optionValue="value"
          placeholder="Select a format"
          class="ms-auto"
          severity="danger"
        ></Select>
        <Button
          :label="`${originalMessage ? 'Update' : 'Edit'} Message`"
          icon="pi pi-save"
          :loading="isCreateMessagePending || isUpdateMessagePending"
          @click="submit"
        ></Button>
      </div>
    </div>
  </template>
</template>
