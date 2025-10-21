<script setup>
import { RouterLink, RouterView, useRoute } from 'vue-router'
import { onMounted } from 'vue'
import { Dropdown } from 'bootstrap'
import { useAuthStore } from '@/stores/auth'

const route = useRoute()
const isActive = (path) => route.path === path

const auth = useAuthStore()

// 保險：初始化所有 dropdown（若純 data-attrs 就可運作，可刪這段）
onMounted(() => {
  document.querySelectorAll('[data-bs-toggle="dropdown"]').forEach((el) => {
    // eslint-disable-next-line no-new
    new Dropdown(el)
  })
})
</script>

<template>
  <main>
    <nav
      class="custom-navbar navbar navbar navbar-expand-md navbar-dark bg-dark"
      arial-label="Furni navigation bar"
    >
      <div class="container">
        <RouterLink class="navbar-brand" to="/">Furni<span>.</span></RouterLink>

        <div class="collapse navbar-collapse" id="navbarsFurni">
          <ul class="custom-navbar-nav navbar-nav ms-auto mb-2 mb-md-0">
            <li class="nav-item" :class="{ active: isActive('/') }">
              <RouterLink class="nav-link" to="/">首頁</RouterLink>
            </li>
            <li class="nav-item" :class="{ active: isActive('/shop') }">
              <RouterLink class="nav-link" to="/shop">購物</RouterLink>
            </li>
            <li class="nav-item" :class="{ active: isActive('/about') }">
              <RouterLink class="nav-link" to="/about">關於我們</RouterLink>
            </li>
            <li class="nav-item" :class="{ active: isActive('/services') }">
              <RouterLink class="nav-link" to="/services">服務項目</RouterLink>
            </li>
            <li class="nav-item" :class="{ active: isActive('/design') }">
              <RouterLink class="nav-link" to="/design">布置靈感</RouterLink>
            </li>
            <li class="nav-item" :class="{ active: isActive('/Contact') }">
              <RouterLink class="nav-link" to="/Contact">聯絡我們</RouterLink>
            </li>
          </ul>

          < class="custom-navbar-cta navbar-nav mb-2 mb-md-0 ms-5">
          <!-- 購物車 icon -->
          <li class="nav-item">
            <RouterLink class="nav-link" to="/cart">
              <img src="/asset/images/cart.svg" />
            </RouterLink>
          </li>

          <!-- Menu -->
          <div class="collapse navbar-collapse" id="navbarsFurni">
            <ul class="custom-navbar-nav navbar-nav ms-auto mb-2 mb-md-0">
              <li class="nav-item" :class="{ active: isActive('/') }">
                <RouterLink class="nav-link" to="/">首頁</RouterLink>
              </li>
              <li class="nav-item" :class="{ active: isActive('/shop') }">
                <RouterLink class="nav-link" to="/shop">購物</RouterLink>
              </li>
              <li class="nav-item" :class="{ active: isActive('/about') }">
                <RouterLink class="nav-link" to="/about">關於我們</RouterLink>
              </li>
              <li class="nav-item" :class="{ active: isActive('/services') }">
                <RouterLink class="nav-link" to="/services">服務項目</RouterLink>
              </li>
              <li class="nav-item" :class="{ active: isActive('/design') }">
                <RouterLink class="nav-link" to="/design">布置靈感</RouterLink>
              </li>
              <li class="nav-item" :class="{ active: isActive('/Contact') }">
                <RouterLink class="nav-link" to="/Contact">聯絡我們</RouterLink>
              </li>
              <li class="nav-item" :class="{ active: isActive('/Order') }">
                <RouterLink class="nav-link" to="/Order">訂單測試</RouterLink>
              </li>
            </ul>
            <!-- 未登入 -->
            <li v-if="!auth.isLoggedIn" class="nav-item">
              <RouterLink class="nav-link" to="/signin">
                <img src="/asset/images/user.svg" />
              </RouterLink>
            </li>

            <!-- 已登入 -->
            <li v-else class="nav-item dropdown">
              <a
                class="nav-link dropdown-toggle d-flex align-items-center gap-2"
                href="#"
                role="button"
                data-bs-toggle="dropdown"
                aria-expanded="false"
              >
                <!-- ✅ 這裡改用 Pinia 計算好的完整頭像網址 :src="auth.avatarUrl" -->
                <img
                  :src="auth.avatarUrl"
                  class="rounded-circle"
                  style="width: 28px; height: 28px; object-fit: cover"
                  alt="avatar"
                />
                <span class="text-white">{{ auth.user?.name || '使用者' }}</span>
              </a>
              <ul class="dropdown-menu dropdown-menu-end">
                <li><RouterLink class="dropdown-item" to="/account">我的帳戶</RouterLink></li>
                <li><hr class="dropdown-divider" /></li>
                <li><button class="dropdown-item" @click="auth.logout()">登出</button></li>
              </ul>
            </li>
          </div>
        </div>
      </div>
    </nav>

    <RouterView />

    <!-- Footer（把 public 圖片改成 / 開頭） -->
    <footer class="footer-section">
      <div class="container relative">
        <div class="sofa-img">
          <img src="/asset/images/sofa.png" alt="Image" class="img-fluid" />
        </div>

        <div class="row">
          <div class="col-lg-8">
            <div class="subscription-form">
              <h3 class="d-flex align-items-center">
                <span class="me-1">
                  <img src="/asset/images/envelope-outline.svg" alt="Image" class="img-fluid" />
                </span>
                <span>Subscribe to Newsletter</span>
              </h3>

              <form action="#" class="row g-3">
                <div class="col-auto">
                  <input type="text" class="form-control" placeholder="Enter your name" />
                </div>
                <div class="col-auto">
                  <input type="email" class="form-control" placeholder="Enter your email" />
                </div>
                <div class="col-auto">
                  <button class="btn btn-primary"><span class="fa fa-paper-plane"></span></button>
                </div>
              </form>
            </div>
          </div>
        </div>

        <!-- 其餘不變 -->
        <!-- ... -->
      </div>
    </footer>
  </main>
</template>
