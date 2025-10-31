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
