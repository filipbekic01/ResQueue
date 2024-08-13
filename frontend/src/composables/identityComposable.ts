import { API_URL } from '@/constants/api'
import { ref } from 'vue'

export interface User {
  email: string
}

const user = ref<User>()

export function useIdentity() {
  return {
    user
  }
}
