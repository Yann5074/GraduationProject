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
        <div class="col-lg-6">
          <div class="product-images">
            <div class="main-image-container mb-3">
              <img :src="selectedImage" :alt="product.fName" class="img-fluid rounded main-image" @error="handleImageError" />
              <div v-if="product.fDiscount > 0" class="position-absolute top-0 start-0 p-3">
                <span class="badge bg-danger fs-6">{{ product.fDiscount }}% OFF</span>
              </div>
            </div>
            
            <div v-if="productImages.length > 1" class="d-flex gap-2 overflow-auto">
              <img v-for="(image, index) in productImages" :key="index" :src="image" class="thumbnail" :class="{ active: selectedImage === image }" @click="selectedImage = image" @error="handleImageError" />
            </div>
          </div>
        </div>

        <!-- 右側：產品資訊 -->
        <div class="col-lg-6">
          <h1 class="product-title mb-3">{{ product.fName }}</h1>
          
          <div class="mb-3">
            <span class="badge" :class="getStatusBadgeClass(product.fPstatus)">{{ product.statusName }}</span>
            <span v-if="product.isAvailable" class="badge bg-success ms-2">有貨</span>
            <span v-else class="badge bg-secondary ms-2">缺貨</span>
          </div>

          <!-- 價格 -->
          <div class="price-section mb-4 p-3 bg-light rounded">
            <div v-if="selectedVariant">
              <div v-if="product.fDiscount > 0" class="mb-2">
                <span class="text-decoration-line-through text-muted">NT$ {{ formatPrice(selectedVariant.originalPrice) }}</span>
              </div>
              <div class="text-primary fw-bold fs-2">NT$ {{ formatPrice(selectedVariant.fPrice) }}</div>
            </div>
            <div v-else class="text-primary fw-bold fs-2">
              NT$ {{ formatPrice(product.minPrice) }}<span v-if="product.minPrice !== product.maxPrice"> - {{ formatPrice(product.maxPrice) }}</span>
            </div>
          </div>

          <p v-if="product.fDescription" class="text-muted mb-4">{{ product.fDescription }}</p>
          
          <hr />

          <!-- 規格選擇 -->
          <div v-if="variants.length > 0" class="mb-4">
            <h5 class="mb-3">選擇規格</h5>
            <div class="row g-2">
              <div v-for="variant in variants" :key="variant.fProductVariantId" class="col-6 col-md-4">
                <button class="variant-button w-100" :class="{ active: selectedVariant?.fProductVariantId === variant.fProductVariantId, disabled: variant.fStock <= 0 }" :disabled="variant.fStock <= 0" @click="selectVariant(variant)">
                  <div class="fw-bold">{{ variant.colorName || 'SKU: ' + variant.fSku }}</div>
                  <div class="text-primary">NT$ {{ formatPrice(variant.fPrice) }}</div>
                  <small v-if="variant.fStock <= 0" class="text-danger">缺貨</small>
                </button>
              </div>
            </div>
          </div>

          <!-- 數量選擇 -->
          <div class="mb-4">
            <h5 class="mb-3">數量</h5>
            <div class="input-group" style="max-width: 200px">
              <button class="btn btn-outline-secondary" :disabled="quantity <= 1" @click="decreaseQuantity">-</button>
              <input v-model.number="quantity" type="number" class="form-control text-center" min="1" :max="maxQuantity" @input="validateQuantity" />
              <button class="btn btn-outline-secondary" :disabled="quantity >= maxQuantity" @click="increaseQuantity">+</button>
            </div>
            <small v-if="selectedVariant" class="text-muted">庫存: {{ selectedVariant.fStock }} 件</small>
          </div>

          <hr />

          <!-- 操作按鈕 -->
          <div class="d-grid gap-2">
            <button class="btn btn-primary btn-lg" :disabled="!canAddToCart" @click="addToCart">加入購物車</button>
            <button class="btn btn-outline-primary btn-lg" :disabled="!canAddToCart" @click="buyNow">立即購買</button>
          </div>

          <div v-if="!canAddToCart" class="alert alert-warning mt-3">{{ getDisabledReason() }}</div>
        </div>
      </div>

      <!-- 產品詳細資訊標籤 -->
      <div class="row mt-5">
        <div class="col-12">
          <ul class="nav nav-tabs">
            <li class="nav-item"><button class="nav-link active" data-bs-toggle="tab" data-bs-target="#details">產品規格</button></li>
            <li class="nav-item"><button class="nav-link" data-bs-toggle="tab" data-bs-target="#description">詳細說明</button></li>
          </ul>
          <div class="tab-content border border-top-0 rounded-bottom p-4">
            <div id="details" class="tab-pane fade show active">
              <div class="row g-3">
                <div class="col-md-6"><strong>類別：</strong> {{ product.categoryName }}</div>
                <div v-if="product.fWarrantyMonth" class="col-md-6"><strong>保固：</strong> {{ product.fWarrantyMonth }} 個月</div>
                <div class="col-md-6"><strong>組裝：</strong> {{ product.fAssemblyRequired ? '需要組裝' : '無需組裝' }}</div>
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
import { ref, computed, onMounted, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { ProductAPI } from '@/api/Product'


const route = useRoute()
const router = useRouter()

const loading = ref(false)
const error = ref(null)
const product = ref(null)
const variants = ref([])
const selectedVariant = ref(null)
const quantity = ref(1)
const selectedImage = ref('')
const productImages = ref([])
const relatedProducts = ref([])

const maxQuantity = computed(() => selectedVariant.value?.fStock || product.value?.totalStock || 0)
const canAddToCart = computed(() => {
  if (!product.value?.isAvailable) return false
  if (variants.value.length > 0 && !selectedVariant.value) return false
  return quantity.value >= 1 && quantity.value <= maxQuantity.value
})

async function loadProductDetail() {
  const productId = route.params.id
  if (!productId) {
    error.value = '產品 ID 不正確'
    return
  }

  loading.value = true
  error.value = null

  console.log('📦 載入產品詳情:', productId)

  try {
    const response = await ProductAPI.getProductById(productId)
    console.log('📥 API 回應:', response)
    
    if (response.success && response.data) {
      // 處理後端資料
      const rawData = response.data
      
      // ⭐ 如果 mainImageUrl 是 null，從 assets 中取得
      if (!rawData.mainImageUrl && rawData.assets && rawData.assets.length > 0) {
        const primaryAsset = rawData.assets.find(a => a.fIsPrimary) || rawData.assets[0]
        rawData.mainImageUrl = primaryAsset.fUrl  // ← 統一使用 fUrl
      }
      
      // ⭐ 設定預設值
      rawData.fPstatus = rawData.fPstatus ?? 1
      rawData.statusName = rawData.statusName || '上架中'
      rawData.fWarrantyMonth = rawData.fWarrantyMonth ?? 0
      rawData.fAssemblyRequired = rawData.fAssemblyRequired ?? false
      
      product.value = rawData
      console.log('✅ 產品資料:', product.value)
      
      setupProductImages()
      await loadVariants(productId)
      await loadRelatedProducts(product.value.fCategoryId)
    } else {
      console.error('❌ 載入失敗:', response)
      error.value = response.message || '載入產品失敗'
    }
  } catch (err) {
    console.error('❌ 發生錯誤:', err)
    console.error('錯誤詳情:', err.response?.data)
    error.value = err.response?.data?.message || err.message || '載入產品時發生錯誤'
  } finally {
    loading.value = false
  }
}

function setupProductImages() {
  productImages.value = []
  
  // 主圖
  if (product.value.mainImageUrl) {
    productImages.value.push(getImageUrl(product.value.mainImageUrl))
  }
  
  // 從 assets 取得其他圖片
  if (product.value.assets && Array.isArray(product.value.assets)) {
    product.value.assets.forEach(asset => {
      const url = asset.fUrl  // ← 統一使用 fUrl
      if (url && !productImages.value.includes(getImageUrl(url))) {
        productImages.value.push(getImageUrl(url))
      }
    })
  }
  
  selectedImage.value = productImages.value[0] || ''
}

async function loadVariants(productId) {
  try {
    const response = await ProductAPI.getProductVariants(productId)
    console.log('📥 變體回應:', response)
    
    if (response.success && response.data) {
      variants.value = response.data.map(v => ({
        ...v,
        originalPrice: calculateOriginalPrice(v.fPrice, product.value.fDiscount)
      }))
      
      const available = variants.value.find(v => v.fStock > 0)
      if (available) selectVariant(available)
      
      console.log('✅ 變體載入:', variants.value.length, '個')
    } else if (product.value.variants && Array.isArray(product.value.variants)) {
      // 如果 API 沒有單獨的變體端點，使用產品資料中的 variants
      variants.value = product.value.variants.map(v => ({
        ...v,
        originalPrice: calculateOriginalPrice(v.fPrice, product.value.fDiscount)
      }))
      
      const available = variants.value.find(v => v.fStock > 0)
      if (available) selectVariant(available)
      
      console.log('✅ 從產品資料載入變體:', variants.value.length, '個')
    }
  } catch (err) {
    console.error('❌ 載入變體失敗:', err)
    // 如果變體 API 失敗，嘗試使用產品資料中的 variants
    if (product.value.variants && Array.isArray(product.value.variants)) {
      variants.value = product.value.variants
      const available = variants.value.find(v => v.fStock > 0)
      if (available) selectVariant(available)
    }
  }
}

async function loadRelatedProducts(categoryId) {
  if (!categoryId) return
  try {
    const response = await ProductAPI.getProducts({ categoryId, pageNumber: 1, pageSize: 8 })
    if (response.success && response.data?.data) {
      relatedProducts.value = response.data.data
        .filter(p => p.fProductId !== product.value.fProductId)
        .slice(0, 4)
        .map(p => {
          // 處理圖片 URL
          let imageUrl = p.mainImageUrl || p.MainImageUrl
          if (!imageUrl && p.assets && p.assets.length > 0) {
            const primaryAsset = p.assets.find(a => a.fIsPrimary) || p.assets[0]
            imageUrl = primaryAsset.fUrl  // ← 統一使用 fUrl
          }
          return { ...p, mainImageUrl: getImageUrl(imageUrl) }
        })
    }
  } catch (err) {
    console.error('❌ 載入相關產品失敗:', err)
  }
}

function selectVariant(variant) {
  selectedVariant.value = variant
  quantity.value = 1
}

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

async function addToCart() {
  if (!canAddToCart.value) return
  try {
    const response = await CartAPI.addToCart({
      fProductId: product.value.fProductId,
      fVariantId: selectedVariant.value?.fProductVariantId || null,
      fQuantity: quantity.value
    })
    if (response.success) {
      alert('已加入購物車！')
      quantity.value = 1
    } else {
      alert(response.message || '加入購物車失敗')
    }
  } catch (err) {
    alert('加入購物車時發生錯誤')
  }
}

function buyNow() {
  if (!canAddToCart.value) return
  addToCart().then(() => router.push('/cart'))
}

function goToProduct(productId) {
  router.push(`/products/${productId}`)
}

function getDisabledReason() {
  if (!product.value.isAvailable) return '產品暫時缺貨'
  if (variants.value.length > 0 && !selectedVariant.value) return '請選擇規格'
  if (quantity.value < 1 || quantity.value > maxQuantity.value) return '數量不正確'
  return ''
}

function getStatusBadgeClass(status) {
  return { 1: 'bg-success', 2: 'bg-warning', 3: 'bg-danger', 4: 'bg-secondary' }[status] || 'bg-secondary'
}

function formatPrice(price) {
  return price ? Math.round(price).toLocaleString() : '0'
}

function calculateOriginalPrice(discountedPrice, discountPercent) {
  return discountPercent > 0 ? Math.round(discountedPrice / (1 - discountPercent / 100)) : discountedPrice
}

function getImageUrl(url) {
  if (!url) return 'https://via.placeholder.com/500/cccccc/666666?text=No+Image'
  if (url.startsWith('http')) return url
  if (url.startsWith('/')) return `${import.meta.env.VITE_API_BASE_URL || 'https://localhost:7131'}${url}`
  return 'https://via.placeholder.com/500/cccccc/666666?text=Invalid+URL'
}

function handleImageError(event) {
  event.target.src = 'https://via.placeholder.com/500/e0e0e0/666666?text=Image+Not+Found'
}

onMounted(() => loadProductDetail())

watch(() => route.params.id, (newId) => {
  if (newId && route.name === 'ProductDetail') {
    loadProductDetail()
    window.scrollTo({ top: 0, behavior: 'smooth' })
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
  background-color: #fff;
  border-radius: 8px;
}

.main-image {
  width: 100%;
  height: auto;
  object-fit: contain;
  max-height: 600px;
}

.thumbnail {
  width: 80px;
  height: 80px;
  object-fit: cover;
  border-radius: 4px;
  cursor: pointer;
  border: 2px solid transparent;
  transition: border 0.3s;
}

.thumbnail:hover,
.thumbnail.active {
  border-color: #0d6efd;
}

.product-title {
  font-size: 2rem;
  font-weight: 700;
}

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
  border-color: #0d6efd;
}

.variant-button.active {
  border-color: #0d6efd;
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
}

@media (max-width: 991px) {
  .product-title {
    font-size: 1.5rem;
  }
}
</style>