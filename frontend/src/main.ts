import "./assets/style.css";
import { QueryClient, VueQueryPlugin } from "@tanstack/vue-query";
import { createApp } from "vue";
import RootCompoment from "./RootCompoment.vue";
import router from "./router";

const app = createApp(RootCompoment);

app.use(router);

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
});

app.mount("#app");
