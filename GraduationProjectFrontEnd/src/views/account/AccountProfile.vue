<script setup>
import { onMounted, reactive, ref, computed } from 'vue'
import { useAuthStore } from '@/stores/auth'
import { getMeAPI, updateMeAPI, uploadPhotoAPI } from '@/api/Member'
import {
  createMemberModel,
  mergeMemberInto,
  createUpdateMeDTO,
  applyUploadPhotoDTO,
} from '@/dtos/MemberDTO'

const auth = useAuthStore()

// 作為「表單」的本地資料，不直接等於 auth.user，避免污染 navbar
const me = reactive(createMemberModel())

const loading = ref(true)
const saving = ref(false)
const okMsg = ref('')
const errMsg = ref('')

// 第一次載入個資
async function load() {
  loading.value = true
  okMsg.value = ''
  errMsg.value = ''
  try {
    // 後端最新資料
    const res = await getMeAPI()

    // 把後端資料合併進表單 me
    // 這會把帳號、暱稱、email、memberImage 等等帶進來
    mergeMemberInto(me, res) // me.imageUrl 會是完整可用的頭貼網址

    // ❌ 不要再整包覆蓋 auth (這會弄壞 navbar)
    // auth.setUser(res)

    // ✅ 改成只更新 navbar 需要顯示的部分欄位(名稱/暱稱/電話等等)，但是保留原本的頭貼
    auth.updateProfileFields({
      name: me.name,
      displayName: me.displayName,
      email: me.email,
      phone: me.phone,
      address: me.address,
      gender: me.gender,
      // 不動 memberImage，因為它本來就已經在 auth.user 內
    })
  } catch (err) {
    errMsg.value = err?.response?.data?.message || '載入失敗（請確認已登入且 Cookie 有帶到）'
  } finally {
    loading.value = false
  }
}

onMounted(load)

// 頭貼顯示用：
// 1. 先用表單裡 me.imageUrl（mergeMemberInto 幫你組好的完整網址）
// 2. 沒有的話 fallback 到 auth.avatarUrl（Pinia 計算好的頭貼）
// 3. 最後才用預設圖
const normalizedImageUrl = computed(() => {
  return me.imageUrl || auth.avatarUrl || '/asset/images/user.svg'
})

// 必填欄位檢查
const canSave = computed(() => {
  const dnLen = me.displayName?.trim().length ?? 0
  const emailLen = me.email?.trim().length ?? 0
  return dnLen > 0 && emailLen > 0
})

// 儲存「基本資料」按鈕
// 儲存基本資料
async function onSave() {
  if (!canSave.value) {
    errMsg.value = '請完成必填欄位'
    return
  }

  saving.value = true
  okMsg.value = ''
  errMsg.value = ''

  try {
    const payload = createUpdateMeDTO(me)
    await updateMeAPI(payload) // 後端回 204 表示成功
    okMsg.value = '已儲存變更'

    // ✅ 更新暱稱、email 等資料到 Pinia
    auth.updateProfileFields({
      name: me.name,
      displayName: me.displayName,
      email: me.email,
      phone: me.phone,
      address: me.address,
      gender: me.gender,
    })

    // ✅ 現在才正式更新 Navbar 頭貼
    auth.setAvatar(me.imageUrl)
  } catch (err) {
    errMsg.value = err?.response?.data?.message || '儲存失敗（請確認欄位與格式）'
  } finally {
    saving.value = false
  }
}

