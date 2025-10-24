<script setup>
import { RouterLink, RouterView, useRoute } from 'vue-router'
const route = useRoute()
const isActive = (name) => route.name === name
</script>

<template>
  <section class="container py-4 py-md-5">
    <div class="row">
      <!-- 左側選單 -->
      <aside class="col-12 col-md-3 mb-4 mb-md-0">
        <div class="list-group shadow-sm">
          <RouterLink
            :class="[
              'list-group-item list-group-item-action',
              { active: isActive('AccountProfile') },
            ]"
            to="/account/profile"
          >
            個人檔案
          </RouterLink>
          <RouterLink
            :class="[
              'list-group-item list-group-item-action',
              { active: isActive('AccountSecurity') },
            ]"
            to="/account/security"
          >
            更改密碼
          </RouterLink>
        </div>
      </aside>

      <!-- 右側內容（務必有這個！） -->
      <div class="col-12 col-md-9">
        <RouterView />
      </div>
    </div>
  </section>
</template>
