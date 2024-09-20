<script lang="ts" setup>
import { useUpdateBrokerMutation } from '@/api/brokers/updateBrokerMutation'
import { useArchiveMessagesMutation } from '@/api/messages/archiveMessagesMutation'
import { usePublishMessagesMutation } from '@/api/messages/publishMessagesMutation'
import {
  useReviewMessagesMutation,
  type ReviewMessagesRequest
} from '@/api/messages/useReviewMessagesMutation'
import type { FormatOption } from '@/components/SelectFormat.vue'
import SelectFormat from '@/components/SelectFormat.vue'
import type { StructureOption } from '@/components/SelectStructure.vue'
import SelectStructure from '@/components/SelectStructure.vue'
import { useExchanges } from '@/composables/exchangesComposable'
import { useIdentity } from '@/composables/identityComposable'
import UpsertMessageDialog from '@/dialogs/UpsertMessageDialog.vue'
import type { BrokerDto } from '@/dtos/broker/brokerDto'
import type { MessageDto } from '@/dtos/message/messageDto'
import type { RabbitMQQueueDto } from '@/dtos/queue/rabbitMQQueueDto'
import { errorToToast } from '@/utils/errorUtils'
import Select from 'primevue/select'
import { useConfirm } from 'primevue/useconfirm'
import { useDialog } from 'primevue/usedialog'
import { useToast } from 'primevue/usetoast'
import { computed, ref, watch } from 'vue'

const props = defineProps<{
  messages: MessageDto[]
  broker: BrokerDto
  selectedMessageIds: string[]
  messageFormat: FormatOption
  messageStructure: StructureOption
  rabbitMqQueue: RabbitMQQueueDto
}>()

const emit = defineEmits<{
  (e: 'archive:message'): void
  (e: 'update:message-format', format: FormatOption): void
  (e: 'update:message-structure', structure: StructureOption): void
}>()

const confirm = useConfirm()
const toast = useToast()
const dialog = useDialog()

const {
  query: { data: user }
} = useIdentity()
const { mutateAsync: updateBrokerAsync } = useUpdateBrokerMutation()
const { mutateAsync: reviewMessagesAsync, isPending: isReviewMessagesPending } =
  useReviewMessagesMutation()
const { mutateAsync: archiveMessagesAsync, isPending: isArchiveMessagesPending } =
  useArchiveMessagesMutation()
const { mutateAsync: publishMessagesAsync, isPending: isPublishMessagesPending } =
  usePublishMessagesMutation()
const { formattedExchanges } = useExchanges(computed(() => props.broker.id))

const updateSelectedMessageFormat = (value: FormatOption) => {
  const access = props.broker.accessList.find((x) => x.userId === user.value?.id)

  if (props.broker && access) {
    updateBrokerAsync({
      broker: {
        ...props.broker,
        rabbitMQConnection: props.broker.rabbitMQConnection
          ? { ...props.broker.rabbitMQConnection, username: '', password: '' }
          : undefined,
        settings: {
          ...access.settings,
          messageFormat: value
        }
      },
      brokerId: props.broker.id
    }).catch((e) => toast.add(errorToToast(e)))
  }

  emit('update:message-format', value)
}

const updateSelectedMessageStructure = (value: StructureOption) => {
  const access = props.broker.accessList.find((x) => x.userId === user.value?.id)

  if (props.broker && access) {
    updateBrokerAsync({
      broker: {
        ...props.broker,
        rabbitMQConnection: props.broker.rabbitMQConnection
          ? { ...props.broker.rabbitMQConnection, username: '', password: '' }
          : undefined,
        settings: {
          ...access.settings,
          messageStructure: value
        }
      },
      brokerId: props.broker.id
    }).catch((e) => toast.add(errorToToast(e)))
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

const selectedExchange = ref()

const publishMessages = () => {
  confirm.require({
    header: 'Publish Messages',
    message: `Do you want to publish ${props.selectedMessageIds.length} messages?`,
    icon: 'pi pi-info-circle',
    rejectProps: {
      label: 'Cancel',
      severity: 'secondary',
      outlined: true
    },
    acceptProps: {
      label: 'Publish',
      severity: ''
    },
    accept: () => {
      publishMessagesAsync({
        exchangeId: selectedExchange.value.id,
        messageIds: props.selectedMessageIds
      })
        .then(() => {
          toast.add({
            severity: 'info',
            summary: 'Publish completed',
            detail: `Messages published to exchange ...`,
            life: 3000
          })
        })
        .catch((e) => toast.add(errorToToast(e)))
    },
    reject: () => {}
  })
}

const openUpsertMessageDialog = () => {
  dialog.open(UpsertMessageDialog, {
    data: {
      broker: props.broker,
      queue: props.rabbitMqQueue
    },
    props: {
      header: 'Message Editor',
      position: 'top',
      modal: true,
      draggable: false
    }
  })
}

watch(
  () => formattedExchanges.value,
  (v) => {
    const access = props.broker.accessList.find((x) => x.userId === user.value?.id)

    if (!v || selectedExchange.value || !access) {
      return
    }

    const name = props.rabbitMqQueue.parsed.name.replace(
      access.settings.deadLetterQueueSuffix ?? '',
      ''
    )

    selectedExchange.value = formattedExchanges.value.find((x) => x.parsed.name == name, '')
  },
  {
    immediate: true
  }
)

const access = props.broker.accessList.find((x) => x.userId === user.value?.id)
if (access) {
  emit(
    'update:message-structure',
    access.settings.messageStructure ? access.settings.messageStructure : 'both'
  )
  emit(
    'update:message-format',
    access.settings.messageFormat ? access.settings.messageFormat : 'clean'
  )
}
</script>

<template>
  <Button
    @click="() => reviewMessages()"
    outlined
    :loading="isReviewMessagesPending"
    :disabled="isReviewMessagesPending || !props.selectedMessageIds.length"
    :label="reviewMessagesLabel"
  ></Button>
  <Button
    @click="() => archiveMessages()"
    outlined
    :loading="isArchiveMessagesPending"
    :disabled="isArchiveMessagesPending || !props.selectedMessageIds.length"
    severity="danger"
    label="Archive"
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

  <Select
    v-model="selectedExchange"
    :options="formattedExchanges"
    optionLabel="parsed.name"
    placeholder="Select an exchange"
    class="w-72"
    filter
    severity="danger"
    :virtualScrollerOptions="{ itemSize: 38, style: 'width:900px' }"
  ></Select>
  <Button
    @click="publishMessages"
    :loading="isPublishMessagesPending"
    :disabled="isPublishMessagesPending || !selectedMessageIds.length"
    label="Requeue"
    icon="pi pi-send"
    icon-pos="right"
  ></Button>

  <Button
    @click="openUpsertMessageDialog"
    :loading="isPublishMessagesPending"
    label=""
    icon="pi pi-plus"
    icon-pos="right"
  ></Button>
</template>
