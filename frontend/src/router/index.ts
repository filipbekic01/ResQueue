import resqueueConfig from '@/config/resqueue'
import Broker from '@/pages/broker/Broker.vue'
import BrokerOverview from '@/pages/broker/overview/BrokerOverview.vue'
import BrokerQueues from '@/pages/broker/queues/BrokerQueues.vue'
import BrokerRecurringJobs from '@/pages/broker/recurring-jobs/BrokerRecurringJobs.vue'
import BrokerTopics from '@/pages/broker/topics/BrokerTopics.vue'
import { createRouter, createWebHistory } from 'vue-router'
import Messages from '../pages/messages/Messages.vue'

const router = createRouter({
  history: createWebHistory('/' + resqueueConfig.prefix),
  routes: [
    {
      path: '',
      redirect: {
        name: 'queues',
      },
      component: Broker,
      children: [
        {
          path: '/overview',
          name: 'overview',
          props: true,
          component: BrokerOverview,
        },
        {
          path: '/topics',
          name: 'topics',
          props: true,
          component: BrokerTopics,
        },
        {
          path: '/queues',
          name: 'queues',
          props: true,
          component: BrokerQueues,
        },
        {
          path: '/recurring-jobs',
          name: 'recurring-jobs',
          props: true,
          component: BrokerRecurringJobs,
        },
      ],
    },
    {
      path: '/queues/:queueName',
      name: 'messages',
      props: true,
      component: Messages,
    },
  ],
})

export default router
