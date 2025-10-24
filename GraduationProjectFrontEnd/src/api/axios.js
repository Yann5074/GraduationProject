import axios from 'axios'
import { useAuthStore } from '../stores/auth'
import router from '@/router'

const apiBaseUrl = import.meta.env.VITE_API_URL || 'http://localhost:7131'

const api = axios.create({
  baseURL: 'https://localhost:7131', // 後端網址
  timeout: 10000, // 等待上限10秒
  withCredentials: true, // ⬅️ 關鍵！讓 Cookie / Session 一起送出
})

// 可選：401 時清掉前端登入狀態
import { useAuthStore } from '@/stores/auth'
api.interceptors.response.use(
  (res) => res,
  (err) => {
    if (err?.response?.status === 401) {
      try {
        useAuthStore().clear()
      } catch {}
    }
    return Promise.reject(err)
  },
)

export default api
