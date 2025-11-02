
<template>
  <div class="product-detail-page">
    <!-- 麵包屑導航 -->
    <div class="container py-3">
      <nav aria-label="breadcrumb">
        <ol class="breadcrumb mb-0">
          <li class="breadcrumb-item"><router-link to="/">首頁</router-link></li>
          <li class="breadcrumb-item"><router-link to="/products">產品列表</router-link></li>
          <li v-if="product" class="breadcrumb-item active">{{ product.fName }}</li>
        </ol>
      </nav>
    </div>

    <!-- 載入中 -->
    <div v-if="loading" class="container py-5 text-center">
      <div class="spinner-border text-primary"></div>
      <p class="mt-3 text-muted">載入產品資訊...</p>
    </div>

    <!-- 錯誤訊息 -->
    <div v-else-if="error" class="container py-5">
      <div class="alert alert-danger">{{ error }}</div>
      <button class="btn btn-primary" @click="loadProductDetail">重新載入</button>
    </div>

    

    <!-- 產品詳情 -->
    <div v-else-if="product" class="container py-4">
      <div class="row g-4">



        <!-- 左側：圖片區 -->
  <!-- 左側：照片 / 3D 切換（請把舊的影像區塊刪掉，改用這一組） -->
  <div class="col-lg-6">
    <!-- 切換按鈕 -->
    <div class="d-flex gap-2 mb-3">
      <button
        class="btn"
        :class="!show3D ? 'btn-primary' : 'btn-outline-primary'"
        @click="show3D = false"
      >
        照片
      </button>
      <button
        class="btn"
        :class="show3D ? 'btn-primary' : 'btn-outline-primary'"
        @click="toggle3D()"
      >
        3D 檢視
      </button>
    </div>

    <!-- 照片區（務必讓 v-else 緊貼 v-if，兩者間不要有註解/空白節點） -->
    <div v-if="!show3D" class="media-box">
      <div class="media-main">
        <img
          v-if="selectedImage"
          :src="selectedImage"
          :alt="product?.fName"
          class="img-fluid rounded shadow-sm w-100 h-100 object-fit-cover"
          @error="handleImageError"
        />
        <div v-else class="placeholder">無主圖</div>
      </div>
      <!-- 縮圖（只有 2 張以上才顯示） -->
<div v-if="thumbs.length > 1" class="media-thumbs">
  <button
    v-for="t in thumbs"
    :key="t.key"
    class="thumb-btn"
    :class="{ active: t.url === selectedImage }"
    type="button"
    @click="onThumbClick(t)"
    @keyup.enter.space="onThumbClick(t)"
    :aria-label="t.alt"
  >
    <img
      :src="t.url"
      :alt="t.alt"
      class="thumb-img"
      loading="lazy"
      decoding="async"
      referrerpolicy="no-referrer"
      @error="onThumbError(t)"
    />
  </button>
