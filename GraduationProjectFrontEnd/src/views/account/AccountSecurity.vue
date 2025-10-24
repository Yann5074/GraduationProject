<script setup>
import { ref, computed } from 'vue'
import { updatePasswordAPI } from '@/api/Member'
import { createUpdatePasswordDTO } from '@/dtos/MemberDTO'

const oldPassword = ref('')
const newPassword = ref('')
const confirmPassword = ref('')
const show = ref(false)
const loading = ref(false)
const okMsg = ref('')
const errMsg = ref('')

const canSubmit = computed(
  () =>
    oldPassword.value &&
    newPassword.value &&
    confirmPassword.value &&
    newPassword.value.length >= 6 &&
    newPassword.value === confirmPassword.value,
)

async function onSubmit(e) {
  e?.preventDefault()
  errMsg.value = ''
  okMsg.value = ''
  if (!canSubmit.value) {
    errMsg.value = '請正確填寫欄位（至少 6 碼且兩次一致）'
    return
  }
  loading.value = true
  try {
    const dto = createUpdatePasswordDTO({
      oldPassword: oldPassword.value,
      newPassword: newPassword.value,
      confirmPassword: confirmPassword.value,
    })
    await updatePasswordAPI(dto)
    okMsg.value = '已更新密碼'
    oldPassword.value = newPassword.value = confirmPassword.value = ''
  } catch (err) {
    errMsg.value = err?.response?.data?.message || '更新失敗'
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <h5 class="mb-4 fw-semibold">更改密碼</h5>
  <form class="vstack gap-3" @submit="onSubmit">
    <div v-if="errMsg" class="alert alert-danger py-2">{{ errMsg }}</div>
    <div v-if="okMsg" class="alert alert-success py-2">{{ okMsg }}</div>

    <div>
      <label class="form-label">目前密碼</label>
      <input
        :type="show ? 'text' : 'password'"
        v-model="oldPassword"
        class="form-control"
        required
      />
    </div>
    <div>
      <label class="form-label">新密碼（至少 6 碼）</label>
      <input
        :type="show ? 'text' : 'password'"
        v-model="newPassword"
        class="form-control"
        minlength="6"
        required
      />
    </div>
    <div>
      <label class="form-label">確認新密碼</label>
      <input
        :type="show ? 'text' : 'password'"
        v-model="confirmPassword"
        class="form-control"
        minlength="6"
        required
      />
    </div>

    <div class="d-flex align-items-center gap-3">
      <div class="form-check">
        <input class="form-check-input" type="checkbox" v-model="show" id="showPw" />
        <label class="form-check-label" for="showPw">顯示密碼</label>
      </div>

      <button class="btn btn-success ms-auto" type="submit" :disabled="!canSubmit || loading">
        <span v-if="loading" class="spinner-border spinner-border-sm me-2"></span>
        儲存
      </button>
    </div>
  </form>
</template>
