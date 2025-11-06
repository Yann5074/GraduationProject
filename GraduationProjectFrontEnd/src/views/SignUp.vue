<script setup>
import { ref, computed, watch } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import {
  registerAPI,
  loginAPI,
  // ✅ 僅保留逐一檢查 API
  checkAccountAPI,
  checkEmailAPI,
  checkPhoneAPI,
} from '@/api/Member'
import http from '@/api/axios'

const router = useRouter()
const auth = useAuthStore()

// -------- 表單資料 --------
const step = ref(1)
const loading = ref(false)

const name = ref('')
const phone = ref('')
const email = ref('')
const account = ref('')
const password = ref('')
const confirmPassword = ref('')
const emailCode = ref('')

// -------- 即時檢查狀態 --------
const accState = ref('idle') // idle | checking | ok | dup | invalid | error
const emailState = ref('idle')
const phoneState = ref('idle')
const accMsg = ref('')
const emailMsg = ref('')
const phoneMsg = ref('')

const showPassword = ref(false)
const showConfirm = ref(false)

const errorMsg = ref('')
const infoMsg = ref('')
const successMsg = ref('')

// -------- 基礎前端格式檢查 --------
const isEmail = (v) => /^[^@\s]+@[^@\s]+\.[^@\s]+$/.test(v)
const isPhone = (v) => /^\d{10}$/.test(v)

const phoneInvalid = computed(() => (phone.value ? !isPhone(phone.value) : false))
const pwdTooShort = computed(() => password.value.length > 0 && password.value.length < 6)
const confirmTooShort = computed(
  () => confirmPassword.value.length > 0 && confirmPassword.value.length < 6,
)
const confirmMismatch = computed(() => {
  if (password.value.length < 6 || confirmPassword.value.length < 6) return false
  return password.value !== confirmPassword.value
})

// -------- debounce 小工具 --------
function useDebounce(fn, delay = 350) {
  let t
  return (...args) => {
    clearTimeout(t)
    t = setTimeout(() => fn(...args), delay)
  }
}

// -------- 逐一檢查（重點） --------
const checkAccount = useDebounce(async () => {
  if (!account.value) {
    accState.value = 'idle'
    accMsg.value = ''
    return
  }
  accState.value = 'checking'
  try {
    const { data } = await checkAccountAPI(account.value) // 期望 { available: boolean }
    accState.value = data.available ? 'ok' : 'dup'
    accMsg.value = data.available ? '此帳號可使用' : '此帳號已被註冊'
  } catch {
    accState.value = 'error'
  }
}, 300)

const checkEmail = useDebounce(async () => {
  if (!email.value) {
    emailState.value = 'idle'
    emailMsg.value = ''
    return
  }
  if (!isEmail(email.value)) {
    emailState.value = 'invalid'
    emailMsg.value = 'Email 格式不正確'
    return
  }
  emailState.value = 'checking'
  try {
    const { data } = await checkEmailAPI(email.value) // { available: boolean }
    emailState.value = data.available ? 'ok' : 'dup'
    emailMsg.value = data.available ? '此 Email 可使用' : '此 Email 已被註冊'
  } catch {
    emailState.value = 'error'
  }
}, 300)

const checkPhone = useDebounce(async () => {
  if (!phone.value) {
    phoneState.value = 'idle'
    phoneMsg.value = ''
    return
  }
  if (!isPhone(phone.value)) {
    phoneState.value = 'invalid'
    phoneMsg.value = '手機需 10 碼數字'
    return
  }
  phoneState.value = 'checking'
  try {
    const { data } = await checkPhoneAPI(phone.value) // { available: boolean }
    phoneState.value = data.available ? 'ok' : 'dup'
    phoneMsg.value = data.available ? '此手機可使用' : '此手機已被註冊'
  } catch {
    phoneState.value = 'error'
  }
}, 300)

// 綁定 watcher（逐一檢查）
watch(account, checkAccount)
watch(email, checkEmail)
watch(phone, checkPhone)

// 是否仍在檢查中 / 三欄是否通過
const checkingAny = computed(() =>
  [accState.value, emailState.value, phoneState.value].includes('checking'),
)
const fieldsOk = computed(
  () => accState.value === 'ok' && emailState.value === 'ok' && phoneState.value === 'ok',
)

