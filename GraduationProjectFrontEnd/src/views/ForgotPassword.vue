<script setup>
import { ref, computed } from 'vue'
import { useRouter } from 'vue-router'
import http from '@/api/axios'

const router = useRouter()

// 兩步狀態
const step = ref(1)
const loading = ref(false)

// Step1 欄位
const account = ref('')
const email = ref('')

// Step2 欄位
const code = ref('')
const newPassword = ref('')
const confirmNewPassword = ref('')

// UI 狀態
const showNewPwd = ref(false)
const showConfirmPwd = ref(false)

const errorMsg = ref('')
const infoMsg = ref('')
const successMsg = ref('')

// --------- 驗證邏輯 ----------
function isEmailFormat(v) {
  return /^[^@\s]+@[^@\s]+\.[^@\s]+$/.test(v)
}

const canSendCode = computed(() => {
  return account.value && email.value && isEmailFormat(email.value) && !loading.value
})

const pwdTooShort = computed(() => newPassword.value.length > 0 && newPassword.value.length < 6)

const confirmMismatch = computed(() => {
  if (newPassword.value.length < 6 || confirmNewPassword.value.length < 6) return false
  return newPassword.value !== confirmNewPassword.value
})

const canReset = computed(() => {
  return (
    code.value &&
    code.value.length >= 6 &&
    newPassword.value.length >= 6 &&
    confirmNewPassword.value.length >= 6 &&
    newPassword.value === confirmNewPassword.value &&
    !loading.value
  )
})

// --------- API 包裝 ----------
function sendResetCodeAPI(payload) {
  // POST /api/Member/send-reset-code
  return http.post('/Member/send-reset-code', payload)
}

function resetPasswordAPI(payload) {
  // POST /api/Member/reset-password
  return http.post('/Member/reset-password', payload)
}

