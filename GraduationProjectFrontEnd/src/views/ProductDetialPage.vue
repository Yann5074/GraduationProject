<template>
  <div class="product-detail-page">
    <div class="container py-4">
      <!-- 載入中 -->
      <div v-if="loading" class="text-center py-5">
        <div class="spinner-border text-primary" role="status" style="width: 3rem; height: 3rem;">
          <span class="visually-hidden">載入中...</span>
        </div>
        <p class="mt-3 text-muted">載入產品資料中...</p>
      </div>

      <!-- 錯誤訊息 -->
      <div v-else-if="error" class="alert alert-danger" role="alert">
        <i class="bi bi-exclamation-triangle me-2"></i>{{ error }}
        <button class="btn btn-primary mt-3" @click="$router.push('/products')">
          返回產品列表
        </button>
      </div>

      <!-- 產品詳情 -->
      <div v-else-if="product">
        <!-- 麵包屑導航 -->
        <nav aria-label="breadcrumb" class="mb-4">
          <ol class="breadcrumb">
            <li class="breadcrumb-item">
              <a href="#" @click.prevent="$router.push('/products')">產品列表</a>
            </li>
            <li v-if="product.categoryName" class="breadcrumb-item">
              <a 
                href="#" 
                @click.prevent="$router.push({ path: '/products', query: { categoryId: product.fCategoryId } })"
              >
                {{ product.categoryName }}
              </a>
            </li>
            <li class="breadcrumb-item active" aria-current="page">
              {{ product.fName }}
            </li>
          </ol>
        </nav>

        <div class="row">
          <!-- 左側：圖片展示區 -->
          <div class="col-lg-6 mb-4">
            <div class="product-gallery">
              <!-- 主圖 -->
              <div class="main-image-container mb-3">
                <img
                  :src="selectedImage"
                  :alt="product.fName"
                  class="img-fluid rounded shadow"
                  @error="handleImageError"
                />
                <!-- 標籤 -->
                <div class="position-absolute top-0 start-0 p-3">
                  <span v-if="!product.isAvailable" class="badge bg-danger">
                    售完
                  </span>
                  <span v-else-if="product.fDiscount" class="badge bg-warning text-dark">
                    {{ product.fDiscount }}% OFF
                  </span>
                  <span v-if="product.isCustomizable" class="badge bg-info ms-1">
                    可客製化
                  </span>
                </div>
              </div>

              <!-- 縮圖輪播 -->
              <div v-if="product.assets && product.assets.length > 1" class="thumbnail-carousel">
                <div class="d-flex gap-2 flex-wrap">
                  <div
                    v-for="(asset, index) in product.assets"
                    :key="asset.fAssetId"
                    class="thumbnail-item"
                    :class="{ active: selectedImage === asset.fUrl }"
                    @click="selectImage(asset.fUrl)"
                  >
                    <img
                      :src="asset.fUrl"
                      :alt="`${product.fName} - 圖 ${index + 1}`"
                      class="img-thumbnail"
                      style="width: 80px; height: 80px; object-fit: cover; cursor: pointer;"
                    />
                  </div>
                </div>
              </div>
            </div>
          </div>

          <!-- 右側：產品資訊 -->
          <div class="col-lg-6">
            <div class="product-info">
              <!-- 產品名稱 -->
              <h1 class="h2 mb-2">{{ product.fName }}</h1>
              
              <!-- 分類和狀態 -->
              <div class="mb-3">
                <span class="badge bg-secondary me-2">{{ product.categoryName }}</span>
                <span class="badge bg-success">{{ product.statusName }}</span>
              </div>

              <!-- 價格 -->
              <div class="price-section mb-4">
                <div v-if="selectedVariant">
                  <div class="h3 text-primary mb-1">
                    ${{ formatPrice(selectedVariant.fPrice) }}
                  </div>
                  <div class="text-muted small">
                    SKU: {{ selectedVariant.fSku }}
                  </div>
                </div>
                <div v-else>
                  <div class="h3 text-primary mb-1">
                    <span v-if="product.minPrice === product.maxPrice">
                      ${{ formatPrice(product.minPrice) }}
                    </span>
                    <span v-else>
                      ${{ formatPrice(product.minPrice) }} - ${{ formatPrice(product.maxPrice) }}
                    </span>
                  </div>
                </div>
              </div>

              <!-- 產品描述 -->
              <div class="product-description mb-4">
                <h5>產品描述</h5>
                <p class="text-muted">{{ product.fDescription || '暫無描述' }}</p>
              </div>

              <!-- 可客製化選項 -->
              <div v-if="product.isCustomizable && product.customizationParts" class="customization-section mb-4">
                <h5 class="mb-3">
                  <i class="bi bi-palette me-2"></i>客製化選項
                </h5>

                <div
                  v-for="part in product.customizationParts"
                  :key="part.fPartId"
                  class="mb-4"
                >
                  <label class="form-label fw-bold">
                    {{ part.fPartName }}
                    <span class="text-danger">*</span>
                  </label>

                  <div class="row g-3">
                    <div
                      v-for="option in part.colorOptions"
                      :key="option.fColorOptionId"
                      class="col-md-6"
                    >
                      <div
                        class="color-option-card p-3 border rounded"
                        :class="{
                          'border-primary border-2': selectedOptions[part.fPartCode] === option.fColorOptionId,
                          'bg-light': selectedOptions[part.fPartCode] !== option.fColorOptionId
                        }"
                        @click="selectColorOption(part.fPartCode, option.fColorOptionId)"
                        style="cursor: pointer;"
                      >
                        <div class="d-flex align-items-center">
                          <!-- 顏色預覽 -->
                          <div
                            v-if="option.fThumbnail"
                            class="me-3"
                          >
                            <img
                              :src="option.fThumbnail"
                              :alt="option.fOptionName"
                              class="rounded"
                              style="width: 50px; height: 50px; object-fit: cover;"
                            />
                          </div>
                          <div
                            v-else-if="option.fColorHex"
                            class="color-preview me-3"
                            :style="{ backgroundColor: option.fColorHex }"
                          ></div>

                          <!-- 選項資訊 -->
                          <div class="flex-grow-1">
                            <div class="fw-bold">{{ option.fOptionName }}</div>
                            <div v-if="option.fPriceAdjustment !== 0" class="small text-muted">
                              {{ option.fPriceAdjustment > 0 ? '+' : '' }}${{ formatPrice(Math.abs(option.fPriceAdjustment)) }}
                            </div>
                            <div v-if="option.fIsDefault" class="small text-info">
                              <i class="bi bi-star-fill"></i> 預設
                            </div>
                          </div>

                          <!-- 選中圖示 -->
                          <div v-if="selectedOptions[part.fPartCode] === option.fColorOptionId">
                            <i class="bi bi-check-circle-fill text-primary fs-4"></i>
                          </div>
                        </div>
                      </div>
                    </div>
                  </div>
                </div>

                <!-- 價格查詢按鈕 -->
                <button
                  class="btn btn-outline-primary w-100 mb-3"
                  :disabled="!allOptionsSelected || loadingPrice"
                  @click="fetchPriceByCustomization"
                >
                  <span v-if="loadingPrice">
                    <span class="spinner-border spinner-border-sm me-2"></span>
                    查詢中...
                  </span>
                  <span v-else>
                    <i class="bi bi-calculator me-2"></i>計算價格與庫存
                  </span>
                </button>

                <!-- 價格結果 -->
                <div v-if="customPrice" class="alert alert-success">
                  <h5 class="alert-heading">
                    <i class="bi bi-check-circle me-2"></i>查詢成功
                  </h5>
                  <hr>
                  <div class="d-flex justify-content-between align-items-center">
                    <div>
                      <strong>價格:</strong> ${{ formatPrice(customPrice.fPrice) }}
                    </div>
                    <div>
                      <strong>庫存:</strong>
                      <span :class="customPrice.isAvailable ? 'text-success' : 'text-danger'">
                        {{ customPrice.fStock }} 件
                      </span>
                    </div>
                  </div>
                  <div class="mt-2">
                    <small class="text-muted">SKU: {{ customPrice.fsku }}</small>
                  </div>
                </div>
              </div>

              <!-- 變體選擇（非客製化產品） -->
              <div v-else-if="product.variants && product.variants.length > 1" class="variants-section mb-4">
                <h5 class="mb-3">選擇規格</h5>
                <div class="row g-2">
                  <div
                    v-for="variant in product.variants"
                    :key="variant.fProductVariantId"
                    class="col-md-6"
                  >
                    <div
                      class="variant-card p-3 border rounded"
                      :class="{
                        'border-primary border-2': selectedVariant?.fProductVariantId === variant.fProductVariantId,
                        'bg-light': selectedVariant?.fProductVariantId !== variant.fProductVariantId
                      }"
                      @click="selectVariant(variant)"
                      style="cursor: pointer;"
                    >
                      <div class="d-flex justify-content-between align-items-center">
                        <div>
                          <div class="fw-bold">{{ variant.colorName || variant.fSizeLabel || '標準版' }}</div>
                          <div class="text-primary">${{ formatPrice(variant.fPrice) }}</div>
                          <div class="small text-muted">庫存: {{ variant.fStock }}</div>
                        </div>
                        <div v-if="selectedVariant?.fProductVariantId === variant.fProductVariantId">
                          <i class="bi bi-check-circle-fill text-primary fs-4"></i>
                        </div>
                      </div>
                    </div>
                  </div>
                </div>
              </div>

              <!-- 數量選擇 -->
              <div class="quantity-section mb-4">
                <label class="form-label fw-bold">數量</label>
                <div class="input-group" style="max-width: 150px;">
                  <button
                    class="btn btn-outline-secondary"
                    type="button"
                    @click="decreaseQuantity"
                    :disabled="quantity <= 1"
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
                    type="button"
                    @click="increaseQuantity"
                    :disabled="quantity >= maxQuantity"
                  >
                    <i class="bi bi-plus"></i>
                  </button>
                </div>
                <div class="text-muted small mt-1">
                  庫存: {{ product.totalStock }} 件
                </div>
              </div>

              <!-- 操作按鈕 -->
              <div class="action-buttons d-flex gap-2 mb-4">
                <button
                  class="btn btn-primary btn-lg flex-grow-1"
                  :disabled="!canAddToCart"
                  @click="addToCart"
                >
                  <i class="bi bi-cart-plus me-2"></i>加入購物車
                </button>
                <button
                  class="btn btn-outline-secondary btn-lg"
                  @click="toggleWishlist"
                >
                  <i :class="isInWishlist ? 'bi-heart-fill' : 'bi-heart'"></i>
                </button>
              </div>

              <!-- 產品資訊卡片 -->
              <div class="product-specs card">
                <div class="card-body">
                  <h5 class="card-title mb-3">產品規格</h5>
                  <ul class="list-unstyled mb-0">
                    <li v-if="product.fWarrantyMonth" class="mb-2">
                      <i class="bi bi-shield-check text-primary me-2"></i>
                      <strong>保固:</strong> {{ product.fWarrantyMonth }} 個月
                    </li>
                    <li v-if="product.fAssemblyRequired !== null" class="mb-2">
                      <i class="bi bi-tools text-primary me-2"></i>
                      <strong>組裝:</strong> {{ product.fAssemblyRequired ? '需要組裝' : '免組裝' }}
                    </li>
                    <li v-if="selectedVariant" class="mb-2">
                      <i class="bi bi-rulers text-primary me-2"></i>
                      <strong>尺寸:</strong>
                      {{ selectedVariant.fLength }}L x {{ selectedVariant.fWidth }}W x {{ selectedVariant.fHeight }}H cm
                    </li>
                    <li v-if="selectedVariant?.fWeight" class="mb-2">
                      <i class="bi bi-box text-primary me-2"></i>
                      <strong>重量:</strong> {{ selectedVariant.fWeight }} kg
                    </li>
                    <li class="mb-2">
                      <i class="bi bi-calendar text-primary me-2"></i>
                      <strong>上架日期:</strong> {{ formatDate(product.fCreateTime) }}
                    </li>
                  </ul>
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- 相似產品 -->
        <div v-if="similarProducts.length > 0" class="similar-products mt-5">
          <h3 class="mb-4">相似產品推薦</h3>
          <div class="row g-4">
            <div
              v-for="similar in similarProducts"
              :key="similar.fProductId"
              class="col-lg-3 col-md-4 col-sm-6"
            >
              <div class="card h-100 shadow-sm product-card" @click="goToProduct(similar.fProductId)">
                <img
                  :src="similar.mainImageUrl || '/placeholder.jpg'"
                  class="card-img-top"
                  :alt="similar.fName"
                  style="height: 200px; object-fit: cover; cursor: pointer;"
                  @error="handleImageError"
                />
                <div class="card-body">
                  <h6 class="card-title">{{ similar.fName }}</h6>
                  <div class="text-primary fw-bold">
                    ${{ formatPrice(similar.minPrice) }}
                    <span v-if="similar.minPrice !== similar.maxPrice">
                      - ${{ formatPrice(similar.maxPrice) }}
                    </span>
                  </div>
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
import { ref, reactive, computed, onMounted, watch } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import ProductAPI from '../api/Product.js'

