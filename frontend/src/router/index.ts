import BrokerInvitation from '@/pages/broker-invitation/BrokerInvitation.vue'
import BrokerOverview from '@/pages/broker/overview/BrokerOverview.vue'
import BrokerQueues from '@/pages/broker/queues/BrokerQueues.vue'
import BrokerTopics from '@/pages/broker/topics/BrokerTopics.vue'
import ForgotPassword from '@/pages/forgot-password/ForgotPassword.vue'
import Login from '@/pages/login/Login.vue'
import Updates from '@/pages/updates/Updates.vue'
import { createRouter, createWebHistory } from 'vue-router'
import Broker from '../pages/broker/Broker.vue'
import ControlPanel from '../pages/control-panel/ControlPanel.vue'
import Home from '../pages/home/Home.vue'
import Messages from '../pages/messages/Messages.vue'
import Pricing from '../pages/pricing/Pricing.vue'
import Support from '../pages/support/Support.vue'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      name: 'home',
      component: Home
    },
    {
      path: '/login',
      name: 'login',
      component: Login
    },
    {
      path: '/forgot-password',
      name: 'forgot-passowrd',
      component: ForgotPassword
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
      component: ControlPanel
    },
    {
      path: '/app/updates',
      name: 'updates',
      component: Updates
    },
    {
      path: '/app/broker-invitation',
      name: 'broker-invitation',
      component: BrokerInvitation
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
    }
  ]
})

export default router
