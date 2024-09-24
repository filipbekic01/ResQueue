<script lang="ts" setup>
import PricingCards from '@/features/pricing-cards/PricingCards.vue'
import WebLayout from '@/layouts/WebLayout.vue'
import { useHead } from '@unhead/vue'

useHead({
  title: 'Choose Your Plan - Queue Management Solutions',
  meta: [
    {
      name: 'description',
      content:
        'Select from our Free, Essentials, or Ultimate plans to access advanced queue management features. Your support helps us continue building innovative tools.'
    },
    {
      name: 'keywords',
      content:
        'queue management, subscription plans, pricing, free plan, essentials plan, ultimate plan, brokers, storage, team collaboration'
    },
    { name: 'robots', content: 'index, follow' }, // SEO instructions
    { property: 'og:title', content: 'Choose Your Plan - Queue Management Solutions' },
    {
      property: 'og:description',
      content:
        'Explore our pricing plans for advanced queue management solutions. From free to unlimited brokers, discover the right plan for your needs.'
    },
    { property: 'og:image', content: 'https://resqueue.io/plan-preview-image.jpg' }, // Replace with your image URL
    { property: 'og:url', content: 'https://resqueue.io/pricing' },
    { property: 'og:type', content: 'website' },
    { name: 'twitter:card', content: 'summary_large_image' },
    { name: 'twitter:title', content: 'Choose Your Plan - Queue Management Solutions' },
    {
      name: 'twitter:description',
      content:
        'Support our development by choosing a plan that fits your needs. Free, Essentials, and Ultimate plans available.'
    },
    { name: 'twitter:image', content: 'https://resqueue.io/twitter-plan-image.jpg' }
  ],
  link: [
    { rel: 'canonical', href: 'https://resqueue.io/pricing' } // Canonical URL for this page
  ],
  htmlAttrs: {
    lang: 'en'
  }
})
</script>

<template>
  <WebLayout>
    <div class="flex grow px-8 py-20">
      <div class="flex grow flex-col items-center">
        <div class="text-4xl font-bold">Choose Your Plan</div>
        <div class="mt-4 text-xl">Help us continue building innovative and impactful tools.</div>
      </div>
    </div>

    <div class="px-16">
      <PricingCards />
    </div>

    <div class="mt-16 flex justify-center">
      <RouterLink :to="{ name: 'support' }" class="cursor-pointer text-gray-600 hover:text-blue-500"
        ><i class="pi pi-question-circle me-1"></i>Experiencing issues or have additional questions? Get help
        here.</RouterLink
      >
    </div>

    <div class="mx-16 pt-8 text-sm text-slate-500">
      While we strive to keep our services accessible, the reality is that maintaining high-quality offerings comes with
      costs. From hosting and databases to ongoing development, these expenses are essential for us to continue
      delivering the tools you rely on. Your support through our payment plans allows us to cover these costs and keep
      improving. We sincerely appreciate your contributions and thank you for helping us grow.
    </div>
    <div class="mx-16 mt-4 text-sm text-slate-500">
      Support is provided through our dedicated Discord channel and active community forums, with the option for
      on-premise, one-to-one direct support through these channels. For custom plan requests, don’t hesitate to reach
      out through any of our support channels—we're more than happy to assist you.
    </div>
    <div class="mx-16 mt-4 text-sm text-slate-500">
      (*) While "unlimited" means you can add as many brokers as you need, it still refers to a practical, though
      extensive, capacity within the system.
    </div>
  </WebLayout>
</template>
