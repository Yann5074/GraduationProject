<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { loginAPI, getMeAPI, googleLoginAPI } from '@/api/Member'
import { createMemberDTO } from '@/dtos/MemberDTO'
import { loadGoogleSdk } from '@/utils/google'

const router = useRouter()
const auth = useAuthStore()

const account = ref('')
const password = ref('')
const loading = ref(false)
const errorMsg = ref('')
const showPassword = ref(false)

const accountValid = computed(() => account.value.trim().length > 0)
const passwordValid = computed(() => password.value.length >= 6)
const canSubmit = computed(() => accountValid.value && passwordValid.value && !loading.value)

const onSubmit = async (e) => {
  e.preventDefault()
  errorMsg.value = ''
  if (!canSubmit.value) {
    errorMsg.value = '請輸入帳號與至少 6 碼的密碼'
    return
  }

  loading.value = true
  try {
    // 1) 送帳密給後端登入 (建立 Session)
    await loginAPI(account.value, password.value)

    // 2) 立刻打 /me 拿當前登入者資訊
    const { data: meRaw } = await getMeAPI()

    // 3) 把後端回傳的欄位整理成前端統一格式
    //    createMemberDTO 會幫你準備好 displayName、imageUrl 等欄位
    const model = createMemberDTO(meRaw)

    // 4) ✅ 用 auth.login() 而不是 setUser()
    //    login() 會：
    //      - 把使用者資料存進 Pinia + localStorage
    //      - 幫你把頭貼寫進 auth (呼叫 setAvatar)
    //    這樣導覽列會立刻拿到頭貼，而不需要重整
    await auth.login({ user: model })

    // 5) 導回首頁
    router.push('/home')
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
// 顯示錯誤訊息用
const gErr = ref('')

// Google 登入初始化
onMounted(async () => {
  try {
    await loadGoogleSdk()

    const clientId = import.meta.env.VITE_GOOGLE_CLIENT_ID
    if (!clientId) {
      gErr.value = '缺少 VITE_GOOGLE_CLIENT_ID 設定'
      return
    }

    // 初始化 + 渲染按鈕
    /* global google */
    window.google.accounts.id.initialize({
      client_id: clientId,
      callback: async (res) => {
        // res.credential 就是 Google 的 id_token
        try {
          const apiRes = await googleLoginAPI(res.credential)
          await auth.login({ user: apiRes.data }) // 和帳密登入完全一致
          router.push('/')
        } catch (err) {
          gErr.value = err?.message || err?.data?.message || 'Google 登入失敗'
        }
      },
    })

    // 渲染按鈕到指定容器
    const btn = document.getElementById('googleSignInBtn')
    if (btn) {
      window.google.accounts.id.renderButton(btn, {
        theme: 'outline',
        size: 'large',
        width: '100%',
        shape: 'pill',
        text: 'signin_with', // or "continue_with"
        logo_alignment: 'left',
      })
    }

    // 可選：顯示 one tap（不需要按按鈕也可以出現）
    // window.google.accounts.id.prompt();
  } catch (e) {
    gErr.value = '無法載入 Google 登入元件'
  }
})
</script>

<template>
  <div class="min-vh-100 d-flex align-items-center">
    <div class="container">
      <div class="row justify-content-center">
        <div class="col-12 col-md-8 col-lg-5">
          <div class="card border-0 shadow-lg">
            <div class="card-header bg-dark text-white text-center py-3">
              <h4 class="mb-0">登入</h4>
            </div>

            <div class="card-body p-4 pb-0">
              <div v-if="errorMsg" class="alert alert-danger py-2" role="alert">{{ errorMsg }}</div>

              <form @submit="onSubmit" novalidate>
                <div class="mb-3">
                  <label for="account" class="form-label">帳號</label>
                  <input
                    id="account"
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

                <div class="mb-3">
                  <label for="password" class="form-label mb-0">密碼</label>

                  <div class="input-group input-group-lg has-validation">
                    <input
                      :type="showPassword ? 'text' : 'password'"
                      id="password"
                      class="form-control border-end-0 rounded-end-0"
                      :class="{ 'is-invalid': password.length > 0 && password.length < 6 }"
                      placeholder="至少 6 碼"
                      v-model="password"
                      autocomplete="current-password"
                      minlength="6"
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

                  <!-- 密碼長度提示 -->
                  <div
                    v-if="password.length > 0 && password.length < 6"
                    class="invalid-feedback d-block"
                  >
                    請輸入至少 6 碼的密碼
                  </div>
                </div>

                <div class="d-grid">
                  <button
                    type="submit"
                    class="btn btn-success btn-lg login-btn"
                    :disabled="!canSubmit || loading"
                  >
                    <span v-if="loading" class="spinner-border spinner-border-sm me-2" />
                    {{ loading ? '登入中...' : '登入' }}
                  </button>
                </div>
              </form>
            </div>
            <!-- 分隔線 -->
            <div class="d-flex align-items-center mt-3 mb-1">
              <hr class="flex-grow-1" />
              <span class="px-2 text-muted small">或</span>
              <hr class="flex-grow-1" />
            </div>

            <!-- Google 登入 -->
            <div>
              <div id="googleSignInBtn" class="w-100 d-flex justify-content-center"></div>
              <div v-if="gErr" class="text-danger small mt-2">{{ gErr }}</div>
            </div>

            <div
              class="card-footer text-center bg-white pt-3 pb-4 d-flex justify-content-evenly align-items-center"
            >
              <small class="text-muted">
                還沒有帳號？
                <RouterLink class="auth-link text-success" to="/signup">註冊</RouterLink>
              </small>
              <div>
                <RouterLink
                  class="auth-link text-decoration-none link-danger small"
                  to="/forgot-password"
                >
                  忘記密碼？
                </RouterLink>
              </div>
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
.card-footer {
  border-top: none !important;
}
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
.card-footer small,
.card-footer a {
  font-size: 0.9rem;
}
/* 讓兩個連結的預設樣式一致 */
.auth-link {
  text-decoration: none; /* 預設沒有底線 */
  font-weight: 400; /* 預設不加粗 */
  transition:
    color 0.15s ease,
    text-decoration-color 0.15s ease,
    font-weight 0.15s ease; /* 平滑過渡 */
}

/* 滑鼠移入 & 鍵盤可見焦點 時才加粗 + 底線（無障礙友善） */
.auth-link:hover,
.auth-link:focus-visible {
  text-decoration: underline;
  font-weight: 600;
}

/* （可選）整行左右排版用的容器微調 */
.auth-inline {
  gap: 0.75rem;
}
/* 登入按鈕的互動效果 */
/* ✅ 用變數覆寫 Bootstrap 按鈕配色（scoped 可用）*/
/* 登入按鈕效果 */
.login-btn {
  background-color: #198754; /* 正常綠色 */
  border-color: #198754;
  color: #fff;
  transition:
    background-color 0.25s ease,
    border-color 0.25s ease,
    box-shadow 0.2s ease;
}

/* 滑鼠移入（可點擊狀態才有效） */
.login-btn:hover:not(:disabled) {
  background-color: #28a96b; /* 稍亮一點 */
  border-color: #28a96b;
}

/* 按下狀態 */
.login-btn:active:not(:disabled) {
  background-color: #157347; /* 稍深一點 */
  border-color: #157347;
}

/* 聚焦狀態（鍵盤導覽時） */
.login-btn:focus-visible {
  box-shadow: 0 0 0 0.25rem rgba(25, 135, 84, 0.25);
}

/* ✅ 禁用狀態（未輸入帳密或登入中） */
.login-btn:disabled {
  background-color: #cde5d6; /* 更淺的綠色 */
  border-color: #cde5d6;
  color: #ffffff;
  cursor: not-allowed; /* 🚫 禁止游標 */
  opacity: 1; /* 不要太透明，清楚顯示變淡 */
}
</style>