</div>
    </div>
    <div v-else class="media-box">
      <Suspense>
        <template #default>
          <Furniture3DViewer
            :key="productId"  
            :product-id="productId"
            :variant-id="currentVariantId"
            :auto-rotate="true"
            :model-scale="1.0"
          />
        </template>
        <template #fallback>
          <div class="placeholder">3D 載入中…</div>
        </template>
      </Suspense>
    </div>
  </div>

        <!-- 右側：產品資訊 -->
        <div class="col-lg-6">
          <h1 class="product-title mb-3">{{ product.fName }}</h1>
          
          <!-- 徽章 -->
          <div class="mb-3">
            <span class="badge bg-success">{{ product.categoryName }}</span>
            <span v-if="isAvailable" class="badge bg-success ms-2">有貨</span>
            <span v-else class="badge bg-secondary ms-2">缺貨</span>
          </div>

          <!-- 價格 -->
          <div class="price-section mb-4 p-3 bg-light rounded">
            <div v-if="selectedVariant">
              <div v-if="product.fDiscount > 0" class="mb-2">
                <span class="text-decoration-line-through text-muted">
                  NT$ {{ formatPrice(selectedVariant.fPrice * (1 + product.fDiscount / 100)) }}
                </span>
              </div>
              <div class="text-primary fw-bold fs-2">NT$ {{ formatPrice(selectedVariant.fPrice) }}</div>
            </div>
            <div v-else class="text-primary fw-bold fs-2">
              NT$ {{ formatPrice(product.minPrice) }}
              <span v-if="product.minPrice !== product.maxPrice"> - {{ formatPrice(product.maxPrice) }}</span>
            </div>
          </div>

          <!-- 產品描述 -->
          <p v-if="product.fDescription" class="text-muted mb-4">{{ product.fDescription }}</p>
          
          <hr />

          <!-- 顏色選擇（視覺化圓圈） -->
          <div v-if="colorVariants.length > 0" class="mb-4">
            <h5 class="mb-3">
              <i class="bi bi-palette me-2"></i>選擇顏色
            </h5>
            <div class="d-flex flex-wrap gap-3">
              <div 
                v-for="variant in colorVariants" 
                :key="variant.fProductVariantId || variant.FProductVariantId"
                class="color-option-wrapper"
              >
                <button
                  class="color-option-btn"
                  :class="{ 
                    active: isVariantSelected(variant),
                    'out-of-stock': getVariantStock(variant) <= 0
                  }"
                  :disabled="getVariantStock(variant) <= 0"
                  :title="getVariantTitle(variant)"
                  @click="selectVariant(variant)"
                >
                  <!-- 顏色圓圈 -->
                  <div 
                    class="color-circle"
                    :style="{ 
                      backgroundColor: variant.colorHex || variant.ColorHex || '#ccc',
                      borderColor: isVariantSelected(variant) ? '#0d6efd' : '#dee2e6'
                    }"
                  >
                    <!-- 缺貨標記 -->
                    <div v-if="getVariantStock(variant) <= 0" class="out-of-stock-overlay">
                      <i class="bi bi-x-lg"></i>
                    </div>
                  </div>
                </button>
                <!-- 顏色名稱 -->
                <small class="color-name d-block text-center mt-2">
                  {{ variant.colorName || variant.ColorName || variant.fSku }}
                </small>
                <!-- 庫存狀態 -->
                <small class="stock-status d-block text-center">
                  <span v-if="getVariantStock(variant) > 0" class="text-success">
                    庫存 {{ getVariantStock(variant) }}
                  </span>
                  <span v-else class="text-danger">缺貨</span>
                </small>
              </div>
            </div>
          </div>

          <!-- 其他變體選擇（如尺寸等，非顏色） -->
          <div v-else-if="variants.length > 1" class="mb-4">
            <h5 class="mb-3">選擇規格</h5>
            <div class="row g-2">
              <div 
                v-for="variant in variants" 
                :key="variant.fProductVariantId || variant.FProductVariantId" 
                class="col-6 col-md-4"
              >
                <button 
                  class="variant-button w-100" 
                  :class="{ 
                    active: isVariantSelected(variant),
                    disabled: getVariantStock(variant) <= 0 
                  }" 
                  :disabled="getVariantStock(variant) <= 0" 
                  @click="selectVariant(variant)"
                >
                  <div class="fw-bold mb-2">{{ variant.fSku || variant.FSku }}</div>
                  <div class="text-primary fw-bold">NT$ {{ formatPrice(variant.fPrice || variant.FPrice) }}</div>
                  <small v-if="getVariantStock(variant) <= 0" class="text-danger">缺貨</small>
                  <small v-else class="text-muted">庫存: {{ getVariantStock(variant) }} 件</small>
                </button>
              </div>
            </div>
          </div>

          <!-- 數量選擇 -->
          <div class="mb-4">
            <h5 class="mb-3">數量</h5>
            <div class="input-group" style="max-width: 200px">
              <button class="btn btn-outline-secondary" :disabled="quantity <= 1" @click="decreaseQuantity">
                <i class="bi bi-dash"></i>
              </button>
              <input 
                v-model.number="quantity" 
                type="number" 
                class="form-control text-center" 
                min="1" 
                :max="maxQuantity" 
                @input="validateQuantity" 
              />
              <button class="btn btn-outline-secondary" :disabled="quantity >= maxQuantity" @click="increaseQuantity">
                <i class="bi bi-plus"></i>
              </button>
            </div>
            <small v-if="maxQuantity > 0" class="text-muted">庫存: {{ maxQuantity }} 件</small>
          </div>

          <hr />

          <!-- 操作按鈕 -->
          <div class="d-grid gap-2">
            <button class="btn btn-primary btn-lg" :disabled="!canAddToCart" @click="addToCart">
              加入購物車
            </button>
            <button class="btn btn-outline-primary btn-lg" :disabled="!canAddToCart" @click="buyNow">
              立即購買
            </button>
          </div>

          <div v-if="!canAddToCart" class="alert alert-warning mt-3">
            {{ getDisabledReason() }}
          </div>
        </div>
      </div>

      <!-- 產品詳細資訊標籤 -->
      <div class="row mt-5">
        <div class="col-12">
          <ul class="nav nav-tabs">
            <li class="nav-item">
              <button class="nav-link active" data-bs-toggle="tab" data-bs-target="#details">產品規格</button>
            </li>
            <li class="nav-item">
              <button class="nav-link" data-bs-toggle="tab" data-bs-target="#description">詳細說明</button>
            </li>
          </ul>
          <div class="tab-content border border-top-0 rounded-bottom p-4">
            <div id="details" class="tab-pane fade show active">
              <div class="row g-3">
                <div class="col-md-6"><strong>類別：</strong> {{ product.categoryName }}</div>
                <div v-if="product.fWarrantyMonth" class="col-md-6">
                  <strong>保固：</strong> {{ product.fWarrantyMonth }} 個月
                </div>
                <div class="col-md-6">
                  <strong>組裝：</strong> {{ product.fAssemblyRequired ? '需要組裝' : '無需組裝' }}
                </div>
                <div v-if="colorVariants.length > 0" class="col-md-6">
                  <strong>顏色選項：</strong> {{ colorVariants.length }} 種
                </div>
                <div v-else-if="variants.length > 0" class="col-md-6">
                  <strong>規格選項：</strong> {{ variants.length }} 種
                </div>
              </div>
            </div>
            <div id="description" class="tab-pane fade">
              <p v-if="product.fDescription">{{ product.fDescription }}</p>
              <p v-else class="text-muted">暫無詳細說明</p>
            </div>
          </div>
        </div>
      </div>

      <!-- 相關產品 -->
      <div v-if="relatedProducts.length > 0" class="row mt-5">
        <div class="col-12">
          <h3 class="mb-4">相關產品</h3>
          <div class="row g-3">
            <div v-for="p in relatedProducts" :key="p.fProductId" class="col-6 col-md-3">
              <div class="card h-100 related-card" @click="goToProduct(p.fProductId)">
                <img :src="p.mainImageUrl" class="card-img-top" :alt="p.fName" @error="handleImageError" />
                <div class="card-body">
                  <h6 class="card-title text-truncate">{{ p.fName }}</h6>
                  <p class="text-primary fw-bold mb-0">NT$ {{ formatPrice(p.minPrice) }}</p>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, watch,defineAsyncComponent } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { ProductAPI } from '@/api/Product'

