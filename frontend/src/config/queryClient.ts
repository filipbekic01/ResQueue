import { QueryClient } from '@tanstack/vue-query'

export default new QueryClient({
  defaultOptions: {
    queries: {
      refetchOnMount: false,
      refetchOnWindowFocus: false,
      staleTime: Infinity
    }
  }
})
