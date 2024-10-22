import Broker from '@/pages/broker/Broker.vue'
import BrokerOverview from '@/pages/overview/BrokerOverview.vue'
import BrokerQueues from '@/pages/queues/BrokerQueues.vue'
import BrokerTopics from '@/pages/topics/BrokerTopics.vue'
import { createRouter, createWebHistory } from 'vue-router'
import Messages from '../pages/messages/Messages.vue'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      redirect: '/overview',
      component: Broker,
      children: [
        {
          path: '/overview',
          name: 'overview',
          props: true,
          component: BrokerOverview
        },
        {
          path: '/topics',
          name: 'topics',
          props: true,
          component: BrokerTopics
        },
        {
          path: '/queues',
          name: 'queues',
          props: true,
          component: BrokerQueues
        },
        {
          path: '/jobs',
          name: 'jobs',
          props: true,
          component: BrokerQueues
        }
      ]
    },
    {
      path: '/queues/:queueName',
      name: 'messages',
      props: true,
      component: Messages
    }
  ]
})

export default router
