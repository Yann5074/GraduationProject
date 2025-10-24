<script setup>
import { ref, computed } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { loginAPI, getMeAPI } from '@/api/Member'
import { createMemberDTO } from '@/dtos/MemberDTO'

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
  <div class="min-vh-100 d-flex align-items-center">
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
