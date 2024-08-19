<script lang="ts" setup>
import WebLayout from '@/layouts/WebLayout.vue'
import PricingCard from './PricingCard.vue'
</script>

<template>
  <WebLayout>
    <div class="flex px-8 py-14 grow">
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
      />

      <PricingCard
        tier="Essentials"
        text="All features unlocked"
        :features="['Unlimited brokers', '5GB Storage', 'Team Collaboration']"
        :price="7.99"
        severity="success"
        :recommended="true"
      />

      <PricingCard
        tier="Ultimate"
        text="All features unlocked"
        :features="['Unlimited brokers', '100GB Storage', 'Team Collaboration']"
        :price="19.99"
        severity="primary"
        :recommended="false"
      />
    </div>
  </WebLayout>
</template>
