import type { UserDto } from '@/dtos/userDto'
import { ref } from 'vue'

const user = ref<UserDto>()

export function useIdentity() {
  return {
    user
  }
}