const router = useRouter()
const route = useRoute()

// 狀態
const loading = ref(false)
const error = ref(null)
const product = ref(null)
const similarProducts = ref([])
const selectedImage = ref('')
const selectedVariant = ref(null)
const selectedOptions = reactive({})
const quantity = ref(1)
const isInWishlist = ref(false)
const loadingPrice = ref(false)
const customPrice = ref(null)

// 計算屬性
const maxQuantity = computed(() => {
  if (selectedVariant.value) {
    return selectedVariant.value.fStock || 0
  }
  return product.value?.totalStock || 0
})

const allOptionsSelected = computed(() => {
  if (!product.value?.isCustomizable || !product.value?.customizationParts) {
    return false
  }
  
  return product.value.customizationParts.every(part => {
    return selectedOptions[part.fPartCode] !== undefined
  })
})

const canAddToCart = computed(() => {
  if (!product.value?.isAvailable) return false
  
  if (product.value.isCustomizable) {
    return allOptionsSelected.value && customPrice.value?.isAvailable
  }
  
  if (product.value.variants && product.value.variants.length > 1) {
    return selectedVariant.value && selectedVariant.value.fStock > 0
  }
  
  return product.value.totalStock > 0
})

// 初始化
onMounted(async () => {
  await loadProduct()
  await loadSimilarProducts()
})

