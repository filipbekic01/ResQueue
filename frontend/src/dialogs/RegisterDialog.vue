<script lang="ts" setup>
import { useRegisterMutation } from '@/api/auth/registerMutation'
import { ref } from 'vue'

const { mutateAsync: registerAsync } = useRegisterMutation()

const email = ref('filip1994sm@gmail.com')
const password = ref('Password1!')
const passwordAgain = ref('Password1!')

const passwordType = ref('password')

const togglePasswordType = () =>
  (passwordType.value = passwordType.value == 'password' ? 'text' : 'password')

const register = () => {
  if (password.value != passwordAgain.value) {
    return
  }

  registerAsync({
    email: email.value,
    password: password.value
  }).then(() => {
    // login(email.value, password.value)
  })
}
</script>

<template>
  <div class="flex flex-col gap-4 mb-4">
    <label for="email" class="font-semibold white">E-Mail Address</label>
    <InputText v-model="email" id="email" class="flex-auto" type="email" autocomplete="off" />
  </div>
  <div class="flex flex-col gap-4 mb-8">
    <label for="password" class="font-semibold white flex items-center"
      >Password <i class="pi pi-eye ms-2 cursor-pointer" @click="togglePasswordType"></i
    ></label>
    <InputText
      id="password"
      v-model="password"
      class="flex-auto"
      :type="passwordType"
      autocomplete="off"
    />
  </div>
  <div class="flex justify-end gap-2">
    <Button type="button" label="Cancel" severity="secondary" tabindex="-1"></Button>
    <Button type="button" label="Register" @click="register"></Button>
  </div>
</template>