// 可否送出「寄送驗證碼」
const canSendCode = computed(() => {
  return (
    name.value &&
    phone.value &&
    email.value &&
    account.value &&
    password.value.length >= 6 &&
    confirmPassword.value.length >= 6 &&
    password.value === confirmPassword.value &&
    isEmail(email.value) &&
    isPhone(phone.value) &&
    fieldsOk.value &&
    !checkingAny.value &&
    !loading.value
  )
})

const canFinish = computed(() => {
  return !!emailCode.value && emailCode.value.length >= 6 && !loading.value
})

// -------- 你原本的 API 包裝 --------
function sendEmailCodeAPI(payload) {
  return http.post('/Member/send-email-code', payload)
}
function verifyEmailCodeAPI(payload) {
  return http.post('/Member/verify-email-code', payload)
}

// -------- Step1 -> Step2：寄送驗證碼 --------
async function handleSendCode() {
  errorMsg.value = ''
  infoMsg.value = ''
  successMsg.value = ''

  if (!canSendCode.value) {
    errorMsg.value = '請確認欄位都有填寫、格式正確，且帳號/手機/Email 未重複'
    return
  }

  loading.value = true
  try {
    const res = await sendEmailCodeAPI({ email: email.value })
    if (!res.data?.ok) {
      errorMsg.value = res.data?.message || '驗證碼寄送失敗'
      return
    }
    infoMsg.value = `驗證碼已寄到 ${email.value}，請在 5 分鐘內輸入`
    step.value = 2
  } catch (err) {
    errorMsg.value =
      err?.response?.data?.message || err?.response?.data?.Message || '寄送驗證碼失敗，請稍後再試'
  } finally {
    loading.value = false
  }
}

// -------- Step2：重新寄送 --------
async function handleResend() {
  if (loading.value) return
  loading.value = true
  errorMsg.value = ''
  infoMsg.value = ''
  successMsg.value = ''
  try {
    const res = await sendEmailCodeAPI({ email: email.value })
    if (!res.data?.ok) {
      errorMsg.value = res.data?.message || '驗證碼寄送失敗'
      return
    }
    infoMsg.value = `新的驗證碼已寄到 ${email.value}，請再確認信箱`
  } catch (err) {
    errorMsg.value = err?.response?.data?.message || err?.response?.data?.Message || '重新寄送失敗'
  } finally {
    loading.value = false
  }
}

