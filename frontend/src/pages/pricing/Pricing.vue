<script lang="ts" setup>
import { useIdentity } from '@/composables/identityComposable'
import RegisterDialog from '@/dialogs/RegisterDialog.vue'
import SubscribeDialog from '@/dialogs/SubscribeDialog.vue'
import WebLayout from '@/layouts/WebLayout.vue'
import { useDialog } from 'primevue/usedialog'
import PricingCard from './PricingCard.vue'

const dialog = useDialog()

const {
  query: { data: user }
} = useIdentity()

const openDialog = (plan?: string) => {
  if (!user.value) {
    openRegisterDialog(plan)
  } else {
    openSubscriptionDialog(plan)
  }
}

const openRegisterDialog = (plan?: string) => {
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

const openSubscriptionDialog = (plan?: string) => {
  dialog.open(SubscribeDialog, {
    data: {
      plan: plan
    },
    props: {
      header: 'Start New Subscription',
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
  <WebLayout>
    <div class="flex px-8 py-20 grow">
      <div class="flex flex-col items-center grow">
        <div class="text-4xl font-bold">Choose Your Plan</div>
        <div class="mt-4 text-xl">Help us continue building innovative and impactful tools.</div>
      </div>
    </div>
    <div class="flex flex-col md:flex-row px-16 justify-center space-y-8 md:space-y-0 md:space-x-8">
      <PricingCard
        tier="Free"
        text="Limited features"
        :features="['1 broker', '10MB Storage', 'No Teams']"
        :price="0"
        severity="secondary"
        :recommended="false"
        @get-started="openDialog(undefined)"
      />

      <PricingCard
        tier="Essentials"
        text="All features unlocked"
        :features="['Unlimited brokers (*)', '5GB Storage', 'Team Collaboration']"
        :price="7.99"
        severity="primary"
        :recommended="true"
        @get-started="openDialog('essentials')"
      />

      <PricingCard
        tier="Ultimate"
        text="All features unlocked"
        :features="['Unlimited brokers (*)', '100GB Storage', 'Team Collaboration']"
        :price="19.99"
        severity="primray"
        :recommended="false"
        @get-started="openDialog('ultimate')"
      />
    </div>

    <div class="flex justify-center mt-16">
      <RouterLink :to="{ name: 'support' }" class="cursor-pointer text-gray-600 hover:text-blue-500"
        ><i class="pi pi-question-circle me-1"></i>Experiencing issues or have additional questions?
        Get help here.</RouterLink
      >
    </div>

    <div class="mx-16 text-sm text-slate-500 pt-8">
      While we strive to keep our services accessible, the reality is that maintaining high-quality
      offerings comes with costs. From hosting and databases to ongoing development, these expenses
      are essential for us to continue delivering the tools you rely on. Your support through our
      payment plans allows us to cover these costs and keep improving. We sincerely appreciate your
      contributions and thank you for helping us grow.
    </div>
    <div class="mx-16 text-sm text-slate-500 mt-4">
      Support is provided through our dedicated Discord channel and active community forums, with
      the option for on-premise, one-to-one direct support through these channels. For custom plan
      requests, don’t hesitate to reach out through any of our support channels—we're more than
      happy to assist you.
    </div>
    <div class="mx-16 text-sm text-slate-500 mt-4">
      (*) While "unlimited" means you can add as many brokers as you need, it still refers to a
      practical, though extensive, capacity within the system.
    </div>
  </WebLayout>
</template>