import { memberAddToCart } from '@/api/Cart'
import { useCartStore } from '@/stores/cartStore'
import { useAuthStore } from '@/stores/auth'


const Furniture3DViewer = defineAsyncComponent(() => import('@/components/Furniture3DViewer.vue'))

const route = useRoute()
const router = useRouter()

const auth = useAuthStore()
const cart = useCartStore()
const loading = ref(false)
const error = ref(null)
const product = ref(null)
const variants = ref([])
const currentVariantId = computed(() =>
  selectedVariant.value?.fProductVariantId ?? selectedVariant.value?.FProductVariantId ?? null
)
const selectedVariant = ref(null)
const quantity = ref(1)
const selectedImage = ref('')
const productImages = ref([])
const relatedProducts = ref([])


const viewerKey = computed(() => `${productId.value}-${currentVariantId.value ?? 'none'}`)
// 3D 切換
const show3D = ref(false)


// 3D 元件要用到的 productId（你在模板裡有 :product-id）
const productId = computed(() => Number(route.params.id))

//  圖片錯誤追蹤（防止閃爍）
const imageErrors = ref(new Set())

// 計算顏色變體（有 colorName 或 colorHex 的變體）
const colorVariants = computed(() => {
  return variants.value.filter(v => 
    (v.colorName || v.ColorName) || (v.colorHex || v.ColorHex)
  )
})