// 監聽路由變化
watch(() => route.params.id, async (newId) => {
  if (newId) {
    await loadProduct()
    await loadSimilarProducts()
  }
})

// 載入產品詳情
async function loadProduct() {
  const productId = parseInt(route.params.id)
  
  if (!productId) {
    error.value = '無效的產品 ID'
    return
  }

  loading.value = true
  error.value = null
  
  try {
    const response = await ProductAPI.getProductById(productId, true)
    
    if (response.success) {
      product.value = response.data
      
      // 設定預設圖片
      if (product.value.assets && product.value.assets.length > 0) {
        const primaryAsset = product.value.assets.find(a => a.fIsPrimary)
        selectedImage.value = primaryAsset?.fUrl || product.value.assets[0].fUrl
      } else {
        selectedImage.value = product.value.mainImageUrl || '/placeholder.jpg'
      }
      
      // 如果是客製化產品，初始化選項
      if (product.value.isCustomizable && product.value.customizationParts) {
        product.value.customizationParts.forEach(part => {
          const defaultOption = part.colorOptions.find(opt => opt.fIsDefault)
          if (defaultOption) {
            selectedOptions[part.fPartCode] = defaultOption.fColorOptionId
          }
        })
      }
      
      // 如果只有一個變體，自動選擇
      if (product.value.variants && product.value.variants.length === 1) {
        selectedVariant.value = product.value.variants[0]
      }
    } else {
      error.value = response.message || '載入產品失敗'
    }
  } catch (err) {
    console.error('載入產品失敗:', err)
    error.value = err.message || '載入產品時發生錯誤'
  } finally {
    loading.value = false
  }
}