// -------- Step2：完成註冊 --------
async function handleFinish() {
  errorMsg.value = ''
  infoMsg.value = ''
  successMsg.value = ''

  if (!canFinish.value) {
    errorMsg.value = '請輸入驗證碼'
    return
  }

  loading.value = true
  try {
    const v1 = await verifyEmailCodeAPI({ email: email.value, code: emailCode.value })
    if (!v1.data?.ok) {
      errorMsg.value = v1.data?.message || '驗證碼錯誤或已過期'
      return
    }

    const registerRes = await registerAPI({
      name: name.value,
      phone: phone.value,
      email: email.value,
      account: account.value,
      password: password.value,
    })
    if (!registerRes.data?.ok) {
      errorMsg.value = registerRes.data?.message || '註冊失敗'
      return
    }

    const loginRes = await loginAPI(account.value, password.value)
    await auth.login({ user: loginRes.data })
    successMsg.value = '會員註冊成功'
    router.push('/home')
  } catch (err) {
    errorMsg.value =
      err?.response?.data?.message || err?.response?.data?.Message || '發生錯誤，請稍後再試'
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <section class="py-5">
    <div class="container d-flex justify-content-center">
      <div class="card shadow-sm" style="max-width: 400px; width: 100%">
        <!-- 標題 -->
        <div class="card-header text-center bg-dark text-white">會員註冊</div>

        <!-- 進度條 -->
        <div class="pt-3 pb-2">
          <div class="signup-steps d-flex align-items-center justify-content-center">
            <!-- Step 1 -->
            <div class="step-item text-center">
              <div class="step-circle" :class="step === 1 ? 'active' : 'done'">
                <span v-if="step === 1">1</span>
                <span v-else>✔</span>
              </div>
              <div
                class="step-label"
                :class="step === 1 || step === 2 ? 'text-dark' : 'text-muted'"
              >
                填寫資料
              </div>
            </div>

            <!-- 線 -->
            <div class="step-line flex-grow-1 mx-2" :class="step === 2 ? 'line-active' : ''"></div>

            <!-- Step 2 -->
            <div class="step-item text-center">
              <div class="step-circle" :class="step === 2 ? 'active' : ''">
                <span>2</span>
              </div>
              <div class="step-label" :class="step === 2 ? 'text-dark' : 'text-muted'">
                信箱驗證
              </div>
            </div>
          </div>
        </div>

        <div class="card-body">
          <!-- 訊息區 -->
          <div v-if="errorMsg" class="alert alert-danger py-2">{{ errorMsg }}</div>
          <div v-if="infoMsg" class="alert alert-info py-2">{{ infoMsg }}</div>
          <div v-if="successMsg" class="alert alert-success py-2">{{ successMsg }}</div>

          <!-- STEP 1：基本資料 -->
          <form v-if="step === 1" @submit.prevent="handleSendCode">
            <!-- 姓名 -->
            <div class="mb-3">
              <label class="form-label">姓名</label>
              <input
                v-model="name"
                type="text"
                class="form-control"
                placeholder="輸入姓名"
                required
              />
            </div>

            <!-- 手機 -->
            <div class="mb-3">
              <label class="form-label">手機</label>
              <input
                v-model="phone"
                type="text"
                class="form-control"
                :class="{
                  'is-invalid': phoneState === 'dup' || phoneState === 'invalid',
                }"
                placeholder="輸入手機（10 碼）"
                required
                @input="phone = phone.replace(/\D/g, '').slice(0, 10)"
              />
              <!-- 基本格式錯誤（舊的檢查仍保留） -->
              <div v-if="phoneInvalid" class="text-danger small mt-1">手機號碼需為 10 位數字</div>
              <!-- 即時檢查提示 -->
              <div class="small mt-1">
                <template v-if="phoneState === 'checking'">
                  <span class="spinner-border spinner-border-sm me-1"></span>檢查中…
                </template>
                <template v-else-if="phoneState === 'ok'">
                  <span class="text-success">{{ phoneMsg || '此手機可使用' }}</span>
                </template>
                <template v-else-if="phoneState === 'dup'">
                  <span class="text-danger">{{ phoneMsg || '此手機已被註冊' }}</span>
                </template>
                <template v-else-if="phoneState === 'invalid'">
                  <span class="text-danger">{{ phoneMsg || '手機需 10 碼數字' }}</span>
                </template>
                <template v-else-if="phoneState === 'error'">
                  <span class="text-danger">檢查失敗，稍後再試</span>
                </template>
              </div>
            </div>

            <!-- Email -->
            <div class="mb-3">
              <label class="form-label">Email</label>
              <input
                v-model.trim="email"
                type="email"
                class="form-control"
                :class="{
                  'is-invalid': emailState === 'dup' || emailState === 'invalid',
                }"
                placeholder="輸入 Email"
                required
              />
              <!-- 前端格式提示（保留） -->
              <!-- <div v-if="email && !isEmail(email)" class="text-danger small mt-1">
                Email 格式不正確
              </div> -->
              <!-- 即時檢查提示 -->
              <div class="small mt-1">
                <template v-if="emailState === 'checking'">
                  <span class="spinner-border spinner-border-sm me-1"></span>檢查中…
                </template>
                <template v-else-if="emailState === 'ok'">
                  <span class="text-success">{{ emailMsg || '此 Email 可使用' }}</span>
                </template>
                <template v-else-if="emailState === 'dup'">
                  <span class="text-danger">{{ emailMsg || '此 Email 已被註冊' }}</span>
                </template>
                <template v-else-if="emailState === 'invalid'">
                  <span class="text-danger">{{ emailMsg || 'Email 格式不正確' }}</span>
                </template>
                <template v-else-if="emailState === 'error'">
                  <span class="text-danger">檢查失敗，稍後再試</span>
                </template>
              </div>
            </div>

            <!-- 帳號 -->
            <div class="mb-3">
              <label class="form-label">帳號</label>
              <input
                v-model="account"
                type="text"
                class="form-control"
                :class="{
                  'is-invalid': accState === 'dup' || accState === 'invalid',
                }"
                placeholder="輸入帳號"
                required
              />
              <!-- 即時檢查提示 -->
              <div class="small mt-1">
                <template v-if="accState === 'checking'">
                  <span class="spinner-border spinner-border-sm me-1"></span>檢查中…
                </template>
                <template v-else-if="accState === 'ok'">
                  <span class="text-success">{{ accMsg || '此帳號可使用' }}</span>
                </template>
                <template v-else-if="accState === 'dup'">
                  <span class="text-danger">{{ accMsg || '此帳號已被註冊' }}</span>
                </template>
                <template v-else-if="accState === 'invalid'">
                  <span class="text-danger">{{ accMsg || '帳號格式不符' }}</span>
                </template>
                <template v-else-if="accState === 'error'">
                  <span class="text-danger">檢查失敗，稍後再試</span>
                </template>
              </div>
            </div>

            <!-- 密碼 -->
            <div class="mb-3">
              <label class="form-label">密碼</label>
              <div class="input-group input-group-lg has-validation">
                <input
                  :type="showPassword ? 'text' : 'password'"
                  class="form-control border-end-0 rounded-end-0"
                  v-model.trim="password"
                  placeholder="至少 6 碼"
                  required
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
              <div v-if="pwdTooShort" class="text-danger small mt-1">密碼至少 6 碼</div>
            </div>

            <!-- 確認密碼 -->
            <div class="mb-3">
              <label class="form-label">確認密碼</label>
              <div class="input-group input-group-lg has-validation">
                <input
                  :type="showConfirm ? 'text' : 'password'"
                  class="form-control border-end-0 rounded-end-0"
                  v-model.trim="confirmPassword"
                  placeholder="至少 6 碼"
                  required
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
              <div v-if="confirmTooShort" class="text-danger small mt-1">密碼至少 6 碼</div>
              <div v-if="confirmMismatch" class="text-danger small mt-1">兩次密碼不一致</div>
            </div>

            <button
              type="submit"
              class="btn btn-success w-100 send-btn"
              :disabled="!canSendCode || loading"
            >
              <span v-if="loading" class="spinner-border spinner-border-sm me-2"></span>
              {{ loading ? '寄送中...' : '寄送驗證碼到我的信箱' }}
            </button>

            <!-- footer 導引：已有帳號？去登入 -->
            <div class="text-center mt-4 small text-muted">
              已經有帳號了？
              <router-link to="/signin" class="link-success text-decoration-none auth-link">
                登入
              </router-link>
            </div>
          </form>

          <!-- STEP 2：驗證碼 / 完成註冊 -->
          <form v-else @submit.prevent="handleFinish">
            <div class="mb-3">
              <label class="form-label">Email</label>
              <input class="form-control" :value="email" type="text" disabled />
            </div>

            <div class="mb-3">
              <label class="form-label">驗證碼 6 碼</label>
              <input
                v-model="emailCode"
                type="text"
                maxlength="6"
                class="form-control"
                placeholder="請輸入驗證碼"
                required
                @input="emailCode = emailCode.replace(/\D/g, '').slice(0, 6)"
              />
            </div>

            <button
              type="submit"
              class="btn btn-primary w-100 finish-btn"
              :disabled="!canFinish || loading"
            >
              <span v-if="loading" class="spinner-border spinner-border-sm me-2"></span>
              {{ loading ? '註冊中...' : '完成註冊並登入' }}
            </button>

            <button
              type="button"
              class="btn btn-link w-100 mt-2"
              :disabled="loading"
              @click="handleResend"
            >
              <span v-if="loading" class="spinner-border spinner-border-sm me-2"></span>
              重新寄驗證碼
            </button>

            <!-- footer 導引：其實我已經有帳號 -->
            <div class="text-center mt-2 small text-muted">
              已經有帳號了？
              <router-link to="/signin" class="link-success text-decoration-none auth-link">
                登入
              </router-link>
            </div>
          </form>
        </div>
      </div>
    </div>
  </section>
