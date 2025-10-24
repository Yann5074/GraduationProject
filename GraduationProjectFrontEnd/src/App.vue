<script setup>
import { RouterLink, RouterView, useRoute, useRouter } from 'vue-router'
import { onMounted, ref } from 'vue'
import { Dropdown } from 'bootstrap'
import http from '../src/api/axios'
import { useAuthStore } from './stores/auth'

const route = useRoute()
const router = useRouter()
const auth = useAuthStore()
const isActive = (path) => route.path === path // 判斷是否為當前頁

const loggingOut = ref(false) // 登出動作的狀態

// 確認前端登出動作，避免重複操作
async function handleLogout() {
  if (loggingOut.value) return // 確認是否正在登出
  loggingOut.value = true
  try {
    // 1) 呼叫後端登出（空 body 即可）
    await http.post('/Member/logout')

    // 2) 清前端登入狀態
    auth.logout()

    // 3) 導回登入頁或首頁（擇一）
    router.push('/') // 或改成 '/'
  } catch (err) {
    console.error('logout failed:', err?.response || err)
    // 就算後端失敗，仍清前端並導頁（避免卡住）
    auth.logout()
    router.push('/signin')
  } finally {
    loggingOut.value = false
  }
}

// Dropdown 初始化（保險）
onMounted(() => {
  document.querySelectorAll('[data-bs-toggle="dropdown"]').forEach((el) => {
    // eslint-disable-next-line no-new
    new Dropdown(el)
  })
})
</script>

<template>
  <main>
    <!-- 自己加的聊天室浮動元件 -->
    <ChatWidget />
    
    <!-- 上方導覽列 -->
    <nav class="custom-navbar navbar navbar navbar-expand-md navbar-dark bg-dark" arial-label="Furni navigation bar">
      <div class="container">
        <!-- 左上 Logo icon -->
        <RouterLink class="navbar-brand" to="/">Furni<span>.</span></RouterLink>
        <div class="collapse navbar-collapse" id="navbarsFurni">
          <!-- 上方導覽 -->
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
              <RouterLink class="nav-link" to="/Order">訂單資訊</RouterLink>
            </li>
          </ul>
          <!-- 右上角 icon -->
          <div class="custom-navbar-cta navbar-nav mb-2 mb-md-0 ms-5">
            <!-- 購物車 icon -->
            <li class="nav-item">
              <RouterLink class="nav-link" to="/cart">
                <img src="/asset/images/cart.svg" />
              </RouterLink>
            </li>
            <!-- 登入 icon -->
            <!-- 未登入 -->
            <li v-if="!auth.isLoggedIn" class="nav-item">
              <RouterLink class="nav-link" to="/signin">
                <img src="/asset/images/user.svg" />
              </RouterLink>
            </li>

            <!-- 已登入 -->
            <li v-else class="nav-item dropdown">
              <a
                class="nav-link dropdown-toggle d-flex align-items-center gap-2" href="#" role="button" data-bs-toggle="dropdown"
                aria-expanded="false"
              >
                <!-- ✅ 這裡改用 Pinia 計算好的完整頭像網址 :src="auth.avatarUrl" -->
                <img
                  :src="auth.avatarUrl" class="rounded-circle" style="width: 28px; height: 28px; object-fit: cover" alt="avatar" />
                <span class="text-white">{{ auth.user?.name || '使用者' }}</span>
              </a>
              <ul class="dropdown-menu dropdown-menu-end">
                <li><RouterLink class="dropdown-item" to="/account">我的帳戶</RouterLink></li>
                <li><hr class="dropdown-divider" /></li>
                <li>
                  <button class="dropdown-item" @click="handleLogout" :disabled="loggingOut">
                    {{ loggingOut ? '登出中…' : '登出' }}
                  </button>
                </li>
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

<style scoped>
 .app-container {
  position: relative;
  min-height: 100vh;
}
header {
  line-height: 1.5;
  max-height: 100vh;
}

.logo {
  display: block;
  margin: 0 auto 2rem;
}

.chat-popup {
  position: fixed;
  right: 20px;
  bottom: 20px;
  width: 400px;
  height: 500px;
  background: white;
  border: 1px solid #ddd;
  box-shadow: 0 4px 10px rgba(0, 0, 0, 0.15);
  border-radius: 12px;
  overflow: hidden;
  z-index: 9999;
}

nav {
  width: 100%;
  font-size: 12px;
  text-align: center;
  margin-top: 2rem;
}

nav a.router-link-exact-active {
  color: var(--color-text);
}

nav a.router-link-exact-active:hover {
  background-color: transparent;
}

nav a {
  display: inline-block;
  padding: 0 1rem;
  border-left: 1px solid var(--color-border);
}

nav a:first-of-type {
  border: 0;
}

@media (min-width: 1024px) {
  header {
    display: flex;
    place-items: center;
    padding-right: calc(var(--section-gap) / 2);
  }

  .logo {
    margin: 0 2rem 0 0;
  }

  header .wrapper {
    display: flex;
    place-items: flex-start;
    flex-wrap: wrap;
  }

  nav {
    text-align: left;
    margin-left: -1rem;
    font-size: 1rem;

    padding: 1rem 0;
    margin-top: 1rem;
  }
}
</style>