// 載入相似產品
async function loadSimilarProducts() {
  const productId = parseInt(route.params.id)
  
  try {
    const response = await ProductAPI.getSimilarProducts(productId, 4)
    
    if (response.success) {
      similarProducts.value = response.data || []
    }
  } catch (err) {
    console.error('載入相似產品失敗:', err)
  }
}

// 選擇圖片
function selectImage(url) {
  selectedImage.value = url
}

// 選擇變體
function selectVariant(variant) {
  selectedVariant.value = variant
}

// 選擇客製化選項
function selectColorOption(partCode, optionId) {
  selectedOptions[partCode] = optionId
  customPrice.value = null // 清除之前的價格查詢結果
}

// 查詢客製化價格
async function fetchPriceByCustomization() {
  if (!allOptionsSelected.value) {
    return
  }

  loadingPrice.value = true
  
  try {
    const response = await ProductAPI.getPriceByCustomization(
      product.value.fProductId,
      selectedOptions
    )
    
    if (response.success) {
      customPrice.value = response.data
    } else {
      alert(response.message || '查詢價格失敗')
    }
  } catch (err) {
    console.error('查詢價格失敗:', err)
    alert('查詢價格時發生錯誤')
  } finally {
    loadingPrice.value = false
  }
}

// 數量控制
function increaseQuantity() {
  if (quantity.value < maxQuantity.value) {
    quantity.value++
  }
}

