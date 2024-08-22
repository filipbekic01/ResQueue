import './assets/main.scss'
import { createApp } from 'vue'
import Rc from './Rc.vue'
import router from './router'
import PrimeVue from 'primevue/config'
import { VueQueryPlugin } from '@tanstack/vue-query'
import ConfirmationService from 'primevue/confirmationservice'
import DialogService from 'primevue/dialogservice'
import ToastService from 'primevue/toastservice'
import { NoirPreset } from '@/config/noirPreset'
import Tooltip from 'primevue/tooltip'
import queryClient from '@/config/queryClient'

const app = createApp(Rc)

app.use(router)
app.use(VueQueryPlugin, { queryClient })
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
