<script lang="ts" setup>
import type { RabbitMqMessageDto } from '@/dtos/rabbitMqMessageDto'
import { ref, watch } from 'vue'

// const props = defineProps<{
//   queueId: string
// }>()

const emit = defineEmits<{
  (e: 'open', messageId: string): void
  (e: 'select', messageIds: string[]): void
}>()

// const { messages } = useRabbitMqMessages(props.queueId)

const selectedMessages = ref<RabbitMqMessageDto[]>()

watch(
  () => selectedMessages.value,
  (x) => {
    emit('select', x?.map((x) => x.id) ?? [])
  }
)
</script>

<!-- <template></template> -->