function decreaseQuantity() {
  if (quantity.value > 1) {
    quantity.value--
  }
}

// 加入購物車
function addToCart() {
  if (!canAddToCart.value) return
  
  let variantId
  
  if (product.value.isCustomizable && customPrice.value) {
    variantId = customPrice.value.fProductVariantId
  } else if (selectedVariant.value) {
    variantId = selectedVariant.value.fProductVariantId
  } else if (product.value.variants && product.value.variants.length === 1) {
    variantId = product.value.variants[0].fProductVariantId
  }
  
  if (!variantId) {
    alert('請選擇產品規格')
    return
  }
  
  // TODO: 實作加入購物車邏輯
  const cartItem = {
    variantId,
    productName: product.value.fName,
    quantity: quantity.value,
    price: customPrice.value?.fPrice || selectedVariant.value?.fPrice,
    image: selectedImage.value
  }
  
  console.log('加入購物車:', cartItem)
  alert(`已加入 ${quantity.value} 件到購物車`)
}

// 切換收藏
function toggleWishlist() {
  isInWishlist.value = !isInWishlist.value
  // TODO: 實作收藏邏輯
  alert(isInWishlist.value ? '已加入收藏' : '已取消收藏')
}

// 前往產品
function goToProduct(productId) {
  router.push({ name: 'ProductDetail', params: { id: productId } })
  window.scrollTo({ top: 0, behavior: 'smooth' })
}

// 格式化價格
function formatPrice(price) {
  if (!price) return '0'
  return new Intl.NumberFormat('zh-TW').format(price)
}

// 格式化日期
function formatDate(dateString) {
  if (!dateString) return '-'
  return new Date(dateString).toLocaleDateString('zh-TW')
}

// 圖片載入失敗處理
function handleImageError(event) {
  event.target.src = '/placeholder.jpg'
}
</script>

<style scoped>
.product-detail-page {
  min-height: 100vh;
  background-color: #f8f9fa;
}

.main-image-container {
  position: relative;
  background-color: #fff;
  border-radius: 0.5rem;
  overflow: hidden;
}

.main-image-container img {
  width: 100%;
  height: auto;
  max-height: 500px;
  object-fit: contain;
}

.thumbnail-item {
  border: 2px solid transparent;
  border-radius: 0.375rem;
  overflow: hidden;
  transition: all 0.2s;
}

.thumbnail-item:hover {
  border-color: #0d6efd;
  transform: scale(1.05);
}

.thumbnail-item.active {
  border-color: #0d6efd;
}

.color-preview {
  width: 50px;
  height: 50px;
  border-radius: 0.375rem;
  border: 2px solid #dee2e6;
}

.color-option-card {
  transition: all 0.2s;
}

.color-option-card:hover {
  box-shadow: 0 0.125rem 0.25rem rgba(0, 0, 0, 0.075);
}

.variant-card {
  transition: all 0.2s;
}

.variant-card:hover {
  box-shadow: 0 0.125rem 0.25rem rgba(0, 0, 0, 0.075);
}

.product-card {
  transition: transform 0.2s, box-shadow 0.2s;
  cursor: pointer;
}

.product-card:hover {
  transform: translateY(-5px);
  box-shadow: 0 0.5rem 1rem rgba(0, 0, 0, 0.15) !important;
}

.breadcrumb-item a {
  text-decoration: none;
  color: #0d6efd;
}

.breadcrumb-item a:hover {
  text-decoration: underline;
}

.action-buttons .btn {
  min-height: 48px;
}

@media (max-width: 991px) {
  .main-image-container img {
    max-height: 400px;
  }
}

@media (max-width: 767px) {
  .main-image-container img {
    max-height: 300px;
  }
  
  .thumbnail-item img {
    width: 60px !important;
    height: 60px !important;
  }
}
</style>