// 計算屬性
const maxQuantity = computed(() => {
  if (selectedVariant.value) {
    return selectedVariant.value.fStock || selectedVariant.value.FStock || 0
  }
  return product.value?.totalStock || 0
})

const isAvailable = computed(() => {
  if (selectedVariant.value) {
    return (selectedVariant.value.fStock || selectedVariant.value.FStock) > 0
  }
  return product.value?.isAvailable || false
})

const canAddToCart = computed(() => {
  if (!isAvailable.value) return false
  if (variants.value.length > 1 && !selectedVariant.value) return false
  return quantity.value >= 1 && quantity.value <= maxQuantity.value
})


// 將 productImages 轉成縮圖用資料結構（附帶 key / alt）
const thumbs = computed(() => {
  const name = product.value?.fName || '產品圖片'
  return (productImages.value || []).map((url, i) => ({
    key: `thumb-${i}-${simpleHash(url)}`,
    url,
    alt: `${name} - 圖片 ${i + 1}`,
  }))
})

function onThumbClick(t) {
  if (!t?.url) return
  selectedImage.value = t.url
}

// 單張縮圖 404 時處理：
function onThumbError(t) {
  if (!t?.url) return
  const old = t.url
  const fallback = '/ProductImages/default.png'
  imageErrors.value.add(old)
  t.url = fallback
  if (selectedImage.value === old) {
    selectedImage.value = fallback
  }
}

 
// 超輕量雜湊（避免 v-for key 因相同 URL 重複）
function simpleHash(str) {
  let h = 0
  for (let i = 0; i < str.length; i++) {
    h = (h << 5) - h + str.charCodeAt(i)
    h |= 0
  }
  return Math.abs(h)
}

// 載入產品詳情
async function loadProductDetail() {
  const pid  = route.params.id
  if (!pid ) {
    error.value = '產品 ID 不正確'
    return
  }

  loading.value = true
  error.value = null

  console.log(' 載入產品詳情:', pid )

  try {
    const response = await ProductAPI.getProductById(pid )
    console.log('API 回應:', response)
    
    if (response.success && response.data) {
      const rawData = response.data
      
      // 處理圖片
      const mainImg = rawData.mainImageUrl || rawData.MainImageUrl
      const assetsList = rawData.assets || rawData.Assets
      
      if ((!mainImg || mainImg === 'null') && assetsList && assetsList.length > 0) {
        const primaryAsset = assetsList.find(a => a.fIsPrimary || a.FIsPrimary)
        rawData.mainImageUrl = primaryAsset 
          ? (primaryAsset.fUrl || primaryAsset.FUrl) 
          : (assetsList[0].fUrl || assetsList[0].FUrl)
      } else if (mainImg) {
        rawData.mainImageUrl = mainImg
      }
      
      rawData.mainImageUrl = rawData.mainImageUrl || '/ProductImages/default.png'
      
      if (!rawData.assets && assetsList) {
        rawData.assets = assetsList
      }
      
      // 設定預設值
      rawData.fPstatus = rawData.fPstatus ?? 1
      rawData.statusName = rawData.statusName || '上架中'
      rawData.fWarrantyMonth = rawData.fWarrantyMonth ?? 0
      rawData.fAssemblyRequired = rawData.fAssemblyRequired ?? false
      
      product.value = rawData
      console.log('✅ 產品資料:', product.value)
      
      setupProductImages()
      await loadVariants(pid )
      await loadRelatedProducts(product.value.fCategoryId)
    } else {
      console.error(' 載入失敗:', response)
      error.value = response.message || '載入產品失敗'
    }
  } catch (err) {
    console.error(' 發生錯誤:', err)
    error.value = err.message || '載入產品時發生錯誤'
  } finally {
    loading.value = false
  }
}

