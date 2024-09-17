<script lang="ts" setup>
import { useUsersBasicQuery } from '@/api/users/usersBasicQuery'
import { computed } from 'vue'

const props = defineProps<{
  userIds: string[]
}>()

const { data: usersBasic } = useUsersBasicQuery(computed(() => props.userIds))
</script>

<template>
  <AvatarGroup class="ms-auto">
    <Avatar
      v-for="user in usersBasic"
      v-tooltip.left="user.fullName"
      :key="user.id"
      :image="user.avatar"
      size="large"
      shape="circle"
      style="background: white"
    />
  </AvatarGroup>
</template>
