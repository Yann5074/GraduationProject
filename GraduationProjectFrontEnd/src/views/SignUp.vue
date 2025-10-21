<!-- src/views/SignUp.vue -->
<script setup>
import { ref, computed } from 'vue'
import { useRouter } from 'vue-router'
import axios from 'axios'

// ===== 建立 Axios 實例 =====
const http = axios.create({
  baseURL: 'https://localhost:7131', // 後端 API 網址
  withCredentials: true, // ✅ 必須開啟，讓瀏覽器帶 Session Cookie
  timeout: 10000,
})

// ===== Router =====
const router = useRouter()

// ===== API 呼叫 =====
async function registerAPI(req) {
  return http.post('/api/Member/create', req)
}

async function loginAPI(account, password) {
  return http.post('/api/Member/login', { account, password })
}

// ===== 表單狀態 =====
const name = ref('')
const phone = ref('')
const email = ref('')
const account = ref('')
const password = ref('')
const confirmPassword = ref('')
const loading = ref(false)
const errorMsg = ref('')
const successMsg = ref('')

// 密碼提示：有輸入但未達 6 碼
const pwdTooShort = computed(() => password.value.length > 0 && password.value.length < 6)

// 確認密碼提示：有輸入但未達 6 碼
const confirmTooShort = computed(
  () => confirmPassword.value.length > 0 && confirmPassword.value.length < 6,
)

// 兩次密碼不一致（都 >= 6 碼才檢查不一致，避免早期就報錯）
const confirmMismatch = computed(
  () =>
    password.value.length >= 6 &&
    confirmPassword.value.length >= 6 &&
    confirmPassword.value !== password.value,
)

// ===== 簡單驗證 =====
const canSubmit = computed(
  () =>
    name.value &&
    phone.value &&
    email.value &&
    account.value &&
    password.value.length >= 6 &&
    confirmPassword.value.length >= 6 &&
    password.value === confirmPassword.value &&
    !loading.value,
)

// ===== 註冊事件 =====
const onSubmit = async (e) => {
  e.preventDefault()
  errorMsg.value = ''
  successMsg.value = ''

  if (!canSubmit.value) {
    errorMsg.value = '請填寫所有欄位，並確認密碼一致'
    return
  }

  const req = {
    name: name.value,
    phone: phone.value,
    email: email.value,
    account: account.value,
    password: password.value,
  }

  loading.value = true
  try {
    // 1️⃣ 呼叫註冊 API
    await registerAPI(req)

    // 2️⃣ 註冊成功後自動登入（可選）
    await loginAPI(account.value, password.value)

    successMsg.value = '註冊成功，正在為您登入...'
    await new Promise((r) => setTimeout(r, 1000))
    router.push('/')
  } catch (err) {
    errorMsg.value = err?.response?.data?.message || '註冊失敗，請再試一次'
  } finally {
    loading.value = false
  }
}
// 加在你的 <script setup> 其它 ref 後面
const showPassword = ref(false)
const showConfirm = ref(false)
</script>

