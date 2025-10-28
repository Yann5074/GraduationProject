<!-- 產品詳情頁 - 對應 7 個資料表後端 API (無客製化功能) -->
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
      <div class="spinner-border text-primary" role="status"></div>
      <p class="mt-3 text-muted">載入產品資訊...</p>
    </div>

    <!-- 錯誤訊息 -->
    <div v-else-if="error" class="container py-5">
      <div class="alert alert-danger">
        <i class="bi bi-exclamation-triangle me-2"></i>
        {{ error }}
      </div>
      <button class="btn btn-primary" @click="loadProductDetail">
        <i class="bi bi-arrow-clockwise me-2"></i>重新載入
      </button>
    </div>

    <!-- 產品詳情 -->
    <div v-else-if="product" class="container py-4">
      <div class="row g-4">
        <!-- 左側：圖片/3D 模型區 -->
        <div class="col-lg-6">
          <!-- 切換按鈕 -->
          <div class="d-flex justify-content-between align-items-center mb-3">
            <div class="btn-group" role="group">
              <button 
                class="btn" 
                :class="viewMode === 'image' ? 'btn-primary' : 'btn-outline-primary'"
                @click="viewMode = 'image'"
              >
                <i class="bi bi-image me-1"></i>照片
              </button>
              <button 
                v-if="product.f3dModelPath"
                class="btn" 
                :class="viewMode === '3d' ? 'btn-primary' : 'btn-outline-primary'"
                @click="viewMode = '3d'"
              >
                <i class="bi bi-badge-3d me-1"></i>3D 模型
              </button>
            </div>
            
            <!-- 3D 可用提示 -->
            <small v-if="product.f3dModelPath && viewMode === 'image'" class="text-muted">
              <i class="bi bi-info-circle me-1"></i>可切換 3D 預覽
            </small>
          </div>

          <!-- 照片視窗 -->
          <div v-show="viewMode === 'image'" class="product-images">
            <div class="main-image-container mb-3 position-relative">
              <img 
                :src="selectedImage" 
                :alt="product.fName" 
                class="img-fluid rounded main-image" 
                @error="handleImageError" 
              />
              <!-- 折扣標籤 -->
              <div v-if="product.fDiscount && product.fDiscount > 0" class="position-absolute top-0 start-0 p-3">
                <span class="badge bg-danger fs-6">{{ Math.round(product.fDiscount * 100) }}% OFF</span>
              </div>
            </div>
            
            <!-- 縮圖列表 -->
            <div v-if="productImages.length > 1" class="d-flex gap-2 overflow-auto pb-2">
              <img 
                v-for="(image, index) in productImages" 
                :key="index" 
                :src="image" 
                class="thumbnail" 
                :class="{ active: selectedImage === image }" 
                @click="selectedImage = image" 
                @error="handleImageError" 
              />
            </div>
          </div>

          <!-- 3D 模型視窗 -->
          <div v-show="viewMode === '3d' && product.f3dModelPath" class="viewer-3d-wrapper">
            <Furniture3DViewer 
              v-if="product.f3dModelPath"
              :model-url="product.f3dModelPath"
              :env-map-url="product.envMapUrl || null"
              @model-loaded="on3DModelLoaded"
              @model-error="on3DModelError"
            />
          </div>

          <!-- 沒有 3D 模型的提示 -->
          <div v-if="!product.f3dModelPath && viewMode === '3d'" class="alert alert-info">
            <i class="bi bi-info-circle me-2"></i>
            此產品暫無 3D 模型
          </div>
        </div>

        <!-- 右側：產品資訊 -->
        <div class="col-lg-6">
          <!-- 產品標題 -->
          <h1 class="product-title mb-3">{{ product.fName }}</h1>
          
          <!-- 狀態標籤 -->
          <div class="mb-3">
            <span class="badge bg-secondary">{{ product.categoryName }}</span>
            <span v-if="product.isAvailable" class="badge bg-success ms-2">
              <i class="bi bi-check-circle me-1"></i>有貨
            </span>
            <span v-else class="badge bg-secondary ms-2">
              <i class="bi bi-x-circle me-1"></i>缺貨
            </span>
            <span v-if="product.fWarrantyMonth" class="badge bg-info ms-2">
              <i class="bi bi-shield-check me-1"></i>保固 {{ product.fWarrantyMonth }} 個月
            </span>
          </div>

          <!-- 價格區 -->
          <div class="price-section mb-4 p-3 bg-light rounded">
            <div v-if="selectedVariant">
              <!-- 有折扣時顯示劃線價格 -->
              <div v-if="product.fDiscount && product.fDiscount > 0" class="mb-2">
                <span class="text-decoration-line-through text-muted fs-5">
                  NT$ {{ formatPrice(calculateOriginalPrice(selectedVariant.fPrice)) }}
                </span>
              </div>
              <!-- 實際價格 -->
              <div class="text-primary fw-bold fs-2">
                NT$ {{ formatPrice(selectedVariant.fPrice) }}
              </div>
            </div>
            <!-- 未選擇變體時顯示價格範圍 -->
            <div v-else class="text-primary fw-bold fs-2">
              <span v-if="product.minPrice === product.maxPrice">
                NT$ {{ formatPrice(product.minPrice) }}
              </span>
              <span v-else>
                NT$ {{ formatPrice(product.minPrice) }} - {{ formatPrice(product.maxPrice) }}
              </span>
            </div>
          </div>

          <!-- 產品描述 -->
          <div v-if="product.fDescription" class="mb-4">
            <p class="text-muted">{{ product.fDescription }}</p>
          </div>
          
          <hr />

          <!-- 變體選擇 -->
          <div v-if="variants.length > 0" class="mb-4">
            <h5 class="mb-3">
              <i class="bi bi-box-seam me-2"></i>選擇規格
            </h5>
            <div class="row g-2">
              <div 
                v-for="variant in variants" 
                :key="variant.fProductVariantId" 
                class="col-6 col-md-4"
              >
                <button 
                  class="variant-button w-100 p-3" 
                  :class="{ 
                    active: selectedVariant?.fProductVariantId === variant.fProductVariantId,
                    disabled: !variant.fStock || variant.fStock <= 0
                  }" 
                  :disabled="!variant.fStock || variant.fStock <= 0" 
                  @click="selectVariant(variant)"
                >
                  <!-- SKU / 尺寸標籤 -->
                  <div class="fw-bold mb-2">
                    {{ variant.fSizeLabel || variant.fSku }}
                  </div>
                  
                  <!-- 價格 -->
                  <div class="text-primary fw-bold mb-1">
                    NT$ {{ formatPrice(variant.fPrice) }}
                  </div>
                  
                  <!-- 庫存狀態 -->
                  <small v-if="!variant.fStock || variant.fStock <= 0" class="text-danger">
                    缺貨
                  </small>
                  <small v-else class="text-muted">
                    庫存: {{ variant.fStock }} 件
                  </small>
                </button>
              </div>
            </div>
          </div>

          <!-- 數量選擇 -->
          <div class="mb-4">
            <h5 class="mb-3">
              <i class="bi bi-bag me-2"></i>數量
            </h5>
            <div class="input-group" style="max-width: 200px">
              <button 
                class="btn btn-outline-secondary" 
                :disabled="quantity <= 1" 
                @click="quantity--"
              >
                <i class="bi bi-dash"></i>
              </button>
              <input 
                v-model.number="quantity" 
                type="number" 
                class="form-control text-center" 
                min="1" 
                :max="maxQuantity"
              />
              <button 
                class="btn btn-outline-secondary" 
                :disabled="quantity >= maxQuantity" 
                @click="quantity++"
              >
                <i class="bi bi-plus"></i>
              </button>
            </div>
            <small v-if="maxQuantity > 0" class="text-muted d-block mt-2">
              最多可購買 {{ maxQuantity }} 件
            </small>
          </div>

          <hr />

          <!-- 操作按鈕 -->
          <div class="d-grid gap-2">
            <button 
              class="btn btn-primary btn-lg" 
              :disabled="!canAddToCart" 
              @click="addToCart"
            >
              <i class="bi bi-cart-plus me-2"></i>加入購物車
            </button>
            <button 
              class="btn btn-outline-primary btn-lg" 
              :disabled="!canAddToCart" 
              @click="buyNow"
            >
              <i class="bi bi-lightning-fill me-2"></i>立即購買
            </button>
          </div>

          <!-- 產品資訊 -->
          <div class="mt-4">
            <div class="card">
              <div class="card-body">
                <h6 class="card-title">
                  <i class="bi bi-info-circle me-2"></i>產品資訊
                </h6>
                <ul class="list-unstyled mb-0">
                  <li v-if="product.fWarrantyMonth" class="mb-2">
                    <strong>保固期限：</strong>{{ product.fWarrantyMonth }} 個月
                  </li>
                  <li v-if="product.fAssemblyRequired !== undefined" class="mb-2">
                    <strong>需要組裝：</strong>{{ product.fAssemblyRequired ? '是' : '否' }}
                  </li>
                  <li class="mb-2">
                    <strong>類別：</strong>{{ product.categoryName }}
                  </li>
                  <li v-if="product.totalStock !== undefined">
                    <strong>總庫存：</strong>{{ product.totalStock }} 件
                  </li>
                </ul>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- 相似產品推薦 -->
      <div v-if="similarProducts.length > 0" class="mt-5">
        <h3 class="mb-4">
          <i class="bi bi-stars me-2"></i>您可能也喜歡
        </h3>
        <div class="row g-3">
          <div 
            v-for="item in similarProducts" 
            :key="item.fProductId" 
            class="col-6 col-md-4 col-lg-3"
          >
            <router-link 
              :to="`/products/${item.fProductId}`" 
              class="text-decoration-none"
            >
              <div class="card h-100 similar-product-card">
                <img 
                  :src="item.mainImageUrl || '/images/default-product.jpg'" 
                  class="card-img-top" 
                  :alt="item.fName"
                  @error="handleImageError"
                />
                <div class="card-body">
                  <h6 class="card-title text-truncate">{{ item.fName }}</h6>
                  <p class="card-text text-primary fw-bold">
                    NT$ {{ formatPrice(item.minPrice) }}
                  </p>
                </div>
              </div>
            </router-link>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, watch, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import ProductAPI from '@/api/Product'
