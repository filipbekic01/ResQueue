<script lang="ts" setup>
import WebLayout from '@/layouts/WebLayout.vue'
import PricingCard from './PricingCard.vue'
import RegisterDialog from '@/dialogs/RegisterDialog.vue'
import { useDialog } from 'primevue/usedialog'

const dialog = useDialog()

const openRegisterDialog = (pricingId?: string) =>
  dialog.open(RegisterDialog, {
    data: {
      pricingId
    },
    props: {
      header: 'Registration',
      style: {
        width: '30rem'
      },
      modal: true
    }
  })
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
        @get-started="openRegisterDialog(undefined)"
      />

      <PricingCard
        tier="Essentials"
        text="All features unlocked"
        :features="['Unlimited brokers (*)', '5GB Storage', 'Team Collaboration']"
        :price="7.99"
        severity="info"
        :recommended="true"
        @get-started="openRegisterDialog('prod_QhMwOLV4urqRjA')"
      />

      <PricingCard
        tier="Ultimate"
        text="All features unlocked"
        :features="['Unlimited brokers (*)', '100GB Storage', 'Team Collaboration']"
        :price="19.99"
        severity="primray"
        :recommended="false"
        @get-started="openRegisterDialog('prod_QhMxhOpIthNWcs')"
      />
    </div>

    <div class="mx-16 text-sm text-gray-500 pt-24">
      While we strive to keep our services accessible, the reality is that maintaining high-quality
      offerings comes with costs. From hosting and databases to ongoing development, these expenses
      are essential for us to continue delivering the tools you rely on. Your support through our
      payment plans allows us to cover these costs and keep improving. We sincerely appreciate your
      contributions and thank you for helping us grow.
    </div>
    <div class="mx-16 text-sm text-gray-500 mt-4">
      Support is provided through our dedicated Discord channel and active community forums, with
      the option for on-premise, one-to-one direct support through these channels. For custom plan
      requests, don’t hesitate to reach out through any of our support channels—we're more than
      happy to assist you.
    </div>
    <div class="mx-16 text-sm text-gray-500 mt-4">
      (*) While "unlimited" means you can add as many brokers as you need, it still refers to a
      practical, though extensive, capacity within the system.
    </div>
  </WebLayout>
</template>
