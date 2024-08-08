<script lang="ts" setup>
import { useMessagesQuery } from '@/api/messages/messagesQuery'
import AppLayout from '@/layouts/AppLayout.vue'
import { computed } from 'vue'
import { useRouter } from 'vue-router'

const props = defineProps<{
  brokerId: string
  queueId: string
  messageId: string
}>()

const router = useRouter()

const { data: messages } = useMessagesQuery(props.queueId)

const message = computed(() => messages.value?.find((x) => x.id === props.messageId))

const rabbitMqMessage = computed(() => {
  if (!message.value) {
    return {}
  }

  return {
    ...message.value,
    ...JSON.parse(message.value.rawData)
  }
})

const backToMessages = () => {
  router.push({
    name: 'messages',
    params: {
      brokerId: props.brokerId,
      queueId: props.queueId
    }
  })
}
</script>

<template>
  <AppLayout>
    <div>
      <Button @click="backToMessages">Back to messages</Button>
    </div>
    <div v-for="(value, key) in rabbitMqMessage" :key="key" class="flex border-gray-200 border-b">
      <div class="basis-44 shrink-0">{{ key }}:</div>
      <div class="text-green-600 shrink-o">{{ value }}</div>
    </div>
  </AppLayout>
</template>
