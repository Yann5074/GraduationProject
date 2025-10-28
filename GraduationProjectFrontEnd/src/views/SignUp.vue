<script setup>
import { ref, computed } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { registerAPI, loginAPI } from '@/api/Member'
import api from '@/api/axios'

const router = useRouter()
const auth = useAuthStore()

const step = ref(1)
const loading = ref(false)

const name = ref('')
const phone = ref('')
const email = ref('')
const account = ref('')
const password = ref('')
const confirmPassword = ref('')
const emailCode = ref('')

const showPassword = ref(false)
const showConfirm = ref(false)

const errorMsg = ref('')
const infoMsg = ref('')
const successMsg = ref('')

const phoneInvalid = computed(() => {
  if (!phone.value) return false
  return !/^\d{10}$/.test(phone.value)
})

const pwdTooShort = computed(() => password.value.length > 0 && password.value.length < 6)

const confirmTooShort = computed(
  () => confirmPassword.value.length > 0 && confirmPassword.value.length < 6,
)

const confirmMismatch = computed(() => {
  if (password.value.length < 6 || confirmPassword.value.length < 6) return false
  return password.value !== confirmPassword.value
})

function isEmailFormat(v) {
  return /^[^@\s]+@[^@\s]+\.[^@\s]+$/.test(v)
}

const canSendCode = computed(() => {
  return (
    name.value &&
    phone.value &&
    email.value &&
    account.value &&
    password.value.length >= 6 &&
    confirmPassword.value.length >= 6 &&
    password.value === confirmPassword.value &&
    !phoneInvalid.value &&
    isEmailFormat(email.value) &&
    !loading.value
  )
})

const canFinish = computed(() => {
  return emailCode.value && emailCode.value.length >= 4 && !loading.value
})

// --- API wrappers ---
function sendEmailCodeAPI(payload) {
  return api.post('/api/Member/send-email-code', payload)
}

function verifyEmailCodeAPI(payload) {
  return api.post('/api/Member/verify-email-code', payload)
}

// Step1 -> Step2
async function handleSendCode() {
  errorMsg.value = ''
  infoMsg.value = ''
  successMsg.value = ''

  if (!canSendCode.value) {
    errorMsg.value = '請確認欄位都有填寫、格式正確，密碼一致且至少6碼'
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

// resend code in step2
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

// Step2 完成註冊
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
    // 1. 驗證 Email Code
    const v1 = await verifyEmailCodeAPI({
      email: email.value,
      code: emailCode.value,
    })
    if (!v1.data?.ok) {
      errorMsg.value = v1.data?.message || '驗證碼錯誤或已過期'
      loading.value = false
      return
    }

    // 2. 建立會員
    const registerRes = await registerAPI({
      name: name.value,
      phone: phone.value,
      email: email.value,
      account: account.value,
      password: password.value,
    })
    if (!registerRes.data?.ok) {
      errorMsg.value = registerRes.data?.message || '註冊失敗'
      loading.value = false
      return
    }

    // 3. 自動登入
    const loginRes = await loginAPI(account.value, password.value)
    const userData = loginRes.data
    await auth.login({ user: userData })

    // 4. 成功提示 + 導回首頁
    successMsg.value = '會員註冊成功'
    router.push('/')
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
                placeholder="輸入手機"
                required
              />
              <div v-if="phoneInvalid" class="text-danger small mt-1">手機號碼需為10位數字</div>
            </div>

            <!-- Email -->
            <div class="mb-3">
              <label class="form-label">Email</label>
              <input
                v-model.trim="email"
                type="email"
                class="form-control"
                placeholder="輸入Email"
                required
              />
              <div v-if="email && !isEmailFormat(email)" class="text-danger small mt-1">
                Email 格式不正確
              </div>
            </div>

            <!-- 帳號 -->
            <div class="mb-3">
              <label class="form-label">帳號</label>
              <input
                v-model="account"
                type="text"
                class="form-control"
                placeholder="輸入帳號"
                required
              />
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

            <button type="submit" class="btn btn-success w-100" :disabled="!canSendCode || loading">
              寄送驗證碼到我的信箱
            </button>

            <!-- footer 導引：已有帳號？去登入 -->
            <div class="text-center mt-4 small text-muted">
              已經有帳號了？
              <router-link to="/signin" class="link-success text-decoration-none">
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
              />
            </div>

            <button type="submit" class="btn btn-primary w-100" :disabled="!canFinish || loading">
              完成註冊並登入
            </button>

            <button
              type="button"
              class="btn btn-link w-100 mt-2"
              :disabled="loading"
              @click="handleResend"
            >
              重新寄驗證碼
            </button>

            <hr class="my-4 divider-line" />

            <!-- footer 導引：其實我已經有帳號 -->
            <div class="text-center mt-4 small text-muted">
              已經有帳號了？
              <router-link to="/signin" class="link-success text-decoration-none">
                直接登入
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
</style>
