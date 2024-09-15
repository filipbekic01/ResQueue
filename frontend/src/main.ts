import './assets/main.scss'
import { NoirPreset } from '@/config/noirPreset'
import { QueryClient, VueQueryPlugin } from '@tanstack/vue-query'
import { createHead } from '@unhead/vue'
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
app.use(ConfirmationService)
app.use(DialogService)
app.use(ToastService)
app.use(createHead())

app.use(VueQueryPlugin, {
  queryClient: new QueryClient({
    defaultOptions: {
      queries: {
        refetchOnWindowFocus: false,
        staleTime: Infinity,
        gcTime: 5 * 60 * 1000
      }
    }
  })
})

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