// 上傳頭像
// 上傳頭像 → 只更新預覽，不改 navbar
async function onPickFile(e) {
  const file = e.target.files?.[0]
  if (!file) return

  okMsg.value = ''
  errMsg.value = ''

  const okTypes = ['image/jpeg', 'image/png', 'image/webp']
  if (!okTypes.includes(file.type)) {
    errMsg.value = '僅支援 jpg、png、webp 格式'
    e.target.value = ''
    return
  }
  if (file.size > 2 * 1024 * 1024) {
    errMsg.value = '檔案需小於 2MB'
    e.target.value = ''
    return
  }

  try {
    const res = await uploadPhotoAPI(file)
    // 只更新右邊預覽
    applyUploadPhotoDTO(me, res)

    // ❌ 不要更新導覽列
    // auth.updateAvatar(me.imageUrl)

    okMsg.value = '已選擇新頭像，請記得按「儲存」'
  } catch (err) {
    errMsg.value = err?.response?.data?.message || '上傳失敗'
  } finally {
    e.target.value = ''
  }
}
</script>

<template>
  <div v-if="loading" class="text-center text-muted py-5">載入中...</div>

  <template v-else>
    <h5 class="mb-4 fw-semibold">我的檔案</h5>

    <div class="row g-4">
      <!-- 左邊：基本資料 -->
      <div class="col-12 col-lg-8">
        <div v-if="errMsg" class="alert alert-danger py-2">{{ errMsg }}</div>
        <div v-if="okMsg" class="alert alert-success py-2">{{ okMsg }}</div>

        <div class="mb-3">
          <label class="form-label">使用者帳號</label>
          <input type="text" class="form-control" :value="me.account" disabled />
        </div>

        <div class="mb-3">
          <label class="form-label"> Email </label>
          <input :value="me.email" type="text" class="form-control" disabled />
        </div>

        <div class="mb-3">
          <label class="form-label">姓名</label>
          <input v-model.trim="me.name" type="text" class="form-control" />
        </div>

        <div class="mb-3">
          <label class="form-label"> 暱稱（顯示名稱） </label>
          <input v-model.trim="me.displayName" type="text" class="form-control" />
        </div>

        <div class="mb-3">
          <label class="form-label">手機</label>
          <input v-model.trim="me.phone" type="text" class="form-control" />
        </div>

        <div class="mb-3">
          <label class="form-label">地址</label>
          <input v-model.trim="me.address" type="text" class="form-control" />
        </div>

        <div class="mb-3">
          <label class="form-label">性別</label>
          <div class="d-flex gap-3">
            <label class="form-check">
              <input v-model="me.gender" class="form-check-input" type="radio" :value="1" />
              <span class="form-check-label">男性</span>
            </label>
            <label class="form-check">
              <input v-model="me.gender" class="form-check-input" type="radio" :value="2" />
              <span class="form-check-label">女性</span>
            </label>
            <label class="form-check">
              <input v-model="me.gender" class="form-check-input" type="radio" :value="3" />
              <span class="form-check-label">中性</span>
            </label>
            <label class="form-check">
              <input v-model="me.gender" class="form-check-input" type="radio" :value="4" />
              <span class="form-check-label">不透露</span>
            </label>
          </div>
        </div>

        <div class="d-grid d-sm-inline-block mt-3">
          <button class="btn btn-success px-4" :disabled="saving || !canSave" @click="onSave">
            <span v-if="saving" class="spinner-border spinner-border-sm me-2"></span>
            {{ saving ? '儲存中...' : '儲存' }}
          </button>
        </div>
      </div>

      <!-- 右邊：頭像上傳 -->
      <div class="col-12 col-lg-4">
        <div class="border rounded-3 p-3 text-center">
          <img
            :src="normalizedImageUrl"
            alt="avatar"
            class="rounded-circle border mb-3"
            style="width: 120px; height: 120px; object-fit: cover"
            @error="$event.target.src = '/asset/images/user.svg'"
          />
          <div class="d-grid">
            <label class="btn btn-outline-secondary btn-sm">
              <input type="file" class="d-none" accept="image/*" @change="onPickFile" />
              選擇圖片
            </label>
          </div>
          <div class="text-muted small mt-2">檔案限制：jpg、png、webp，2MB 以內</div>
        </div>
      </div>
    </div>
  </template>
</template>