// 只允許圖片副檔名，並排除 3D 資源資料夾
function isImageUrl(url) {
  if (!url) return false
  // 排除 3D 模型/貼圖所在路徑
  if (/\/3D\//i.test(url)) return false
  // 只收常見圖片副檔名
  return /\.(png|jpe?g|webp|gif|bmp|svg)$/i.test(url)
}

// 正：設定產品圖片（避免重複、排除 3D）
function setupProductImages() {
  console.log(' 設定產品圖片')

  const imageSet = new Set()
  const newImages = []

  // 輔助：加入圖片（防重複、只收圖片）
  const addImage = (url) => {
    if (!url || url === 'null' || url === '') return
    if (!isImageUrl(url)) return
    const stableUrl = getStableImageUrl(url)
    if (!stableUrl || imageSet.has(stableUrl)) return
    imageSet.add(stableUrl)
    newImages.push(stableUrl)
    console.log('   加入圖片:', stableUrl)
  }

  // 1) 主圖優先
  const main = (product.value && (product.value.mainImageUrl || product.value.MainImageUrl)) || null
  addImage(main)

  // 2) Assets 只加圖片（自動排除 /3D/ 與非圖片副檔名）
  const assetsList = (product.value && (product.value.assets || product.value.Assets)) || []
  for (let i = 0; i < assetsList.length; i++) {
    const a = assetsList[i]
    const url = a.fUrl || a.FUrl || a.fAssetUrl || a.FAssetUrl
    addImage(url)
  }

  // 3) 變體縮圖（顏色縮圖等）
  const vs = (product.value && product.value.variants) || []
  for (let i = 0; i < vs.length; i++) {
    addImage(vs[i].colorThumbnail || vs[i].ColorThumbnail)
  }

  // 4) 若仍沒有任何可用圖片，採用預設圖
  if (newImages.length === 0) {
    // 請確認專案內真的存在這張圖
    addImage('/ProductImages/default.png')
    if (newImages.length === 0) {
      addImage('/images/default-product.jpg')
    }
  }

  // 5) 主圖置頂（若已加入且被其它圖片擠到後面）
  if (main) {
    const mainUrl = getStableImageUrl(main)
    const idx = newImages.findIndex(function (x) { return x === mainUrl })
    if (idx > 0) {
      // 把主圖移到陣列最前
      newImages.unshift(newImages.splice(idx, 1)[0])
    }
  }

  productImages.value = newImages
  selectedImage.value = newImages[0]
  console.log(' 圖片設定完成，共 ' + newImages.length + ' 張（無 3D/貼圖，且無重複）')
}

async function toggle3D() {
  if (!show3D.value) {
    try {
      const res = await ProductAPI.getPBR(productId.value)
      
      console.log(' PBR API 回應:', res)
      
      if (!res || !res.success) {
        alert('無法載入 3D 資料')
        return
      }
      
      if (!res.data || !Array.isArray(res.data) || res.data.length === 0) {
        alert('此商品尚未配置 3D 模型，請稍後再試')
        return
      }
      
      const firstItem = res.data[0]
      if (!firstItem.modelUrl) {
        alert('此商品尚未配置 3D 模型檔案')
        return
      }
      
      console.log(' 3D 模型 URL:', firstItem.modelUrl)
      
    } catch (e) {
      console.error(' 查詢 PBR 失敗', e)
      alert('無法載入 3D 模型：' + (e.message || '未知錯誤'))
      return
    }
  }
  
  show3D.value = !show3D.value
}


// 載入變體
async function loadVariants(productId) {
  if (product.value.variants && Array.isArray(product.value.variants)) {
    console.log('使用產品資料中的變體:', product.value.variants.length, '個')
    
    variants.value = product.value.variants
    
    // 自動選擇第一個有庫存的變體
    const available = variants.value.find(v => (v.fStock || v.FStock) > 0)
    if (available) {
      selectVariant(available)
      console.log(' 已選擇變體:', available.fSku || available.FSku)
    } else if (variants.value.length === 1) {
      // 如果只有一個變體，即使缺貨也選擇它
      selectVariant(variants.value[0])
    }
  }
}

// 載入相關產品
async function loadRelatedProducts(categoryId) {
  if (!categoryId) return
  try {
    const response = await ProductAPI.getProducts({ 
      categoryId, 
      pageNumber: 1, 
      pageSize: 8 
    })
    
    if (response.ok && response.data.data) {
      relatedProducts.value = response.data.data
        .filter(p => p.fProductId !== product.value.fProductId)
        .slice(0, 4)
        .map(p => {
          let imageUrl = p.mainImageUrl || p.MainImageUrl
          if (!imageUrl && p.assets && p.assets.length > 0) {
            const primaryAsset = p.assets.find(a => a.fIsPrimary) || p.assets[0]
            imageUrl = primaryAsset.fUrl || primaryAsset.FUrl
          }
          return { ...p, mainImageUrl: getStableImageUrl(imageUrl) }
        })
    }
  } catch (err) {
    console.error(' 載入相關產品失敗:', err)
  }
}

// 選擇變體
function selectVariant(variant) {
  selectedVariant.value = variant
  quantity.value = 1
  
  // 如果變體有專屬圖片，切換到該圖片
  const variantImage = variant.colorThumbnail || variant.ColorThumbnail
  if (variantImage) {
    const imageUrl = getStableImageUrl(variantImage)
    if (productImages.value.includes(imageUrl)) {
      selectedImage.value = imageUrl
    }
  }
}

// 檢查是否選中
function isVariantSelected(variant) {
  const variantId = variant.fProductVariantId || variant.FProductVariantId
  const selectedId = selectedVariant.value?.fProductVariantId || selectedVariant.value?.FProductVariantId
  return variantId === selectedId
}

// 取得變體庫存
function getVariantStock(variant) {
  return variant.fStock || variant.FStock || 0
}

// 取得變體提示文字
function getVariantTitle(variant) {
  const name = variant.colorName || variant.ColorName || variant.fSku
  const stock = getVariantStock(variant)
  const price = formatPrice(variant.fPrice || variant.FPrice)
  
  if (stock <= 0) {
    return `${name} - 缺貨`
  }
  return `${name} - NT$ ${price} (庫存 ${stock} 件)`
}

// 數量控制
function increaseQuantity() {
  if (quantity.value < maxQuantity.value) quantity.value++
}

function decreaseQuantity() {
  if (quantity.value > 1) quantity.value--
}

function validateQuantity() {
  if (quantity.value < 1) quantity.value = 1
  else if (quantity.value > maxQuantity.value) quantity.value = maxQuantity.value
}

//加入購物車方法
async function addToCart() {
  if (!canAddToCart.value) return
    const productVariantId = selectedVariant.value?.fProductVariantId || selectedVariant.value?.FProductVariantId
    const qty = quantity.value

    if(!productVariantId){
      alert('請先選擇商品規格')
    }

    const atc = {
      productVariantId,
      qty
    }
    try{
      if (auth.isLoggedIn){
        const result = await memberAddToCart(atc)
        if (result.ok){
          alert(result.message)
      }
      }else{
        const productVariantId = selectedVariant.value?.fProductVariantId || selectedVariant.value?.FProductVariantId
        const productName = product.value?.fName
        const imageUrl = selectedImage.value || product.value?.ImageUrl
        const unitPrice = selectedVariant.value?.fPrice || product.value?.minPrice
        cart.addItem(productVariantId, qty, productName, imageUrl, unitPrice)
        // console.log('訪客購物車', cart.items)
        alert('已加入訪客購物車')
      }
  }catch(err){
    console.error('錯誤', err)
    alert('加入購物車時發生問題，請重新確認')
  }
}

async function buyNow() {
  if (!canAddToCart.value) return
    const productVariantId = selectedVariant.value?.fProductVariantId || selectedVariant.value?.FProductVariantId
    const qty = quantity.value

    if(!productVariantId){
      alert('請先選擇商品規格')
    }

    const atc = {
      productVariantId,
      qty
    }
    try{
      if (auth.isLoggedIn){
        await memberAddToCart(atc)
        router.push('/cart')
      }else{
        const productName = product.value?.fName
        const imageUrl = selectedImage.value || product.value?.ImageUrl
        const unitPrice = selectedVariant.value?.fPrice || product.value?.minPrice
        cart.addItem(productVariantId, qty, productName, imageUrl, unitPrice)
        alert('尚未登入，商品以加入購物車')
        router.push('/cart')
      }
  }catch(err){
    console.error('立即購買發生錯誤', err)
  }
}

// 前往產品
function goToProduct(productId) {
  router.push(`/products/${productId}`)
}

// 取得禁用原因
function getDisabledReason() {
  if (!isAvailable.value) return '產品暫時缺貨'
  if (variants.value.length > 1 && !selectedVariant.value) return '請選擇規格'
  if (quantity.value < 1 || quantity.value > maxQuantity.value) return '數量不正確'
  return ''
}

// 格式化價格
function formatPrice(price) {
  return price ? Math.round(price).toLocaleString() : '0'
}

// 穩定的圖片 URL 函數（防止閃爍）
function getStableImageUrl(url, fallback = '/ProductImages/default.png') {
  if (imageErrors.value.has(url)) {
    return fallback
  }
  
  if (!url || url === 'null' || url === '') {
    return fallback
  }
  
  if (url.startsWith('http://') || url.startsWith('https://')) {
    return url
  }
  
  if (url.startsWith('/')) {
    const API_BASE = import.meta.env.VITE_API_BASE_URL || 'https://localhost:7131'
    return `${API_BASE}${url}`
  }
  
  return url
}

// 穩定的錯誤處理（防止重複執行）
function handleImageError(event) {
  const failedUrl = event.target.src
  
  if (!imageErrors.value.has(failedUrl)) {
    imageErrors.value.add(failedUrl)
    console.warn('圖片載入失敗:', failedUrl)
    event.target.src = '/ProductImages/default.png'
  }
}

// 進入頁面時直接跳轉到頂部
onMounted(() => {
  loadProductDetail()
  // 從列表進入詳情頁時，直接跳轉到頂部（無動畫）
  window.scrollTo(0, 0)
  console.log('📜 進入產品頁，直接跳轉到頂部')
})

// 切換產品時直接跳轉到頂部
watch(() => route.params.id, (newId, oldId) => {
  if (newId && route.name === 'ProductDetail') {
    imageErrors.value.clear()
    loadProductDetail()
    // 在詳情頁內切換產品時，直接跳轉到頂部（無動畫）
    window.scrollTo(0, 0)
    console.log('📜 切換產品，直接跳轉到頂部')
  }
})

onMounted(async () => {
  // 拉你的 variants 列表（假設已有 API）
  try {
    const v = await ProductAPI.getProductVariants(productId.value)
    // 兼容：若回傳包裝層不同可以視情況調整
    variants.value = Array.isArray(v?.data) ? v.data : (Array.isArray(v) ? v : [])
    // 預設選第一個
    const firstAvailable = variants.value.find(x => (x.fStock ?? x.FStock ?? 0) > 0) ?? variants.value[0]
    if (firstAvailable) selectedVariant.value = firstAvailable 
  } catch {
    variants.value = []
   
  }
})



</script>

<style scoped>
.product-detail-page {
  min-height: 100vh;
  background-color: #f8f9fa;
}

.main-image-container {
  position: relative;
  background-color: #f8f9fa;
  border-radius: 8px;
  min-height: 400px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.main-image {
  width: 100%;
  height: auto;
  object-fit: contain;
  max-height: 600px;
  backface-visibility: hidden;
  transform: translateZ(0);
  background-color: #f8f9fa;
}

.thumbnail {
  width: 80px;
  height: 80px;
  object-fit: cover;
  border-radius: 4px;
  cursor: pointer;
  border: 2px solid #dee2e6;
  transition: all 0.2s ease;
  flex-shrink: 0;
  backface-visibility: hidden;
  transform: translateZ(0);
  background-color: #f8f9fa;
}

.thumbnail:hover {
  border-color: #0d6efd;
  opacity: 0.9;
}

.thumbnail.active {
  border-color: #0d6efd;
  box-shadow: 0 0 0 2px rgba(13, 110, 253, 0.25);
}


.media-thumbs {
  display: flex;
  gap: 8px;
  margin-top: 12px;
  overflow-x: auto;
  padding-bottom: 4px;
  -webkit-overflow-scrolling: touch;
}

.thumb-btn {
  border: 0;
  padding: 0;
  background: transparent;
  border-radius: 10px;
  outline: none;
  position: relative;
  flex: 0 0 auto;
}

.thumb-img {
  width: 76px;            /* 同一寬度 */
  aspect-ratio: 1 / 1;    /* 保持正方形 */
  object-fit: cover;      /* 不變形 */
  border-radius: 10px;
  border: 2px solid transparent;
  display: block;
  background: #f4f5f7;
}

.thumb-btn.active .thumb-img {
  border-color: #0d6efd;
  box-shadow: 0 0 0 2px rgba(13,110,253,.18);
}

@media (max-width: 576px) {
  .thumb-img { width: 64px; }
}


.product-title {
  font-size: 2rem;
  font-weight: 700;
}

/* 顏色選擇樣式 */
.color-option-wrapper {
  display: flex;
  flex-direction: column;
  align-items: center;
}

.color-option-btn {
  background: none;
  border: none;
  padding: 0;
  cursor: pointer;
  transition: transform 0.2s ease;
}

.color-option-btn:hover:not(:disabled) {
  transform: scale(1.1);
}

.color-option-btn:disabled {
  cursor: not-allowed;
  opacity: 0.5;
}

.color-option-btn.active {
  transform: scale(1.15);
}

.color-circle {
  position: relative;
  width: 50px;
  height: 50px;
  border-radius: 50%;
  border: 3px solid #dee2e6;
  transition: all 0.3s ease;
  display: flex;
  align-items: center;
  justify-content: center;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
}

.color-option-btn:hover:not(:disabled) .color-circle {
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.2);
}

.color-option-btn.active .color-circle {
  border-width: 4px;
  border-color: #fd0d0d;
  box-shadow: 0 0 0 3px rgba(13, 110, 253, 0.25);
}

.out-of-stock-overlay {
  position: absolute;
  width: 100%;
  height: 100%;
  background-color: rgba(255, 255, 255, 0.7);
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  color: #dc3545;
  font-size: 1.5rem;
}

.selected-check {
  position: absolute;
  width: 24px;
  height: 24px;
  background-color: #fd0d0d;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  color: white;
  font-size: 0.875rem;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.2);
}

