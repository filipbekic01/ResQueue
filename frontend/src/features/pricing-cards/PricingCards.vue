<script setup lang="ts">
import { useIdentity } from '@/composables/identityComposable'
import RegisterDialog from '@/dialogs/RegisterDialog.vue'
import { useDialog } from 'primevue/usedialog'
import { useRouter } from 'vue-router'
import PricingCard from './PricingCard.vue'

const router = useRouter()
const dialog = useDialog()

const {
  query: { data: user },
  activeSubscription
} = useIdentity()

const openDialog = (plan?: string) => {
  if (user.value && activeSubscription.value) {
    router.push({
      name: 'app'
    })

    return
  }

  dialog.open(RegisterDialog, {
    data: {
      plan: plan
    },
    props: {
      header: 'Registration',
      style: {
        width: '30rem'
      },
      modal: true,
      draggable: false
    }
  })
}
</script>

<template>
  <div class="flex flex-col justify-center space-y-8 md:flex-row md:space-x-8 md:space-y-0">
    <PricingCard
      tier="Free"
      text="For Explorers"
      :features="['1 Broker', '10 Messages', 'No Teams']"
      :price="0"
      :recommended="false"
      @get-started="openDialog(undefined)"
      :disabled="!!user"
    />

    <PricingCard
      tier="Ultimate"
      text="For Teams"
      :features="['Unlimited Brokers', 'Unlimited Messages', 'Teams Collaboration']"
      :price="14.99"
      :recommended="true"
      @get-started="openDialog('ultimate')"
      :disabled="false"
    />

    <PricingCard
      tier="Essentials"
      text="For Individuals"
      :features="['Unlimited Brokres', 'Unlimited Messages', 'No Teams']"
      :price="9.99"
      :recommended="false"
      @get-started="openDialog('essentials')"
      :disabled="activeSubscription?.type === 'essentials'"
    />
  </div>
</template>
