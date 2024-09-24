<script lang="ts" setup>
const faqs = [
  {
    question: `How does ResQueue operate?`,
    answer: `ResQueue synchronizes queues and topics from your connected broker. Once synced, you can access queues and pull messages. 
    Pulling messages involves acknowledging them from the remote broker's queue and saving them in ResQueue's local database. 
    Afterward, you can update, duplicate, or archive messages locally. When ready, you can requeue (publish) the messages back 
    to the broker.`
  },
  {
    question: `What is the order of messages when requeued (published) from ResQueue to the remote broker?`,
    answer: `When messages are pulled into ResQueue's database, their original order is preserved, ensuring that new messages 
    always appear at the end of the table. When publishing messages back to the broker, the original order is maintained. 
    However, be cautious when selectively requeuing specific messages, as this may disrupt the intended order.`
  },
  {
    question: `What is the difference between the "Messages" and "Inbox" columns in the queues table?`,
    answer: `The "Messages" column displays the number of messages available remotely on the broker. The "Inbox" column shows 
    the number of messages that have been acknowledged and saved locally in ResQueue’s database (these messages are no longer 
    available on the broker).`
  },
  {
    question: `How does collaboration work?`,
    answer: `Each user can add a broker. Team settings on broker page, however, are only available to users on the Ultimate plan. Initially, 
    each broker has a single owner (the broker’s creator), who can invite others to join. Create can invite more 
    members and modify their roles. All invitees must also be on the Ultimate plan.`
  },
  {
    question: `What are the different broker roles?`,
    answer: `The roles include Owner, Manager, and Agent. The Owner can invite others and assign roles. Managers can modify 
    broker settings, while Agents can only manage queue messages.`
  },
  {
    question: `Why do I need to verify my email address?`,
    answer: `Verifying your email address is essential for resetting your account password if it's ever lost. It's also necessary 
    for additional security measures, such as verifying your account in the event of a security breach.`
  },
  {
    question: `What happens if I upgrade from Essentials to Ultimate or downgrade from Ultimate to Essentials?`,
    answer: `When you upgrade from Essentials to Ultimate, your credit card will be charged the difference in price. 
    If you downgrade from Ultimate to Essentials, the difference will be credited towards your next payment, reducing 
    the amount of your future charge. If you cancel your subscription, your credit card will not be charged further, 
    and your subscription will remain active until the current billing period expires.`
  }
]
</script>

<template>
  <div class="mx-auto max-w-2xl px-6 pb-6 pt-3">
    <ul class="space-y-8">
      <li v-for="faq in faqs" :key="faq.question" class="border-b pb-4">
        <h3 class="mb-2 text-lg font-semibold text-gray-800">
          {{ faq.question }}
        </h3>
        <p class="leading-relaxed text-gray-600">
          {{ faq.answer }}
        </p>
      </li>
    </ul>
  </div>
</template>
