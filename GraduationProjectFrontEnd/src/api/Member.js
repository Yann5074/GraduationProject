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
// 一次檢查多欄位：POST /api/Member/check-unique
export async function checkDuplicateAPI(payload) {
  const res = await http.post('/Member/check-unique', payload)
  // 後端是 ResultDTO，真正的內容在 data.data
  const raw = res.data?.data || {} // ← 修正重點

  // 可能是 camelCase 或 PascalCase，兩種都接
  const accountTaken = raw.accountTaken ?? raw.AccountTaken
  const emailTaken = raw.emailTaken ?? raw.EmailTaken
  const phoneTaken = raw.phoneTaken ?? raw.PhoneTaken

  // 回傳「available = 可用」的統一格式
  return {
    data: {
      account: accountTaken === undefined ? undefined : !accountTaken,
      email: emailTaken === undefined ? undefined : !emailTaken,
      phone: phoneTaken === undefined ? undefined : !phoneTaken,
    },
  }
}

// GET /api/Member/check-account?account=xxx
export async function checkAccountAPI(account) {
  const res = await http.get('/Member/check-account', { params: { account } })
  const d = res.data?.data // ← 修正重點（ResultDTO.data）
  // 後端有可能給 { taken: bool } 或 { AccountTaken: bool }
  const taken =
    d && 'taken' in d
      ? d.taken
      : d && 'AccountTaken' in d
        ? d.AccountTaken
        : // 少數情況：直接回 boolean 或 { available: bool }
          typeof res.data === 'boolean'
          ? res.data
          : res.data && typeof res.data === 'object' && 'available' in res.data
            ? !res.data.available
            : false
  return { data: { available: !taken } }
}

// GET /api/Member/check-email?email=xxx
export async function checkEmailAPI(email) {
  const res = await http.get('/Member/check-email', { params: { email } })
  const d = res.data?.data
  const taken =
    d && 'taken' in d
      ? d.taken
      : d && 'EmailTaken' in d
        ? d.EmailTaken
        : typeof res.data === 'boolean'
          ? res.data
          : res.data && typeof res.data === 'object' && 'available' in res.data
            ? !res.data.available
            : false
  return { data: { available: !taken } }
}

// GET /api/Member/check-phone?phone=09xxxxxxxx
export async function checkPhoneAPI(phone) {
  const res = await http.get('/Member/check-phone', { params: { phone } })
  const d = res.data?.data
  const taken =
    d && 'taken' in d
      ? d.taken
      : d && 'PhoneTaken' in d
        ? d.PhoneTaken
        : typeof res.data === 'boolean'
          ? res.data
          : res.data && typeof res.data === 'object' && 'available' in res.data
            ? !res.data.available
            : false
  return { data: { available: !taken } }
}
