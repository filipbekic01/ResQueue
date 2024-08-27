import './assets/main.scss'
import { NoirPreset } from '@/config/noirPreset'
import { QueryClient, VueQueryPlugin } from '@tanstack/vue-query'
import PrimeVue from 'primevue/config'
import ConfirmationService from 'primevue/confirmationservice'
import DialogService from 'primevue/dialogservice'
import ToastService from 'primevue/toastservice'
import Tooltip from 'primevue/tooltip'
import { createApp } from 'vue'
import Rc from './Rc.vue'
import router from './router'

const app = createApp(Rc)

app.use(router)

app.use(VueQueryPlugin, {
  queryClient: new QueryClient({
    defaultOptions: {
      queries: {
        refetchOnMount: false,
        refetchOnWindowFocus: false,
        staleTime: Infinity
      }
    }
  })
})

app.use(ConfirmationService)
app.use(DialogService)
app.use(ToastService)
app.use(PrimeVue, {
  ripple: true,
  theme: {
    preset: NoirPreset,
    options: {
      darkModeSelector: '.dark-mode'
    }
  }
})

app.directive('tooltip', Tooltip)

app.mount('#app')
