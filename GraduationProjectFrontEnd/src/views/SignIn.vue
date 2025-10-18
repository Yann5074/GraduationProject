<!-- src/views/SignIn.vue -->
<script setup>
import { ref, computed } from 'vue'
import { useRouter } from 'vue-router'
import axios from 'axios'

export const http = axios.create({
  baseURL: 'https://localhost:7131', // 你的 API 根網址
  withCredentials: true, // ✅ 讓瀏覽器帶上/保存 Cookie
  timeout: 10000,
})

const router = useRouter()

/* =============== 2) 登入 API 封裝（同檔案） =============== */
async function loginAPI(account, password) {
  // 後端期望的是 { account, password }
  return http.post('/api/Member/login', { account, password })
}
async function meAPI() {
  return http.get('/api/member/me')
}

/* =============== 3) 表單狀態 =============== */
const account = ref('') // ✅ 用 account，不是 email
const password = ref('')
const remember = ref(true) // 若後端需要可一併傳給 loginAPI
const showPassword = ref(false)
const loading = ref(false)
const errorMsg = ref('')

/* 簡單驗證 */
const accountValid = computed(() => account.value.trim().length > 0)
const passwordValid = computed(() => password.value.length >= 6)
const canSubmit = computed(() => accountValid.value && passwordValid.value && !loading.value)

/* 送出 */
const onSubmit = async (e) => {
  e.preventDefault()
  errorMsg.value = ''
  if (!canSubmit.value) {
    errorMsg.value = '請輸入帳號與至少 6 碼的密碼'
    return
  }
  loading.value = true
  try {
    // ✅ 登入：後端會回 Set-Cookie，Axios（withCredentials）會自動保存
    await loginAPI(account.value, password.value)

    // 可選：立即取得登入者資料確認
    await meAPI()

    // 導頁
    router.push('/')
  } catch (err) {
    errorMsg.value = err?.response?.data?.message || '登入失敗，請再試一次'
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <!-- 你的畫面保持不變，只把 Email 欄位改成 Account（帳號） -->
  <div class="hero">
    <div class="container">
      <div class="row justify-content-between">
        <div class="col-lg-5">
          <div class="intro-excerpt"><h1>Sign In</h1></div>
        </div>
        <div class="col-lg-7"></div>
      </div>
    </div>
  </div>

  <div
    class="min-vh-100 d-flex align-items-center"
    :style="{ backgroundSize: 'cover', backgroundPosition: 'center' }"
  >
    <div class="container">
      <div class="row justify-content-center">
        <div class="col-12 col-md-8 col-lg-5">
          <div class="card border-0 shadow-lg">
            <div class="card-header bg-dark text-white text-center py-3">
              <h4 class="mb-0">Sign In</h4>
            </div>

            <div class="card-body p-4">
              <div v-if="errorMsg" class="alert alert-danger py-2" role="alert">{{ errorMsg }}</div>

              <form @submit="onSubmit" novalidate>
                <!-- Account -->
                <div class="mb-3">
                  <label for="account" class="form-label">Account</label>
                  <input
                    id="account"
                    name="account"
                    type="text"
                    class="form-control"
                    :class="{ 'is-invalid': account && !accountValid }"
                    placeholder="輸入帳號"
                    v-model.trim="account"
                    autocomplete="username"
                    required
                  />
                  <div class="invalid-feedback">請輸入帳號</div>
                </div>

                <!-- Password -->
                <div class="mb-3">
                  <div class="d-flex justify-content-between align-items-center">
                    <label for="password" class="form-label mb-0">Password</label>
                    <button
                      type="button"
                      class="btn btn-sm btn-link text-decoration-none"
                      @click="showPassword = !showPassword"
                    >
                      {{ showPassword ? '隱藏' : '顯示' }}密碼
                    </button>
                  </div>
                  <input
                    :type="showPassword ? 'text' : 'password'"
                    id="password"
                    name="password"
                    class="form-control"
                    :class="{ 'is-invalid': password && !passwordValid }"
                    placeholder="至少 6 碼"
                    v-model="password"
                    autocomplete="current-password"
                    minlength="6"
                    required
                  />
                  <div class="invalid-feedback">密碼長度至少 6 碼</div>
                </div>

                <!-- Remember me（若後端要支援可帶到 loginAPI 第三個參數） -->
                <div class="form-check form-switch mb-3">
                  <input
                    class="form-check-input"
                    type="checkbox"
                    id="rememberMe"
                    v-model="remember"
                  />
                  <label class="form-check-label" for="rememberMe">Remember me</label>
                </div>

                <div class="d-grid">
                  <button type="submit" class="btn btn-success btn-lg" :disabled="!canSubmit">
                    <span v-if="loading" class="spinner-border spinner-border-sm me-2" />
                    {{ loading ? 'Signing in...' : 'Sign In' }}
                  </button>
                </div>
              </form>
            </div>

            <div class="card-footer text-center bg-white py-3">
              <small class="text-muted">
                還沒有帳號？
                <RouterLink class="text-success fw-bold" to="/signup">Sign up</RouterLink>
              </small>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.card {
  border-radius: 1rem;
}
</style>
