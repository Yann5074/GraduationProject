<!-- src/views/SignIn.vue -->
<script setup>
import { ref, computed } from 'vue'
import { useRouter } from 'vue-router'

const router = useRouter()

// form state
const email = ref('')
const password = ref('')
const remember = ref(true)
const showPassword = ref(false)
const loading = ref(false)
const errorMsg = ref('')

// basic validation
const emailValid = computed(() => /\S+@\S+\.\S+/.test(email.value))
const passwordValid = computed(() => password.value.length >= 6)
const canSubmit = computed(() => emailValid.value && passwordValid.value && !loading.value)

const onSubmit = async (e) => {
  e.preventDefault()
  errorMsg.value = ''
  if (!canSubmit.value) {
    errorMsg.value = '請輸入有效的 Email 與至少 6 碼的密碼'
    return
  }
  loading.value = true
  try {
    // TODO: 這裡換成你實際的登入 API
    // const res = await fetch('/api/auth/login', { method:'POST', body: JSON.stringify({ email: email.value, password: password.value, remember: remember.value }) })
    // if (!res.ok) throw new Error('登入失敗')

    // 模擬成功
    await new Promise(r => setTimeout(r, 600))

    // 登入後導頁（自行替換路徑）
    router.push('/')

  } catch (err) {
    errorMsg.value = (err && err.message) || '登入失敗，請再試一次'
  } finally {
    loading.value = false
  }
}
</script>

<template>
<!-- Start Hero Section -->
	<div class="hero">
		<div class="container">
			<div class="row justify-content-between">
				<div class="col-lg-5">
					<div class="intro-excerpt">
						<h1>Sign In</h1>
					</div>
				</div>
				<div class="col-lg-7"></div>
			</div>
		</div>
	</div>
<!-- End Hero Section -->

  <div
    class="min-vh-100 d-flex align-items-center"
    :style="{
      backgroundSize: 'cover',
      backgroundPosition: 'center'
    }"
  >
    <div class="container">
      <div class="row justify-content-center">
        <div class="col-12 col-md-8 col-lg-5">
          <!-- card -->
          <div class="card border-0 shadow-lg">
            <div class="card-header bg-dark text-white text-center py-3">
              <h4 class="mb-0">Sign In</h4>
            </div>

            <div class="card-body p-4">
              <!-- error -->
              <div v-if="errorMsg" class="alert alert-danger py-2" role="alert">
                {{ errorMsg }}
              </div>

              <form @submit="onSubmit" novalidate>
                <!-- Email -->
                <div class="mb-3">
                  <label for="email" class="form-label">Email</label>
                  <input
                    id="email"
                    name="email"
                    type="email"
                    class="form-control"
                    :class="{ 'is-invalid': email && !emailValid }"
                    placeholder="you@example.com"
                    v-model.trim="email"
                    autocomplete="email"
                    required
                  />
                  <div class="invalid-feedback">請輸入有效的 Email</div>
                </div>

                <!-- Password -->
                <div class="mb-3">
                  <div class="d-flex justify-content-between align-items-center">
                    <label for="password" class="form-label mb-0">Password</label>
                    <button
                      type="button"
                      class="btn btn-sm btn-link text-decoration-none"
                      @click="showPassword = !showPassword"
                      :aria-pressed="showPassword"
                    >
                      {{ showPassword ? '隱藏' : '顯示' }}密碼
                    </button>
                  </div>

                  <div class="input-group">
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
                  </div>
                  <div class="invalid-feedback">密碼長度至少 6 碼</div>
                </div>

                <!-- Remember me -->
                <div class="form-check form-switch mb-3">
                  <input
                    class="form-check-input"
                    type="checkbox"
                    id="rememberMe"
                    name="rememberMe"
                    v-model="remember"
                  />
                  <label class="form-check-label" for="rememberMe">Remember me</label>
                </div>

                <!-- Submit -->
                <div class="d-grid">
                  <button
                    type="submit"
                    class="btn btn-success btn-lg"
                    :disabled="!canSubmit"
                  >
                    <span v-if="loading" class="spinner-border spinner-border-sm me-2" role="status" aria-hidden="true"></span>
                    {{ loading ? 'Signing in...' : 'Sign In' }}
                  </button>
                </div>

                <!-- Divider -->
                <div class="text-center text-muted my-3">
                  <small>or continue with</small>
                </div>

                <!-- Social (可自行移除) -->
                <div class="d-flex justify-content-center gap-2">
                  <button type="button" class="btn btn-outline-secondary">
                    <i class="fab fa-facebook me-1"></i> Facebook
                  </button>
                  <button type="button" class="btn btn-outline-secondary">
                    <i class="fab fa-github me-1"></i> GitHub
                  </button>
                  <button type="button" class="btn btn-outline-secondary">
                    <i class="fab fa-google me-1"></i> Google
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
          <!-- /card -->
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
/* 讓卡片有「浮起來」感覺 */
.card {
  border-radius: 1rem;
}
</style>
