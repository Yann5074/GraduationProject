// src/stores/auth.js
import { defineStore } from 'pinia'
import { getMeAPI } from '@/api/Member'

const FALLBACK_AVATAR = '/asset/images/user.svg'
const LS_KEY = 'auth_user'

function buildImageUrl(fileName) {
  // 沒圖 -> 用預設頭像
  if (!fileName) return FALLBACK_AVATAR

  const v = String(fileName)

  // 已經是完整網址: https://...
  if (/^https?:\/\//i.test(v)) {
    return v
  }

  // 已經是 /MemberHeadImages/aaa.png 這種
  if (v.startsWith('/')) {
    return v
  }

  // 只是一個檔名 -> 幫它組完整網址
  const base = (
    import.meta.env.VITE_IMAGE_BASE ||
    import.meta.env.VITE_IMG_BASE ||
    import.meta.env.VITE_API_BASE ||
    'https://localhost:7131/MemberHeadImages/'
  ).replace(/\/+$/, '')

  return `${base}/${v.replace(/^\/+/, '')}`
}

export const useAuthStore = defineStore('auth', {
  state: () => ({
    // 直接存後端回來的使用者物件，不強制改欄位
    user: null,
  }),

  getters: {
    isLoggedIn: (s) => !!s.user,
    displayName: (s) => s.user?.displayName || s.user?.name || '',

    // ✅ 關鍵：頭像網址
    // 優先用 user.memberImage（後端欄位）
    // 如果未來你自己手動塞 user.imageUrl，也支援
    avatarUrl: (s) => {
      const raw =
        s.user?.imageUrl || // 如果你手動放過完整網址
        s.user?.memberImage || // 後端現在回的欄位（小寫）
        s.user?.MemberImage || // 另一種命名，保險
        s.user?.avatar || // 其他地方可能用 avatar
        ''

      return buildImageUrl(raw)
    },
  },

  actions: {
    // 登入 / 取得 me 後呼叫
    setUser(rawUser) {
      this.user = rawUser || null
      localStorage.setItem(LS_KEY, JSON.stringify(this.user))
    },

    async login({ user }) {
      // 1. 先把整個使用者物件存進 state / localStorage
      this.setUser(user)

      // 2. 幫頭貼也同步進來，確保 navbar 立刻有圖
      const avatarCandidate =
        user.imageUrl || user.memberImage || user.MemberImage || user.avatar || ''

      if (avatarCandidate) {
        this.setAvatar(avatarCandidate)
      }
    },

    // 上傳新頭貼成功後你可以呼叫這個
    // newFileName 可以是「10029_...png」或完整網址
    setAvatar(newFileNameOrUrl) {
      if (!this.user) this.user = {}
      // 我們同樣不去重構結構，直接更新 user.memberImage
      this.user.memberImage = newFileNameOrUrl
      this.user.imageUrl = newFileNameOrUrl // 也塞一份，方便 avatarUrl 取到
      localStorage.setItem(LS_KEY, JSON.stringify(this.user))
    },

    logout() {
      this.user = null
      localStorage.removeItem(LS_KEY)
    },

    // 拿 /api/Member/me 重新同步
    async refreshMe() {
      try {
        const res = await getMeAPI() // 這應該是 axios.get('/api/Member/me')
        const me = res.data || res
        this.setUser(me)
      } catch {
        this.logout()
      }
    },

    // ✅ 只更新會員部分欄位（例如暱稱、電話），不覆蓋整個 user
    updateProfileFields(partial) {
      if (!this.user) this.user = {}

      // 只改有傳入的欄位，其它像 memberImage、account 都保留
      this.user = {
        ...this.user,
        ...partial,
      }

      // 同步存回 localStorage，重新整理也保留
      localStorage.setItem(LS_KEY, JSON.stringify(this.user))
    },

    // 重整後從 localStorage 還原
    hydrate() {
      try {
        const raw = localStorage.getItem(LS_KEY)
        if (!raw) return
        this.user = JSON.parse(raw)
      } catch {
        this.user = null
        localStorage.removeItem(LS_KEY)
      }
    },
  },
})