import Furniture3DViewer from '@/components/Furniture3DViewer.vue'

const route = useRoute()
const router = useRouter()

// 狀態
const loading = ref(false)
const error = ref(null)
const product = ref(null)
const variants = ref([])
const selectedVariant = ref(null)
const selectedImage = ref('')
const productImages = ref([])
const viewMode = ref('image') // 'image' | '3d'
const quantity = ref(1)
const similarProducts = ref([])

// 計算屬性
const maxQuantity = computed(() => {
  if (!selectedVariant.value) return 0
  return selectedVariant.value.fStock || 0
})

const canAddToCart = computed(() => {
  if (!product.value?.isAvailable) return false
  if (variants.value.length > 0 && !selectedVariant.value) return false
  if (!selectedVariant.value?.fStock || selectedVariant.value.fStock <= 0) return false
  return quantity.value >= 1 && quantity.value <= maxQuantity.value
})

// 監聽路由變化
watch(() => route.params.id, (newId) => {
  if (newId) {
    loadProductDetail()
  }
})

// 方法
const loadProductDetail = async () => {
  const productId = route.params.id
  if (!productId) {
    error.value = '產品 ID 不正確'
    return
  }

  loading.value = true
  error.value = null

  try {
    // ✅ 使用新版 API（無 includeCustomization 參數）
    const response = await ProductAPI.getProductById(productId)
    
    if (response.ok && response.data) {
      const rawData = response.data
      
      // 處理圖片
      if (response.data.assets && response.data.assets.length > 0) {
        productImages.value = response.data.assets
          .filter(a => a.fAssetType === 'Image' || !a.fAssetType)
          .sort((a, b) => (a.fSortOrder || 0) - (b.fSortOrder || 0))
          .map(a => a.fUrl)
      }
      
      // 設定主圖
      selectedImage.value = product.value.mainImageUrl || productImages.value[0] || '/images/default-product.jpg'
      
      // 自動選擇第一個有庫存的變體
      if (variants.value.length > 0) {
        const firstAvailableVariant = variants.value.find(v => v.fStock && v.fStock > 0)
        if (firstAvailableVariant) {
          selectedVariant.value = firstAvailableVariant
        }
      }
      
      // 載入相似產品
      loadSimilarProducts(productId)
      
    } else {
      error.value = response.message || '產品不存在'
    }
  } catch (err) {
    console.error('載入產品詳情錯誤:', err)
    error.value = '載入產品失敗，請稍後再試'
  } finally {
    loading.value = false
  }
}

