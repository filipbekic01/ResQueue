import { createRouter, createWebHistory } from 'vue-router'
import Broker from '../pages/broker/Broker.vue'
import Dashboard from '../pages/dashboard/Dashboard.vue'
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
      path: '/app/broker/:id',
      name: 'broker',
      props: true,
      component: Broker
    }
  ]
})

export default router
