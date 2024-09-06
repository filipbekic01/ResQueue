import type { FormatOption } from '@/components/SelectFormat.vue'
import type { StructureOption } from '@/components/SelectStructure.vue'

export interface BrokerSettingsDto {
  quickSearches: string[]
  deadLetterQueueSuffix: string
  messageFormat: FormatOption
  messageStructure: StructureOption
}
