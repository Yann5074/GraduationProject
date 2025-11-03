<script setup>
import { RouterLink, RouterView, useRoute, useRouter } from 'vue-router'
import { onMounted, ref, watch } from 'vue'
import { Dropdown } from 'bootstrap'
import http from '../src/api/axios'
import { useAuthStore } from '@/stores/auth'
import ChatWidget from '@/components/Chat/ChatWidget.vue'
import { useCartStore } from './stores/cartStore'
import { syncCart } from './api/Cart'

const route = useRoute()
const router = useRouter()
const isActive = (path) => route.path.startsWith(path)
const auth = useAuthStore()
const cart = useCartStore()

/** ✅ 啟動時同步伺服器 Session 狀態 */
// ✅ 修正版：讓 Pinia 自動用 MemberDTO 處理 imageUrl
async function hydrateFromServer() {
  try {
    const { data: me } = await http.get('/Member/me')
    await auth.login({ user: me }) // 交給 auth 自己轉換 DTO
  } catch {
    auth.logout()
  }
}

const loggingOut = ref(false)
async function handleLogout() {
  if (loggingOut.value) return // 確認是否正在登出
  loggingOut.value = true
  try {
    await http.post('/Member/logout')
  } catch (err) {
    console.warn('logout api error:', err?.response || err)
  } finally {
    auth.logout()
    loggingOut.value = false
    router.push('/signin')
  }
}

/** 初始化 dropdown 並同步登入狀態 */
onMounted(() => {
  document.querySelectorAll('[data-bs-toggle="dropdown"]').forEach((el) => new Dropdown(el))
  auth.hydrate()
  hydrateFromServer()
  cart.hydrateCart()
})

// 登入後購物車同步
watch(
  () => auth.isLoggedIn,
  async (LoggedIn) => {
    if (LoggedIn && cart.items.length > 0) {
      try {
        const reqDTO = {
          memberId: auth.user?.memberId || auth.user?.fMemberId,
          cartItem: cart.items.map((i) => ({
            productVariantId: i.productVariantId,
            quantity: i.qty,
          })),
        }
        console.log('同步購物車', reqDTO)
        await syncCart(reqDTO)
        cart.clearCart()
        console.log('購物車已同步')
      } catch (err) {
        console.log('購物車同步失敗', err)
      }
    }
  },
)
</script>

