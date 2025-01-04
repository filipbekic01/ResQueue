import './assets/main.scss'
import { QueryClient, VueQueryPlugin } from '@tanstack/vue-query'
import PrimeVue from 'primevue/config'
import ConfirmationService from 'primevue/confirmationservice'
import DialogService from 'primevue/dialogservice'
import ToastService from 'primevue/toastservice'
import Tooltip from 'primevue/tooltip'
import { createApp } from 'vue'
import RootCompoment from './RootCompoment.vue'
import router from './router'

const app = createApp(RootCompoment)

app.use(router)
app.use(ConfirmationService)
app.use(DialogService)
app.use(ToastService)

app.use(VueQueryPlugin, {
  queryClient: new QueryClient({
    defaultOptions: {
      queries: {
        refetchOnWindowFocus: false,
        staleTime: Infinity,
        gcTime: 5 * 60 * 1000,
      },
    },
  }),
})

app.use(PrimeVue, {
  ripple: true,
  theme: 'none',
})

app.directive('tooltip', Tooltip)

app.mount('#app')