</template>

<style scoped>
/* ===== 註冊流程進度條 ===== */
.signup-steps {
  font-size: 14px;
  line-height: 1.2;
}

.step-item {
  display: flex;
  flex-direction: column;
  align-items: center;
  min-width: 70px;
}

.step-circle {
  width: 32px;
  height: 32px;
  border-radius: 50%;
  border: 2px solid #6c757d; /* 預設灰邊 */
  color: #6c757d; /* 預設灰字 */
  background-color: #fff;
  font-size: 14px;
  font-weight: 600;
  display: flex;
  align-items: center;
  justify-content: center;
}

.step-circle.active {
  border-color: #198754; /* success 綠 */
  color: #198754;
  background-color: #eaf6ef; /* 淡綠底，呼應你的 btn-eye.active */
}

.step-circle.done {
  border-color: #198754;
  background-color: #198754;
  color: #fff;
}

.step-label {
  margin-top: 6px;
  font-size: 12px;
}

.step-line {
  height: 2px;
  background-color: #ced4da; /* 淡灰線 */
  border-radius: 1px;
  min-width: 40px;
}

.step-line.line-active {
  background-color: #198754; /* 綠線，表示已走到步驟2 */
}

/* ===== 顯示/隱藏密碼按鈕 ===== */
.btn-eye {
  color: #6c757d;
  border-color: #ced4da;
  background: transparent;
}
.btn-eye:hover {
  background: #f8f9fa;
  color: #495057;
}
.btn-eye.active {
  color: #198754;
  border-color: #198754;
  background: #eaf6ef;
}

