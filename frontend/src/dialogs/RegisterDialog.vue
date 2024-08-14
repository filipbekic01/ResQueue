<script lang="ts" setup>
import { useLoginMutation } from '@/api/auth/loginMutation'
import { useMeQuery } from '@/api/auth/meQuery'
import { useRegisterMutation } from '@/api/auth/registerMutation'
import { useIdentity } from '@/composables/identityComposable'
import type { DynamicDialogOptions } from 'primevue/dynamicdialogoptions'
import { inject, ref, type Ref } from 'vue'

const { user } = useIdentity()
const { mutateAsync: registerAsync } = useRegisterMutation()
const { refetch } = useMeQuery()

const { mutateAsync: loginAsync } = useLoginMutation()

const email = ref('filip1994sm@gmail.com')
const password = ref('Password1!')

const passwordType = ref('password')

const dialogRef = inject<Ref<DynamicDialogOptions>>('dialogRef')

const togglePasswordType = () =>
  (passwordType.value = passwordType.value == 'password' ? 'text' : 'password')

const register = () => {
  registerAsync({
    email: email.value,
    password: password.value
  }).then(() => {
    loginAsync({
      email: email.value,
      password: password.value
    }).then(() => {
      refetch().then((x) => {
        user.value = x.data
        dialogRef?.value.close()
      })
    })
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
