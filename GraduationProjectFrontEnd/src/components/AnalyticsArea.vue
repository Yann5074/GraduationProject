<script setup>
import { ref, onMounted } from 'vue'
import { getBestSellers } from '@/api/Analytics.js'
import ProductAPI from '@/api/Product.js'

// ====== state ======
const items = ref([])
const loading = ref(true)
const errorMsg = ref('')

// 圖片顯示用：只在預載成功後才寫入，避免閃爍
const displaySrcMap = ref({})
const failedSet = ref(new Set())

// ====== utils ======
/** 將相對路徑補成完整網址（優先用 VITE_BASE_URL） */
const resolveUrl = (u) => {
  if (!u) return null
  if (/^https?:\/\//i.test(u)) return u
  const base = import.meta.env.VITE_BASE_URL || ''
  const b = base.endsWith('/') ? base.slice(0, -1) : base
  const p = u.startsWith('/') ? u : `/${u}`
  return `${b}${p}`
}

const DEFAULT_IMG = resolveUrl('/ProductImages/default.png')

/** 預載圖片：成功後才把 src 設進 map（避免 reflow/閃爍） */
const preloadAndSet = (id, rawUrl) => {
  const finalUrl = resolveUrl(rawUrl) || DEFAULT_IMG
  if (displaySrcMap.value[id] === finalUrl) return // 同值不重設

  const img = new Image()
  img.decoding = 'async'
  img.fetchPriority = 'low'
  img.onload = () => {
    displaySrcMap.value[id] = finalUrl
    failedSet.value.delete(id)
  }
  img.onerror = () => {
    displaySrcMap.value[id] = DEFAULT_IMG
    failedSet.value.add(id)
  }
  img.src = finalUrl
}

/** <img> onerror：防止 error 迴圈 */
const onImgError = (id, e) => {
  const el = e?.target
  if (!el) return
  if (!el.src.endsWith('/ProductImages/default.png')) {
    el.src = DEFAULT_IMG
    failedSet.value.add(id)
  }
}

/** 批次為排行榜商品載入主圖 */
const fetchImagesFor = async (list) => {
  const ids = [...new Set(list.map(x => x.productId).filter(Boolean))]
  if (!ids.length) return

  await Promise.all(ids.map(async (id) => {
    if (displaySrcMap.value[id]) return // 已載入過就跳過
    const res = await ProductAPI.getProductById(id)
    const d = res?.data || null

    // 後端 DTO 經 System.Text.Json 會是 camelCase
    const raw =
      d?.mainImageUrl ||
      d?.images?.find(i => i?.fUrl)?.fUrl ||
      null

    preloadAndSet(id, raw)
  }))
}

/** 點卡片跳商品詳頁 */
const getDetailTo = (p) => ({
  name: 'ProductDetail',
  params: { id: p.productId }
})

// ====== lifecycle ======
onMounted(async () => {
  const from = new Date(Date.now() - 365 * 24 * 3600 * 1000).toISOString()//把365天換成毫秒
  const to = new Date().toISOString()

  try {
    const list = await getBestSellers({ from, to, top: 5, statuses: '2,3,4,5' })
    items.value = Array.isArray(list) ? list : []
    await fetchImagesFor(items.value)
  } catch (err) {
    console.error('載入熱銷排行失敗', err)
    errorMsg.value = '載入資料時發生錯誤，請稍後再試'
  } finally {
    loading.value = false
  }
})
</script>

<template>
  <section class="container my-5">

    <div v-if="loading" class="text-muted">載入中...</div>
    <div v-else-if="errorMsg" class="text-danger">{{ errorMsg }}</div>
    <div v-else-if="!items.length" class="text-muted">目前無資料</div>

    <div v-else class="row g-3">
      <div
        v-for="(p, i) in items"
        :key="p.productId"
        class="col-12 col-md-6 col-lg-4"
      >
        <div class="card h-100 shadow-sm border-0 hover-shadow position-relative">
          <!-- 圖片區：固定比例 + 預載完成後淡入 -->
          <div class="ratio-box">
            <img
              :src="displaySrcMap[p.productId] || DEFAULT_IMG"
              class="w-100 h-100 object-fit-cover fade-img"
              :class="{ 'is-loaded': !!displaySrcMap[p.productId] && !failedSet.has(p.productId) }"
              alt="產品主圖"
              loading="lazy"
              decoding="async"
              fetchpriority="low"
              @error="(e) => onImgError(p.productId, e)"
            />
          </div>

          <div class="card-body">
            <div class="d-flex justify-content-between align-items-center mb-2">
              <h5 class="card-title mb-0">
                #{{ i + 1 }} {{ p.productName }}
              </h5>
              <span class="badge bg-dark">銷量 {{ p.totalQuantity }}</span>
            </div>

            <!-- <p class="small text-muted mb-0">
              總金額：
              <span class="text-success fw-bold">
                NT$ {{ p.totalSalesAmount.toLocaleString('zh-TW') }}
              </span>
            </p> -->

            <p class="small text-muted mb-0">
              {{ p.variantCount }} 種規格
              <span v-if="p.topVariantSKU">｜熱銷款 SKU：{{ p.topVariantSKU }}</span>
            </p>

            <!-- 整張卡片可點，連到詳頁 -->
            <RouterLink
              class="stretched-link"
              :to="getDetailTo(p)"
              :aria-label="`前往 ${p.productName} 詳細頁`"
            />
          </div>
        </div>
      </div>
    </div>
  </section>
</template>

<style scoped>
.hover-shadow:hover {
  box-shadow: 0 4px 12px rgba(0,0,0,.15);
  transform: translateY(-2px);
  transition: all .2s ease;
}
.object-fit-cover { object-fit: cover; }

/* 固定 4:3 比例，避免版面跳動 */
.ratio-box {
  position: relative;
  width: 100%;
  padding-top: 75%;
  background: #f6f6f6;
  overflow: hidden;
  border-top-left-radius: .5rem;
  border-top-right-radius: .5rem;
}
.ratio-box > img { position: absolute; inset: 0; }

/* 預載完成後淡入，避免閃爍 */
.fade-img { opacity: 0; transition: opacity .25s ease; }
.fade-img.is-loaded { opacity: 1; }
</style>