/* ===== 分隔線（Step2 下半部登入導引上方） ===== */
.divider-line {
  border: 0;
  border-top: 1px solid #dee2e6;
  margin-top: 1.5rem;
  margin-bottom: 1.5rem;
  opacity: 0.75;
}

/* ===== 通用：連結 hover/聚焦 效果 ===== */
.auth-link {
  text-decoration: none;
  font-weight: 400;
  transition:
    color 0.15s ease,
    text-decoration-color 0.15s ease,
    font-weight 0.15s ease;
}

.auth-link:hover,
.auth-link:focus-visible {
  text-decoration: underline !important; /* 蓋掉 text-decoration-none */
  font-weight: 600;
}
/* ===== 寄送驗證碼按鈕：與 SignIn 登入按鈕一致的互動效果 ===== */
.send-btn {
  background-color: #198754; /* 正常綠色 */
  border-color: #198754;
  color: #fff;
  transition:
    background-color 0.25s ease,
    border-color 0.25s ease,
    box-shadow 0.2s ease;
}

/* 滑鼠移入（只有可點擊時才生效） */
.send-btn:hover:not(:disabled) {
  background-color: #28a96b; /* 略亮 */
  border-color: #28a96b;
}

/* 按下按鈕 */
.send-btn:active:not(:disabled) {
  background-color: #157347; /* 略深 */
  border-color: #157347;
}

/* 聚焦外框（無障礙） */
.send-btn:focus-visible {
  box-shadow: 0 0 0 0.25rem rgba(25, 135, 84, 0.25);
}

/* ✅ 禁用樣式（欄位不完整或寄送中） */
.send-btn:disabled {
  background-color: #cde5d6; /* 淺綠，顯示不可用 */
  border-color: #cde5d6;
  color: #ffffff;
  cursor: not-allowed; /* 禁止游標 */
  opacity: 1;
}
/* ===== Step2 完成註冊按鈕（主色藍） ===== */
.finish-btn {
  background-color: #0d6efd; /* Bootstrap primary */
  border-color: #0d6efd;
  color: #fff;
  transition:
    background-color 0.25s ease,
    border-color 0.25s ease,
    box-shadow 0.2s ease;
}

/* 滑鼠移入（只有可點擊時才生效） */
.finish-btn:hover:not(:disabled) {
  background-color: #0b5ed7; /* 深一點的藍 */
  border-color: #0a58ca;
}

/* 按下按鈕 */
.finish-btn:active:not(:disabled) {
  background-color: #0a58ca;
  border-color: #0a53be;
}

/* 聚焦外框（無障礙） */
.finish-btn:focus-visible {
  box-shadow: 0 0 0 0.25rem rgba(13, 110, 253, 0.25);
}

/* ✅ 禁用樣式（未滿 6 碼或 loading）→ 淺藍色且不可按 */
.finish-btn:disabled {
  background-color: #cfe2ff; /* 淺藍 */
  border-color: #cfe2ff;
  color: #ffffff;
  cursor: not-allowed;
  opacity: 1; /* 避免被 Bootstrap 灰掉 */
}
</style>
