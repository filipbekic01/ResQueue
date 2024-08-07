import './assets/main.css'

import { createApp } from 'vue'
import Rc from './Rc.vue'
import router from './router'
import PrimeVue from 'primevue/config'
import Aura from '@primevue/themes/aura'
import { VueQueryPlugin } from '@tanstack/vue-query'
import ConfirmationService from 'primevue/confirmationservice'
import DialogService from 'primevue/dialogservice'
import ToastService from 'primevue/toastservice'

const app = createApp(Rc)

app.use(router)
app.use(VueQueryPlugin)
app.use(ConfirmationService)
app.use(DialogService)
app.use(ToastService)
app.use(PrimeVue, {
  ripple: true,
  theme: {
    preset: Aura,
    options: {
      darkModeSelector: '.dark-mode'
    }
  }
})

app.mount('#app')