.color-name {
  font-size: 0.875rem;
  color: #495057;
  max-width: 80px;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.stock-status {
  font-size: 0.75rem;
  font-weight: 500;
}

/* 其他變體按鈕 */
.variant-button {
  padding: 0.75rem;
  border: 2px solid #dee2e6;
  border-radius: 8px;
  background-color: #fff;
  cursor: pointer;
  transition: all 0.3s;
  text-align: left;
}

.variant-button:hover:not(.disabled) {
  border-color: #ffe3be;
  transform: translateY(-2px);
  box-shadow: 0 4px 8px rgba(0,0,0,0.1);
}

.variant-button.active {
  border-color: #ffedd1;
  background-color: #e7f1ff;
}

.variant-button.disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.related-card {
  cursor: pointer;
  transition: transform 0.3s;
}

.related-card:hover {
  transform: translateY(-5px);
}

.related-card .card-img-top {
  height: 200px;
  object-fit: cover;
  background-color: #f8f9fa;
}

@media (max-width: 991px) {
  .product-title {
    font-size: 1.5rem;
  }
  
  .color-circle {
    width: 45px;
    height: 45px;
  }
}

@media (max-width: 576px) {
  .color-circle {
    width: 40px;
    height: 40px;
  }
  
  .color-name {
    font-size: 0.75rem;
  }
}
</style>