import { createRouter, createWebHistory } from 'vue-router'
import Broker from '../pages/broker/Broker.vue'
import Dashboard from '../pages/dashboard/Dashboard.vue'
import Settings from '../pages/settings/Settings.vue'
import Messages from '../pages/messages/Messages.vue'
import Message from '../pages/message/Message.vue'
import Home from '../pages/home/Home.vue'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      name: 'home',
      component: Home
    },
    {
      path: '/app',
      name: 'app',
      component: Dashboard
    },
    {
      path: '/settings',
      name: 'settings',
      component: Settings
    },
    {
      path: '/app/brokers/:brokerId',
      name: 'broker',
      props: true,
      component: Broker
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
