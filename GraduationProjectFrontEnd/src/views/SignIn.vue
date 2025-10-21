<script setup>
import { ref, computed } from 'vue'
import { useRouter } from 'vue-router'
import axios from 'axios'
import { useAuthStore } from '@/stores/auth'

const router = useRouter()
const auth = useAuthStore()

const http = axios.create({
  baseURL: 'https://localhost:7131',
  withCredentials: true,
  timeout: 10000,
})

http.interceptors.response.use(
  (res) => res,
  (err) => {
    if (err.response?.status === 401) auth.logout()
    return Promise.reject(err)
  },
)

/* ===== API ===== */
async function loginAPI(account, password) {
  return http.post('/api/Member/login', { account, password })
}
async function meAPI() {
  return http.get('/api/Member/me')
}

/* ===== 表單狀態與驗證（原樣） ===== */
const account = ref('')
const password = ref('')
const remember = ref(true)
const showPassword = ref(false)
const loading = ref(false)
const errorMsg = ref('')

const accountValid = computed(() => account.value.trim().length > 0)
const passwordValid = computed(() => password.value.length >= 6)
const canSubmit = computed(() => accountValid.value && passwordValid.value && !loading.value)
const showPwdTip = computed(() => password.value.length > 0 && password.value.length < 6)

/* ✅ 新增：把 /me 的資料標準化成我們要的結構（一定有 user.avatar） */
function normalizeUser(me) {
  // 👉 這裡列出可能的欄位名稱（看到真實欄位後，把不需要的刪掉）
  const avatarCandidate =
    me.avatar ??
    me.avatarUrl ??
    me.photo ??
    me.memberImage ??
    me.imageFileName ??
    me.headImageFile ??
    me.image ??
    '' // 沒有就給空字串

  return {
    ...me,
    avatar: avatarCandidate, // 前端 store 會依這個欄位去組完整圖片 URL
  }
}

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
    // 1) 登入（建立 Session Cookie）
    await loginAPI(account.value, password.value)

    // 2) 拿目前登入者
    const { data: me } = await meAPI()
    console.log('[meAPI] 回傳資料 =', me) // ✅ 看清楚後端真實欄位

    // 3) ✅ 標準化後再寫進 Pinia
    const normalizedUser = normalizeUser(me)
    console.log('[normalizeUser] 之後 =', normalizedUser) // ✅ 確認 avatar 有沒有值
    await auth.login({ token: null, user: normalizedUser })

    // 4) 導頁
    router.push('/')
  } catch (err) {
    const status = err?.response?.status
    errorMsg.value =
      status === 401
        ? '帳號或密碼有誤，請再試一次'
        : err?.response?.data?.message || '登入失敗，請再試一次'
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <!-- 你的畫面保持不變，只把 Email 欄位改成 Account（帳號） -->
  <!-- <div class="hero">
    <div class="container">
      <div class="row justify-content-between">
        <div class="col-lg-5">
          <div class="intro-excerpt"><h1>Sign In</h1></div>
        </div>
        <div class="col-lg-7"></div>
      </div>
    </div>
  </div> -->

  <div
    class="min-vh-100 d-flex align-items-center"
    :style="{ backgroundSize: 'cover', backgroundPosition: 'center' }"
  >
    <div class="container">
      <div class="row justify-content-center">
        <div class="col-12 col-md-8 col-lg-5">
          <div class="card border-0 shadow-lg">
            <div class="card-header bg-dark text-white text-center py-3">
              <h4 class="mb-0">登入</h4>
            </div>

            <div class="card-body p-4">
              <div v-if="errorMsg" class="alert alert-danger py-2" role="alert">{{ errorMsg }}</div>

              <form @submit="onSubmit" novalidate>
                <!-- Account -->
                <div class="mb-3">
                  <label for="account" class="form-label">帳號</label>
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

                <!-- 密碼 -->
                <div class="mb-3">
                  <label for="password" class="form-label mb-0">密碼</label>

                  <div class="input-group input-group-lg has-validation">
                    <input
                      :type="showPassword ? 'text' : 'password'"
                      id="password"
                      name="password"
                      class="form-control border-end-0 rounded-end-0"
                      :class="{ 'is-invalid': showPwdTip }"
                      placeholder="至少 6 碼"
                      v-model="password"
                      autocomplete="current-password"
                      minlength="6"
                      required
                      :aria-invalid="showPwdTip ? 'true' : 'false'"
                      :aria-describedby="showPwdTip ? 'pwdHelp' : null"
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

                  <!-- 方式 A：沿用 Bootstrap 的 invalid-feedback，但用 d-block 強制顯示 -->
                  <div v-if="showPwdTip" id="pwdHelp" class="invalid-feedback d-block">
                    請輸入至少 6 碼的密碼
                  </div>
                </div>
                <div class="d-grid">
                  <button type="submit" class="btn btn-success btn-lg" :disabled="!canSubmit">
                    <span v-if="loading" class="spinner-border spinner-border-sm me-2" />
                    {{ loading ? '登入中...' : '登入' }}
                  </button>
                </div>
              </form>
            </div>

            <div class="card-footer text-center bg-white py-3">
              <small class="text-muted">
                還沒有帳號？
                <RouterLink class="text-success fw-bold" to="/signup">註冊</RouterLink>
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
/* 眼睛按鈕：預設灰、hover較亮；切換狀態變綠色 */
.btn-eye {
  color: #6c757d; /* 圖示顏色（跟 outline-secondary 文字色一致） */
  border-color: #ced4da; /* 跟輸入框的邊線一致 */
  background: transparent;
}
.btn-eye:hover {
  background: #f8f9fa;
  color: #495057;
}

/* showPassword = true 時套用 active：變綠、更明顯 */
.btn-eye.active {
  color: #198754; /* Bootstrap success 綠 */
  border-color: #198754;
  background: #eaf6ef; /* 淡綠底（可調） */
}

/* 如果之前還留著 .toggle-btn（position:absolute）的樣式，請刪除避免干擾 */

/* 如果你之前有 .toggle-btn（position:absolute）的樣式，請刪掉避免衝突 */
</style>