<template>
  <main>
    <!-- 自己加的聊天室浮動元件 -->
    <ChatWidget />

    <!-- 上方導覽列 -->
    <nav
      class="navbar navbar-expand-md nav-neo" 
      :class="{ 'nav-solid': $route.path !== '/home' }" 
      aria-label="Main"
    >
      <div class="container">
        <!-- 左上 Logo icon -->
        <RouterLink class="navbar-brand" to="/home"> Viewrniture<span>.</span></RouterLink>
        <!-- <RouterLink class="navbar-brand" to="/home"><img src="./assets/images/Viewrniture.png" class="logo"/> Viewrniture<span>.</span></RouterLink> -->

        <button class="navbar-toggler" type="button" data-bs-toggle="collapse" data-bs-target="#navMain">
          <span class="navbar-toggler-icon"></span>
        </button>

        <div class="collapse navbar-collapse" id="navMain">
          <!-- 上方導覽 -->
          <ul class="custom-navbar-nav navbar-nav ms-auto mb-2 mb-md-0">
            <li class="nav-item" :class="{ active: isActive('/home') }">
              <RouterLink class="nav-link" to="/home">首頁</RouterLink>
            </li>
            <li class="nav-item" :class="{ active: isActive('/products') }">
              <RouterLink class="nav-link" to="/products">購物</RouterLink>
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
          <!-- 右上角功能列 -->
          <ul class="custom-navbar-cta navbar-nav mb-2 mb-md-0 ms-5 align-items-center">
            <!-- 購物車 -->
            <li class="nav-item">
              <RouterLink class="nav-link" to="/cart">
                <img src="/asset/images/cart.svg" alt="cart" />
              </RouterLink>
            </li>

            <!-- 未登入：登入｜註冊 -->
            <template v-if="!auth.isLoggedIn">
              <li class="nav-item">
                <RouterLink class="nav-link px-2 auth-link" to="/signin">登入</RouterLink>
              </li>
              <li class="nav-item disabled">
                <span class="nav-link px-0">|</span>
              </li>
              <li class="nav-item">
                <RouterLink class="nav-link px-2 auth-link" to="/signup">註冊</RouterLink>
              </li>
            </template>

            <!-- 已登入：頭貼＋暱稱＋下拉 -->
            <template v-else>
              <li class="nav-item dropdown">
                <a
                  class="nav-link dropdown-toggle d-flex align-items-center gap-2"
                  href="#"
                  role="button"
                  data-bs-toggle="dropdown"
                  aria-expanded="false"
                >
                  <img
                    :src="auth.avatarUrl || fallbackAvatar"
                    alt="avatar"
                    class="rounded-circle border object-fit-cover"
                    width="28"
                    height="28"
                    @error="onAvatarError"
                  />
                  <span class="text-white fw-semibold">
                    {{ auth.displayName || auth.user?.name || auth.user?.account || '使用者' }}
                  </span>
                </a>

                <ul class="dropdown-menu dropdown-menu-end">
                  <!-- <li class="dropdown-header small text-muted px-3">
                    {{ auth.user?.email }}
                  </li> -->
                  <!-- <li><hr class="dropdown-divider" /></li> -->
                  <li>
                    <RouterLink class="dropdown-item" to="/account/profile">我的帳戶</RouterLink>
                  </li>
                  <li><hr class="dropdown-divider" /></li>
                  <li>
                    <RouterLink class="dropdown-item" to="/Order">訂單資訊</RouterLink>
                  </li>
                  <li><hr class="dropdown-divider" /></li>
                  <li>
                    <button
                      class="dropdown-item text-danger"
                      @click="handleLogout"
                      :disabled="loggingOut"
                    >
                      {{ loggingOut ? '登出中…' : '登出' }}
                    </button>
                  </li>
                </ul>
              </li>
            </template>
          </ul>
        </div>
      </div>
    </nav>

    <RouterView />

    <!-- Footer（把 public 圖片改成 / 開頭） -->
    <footer class="footer-section">
      <div class="container relative">
        <div class="sofa-img">
          <!-- public 底下的圖，請用 /asset/... -->
          <!-- <img src="/asset/images/sofa.png" alt="Image" class="img-fluid" /> -->
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
                  <button class="btn btn-primary">
                    <span class="fa fa-paper-plane"></span>
                  </button>
                </div>
              </form>
            </div>
          </div>
        </div>

        <div class="row g-5 mb-5">
          <div class="col-lg-4">
            <div class="mb-4 footer-logo-wrap">
              <a href="#" class="footer-logo">Furni<span>.</span></a>
            </div>
            <p class="mb-4">
              Donec facilisis quam ut purus rutrum lobortis. Donec vitae odio quis nisl dapibus
              malesuada. Nullam ac aliquet velit. Aliquam vulputate velit imperdiet dolor tempor
              tristique. Pellentesque habitant
            </p>

            <ul class="list-unstyled custom-social">
              <li>
                <a href="#"><span class="fa fa-brands fa-facebook-f"></span></a>
              </li>
              <li>
                <a href="#"><span class="fa fa-brands fa-twitter"></span></a>
              </li>
              <li>
                <a href="#"><span class="fa fa-brands fa-instagram"></span></a>
              </li>
              <li>
                <a href="#"><span class="fa fa-brands fa-linkedin"></span></a>
              </li>
            </ul>
          </div>

          <div class="col-lg-8">
            <div class="row links-wrap">
              <div class="col-6 col-sm-6 col-md-3">
                <ul class="list-unstyled">
                  <li><a href="#">About us</a></li>
                  <li><a href="#">Services</a></li>
                  <li><a href="#">Blog</a></li>
                  <li><a href="#">Contact us</a></li>
                </ul>
              </div>

              <div class="col-6 col-sm-6 col-md-3">
                <ul class="list-unstyled">
                  <li><a href="#">Support</a></li>
                  <li><a href="#">Knowledge base</a></li>
                  <li><a href="#">Live chat</a></li>
                </ul>
              </div>

              <div class="col-6 col-sm-6 col-md-3">
                <ul class="list-unstyled">
                  <li><a href="#">Jobs</a></li>
                  <li><a href="#">Our team</a></li>
                  <li><a href="#">Leadership</a></li>
                  <li><a href="#">Privacy Policy</a></li>
                </ul>
              </div>

              <div class="col-6 col-sm-6 col-md-3">
                <ul class="list-unstyled">
                  <li><a href="#">Nordic Chair</a></li>
                  <li><a href="#">Kruzo Aero</a></li>
                  <li><a href="#">Ergonomic Chair</a></li>
                </ul>
              </div>
            </div>
          </div>
        </div>

        <div class="border-top copyright">
          <div class="row pt-4">
            <div class="col-lg-6">
              <p class="mb-2 text-center text-lg-start">
                Copyright &copy;{{ new Date().getFullYear() }}. All Rights Reserved. &mdash;
                Designed with love by <a href="https://untree.co">Untree.co</a> Distributed By
                <a hreff="https://themewagon.com">ThemeWagon</a>
              </p>
            </div>

            <div class="col-lg-6 text-center text-lg-end">
              <ul class="list-unstyled d-inline-flex ms-auto">
                <li class="me-4"><a href="#">Terms &amp; Conditions</a></li>
                <li><a href="#">Privacy Policy</a></li>
              </ul>
            </div>
          </div>
        </div>
      </div>
    </footer>
    <!-- End Footer Section -->
  </main>
</template>

<style scoped>
/* .app-container {
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
} */

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
.logo {
  width: 75px;
  height: 75px;
}
/*.auth-link {
  color: #212529;
  text-decoration: none;
  font-weight: 500;
}

.auth-link:hover {
  color: #198754; /* Bootstrap success 綠 
  text-decoration: underline;
}*/

.nav-neo {
  position: fixed; top:0; left:0; right:0; z-index: 1030;
  background: transparent;
  transition: background .3s ease, box-shadow .3s ease;
}
.nav-neo.nav-solid { background: rgba(15,17,19,.9); backdrop-filter: blur(8px); box-shadow: 0 1px 0 rgba(255,255,255,.06); }
.nav-neo .nav-link { color: #fff; opacity:.9; }
.nav-neo .nav-link.router-link-active { opacity:1 }

/* nav {
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
} */

/* @media (min-width: 1024px) {
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
.floating-button {
  position: fixed;
  bottom: 20px;
  right: 20px;
  background: #007bff;
  color: white;
  font-size: 24px;
  border-radius: 50%;
  width: 60px;
  height: 60px;
  display: flex;
  justify-content: center;
  align-items: center;
  cursor: pointer;
  z-index: 99999;
}*/
</style>