const loadSimilarProducts = async (productId) => {
  try {
    const response = await ProductAPI.getSimilarProducts(productId, 4)
    if (response.ok && response.data.data) {
      similarProducts.value = response.data.data
    }
  } catch (err) {
    console.error('載入相似產品失敗:', err)
  }
}

const selectVariant = (variant) => {
  selectedVariant.value = variant
  quantity.value = 1
}

function isVariantSelected(variant) {
  const variantId = variant.fProductVariantId || variant.FProductVariantId
  const selectedId = selectedVariant.value?.fProductVariantId || selectedVariant.value?.FProductVariantId
  return variantId === selectedId
}

function getVariantStock(variant) {
  return variant.fStock || variant.FStock || 0
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

//加入購物車方法
async function addToCart() {
  if (!canAddToCart.value) return
  try{
    const productVariantId = selectedVariant.value?.fProductVariantId || selectedVariant.value?.FProductVariantId
    const qty = quantity.value

    if(!productVariantId){
      alert('請先選擇商品規格')
    }

    const atc = {
      productVariantId,
      qty
    }
    const result = await memberAddToCart(atc)
    if (result.ok){
      alert(result.message)
    }else{
      alert(result.message)
    }
  }catch(err){
    console.error('錯誤', err)
    alert('加入購物車時發生問題，請重新確認')
  }
}

async function buyNow() {
  if (!canAddToCart.value) return
  try{
    await memberAddToCart()
    router.push('/cart')
  }catch(err){
    console.error('立即購買發生錯誤', err)
  }
}

function goToProduct(productId) {
  router.push(`/products/${productId}`)
}
const formatPrice = (price) => {
  if (price === null || price === undefined) return '0'
  return Math.round(price).toLocaleString()
}

const calculateOriginalPrice = (discountedPrice) => {
  if (!product.value?.fDiscount) return discountedPrice
  return discountedPrice / (1 - product.value.fDiscount)
}

const handleImageError = (e) => {
  e.target.src = '/images/default-product.jpg'
}

const on3DModelLoaded = () => {
  console.log('✅ 3D 模型載入成功')
}

const on3DModelError = (error) => {
  console.error('❌ 3D 模型載入失敗:', error)
  viewMode.value = 'image'
}

// 生命週期
onMounted(() => {
  loadProductDetail()
})
</script>

<style scoped>
/* 主圖容器 */
.main-image-container {
  aspect-ratio: 1 / 1;
  overflow: hidden;
  border-radius: 8px;
  background: #f8f9fa;
}

.main-image {
  width: 100%;
  height: 100%;
  object-fit: contain;
}

/* 縮圖 */
.thumbnail {
  width: 80px;
  height: 80px;
  object-fit: cover;
  border-radius: 4px;
  cursor: pointer;
  border: 2px solid transparent;
  transition: all 0.2s;
}

.thumbnail:hover {
  border-color: var(--bs-primary);
}

.thumbnail.active {
  border-color: var(--bs-primary);
  box-shadow: 0 0 0 2px rgba(13, 110, 253, 0.25);
}

/* 3D 視窗 */
.viewer-3d-wrapper {
  aspect-ratio: 1 / 1;
  border-radius: 8px;
  overflow: hidden;
  background: #f8f9fa;
}

/* 變體按鈕 */
.variant-button {
  border: 2px solid #dee2e6;
  border-radius: 8px;
  background: white;
  transition: all 0.2s;
  text-align: center;
}

.variant-button:not(.disabled):hover {
  border-color: var(--bs-primary);
  transform: translateY(-2px);
  box-shadow: 0 4px 8px rgba(0,0,0,0.1);
}

.variant-button.active {
  border-color: var(--bs-primary);
  background: rgba(13, 110, 253, 0.1);
}

.variant-button.disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

/* 價格區 */
.price-section {
  border-left: 4px solid var(--bs-primary);
}

/* 相似產品卡片 */
.similar-product-card {
  transition: transform 0.2s, box-shadow 0.2s;
}

.similar-product-card:hover {
  transform: translateY(-4px);
  box-shadow: 0 4px 12px rgba(0,0,0,0.15);
}

.similar-product-card .card-img-top {
  aspect-ratio: 1 / 1;
  object-fit: cover;
}

/* 響應式 */
@media (max-width: 768px) {
  .product-title {
    font-size: 1.5rem;
  }
  
  .price-section .fs-2 {
    font-size: 1.5rem !important;
  }
}
</style>