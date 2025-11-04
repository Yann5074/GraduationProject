<script setup>
import { ref } from 'vue'
import axios from 'axios'

const http = axios.create({
  baseURL: import.meta.env.VITE_API_BASE || 'https://localhost:7131',
  withCredentials: true,
  timeout: 10000,
})

const oldPassword = ref('')
const newPassword = ref('')
const confirmNewPassword = ref('')

const showOld = ref(false)
const showNew = ref(false)
const showConfirm = ref(false)

const saving = ref(false)
const okMsg = ref('')
const errMsg = ref('')

// ✅ 目前密碼檢查相關狀態
const checkingPwd = ref(false) // 是否正在驗證
const oldPwdValid = ref(true) // 驗證結果：true=正確 / false=錯誤
const oldPwdTouched = ref(false) // 使用者是否曾經離開該欄位（避免一開始就顯示錯誤）

// ✅ 目前密碼驗證：使用 /api/Member/me/checkPassword 來檢查
async function checkCurrentPassword() {
  // 如果沒有輸入，就不打 API
  if (!oldPassword.value) {
    oldPwdTouched.value = false
    oldPwdValid.value = true
    checkingPwd.value = false
    return
  }

  oldPwdTouched.value = true
  checkingPwd.value = true
  oldPwdValid.value = true

  try {
    const { data } = await http.post('/api/Member/me/checkPassword', {
      password: oldPassword.value,
    })
    // 後端回傳 { ok: true/false, message: "..." }
    oldPwdValid.value = !!data?.ok
  } catch (err) {
    console.warn('驗證目前密碼時發生錯誤:', err)
    oldPwdValid.value = false
  } finally {
    checkingPwd.value = false
  }
}

let oldPwdTimer
// 使用者在修改舊密碼時，先把錯誤提示暫時收起來
function handleOldPwdInput() {
  oldPwdValid.value = true
  // 等到使用者離開輸入框 (blur) 再真正去打 API
  oldPwdTouched.value = true
  if (oldPwdTimer) clearTimeout(oldPwdTimer)
  oldPwdTimer = setTimeout(() => {
    checkCurrentPassword()
  }, 350)
}

// ✅ 點「儲存」時送更新密碼 API
async function onSave() {
  okMsg.value = ''
  errMsg.value = ''

  // 基本欄位檢查
  if (!oldPassword.value || !newPassword.value || !confirmNewPassword.value) {
    errMsg.value = '請完整填寫所有欄位'
    return
  }

  // 如果使用者還沒觸發過 blur 檢查，就先檢查一次
  if (!oldPwdTouched.value) {
    await checkCurrentPassword()
  }
  // 若剛好在驗證中，等它完成（避免競態）
  while (checkingPwd.value) {
    await new Promise((r) => setTimeout(r, 50))
  }
  if (!oldPwdValid.value) {
    errMsg.value = '目前密碼不正確'
    return
  }

  if (newPassword.value.length < 6) {
    errMsg.value = '新密碼至少需要 6 碼'
    return
  }

  if (newPassword.value !== confirmNewPassword.value) {
    errMsg.value = '新密碼與確認密碼不一致'
    return
  }

  saving.value = true
  try {
    await http.put('/api/Member/me/UpdatePassword', {
      oldPassword: oldPassword.value,
      newPassword: newPassword.value,
      confirmNewPassword: confirmNewPassword.value,
    })

    okMsg.value = '密碼已更新'
    errMsg.value = ''

    // 清空欄位
    oldPassword.value = ''
    newPassword.value = ''
    confirmNewPassword.value = ''

    // 重置驗證狀態
    oldPwdValid.value = true
    oldPwdTouched.value = false
    checkingPwd.value = false
  } catch (err) {
    errMsg.value =
      err?.response?.data?.message ||
      err?.response?.data?.Message ||
      '修改失敗，請確認目前密碼是否正確'
  } finally {
    saving.value = false
  }
}
</script>

<template>
  <div class="container py-4">
    <h5 class="mb-4 fw-semibold">更改密碼</h5>

    <div v-if="errMsg" class="alert alert-danger py-2">{{ errMsg }}</div>
    <div v-if="okMsg" class="alert alert-success py-2">{{ okMsg }}</div>

    <div class="row g-4">
      <div class="col-12 col-lg-6">
        <!-- 目前密碼 -->
        <div class="mb-3">
          <label class="form-label">目前密碼</label>
          <div class="input-group input-group-lg has-validation">
            <input
              :type="showOld ? 'text' : 'password'"
              class="form-control border-end-0 rounded-end-0"
              v-model.trim="oldPassword"
              @input="handleOldPwdInput"
              @blur="checkCurrentPassword"
              :class="{ 'is-invalid': oldPwdTouched && !oldPwdValid }"
              aria-describedby="oldPwdFeedback"
              autocomplete="current-password"
              required
            />
            <button
              type="button"
              class="btn btn-outline-secondary btn-eye border-start-0 rounded-start-0"
              @click="showOld = !showOld"
              tabindex="-1"
              :aria-label="showOld ? '隱藏密碼' : '顯示密碼'"
              :title="showOld ? '隱藏密碼' : '顯示密碼'"
            >
              <i :class="showOld ? 'bi bi-eye-slash' : 'bi bi-eye'"></i>
            </button>
          </div>

          <!-- 動態提示：驗證中 / 錯誤 -->
          <div v-if="checkingPwd" class="text-muted small mt-1">正在驗證密碼...</div>

          <div
            v-else-if="oldPwdTouched && !oldPwdValid"
            class="invalid-feedback d-block mt-1"
            id="oldPwdFeedback"
          >
            目前密碼不正確
          </div>
        </div>

        <!-- 新密碼 -->
        <div class="mb-3">
          <label class="form-label">新密碼（至少 6 碼）</label>
          <div class="input-group input-group-lg has-validation">
            <input
              :type="showNew ? 'text' : 'password'"
              class="form-control border-end-0 rounded-end-0"
              v-model.trim="newPassword"
              autocomplete="new-password"
              minlength="6"
              required
            />
            <button
              type="button"
              class="btn btn-outline-secondary btn-eye border-start-0 rounded-start-0"
              @click="showNew = !showNew"
              tabindex="-1"
              :aria-label="showNew ? '隱藏密碼' : '顯示密碼'"
              :title="showNew ? '隱藏密碼' : '顯示密碼'"
            >
              <i :class="showNew ? 'bi bi-eye-slash' : 'bi bi-eye'"></i>
            </button>
          </div>

          <div
            v-if="newPassword.length > 0 && newPassword.length < 6"
            class="invalid-feedback d-block"
          >
            新密碼至少需要 6 碼
          </div>
        </div>

        <!-- 確認新密碼 -->
        <div class="mb-3">
          <label class="form-label">確認新密碼</label>
          <div class="input-group input-group-lg has-validation">
            <input
              :type="showConfirm ? 'text' : 'password'"
              class="form-control border-end-0 rounded-end-0"
              v-model.trim="confirmNewPassword"
              autocomplete="new-password"
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

          <div
            v-if="confirmNewPassword.length > 0 && confirmNewPassword !== newPassword"
            class="invalid-feedback d-block"
          >
            兩次輸入的密碼不一致
          </div>
        </div>

        <!-- 儲存按鈕 -->
        <div class="text-start mt-3">
          <button class="btn btn-success px-4" :disabled="saving" @click="onSave">
            <span v-if="saving" class="spinner-border spinner-border-sm me-2"></span>
            {{ saving ? '儲存中...' : '儲存' }}
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
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
</style>
