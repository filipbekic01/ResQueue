<script lang="ts" setup>
import { useUpdateBrokerMutation } from '@/api/broker/updateBrokerMutation'
import { useArchiveMessagesMutation } from '@/api/messages/archiveMessagesMutation'
import {
  useReviewMessagesMutation,
  type ReviewMessagesRequest
} from '@/api/messages/useReviewMessagesMutation'
import type { FormatOption } from '@/components/SelectFormat.vue'
import SelectFormat from '@/components/SelectFormat.vue'
import type { StructureOption } from '@/components/SelectStructure.vue'
import SelectStructure from '@/components/SelectStructure.vue'
import type { BrokerDto } from '@/dtos/brokerDto'
import type { MessageDto } from '@/dtos/messageDto'
import { useConfirm } from 'primevue/useconfirm'
import { useToast } from 'primevue/usetoast'
import { computed } from 'vue'

const props = defineProps<{
  messages: MessageDto[]
  broker: BrokerDto
  selectedMessageIds: string[]
  messageFormat: FormatOption
  messageStructure: StructureOption
}>()

const emit = defineEmits<{
  (e: 'archive:message'): void
  (e: 'update:message-format', format: FormatOption): void
  (e: 'update:message-structure', structure: StructureOption): void
}>()

const confirm = useConfirm()
const toast = useToast()

const { mutateAsync: updateBrokerAsync } = useUpdateBrokerMutation()
const { mutateAsync: reviewMessagesAsync, isPending: isReviewMessagesPending } =
  useReviewMessagesMutation()
const { mutateAsync: archiveMessagesAsync, isPending: isArchiveMessagesPending } =
  useArchiveMessagesMutation()

const updateSelectedMessageFormat = (value: FormatOption) => {
  if (props.broker) {
    updateBrokerAsync({
      broker: {
        ...props.broker,
        settings: {
          ...props.broker.settings,
          messageFormat: value
        },
        username: '',
        password: ''
      },
      brokerId: props.broker.id
    })
  }

  emit('update:message-format', value)
}

const updateSelectedMessageStructure = (value: StructureOption) => {
  if (props.broker) {
    updateBrokerAsync({
      broker: {
        ...props.broker,
        settings: {
          ...props.broker.settings,
          messageStructure: value
        },
        username: '',
        password: ''
      },
      brokerId: props.broker.id
    })
  }

  emit('update:message-structure', value)
}

const reviewMessages = () => {
  const notReviewedIds =
    props.messages
      ?.filter((x) => props.selectedMessageIds.includes(x.id))
      .filter((x) => !x.isReviewed)
      .map((x) => x.id) ?? []

  const reviewedIds =
    props.messages
      ?.filter((x) => props.selectedMessageIds.includes(x.id))
      .filter((x) => x.isReviewed)
      .map((x) => x.id) ?? []

  let request: ReviewMessagesRequest = {
    idsToFalse: [],
    idsToTrue: []
  }

  if (reviewedIds.length && !notReviewedIds.length) {
    request.idsToFalse = reviewedIds
  } else if (request.idsToFalse.length && request.idsToTrue.length) {
    request.idsToTrue = notReviewedIds
  } else {
    request.idsToTrue = notReviewedIds
  }

  reviewMessagesAsync(request).then(() => {
    toast.add({
      severity: 'info',
      summary: 'Marked as reviewed',
      detail: `Messages successfully reviewed`,
      life: 3000
    })
  })
}

const archiveMessages = () => {
  confirm.require({
    header: 'Archive Messages',
    message: `Do you want to archive ${props.selectedMessageIds.length} messages?`,
    icon: 'pi pi-info-circle',
    rejectProps: {
      label: 'Cancel',
      severity: 'secondary',
      outlined: true
    },
    acceptProps: {
      label: 'Archive',
      severity: 'danger'
    },
    accept: () => {
      archiveMessagesAsync(props.selectedMessageIds).then(() => {
        emit('archive:message')

        toast.add({
          severity: 'info',
          summary: 'Archived Messages',
          detail: `Messages successfully reviewed!`,
          life: 3000
        })
      })
    },
    reject: () => {}
  })
}

const reviewMessagesLabel = computed(() => {
  let message = 'Mark as Reviewed'

  if (
    props.selectedMessageIds.length >= 1 &&
    props.messages
      ?.filter((x) => props.selectedMessageIds.includes(x.id))
      .every((x) => x.isReviewed)
  ) {
    message = 'Mark as Unreviewed'
  }

  return message
})
</script>

<template>
  <Button
    @click="() => reviewMessages()"
    outlined
    :loading="isReviewMessagesPending"
    :disabled="isReviewMessagesPending || !props.selectedMessageIds.length"
    :label="reviewMessagesLabel"
    icon="pi pi-check"
  ></Button>
  <Button
    @click="() => archiveMessages()"
    outlined
    :loading="isArchiveMessagesPending"
    :disabled="isArchiveMessagesPending || !props.selectedMessageIds.length"
    severity="danger"
    icon="pi pi-trash"
  ></Button>

  <SelectStructure
    :model-value="messageStructure"
    @update:model-value="(e) => updateSelectedMessageStructure(e)"
    class="ms-auto"
  />

  <SelectFormat
    :model-value="messageFormat"
    @update:model-value="(e) => updateSelectedMessageFormat(e)"
  />
</template>
