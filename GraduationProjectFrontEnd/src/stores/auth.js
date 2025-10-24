// src/stores/auth.js
import { defineStore } from 'pinia'
import { computed, ref } from 'vue'

export const useAuthStore = defineStore('auth', () => {
  const user = ref(null) // 後端可能給 { id, name, avatar } 或 avatarUrl / photo / headImage
  const token = ref(null)

  // 1) 讀取本地登入資訊
  function load() {
    const raw = localStorage.getItem('auth-state')
    if (!raw) return
    try {
      const parsed = JSON.parse(raw)
      user.value = parsed.user ?? null
      token.value = parsed.token ?? null
    } catch { }
  }
  load()

  // 2) 儲存
  function persist() {
    localStorage.setItem('auth-state', JSON.stringify({ user: user.value, token: token.value }))
  }

  // 3) 是否登入
  const isLoggedIn = computed(() => !!user.value)

  // ====== 4) 健壯化的頭像網址 ======
  // API base：可用 .env 設定，沒有就用本機預設
  const IMAGE_BASE = import.meta.env.VITE_BASE_URL || 'https://localhost:7131'
  const AVATAR_PREFIX = '/MemberHeadImages/'

  // 從 user 取出「可能的」頭像欄位（檔名或完整網址）
  const avatarFile = computed(() => {
    const u = user.value || {}
    return (u.avatar ?? u.memberImage ?? u.avatarUrl ?? u.photo ?? u.headImage ?? '')
      .toString()
      .trim()
  })

  const avatarUrl = computed(() => {
    const f = avatarFile.value
    if (!f) return '/asset/images/default-avatar.png' // 沒給 → 前端預設圖
    if (/^https?:\/\//i.test(f)) return f // 已是完整 URL → 直接用
    if (f.startsWith('/')) return `${IMAGE_BASE}${f}` // 伺服器絕對路徑 → 接上 API_BASE
    return `${IMAGE_BASE}${AVATAR_PREFIX}${f}` // 純檔名 → /MemberHeadImages/檔名
  })
  // ====== end 頭像網址 ======

  // 5) 登入、登出
  async function login({ token: tk, user: u }) {
    token.value = tk
    user.value = u
    persist()
  }

  function logout() {
    token.value = null
    user.value = null
    localStorage.removeItem('auth-state')
  }

  // 6) 匯出
  return { user, token, isLoggedIn, avatarUrl, login, logout }
})
