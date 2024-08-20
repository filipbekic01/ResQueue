import { createRouter, createWebHistory } from 'vue-router'
import Broker from '../pages/broker/Broker.vue'
import Dashboard from '../pages/dashboard/Dashboard.vue'
import Settings from '../pages/settings/Settings.vue'
import Messages from '../pages/messages/Messages.vue'
import Message from '../pages/message/Message.vue'
import Home from '../pages/home/Home.vue'
import Support from '../pages/support/Support.vue'
import Pricing from '../pages/pricing/Pricing.vue'
import BrokerQueues from '@/pages/broker/queues/BrokerQueues.vue'
import BrokerTopics from '@/pages/broker/topics/BrokerTopics.vue'
import BrokerOverview from '@/pages/broker/overview/BrokerOverview.vue'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      name: 'home',
      component: Home
    },
    {
      path: '/settings',
      name: 'settings',
      component: Settings
    },
    {
      path: '/support',
      name: 'support',
      component: Support
    },
    {
      path: '/pricing',
      name: 'pricing',
      component: Pricing
    },
    {
      path: '/app',
      name: 'app',
      component: Dashboard
    },
    {
      path: '/app/brokers/:brokerId',
      name: 'broker',
      props: true,
      component: Broker,
      children: [
        {
          path: 'overview',
          name: 'overview',
          props: true,
          component: BrokerOverview
        },
        {
          path: 'topics',
          name: 'topics',
          props: true,
          component: BrokerTopics
        },
        {
          path: 'queues',
          name: 'queues',
          props: true,
          component: BrokerQueues
        }
      ]
    },
    {
      path: '/app/brokers/:brokerId/queues/:queueId/messages',
      name: 'messages',
      props: true,
      component: Messages
    },
    {
      path: '/app/brokers/:brokerId/queues/:queueId/messages/:messageId',
      name: 'message',
      props: true,
      component: Message
    }
  ]
})

export default router
