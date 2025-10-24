<script setup>
import { ref } from 'vue'
import { RouterLink, useRoute } from 'vue-router'
const route = useRoute()
const open = ref(true)
const isActive = (p) => route.path.startsWith(p)
</script>

<template>
  <aside class="border rounded-3 bg-white p-3">
    <button class="btn btn-outline-secondary w-100 d-md-none mb-3" @click="open = !open">
      {{ open ? '收合選單' : '展開選單' }}
    </button>

    <div
      :class="['list-group', 'list-group-flush', { 'd-none': !open && window.innerWidth < 768 }]"
    >
      <RouterLink
        to="/account/profile"
        class="list-group-item list-group-item-action"
        :class="{ active: isActive('/account/profile') }"
        >個人檔案</RouterLink
      >
      <RouterLink
        to="/account/security"
        class="list-group-item list-group-item-action"
        :class="{ active: isActive('/account/security') }"
        >更改密碼</RouterLink
      >
    </div>
  </aside>
</template>