<template>
  <!-- Hero 標題區 -->
  <!-- <div class="hero py-4">
    <div class="container">
      <div class="row justify-content-center">
        <div class="col-auto">
          <h1 class="h3 mb-0 fw-semibold">會員註冊</h1>
        </div>
      </div>
    </div>
  </div> -->

  <!-- 表單區 -->
  <section class="auth-wrap">
    <div class="container">
      <div class="row justify-content-center">
        <div class="col-12 col-sm-10 col-md-8 col-lg-6 col-xl-5">
          <div class="card auth-card border-0 shadow-lg">
            <div class="card-header bg-dark text-white text-center py-3">
              <h4 class="mb-0">會員註冊</h4>
            </div>

            <div class="card-body p-4 p-md-5">
              <!-- 訊息區 -->
              <div v-if="errorMsg" class="alert alert-danger py-2 mb-3">{{ errorMsg }}</div>
              <div v-if="successMsg" class="alert alert-success py-2 mb-3">{{ successMsg }}</div>

              <form @submit="onSubmit" novalidate class="vstack gap-3">
                <!-- Name -->
                <div>
                  <label class="form-label small text-muted">姓名</label>
                  <input
                    v-model.trim="name"
                    type="text"
                    class="form-control form-control-lg"
                    placeholder="輸入姓名"
                    required
                  />
                </div>

                <!-- Phone -->
                <div>
                  <label class="form-label small text-muted">手機</label>
                  <input
                    v-model.trim="phone"
                    type="text"
                    class="form-control form-control-lg"
                    placeholder="輸入手機"
                    required
                  />
                </div>

                <!-- Email -->
                <div>
                  <label class="form-label small text-muted">Email</label>
                  <input
                    v-model.trim="email"
                    type="email"
                    class="form-control form-control-lg"
                    placeholder="輸入Email"
                    required
                  />
                </div>

                <!-- Account -->
                <div>
                  <label class="form-label small text-muted">帳號</label>
                  <input
                    v-model.trim="account"
                    type="text"
                    class="form-control form-control-lg"
                    placeholder="輸入帳號"
                    required
                  />
                </div>

                <!-- Password -->
                <div class="mb-3">
                  <label class="form-label small text-muted" for="pwd">密碼</label>
                  <div class="input-group input-group-lg has-validation">
                    <input
                      id="pwd"
                      v-model="password"
                      :type="showPassword ? 'text' : 'password'"
                      class="form-control border-end-0 rounded-end-0"
                      :class="{ 'is-invalid': pwdTooShort }"
                      minlength="6"
                      placeholder="至少 6 碼"
                      required
                      :aria-invalid="pwdTooShort ? 'true' : 'false'"
                      :aria-describedby="pwdTooShort ? 'pwdHelp' : null"
                    />
                    <button
                      type="button"
                      class="btn btn-outline-secondary btn-eye border-start-0 rounded-start-0"
                      @click="showPassword = !showPassword"
                      tabindex="-1"
                      :aria-label="showPassword ? '隱藏密碼' : '顯示密碼'"
                      :title="showPassword ? '隱藏密碼' : '顯示密碼'"
                    >
                      <i :class="showPassword ? 'bi bi-eye-slash' : 'bi bi-eye'"></i>
                    </button>
                  </div>

                  <!-- 提示（不足 6 碼時顯示） -->
                  <div v-if="pwdTooShort" id="pwdHelp" class="invalid-feedback d-block">
                    請輸入至少 6 碼的密碼
                  </div>
                </div>

                <!-- Confirm Password -->
                <div class="mb-3">
                  <label class="form-label small text-muted" for="pwd2">確認密碼</label>
                  <div class="input-group input-group-lg has-validation">
                    <input
                      id="pwd2"
                      v-model="confirmPassword"
                      :type="showConfirm ? 'text' : 'password'"
                      class="form-control border-end-0 rounded-end-0"
                      :class="{ 'is-invalid': confirmTooShort || confirmMismatch }"
                      minlength="6"
                      placeholder="至少 6 碼"
                      required
                      :aria-invalid="confirmTooShort || confirmMismatch ? 'true' : 'false'"
                      :aria-describedby="confirmTooShort || confirmMismatch ? 'pwd2Help' : null"
                    />
                    <button
                      type="button"
                      class="btn btn-outline-secondary btn-eye border-start-0 rounded-start-0"
                      @click="showConfirm = !showConfirm"
                      tabindex="-1"
                      :aria-label="showConfirm ? '隱藏密碼' : '顯示密碼'"
                      :title="showConfirm ? '隱藏密碼' : '顯示密碼'"
                    >
                      <i :class="showConfirm ? 'bi bi-eye-slash' : 'bi bi-eye'"></i>
                    </button>
                  </div>

                  <!-- 提示（不足 6 碼 / 不一致） -->
                  <div v-if="confirmTooShort" id="pwd2Help" class="invalid-feedback d-block">
                    請輸入至少 6 碼的密碼
                  </div>
                  <div v-else-if="confirmMismatch" id="pwd2Help" class="invalid-feedback d-block">
                    兩次輸入的密碼不一致
                  </div>
                </div>

                <!-- Submit -->
                <button
                  type="submit"
                  class="btn btn-success btn-lg w-100 mt-2"
                  :disabled="!canSubmit"
                >
                  <span v-if="loading" class="spinner-border spinner-border-sm me-2"></span>
                  {{ loading ? '註冊中...' : '註冊' }}
                </button>
              </form>
            </div>

            <div class="card-footer text-center bg-white py-3">
              <small class="text-muted">
                已有帳號？
                <RouterLink class="text-success fw-bold" to="/signin">登入</RouterLink>
              </small>
            </div>
          </div>
        </div>
      </div>
    </div>
  </section>
</template>

<style scoped>
/* 讓整段表單區塊上下有呼吸空間、置中對齊 */
.auth-wrap {
  min-height: calc(100vh - 120px);
  display: grid;
  place-items: start center;
  padding: 24px 0 48px;
}

/* 控制卡片的最大寬度與圓角陰影 */
.auth-card {
  max-width: 520px; /* 桌機視覺寬度 */
  margin: 0 auto;
  border-radius: 1rem;
}

/* 表單可再微調密度 */
.form-control.form-control-lg {
  padding-top: 0.7rem;
  padding-bottom: 0.7rem;
}

/* 可視覺更輕盈一點 */
.form-label {
  margin-bottom: 0.25rem;
}

.toggle-btn {
  position: absolute;
  top: 50%;
  right: 10px;
  transform: translateY(-50%);
  padding: 0;
  border: none;
  background: none;
  color: #666;
  font-size: 1.2rem;
  line-height: 1;
}

.toggle-btn:hover {
  color: #198754; /* Bootstrap 綠色 */
}
/* 眼睛按鈕：與輸入框同高、邊線顏色一致 */
.btn-eye {
  color: #6c757d;
  border-color: #ced4da;
  background: transparent;
}
.btn-eye:hover {
  background: #f8f9fa;
  color: #495057;
}
/* 以前用 position:absolute 的 .toggle-btn 記得移除，避免干擾 */
</style>
