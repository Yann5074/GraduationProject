import http from './axios'

// 註冊
export function registerAPI(req) {
  return http.post('/Member/create', req)
}

// 登入
export function loginAPI(account, password) {
  return http.post('/Member/login', { account, password })
}

// 登出
export function logoutAPI() {
  return http.post('/Member/logout')
}

// 取得目前登入者
export function getMeAPI() {
  return http.get('/Member/me')
}

// 更新登入者資料（回 204 NoContent）
// 後端只接受 DisplayName、Name、Gender、Phone、Email、Address
export function updateMeAPI(payload) {
  return http.put('/Member/UpdateMe', payload)
}

// 修改密碼
export function updatePasswordAPI(dto) {
  // dto = { oldPassword, newPassword, confirmPassword }
  return http.put('/Member/me/UpdatePassword', dto)
}

// 上傳大頭貼（multipart/form-data）
// 要使用 multipart/form-data
export function uploadPhotoAPI(file) {
  const form = new FormData()
  form.append('file', file)
  return http.post('/Member/me/uploadphoto', form)
}

// Google 登入
export function googleLoginAPI(idToken) {
  return http.post('/Member/oauth/google', { idToken })
}
// 單一整合式檢查：一次檢查多欄位
// 一次檢查多欄位：POST /api/Member/check-unique
export async function checkDuplicateAPI(payload) {
  const res = await http.post('/Member/check-unique', payload)
  const d = res.data || {}
  // 後端回 { accountTaken, emailTaken, phoneTaken } ＝ 已被使用
  return {
    data: {
      account: d.accountTaken === undefined ? undefined : !d.accountTaken, // true = 可用
      email: d.emailTaken === undefined ? undefined : !d.emailTaken,
      phone: d.phoneTaken === undefined ? undefined : !d.phoneTaken,
    },
  }
}

// GET /api/Member/check-account?account=xxx
export async function checkAccountAPI(account) {
  const res = await http.get('/Member/check-account', { params: { account } })
  const raw = res.data
  // 後端可能回 { taken:true/false } 或直接回 boolean，或少數情況回 { available:true/false }
  const taken =
    raw && typeof raw === 'object' && 'taken' in raw
      ? raw.taken
      : raw && typeof raw === 'object' && 'AccountTaken' in raw
        ? raw.AccountTaken
        : typeof raw === 'boolean'
          ? raw
          : raw && typeof raw === 'object' && 'available' in raw
            ? !raw.available
            : false
  return { data: { available: !taken } }
}

// GET /api/Member/check-email?email=xxx
export async function checkEmailAPI(email) {
  const res = await http.get('/Member/check-email', { params: { email } })
  const raw = res.data
  const taken =
    raw && typeof raw === 'object' && 'taken' in raw
      ? raw.taken
      : raw && typeof raw === 'object' && 'EmailTaken' in raw
        ? raw.EmailTaken
        : typeof raw === 'boolean'
          ? raw
          : raw && typeof raw === 'object' && 'available' in raw
            ? !raw.available
            : false
  return { data: { available: !taken } }
}

// GET /api/Member/check-phone?phone=09xxxxxxxx
export async function checkPhoneAPI(phone) {
  const res = await http.get('/Member/check-phone', { params: { phone } })
  const raw = res.data
  const taken =
    raw && typeof raw === 'object' && 'taken' in raw
      ? raw.taken
      : raw && typeof raw === 'object' && 'PhoneTaken' in raw
        ? raw.PhoneTaken
        : typeof raw === 'boolean'
          ? raw
          : raw && typeof raw === 'object' && 'available' in raw
            ? !raw.available
            : false
  return { data: { available: !taken } }
}
