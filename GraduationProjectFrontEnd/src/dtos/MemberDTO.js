// src/dtos/MemberDTO.js
// src/dtos/MemberDTO.js
// 前端 <-> 後端 會員資料 欄位轉換 & 正規化工具

// ✅ 改良版：自動偵測並組出正確的圖片基底路徑
function getImageBase() {
  // 1️⃣ 優先使用 .env 檔中明確指定的變數
  if (import.meta.env?.VITE_IMAGE_BASE) {
    return import.meta.env.VITE_IMAGE_BASE.replace(/\/+$/, '')
  }

  // 2️⃣ 次要備援名稱
  if (import.meta.env?.VITE_IMG_BASE) {
    return import.meta.env.VITE_IMG_BASE.replace(/\/+$/, '')
  }

  // 3️⃣ 若有設定 API base，自動補上 /MemberHeadImages
  if (import.meta.env?.VITE_API_BASE) {
    return `${import.meta.env.VITE_API_BASE.replace(/\/+$/, '')}/MemberHeadImages`
  }

  // 4️⃣ 最後保底：開發環境預設本機後端位置
  return 'https://localhost:7131/MemberHeadImages'
}

/** 將後端回傳的圖片欄位組成可用的 <img :src> */
function toImageUrl(v) {
  if (!v) return ''

  // 已是完整網址 (http / https 開頭)
  if (/^https?:\/\//i.test(v)) return v

  // 已是絕對路徑（例如 /MemberHeadImages/xxx.png）
  if (v.startsWith('/')) return v

  // 否則視為檔名 → 補成完整路徑
  return `${getImageBase()}/${v}`.replace(/([^:]\/)\/+/g, '$1')
}

/** 性別值正規化：轉數字 (1~4) 或 null */
function normalizeGender(g) {
  if (g === undefined || g === null || g === '') return null
  const n = Number(g)
  return Number.isFinite(n) ? n : null
}

// 1) 前端表單模型（初始值）
export function createMemberModel() {
  return {
    memberId: null,
    account: '',
    displayName: '',
    name: '',
    email: '',
    phone: '',
    address: '',
    gender: null, // 1:男 2:女 3:中性 4:不透露
    imageUrl: '', // <img :src="imageUrl">
  }
}

// 2) 後端回傳 → 前端模型
export function createMemberDTO(raw) {
  const src = raw?.data ?? raw ?? {}

  const memberId = src.MemberId ?? src.memberId ?? null
  const account = src.Account ?? src.account ?? ''
  const displayName = src.DisplayName ?? src.displayName ?? ''
  const name = src.Name ?? src.name ?? ''
  const email = src.Email ?? src.email ?? ''
  const phone = src.Phone ?? src.phone ?? ''
  const address = src.Address ?? src.address ?? ''
  const gender = src.Gender ?? src.gender ?? null

  // 依序撈常見的影像欄位（同時支援大小寫 / 不同命名）
  const rawImg =
    src.memberImage ??
    src.MemberImage ??
    src.imageUrl ??
    src.ImageUrl ??
    src.photoUrl ??
    src.PhotoUrl ??
    src.avatarUrl ??
    src.AvatarUrl ??
    src.fileName ??
    src.FileName ??
    ''

  return {
    memberId,
    account,
    displayName,
    name,
    email,
    phone,
    address,
    gender: normalizeGender(gender),
    imageUrl: toImageUrl(rawImg), // 👈 這裡會組出完整可用網址
  }
}

// 3) 前端模型 → 後端更新 payload
export function createUpdateMeDTO(me) {
  return {
    displayName: me.displayName?.trim() || '',
    name: me.name?.trim() || '',
    gender: normalizeGender(me.gender),
    phone: me.phone?.trim() || '',
    email: me.email?.trim() || '',
    address: me.address?.trim() || '',
  }
}

// 4) 上傳頭像回傳 → 立即更新前端模型的 imageUrl
export function applyUploadPhotoDTO(model, raw) {
  const d = raw?.data ?? raw ?? {}
  const v = d.imageUrl ?? d.url ?? d.relative ?? d.relativePath ?? d.fileName ?? d.path ?? ''
  model.imageUrl = toImageUrl(v)
}

// 5) 把 API 回來的資料覆蓋到現有模型（保留 reactivity）
export function mergeMemberInto(model, raw) {
  Object.assign(model, createMemberDTO(raw))
}

// 變更密碼 DTO
export const createUpdatePasswordDTO = (form) => ({
  oldPassword: form.oldPassword,
  newPassword: form.newPassword,
  confirmNewPassword: form.confirmNewPassword,
})
export const createUpdatePwdDTO = createUpdatePasswordDTO