// --------- Step1: 寄驗證碼 ----------
async function handleSendCode() {
  errorMsg.value = ''
  infoMsg.value = ''
  successMsg.value = ''

  if (!canSendCode.value) {
    errorMsg.value = '請輸入正確的帳號與 Email'
    return
  }

  loading.value = true
  try {
    const res = await sendResetCodeAPI({
      account: account.value,
      email: email.value,
    })

    if (!res.data?.ok) {
      errorMsg.value = res.data?.message || '寄送失敗'
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

// --------- Step2: 重設密碼 ----------
async function handleReset() {
  errorMsg.value = ''
  infoMsg.value = ''
  successMsg.value = ''

  if (!canReset.value) {
    errorMsg.value = '請輸入驗證碼，並設定至少 6 碼的新密碼'
    return
  }

  loading.value = true
  try {
    const res = await resetPasswordAPI({
      account: account.value,
      email: email.value,
      code: code.value,
      newPassword: newPassword.value,
      confirmNewPassword: confirmNewPassword.value,
    })

    if (!res.data?.ok) {
      errorMsg.value = res.data?.message || '重設失敗'
      loading.value = false
      return
    }

    successMsg.value = '密碼已更新，請使用新密碼登入'
    // 導回登入
    router.push('/signin')
  } catch (err) {
    errorMsg.value =
      err?.response?.data?.message || err?.response?.data?.Message || '重設失敗，請稍後再試'
  } finally {
    loading.value = false
  }
}

// 重新寄送
async function handleResend() {
  if (loading.value) return
  loading.value = true
  errorMsg.value = ''
  infoMsg.value = ''
  successMsg.value = ''

  try {
    const res = await sendResetCodeAPI({
      account: account.value,
      email: email.value,
    })

    if (!res.data?.ok) {
      errorMsg.value = res.data?.message || '重新寄送失敗'
      return
    }

    infoMsg.value = `新的驗證碼已寄到 ${email.value}，請再確認信箱`
  } catch (err) {
    errorMsg.value = err?.response?.data?.message || err?.response?.data?.Message || '重新寄送失敗'
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
        <div class="card-header text-center bg-dark text-white">忘記密碼</div>

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
                驗證身份
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
                重設密碼
              </div>
            </div>
          </div>
        </div>

        <!-- 內文 -->
        <div class="card-body">
          <!-- 訊息區 -->
          <div v-if="errorMsg" class="alert alert-danger py-2">{{ errorMsg }}</div>
          <div v-if="infoMsg" class="alert alert-info py-2">{{ infoMsg }}</div>
          <div v-if="successMsg" class="alert alert-success py-2">{{ successMsg }}</div>

          <!-- STEP 1：輸入帳號 + email 以寄驗證碼 -->
          <form v-if="step === 1" @submit.prevent="handleSendCode">
            <div class="mb-3">
              <label class="form-label">帳號</label>
              <input
                v-model="account"
                class="form-control"
                type="text"
                placeholder="輸入帳號"
                required
              />
            </div>

            <div class="mb-3">
              <label class="form-label">Email</label>
              <input
                v-model.trim="email"
                class="form-control"
                type="email"
                placeholder="輸入註冊用 Email"
                required
              />
              <div v-if="email && !isEmailFormat(email)" class="text-danger small mt-1">
                Email 格式不正確
              </div>
            </div>

            <button
              type="submit"
              class="btn btn-success w-100 send-btn"
              :disabled="!canSendCode || loading"
            >
              <span v-if="loading" class="spinner-border spinner-border-sm me-2"></span>
              {{ loading ? '寄送中...' : '寄送驗證碼到我的信箱' }}
            </button>

            <!-- 回登入 -->
            <div class="text-center mt-4 small text-muted">
              想起密碼了？
              <router-link to="/signin" class="link-success text-decoration-none auth-link">
                回登入
              </router-link>
            </div>
          </form>

          <!-- STEP 2：輸入驗證碼 + 設定新密碼 -->
          <form v-else @submit.prevent="handleReset">
            <div class="mb-3">
              <label class="form-label">帳號</label>
              <input class="form-control" :value="account" disabled />
            </div>

            <div class="mb-3">
              <label class="form-label">Email</label>
              <input class="form-control" :value="email" disabled />
            </div>

            <div class="mb-3">
              <label class="form-label">驗證碼 6 碼</label>
              <input
                v-model="code"
                class="form-control"
                type="text"
                maxlength="6"
                placeholder="輸入 Email 收到的驗證碼"
                required
                @input="code = code.replace(/\D/g, '').slice(0, 6)"
              />
            </div>

            <div class="mb-3">
              <label class="form-label">新密碼</label>
              <div class="input-group input-group-lg has-validation">
                <input
                  :type="showNewPwd ? 'text' : 'password'"
                  class="form-control border-end-0 rounded-end-0"
                  v-model.trim="newPassword"
                  placeholder="至少 6 碼"
                  required
                />
                <button
                  type="button"
                  class="btn btn-outline-secondary btn-eye border-start-0 rounded-start-0"
                  @click="showNewPwd = !showNewPwd"
                  tabindex="-1"
                  :aria-label="showNewPwd ? '隱藏密碼' : '顯示密碼'"
                  :title="showNewPwd ? '隱藏密碼' : '顯示密碼'"
                >
                  <i :class="showNewPwd ? 'bi bi-eye-slash' : 'bi bi-eye'"></i>
                </button>
              </div>
              <div v-if="pwdTooShort" class="text-danger small mt-1">密碼至少 6 碼</div>
            </div>

            <div class="mb-3">
              <label class="form-label">確認新密碼</label>
              <div class="input-group input-group-lg has-validation">
                <input
                  :type="showConfirmPwd ? 'text' : 'password'"
                  class="form-control border-end-0 rounded-end-0"
                  v-model.trim="confirmNewPassword"
                  placeholder="再次輸入新密碼"
                  required
                />
                <button
                  type="button"
                  class="btn btn-outline-secondary btn-eye border-start-0 rounded-start-0"
                  @click="showConfirmPwd = !showConfirmPwd"
                  tabindex="-1"
                  :aria-label="showConfirmPwd ? '隱藏密碼' : '顯示密碼'"
                  :title="showConfirmPwd ? '隱藏密碼' : '顯示密碼'"
                >
                  <i :class="showConfirmPwd ? 'bi bi-eye-slash' : 'bi bi-eye'"></i>
                </button>
              </div>

              <div v-if="confirmMismatch" class="text-danger small mt-1">兩次密碼不一致</div>
            </div>

            <button
              type="submit"
              class="btn btn-primary w-100 finish-btn"
              :disabled="!canReset || loading"
            >
              重設密碼
            </button>

            <button
              type="button"
              class="btn btn-link w-100 mt-2 auth-link"
              :disabled="loading"
              @click="handleResend"
            >
              重新寄驗證碼
            </button>

            <div class="text-center mt-3 small text-muted">
              已經重設成功了？
              <router-link to="/signin" class="link-success text-decoration-none auth-link">
                立即登入
              </router-link>
            </div>
          </form>
        </div>
      </div>
    </div>
  </section>
</template>

<style scoped>
/* 進度條（沿用 SignUp.vue 風格） */
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
  border: 2px solid #6c757d;
  color: #6c757d;
  background-color: #fff;
  font-size: 14px;
  font-weight: 600;
  display: flex;
  align-items: center;
  justify-content: center;
}

.step-circle.active {
  border-color: #198754;
  color: #198754;
  background-color: #eaf6ef;
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
  background-color: #ced4da;
  border-radius: 1px;
  min-width: 40px;
}

.step-line.line-active {
  background-color: #198754;
}

/* 眼睛按鈕 */
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

/* 分隔線 */
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
/* ===== Step2 提交按鈕（與 SignUp.vue 完成註冊相同風格） ===== */
.finish-btn {
  background-color: #0d6efd; /* Bootstrap primary */
  border-color: #0d6efd;
  color: #fff;
  transition:
    background-color 0.25s ease,
    border-color 0.25s ease,
    box-shadow 0.2s ease;
}

/* 滑鼠移入（僅可點擊時） */
.finish-btn:hover:not(:disabled) {
  background-color: #0b5ed7;
  border-color: #0a58ca;
}

/* 按下 */
.finish-btn:active:not(:disabled) {
  background-color: #0a58ca;
  border-color: #0a53be;
}

/* 聚焦外框（無障礙） */
.finish-btn:focus-visible {
  box-shadow: 0 0 0 0.25rem rgba(13, 110, 253, 0.25);
}

/* ✅ 禁用（條件未滿或 loading）→ 淺藍、不可按 */
.finish-btn:disabled {
  background-color: #cfe2ff; /* 淺藍 */
  border-color: #cfe2ff;
  color: #ffffff;
  cursor: not-allowed;
  opacity: 1; /* 避免 Bootstrap 把按鈕灰掉 */
}
/* ===== 寄送驗證碼按鈕：與 SignUp.vue 一致 ===== */
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
  background-color: #cde5d6; /* 淺綠 */
  border-color: #cde5d6;
  color: #ffffff;
  cursor: not-allowed;
  opacity: 1; /* 避免被 Bootstrap 灰掉 */
}
</style>
