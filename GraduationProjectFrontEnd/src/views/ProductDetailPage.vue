<!-- 完整版：支援客製化選項（透過 SKU 對應） -->
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

          <!-- 3D 模型視窗 -->
          <div v-show="viewMode === '3d' && product.f3dModelPath" class="viewer-3d-wrapper">
             <Furniture3DViewer 
                v-if="product.f3dModelPath"
                :model-url="product.f3dModelPath"
                :textures="modelTextures"
                :env-map-url="product.envMapUrl || null"
                @model-loaded="on3DModelLoaded"
                @model-error="on3DModelError"
              />
          </div>

          <!-- 沒有 3D 模型的提示 -->
          <div v-if="!product.model3dUrl && viewMode === '3d'" class="alert alert-info">
            <i class="bi bi-info-circle me-2"></i>
            此產品暫無 3D 模型
          </div>
        </div>

        <!-- 右側：產品資訊 -->
        <div class="col-lg-6">
          <h1 class="product-title mb-3">{{ product.fName }}</h1>
          
          <div class="mb-3">
            <span class="badge bg-success">{{ product.categoryName }}</span>
            <span v-if="isVariantAvailable" class="badge bg-success ms-2">有貨</span>
            <span v-else class="badge bg-secondary ms-2">缺貨</span>
          </div>

          <!-- 價格 -->
          <div class="price-section mb-4 p-3 bg-light rounded">
            <div v-if="selectedVariant">
              <div v-if="product.fDiscount > 0" class="mb-2">
                <span class="text-decoration-line-through text-muted">NT$ {{ formatPrice(selectedVariant.fPrice * (1 + product.fDiscount / 100)) }}</span>
              </div>
              <div class="text-primary fw-bold fs-2">NT$ {{ formatPrice(selectedVariant.fPrice) }}</div>
            </div>
            <div v-else class="text-primary fw-bold fs-2">
              NT$ {{ formatPrice(product.minPrice) }}<span v-if="product.minPrice !== product.maxPrice"> - {{ formatPrice(product.maxPrice) }}</span>
            </div>
          </div>

          <p v-if="product.fDescription" class="text-muted mb-4">{{ product.fDescription }}</p>
          
          <hr />

          <!-- ⭐ 客製化部位選擇 -->
          <div v-if="product.isCustomizable && customizationParts.length > 0" class="mb-4">
            <h5 class="mb-3">
              <i class="bi bi-palette me-2"></i>自訂您的產品
            </h5>
            
            <div v-for="part in customizationParts" :key="part.fPartId" class="mb-4">
              <h6 class="mb-2 fw-bold">{{ part.fPartName }}</h6>
              <div class="row g-2">
                <div 
                  v-for="option in part.colorOptions" 
                  :key="option.fColorOptionId" 
                  class="col-6 col-md-4"
                >
                  <button 
                    class="color-option-button w-100" 
                    :class="{ 
                      active: isOptionSelected(part.fPartId, option.fColorOptionId),
                      disabled: !isOptionAvailable(part, option)
                    }"
                    :disabled="!isOptionAvailable(part, option)"
                    @click="selectColorOption(part.fPartId, option.fColorOptionId)"
                  >
                    <div class="d-flex align-items-center gap-2 mb-2">
                      <!-- 顏色圓圈 -->
                      <div 
                        class="color-circle" 
                        :style="{ backgroundColor: option.fColorHex || '#ccc' }"
                      ></div>
                      <div class="flex-grow-1 text-start">
                        <div class="fw-bold small">{{ option.fOptionName }}</div>
                        <small v-if="option.fPrice" class="text-primary">
                          +NT$ {{ formatPrice(option.fPrice) }}
                        </small>
                      </div>
                    </div>
                    <!-- 縮圖 -->
                    <img 
                      v-if="option.fThumbnail" 
                      :src="getImageUrl(option.fThumbnail)" 
                      class="option-thumbnail"
                      @error="handleImageError"
                    />
                  </button>
                </div>
              </div>
            </div>
          </div>

          <!-- 簡單變體選擇（如果沒有客製化） -->
          <div v-else-if="variants.length > 0" class="mb-4">
            <h5 class="mb-3">選擇規格</h5>
            <div class="row g-2">
              <div v-for="variant in variants" :key="variant.fProductVariantId || variant.FProductVariantId" class="col-6 col-md-4">
                <button 
                  class="variant-button w-100" 
                  :class="{ 
                    active: isVariantSelected(variant),
                    disabled: getVariantStock(variant) <= 0 
                  }" 
                  :disabled="getVariantStock(variant) <= 0" 
                  @click="selectVariant(variant)"
                >
                  <!-- 如果有顏色資訊 -->
                  <div v-if="variant.colorName || variant.ColorName" class="d-flex align-items-center gap-2 mb-2">
                    <!-- 顏色圓圈 -->
                    <div 
                      v-if="variant.colorHex || variant.ColorHex"
                      class="color-circle" 
                      :style="{ backgroundColor: variant.colorHex || variant.ColorHex }"
                    ></div>
                    <div class="flex-grow-1 text-start">
                      <div class="fw-bold">{{ variant.colorName || variant.ColorName }}</div>
                    </div>
                  </div>
                  <!-- 如果沒有顏色資訊，顯示 SKU -->
                  <div v-else class="fw-bold mb-2">{{ variant.fSku || variant.FSku }}</div>
                  
                  <!-- 縮圖 -->
                  <img 
                    v-if="variant.colorThumbnail || variant.ColorThumbnail" 
                    :src="getImageUrl(variant.colorThumbnail || variant.ColorThumbnail)" 
                    class="variant-thumbnail mb-2"
                    @error="handleImageError"
                  />
                  
                  <!-- 價格和庫存 -->
                  <div class="text-primary fw-bold">NT$ {{ formatPrice(variant.fPrice || variant.FPrice) }}</div>
                  <small v-if="getVariantStock(variant) <= 0" class="text-danger">缺貨</small>
                  <small v-else class="text-muted">庫存: {{ getVariantStock(variant) }} 件</small>
                </button>
              </div>
            </div>
          </div>

          <!-- 選擇的配置摘要 -->
          <div v-if="product.isCustomizable && selectedVariant" class="alert alert-info mb-4">
            <h6 class="alert-heading mb-2">
              <i class="bi bi-check-circle me-2"></i>您的配置
            </h6>
            <ul class="list-unstyled mb-0 small">
              <li v-for="(optionId, partId) in selectedOptions" :key="partId" class="mb-1">
                <strong>{{ getPartName(partId) }}:</strong> {{ getOptionName(partId, optionId) }}
              </li>
            </ul>
          </div>

          <!-- 數量選擇 -->
          <div class="mb-4">
            <h5 class="mb-3">數量</h5>
            <div class="input-group" style="max-width: 200px">
              <button class="btn btn-outline-secondary" :disabled="quantity <= 1" @click="decreaseQuantity">
                <i class="bi bi-dash"></i>
              </button>
              <input v-model.number="quantity" type="number" class="form-control text-center" min="1" :max="maxQuantity" @input="validateQuantity" />
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
            <i class="bi bi-exclamation-triangle me-2"></i>{{ getDisabledReason() }}
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
                <div v-if="product.isCustomizable" class="col-md-6">
                  <strong>客製化：</strong> <span class="badge bg-info">支援客製化</span>
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
import { ref, computed, onMounted, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { ProductAPI } from '@/api/Product'
import Furniture3DViewer from '@/components/Furniture3DViewer.vue'

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

// 客製化相關
const customizationParts = ref([])
const selectedOptions = ref({}) // { partId: colorOptionId }

// 3D 檢視模式
const viewMode = ref('image') // 'image' 或 '3d'

const maxQuantity = computed(() => {
  if (selectedVariant.value) {
    return selectedVariant.value.fStock || selectedVariant.value.FStock || 0
  }
  return product.value?.totalStock || 0
})

const isVariantAvailable = computed(() => {
  if (product.value?.isCustomizable) {
    return selectedVariant.value && (selectedVariant.value.fStock || selectedVariant.value.FStock) > 0
  }
  return product.value?.isAvailable
})

const canAddToCart = computed(() => {
  if (!isVariantAvailable.value) return false
  
  // 客製化產品需要選擇所有部位
  if (product.value?.isCustomizable && customizationParts.value.length > 0) {
    const allPartsSelected = customizationParts.value.every(part => 
      selectedOptions.value[part.fPartId] !== undefined
    )
    if (!allPartsSelected) return false
    if (!selectedVariant.value) return false
  } else if (variants.value.length > 0 && !selectedVariant.value) {
    return false
  }
  
  return quantity.value >= 1 && quantity.value <= maxQuantity.value
})

// SKU 解析函數（對應後端 ParseSKUForCart 的邏輯）
const PRODUCT_CODE_LENGTH = 4  // 產品代碼長度
const PART_CODE_LENGTH = 3     // 部位代碼長度  
const OPTION_ID_LENGTH = 2     // 選項 ID 長度

function parseSkuToOptions(sku) {
  // 驗證輸入
  if (!sku || sku.length <= PRODUCT_CODE_LENGTH) {
    return {}
  }
  
  // 移除產品代碼部分 (前4碼)
  const optionsPart = sku.substring(PRODUCT_CODE_LENGTH)
  
  const result = {}
  let position = 0
  
  // 按照部位的 displayOrder 順序解析
  const sortedParts = [...customizationParts.value].sort((a, b) => 
    (a.fDisplayOrder || 0) - (b.fDisplayOrder || 0)
  )
  
  // 逐一解析每個部位
  for (const part of sortedParts) {
    // 檢查是否還有足夠的字元
    if (position + PART_CODE_LENGTH + OPTION_ID_LENGTH > optionsPart.length) {
      break
    }
    
    // 讀取部位代碼（3 字元）
    const partCode = optionsPart.substring(position, position + PART_CODE_LENGTH)
    position += PART_CODE_LENGTH
    
    // 讀取選項 ID（2 字元）
    const optionIdStr = optionsPart.substring(position, position + OPTION_ID_LENGTH)
    position += OPTION_ID_LENGTH
    
    // 驗證部位代碼並轉換選項 ID
    if (partCode === part.fPartCode) {
      const optionId = parseInt(optionIdStr, 10)
      
      // 驗證選項是否存在
      const colorOption = part.colorOptions?.find(o => o.fColorOptionId === optionId)
      if (colorOption) {
        result[part.fPartId] = optionId
      }
    }
  }
  
  return result
}

// 根據選擇找到對應的 variant
function matchVariantBySelection() {
  if (!product.value?.isCustomizable || variants.value.length === 0) {
    return
  }
  
  console.log('尋找對應的變體')
  console.log('  選擇:', selectedOptions.value)
  
  // 確認所有部位都已選擇
  const allPartsSelected = customizationParts.value.every(part => 
    selectedOptions.value[part.fPartId] !== undefined
  )
  
  if (!allPartsSelected) {
    console.log('  尚未選擇所有部位')
    selectedVariant.value = null
    return
  }
  
  // 找到匹配的 variant
  const matchedVariant = variants.value.find(variant => {
    const sku = variant.fSku || variant.FSku
    const skuOptions = parseSkuToOptions(sku)
    
    console.log(`  檢查 SKU: ${sku}`, skuOptions)
    
    // 檢查是否完全匹配
    return Object.entries(selectedOptions.value).every(([partId, optionId]) => {
      return skuOptions[partId] === optionId
    })
  })
  
  if (matchedVariant) {
    selectedVariant.value = matchedVariant
    console.log('找到匹配的變體:', matchedVariant.fSku || matchedVariant.FSku)
    console.log('  價格:', matchedVariant.fPrice || matchedVariant.FPrice)
    console.log('  庫存:', matchedVariant.fStock || matchedVariant.FStock)
  } else {
    selectedVariant.value = null
    console.log('沒有找到匹配的變體')
  }
}

// 選擇顏色選項
function selectColorOption(partId, optionId) {
  selectedOptions.value[partId] = optionId
  console.log('選擇:', { partId, optionId, selectedOptions: selectedOptions.value })
  matchVariantBySelection()
}

function isOptionSelected(partId, optionId) {
  return selectedOptions.value[partId] === optionId
}

function isOptionAvailable(part, option) {
  // TODO: 可以加入邏輯檢查這個選項是否有庫存
  return true
}

function getPartName(partId) {
  const part = customizationParts.value.find(p => p.fPartId == partId)
  return part?.fPartName || ''
}

function getOptionName(partId, optionId) {
  const part = customizationParts.value.find(p => p.fPartId == partId)
  const option = part?.colorOptions.find(o => o.fColorOptionId == optionId)
  return option?.fOptionName || ''
}

async function loadProductDetail() {
  const productId = route.params.id
  if (!productId) {
    error.value = '產品 ID 不正確'
    return
  }

  loading.value = true
  error.value = null

  console.log('載入產品詳情:', productId)

  try {
    // 使用 includeCustomization=true
    const response = await ProductAPI.getProductById(productId, true)
    console.log('API 回應:', response)
    
    if (response.ok && response.data) {
      const rawData = response.data
      
      // 處理圖片
      const mainImg = rawData.mainImageUrl || rawData.MainImageUrl
      const assetsList = rawData.assets || rawData.Assets
      
      if ((!mainImg || mainImg === 'null') && assetsList && assetsList.length > 0) {
        const primaryAsset = assetsList.find(a => a.fIsPrimary || a.FIsPrimary)
        rawData.mainImageUrl = primaryAsset ? (primaryAsset.fUrl || primaryAsset.FUrl) : (assetsList[0].fUrl || assetsList[0].FUrl)
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
      console.log('產品資料:', product.value)
      
      // 處理客製化資料
      if (rawData.isCustomizable && rawData.customizationParts) {
        customizationParts.value = rawData.customizationParts
        console.log('客製化部位:', customizationParts.value.length, '個')
        
        // 初始化選擇（選擇第一個或預設選項）
        customizationParts.value.forEach(part => {
          if (part.colorOptions && part.colorOptions.length > 0) {
            const defaultOption = part.colorOptions.find(opt => opt.fIsDefault) || part.colorOptions[0]
            selectedOptions.value[part.fPartId] = defaultOption.fColorOptionId
          }
        })
        console.log('預設選擇:', selectedOptions.value)
      }
      
      setupProductImages()
      await loadVariants(productId)
      
      // ⭐ 如果是客製化產品，根據預設選擇找到對應的 variant
      if (product.value.isCustomizable) {
        matchVariantBySelection()
      }
      
      await loadRelatedProducts(product.value.fCategoryId)
    } else {
      console.error('載入失敗:', response)
      error.value = response.message || '載入產品失敗'
    }
  } catch (err) {
    console.error('發生錯誤:', err)
    error.value = err.message || '載入產品時發生錯誤'
  } finally {
    loading.value = false
  }
}

function setupProductImages() {
  productImages.value = []
  
  if (product.value.mainImageUrl) {
    productImages.value.push(getImageUrl(product.value.mainImageUrl))
  }
  
  const assetsList = product.value.assets || product.value.Assets
  if (assetsList && Array.isArray(assetsList)) {
    assetsList.forEach(asset => {
      const url = asset.fUrl || asset.FUrl
      if (url && !productImages.value.includes(getImageUrl(url))) {
        productImages.value.push(getImageUrl(url))
      }
    })
  }
  
  selectedImage.value = productImages.value[0] || 'data:image/svg+xml,%3Csvg xmlns="http://www.w3.org/2000/svg" width="500" height="500"%3E%3Crect fill="%23ddd" width="500" height="500"/%3E%3Ctext fill="%23999" x="50%25" y="50%25" text-anchor="middle" dominant-baseline="middle" font-size="24"%3E無圖片%3C/text%3E%3C/svg%3E'
}

async function loadVariants(productId) {
  if (product.value.variants && Array.isArray(product.value.variants)) {
    console.log('📦 使用產品資料中的變體:', product.value.variants.length, '個')
    
    variants.value = product.value.variants
    
    // 如果不是客製化產品，選擇第一個有庫存的變體
    if (!product.value.isCustomizable) {
      const available = variants.value.find(v => (v.fStock || v.FStock) > 0)
      if (available) {
        selectVariant(available)
        console.log('已選擇變體:', available.fSku || available.FSku)
      }
    }
  }
}

async function loadRelatedProducts(categoryId) {
  if (!categoryId) return
  try {
    const response = await ProductAPI.getProducts({ categoryId, pageNumber: 1, pageSize: 8 })
    if (response.ok && response.data?.data) {
      relatedProducts.value = response.data.data
        .filter(p => p.fProductId !== product.value.fProductId)
        .slice(0, 4)
        .map(p => {
          let imageUrl = p.mainImageUrl || p.MainImageUrl
          if (!imageUrl && p.assets && p.assets.length > 0) {
            const primaryAsset = p.assets.find(a => a.fIsPrimary) || p.assets[0]
            imageUrl = primaryAsset.fUrl || primaryAsset.FUrl
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

async function addToCart() {
  if (!canAddToCart.value) return
  try {
    const response = await CartAPI.addToCart({
      fProductId: product.value.fProductId,
      fVariantId: selectedVariant.value?.fProductVariantId || selectedVariant.value?.FProductVariantId || null,
      fQuantity: quantity.value
    })
    if (response.ok) {
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
  if (!isVariantAvailable.value) return '產品暫時缺貨'
  if (product.value?.isCustomizable && customizationParts.value.length > 0) {
    const allSelected = customizationParts.value.every(part => selectedOptions.value[part.fPartId])
    if (!allSelected) return '請選擇所有部位的顏色'
    if (!selectedVariant.value) return '此組合暫時缺貨'
  }
  if (variants.value.length > 0 && !selectedVariant.value) return '請選擇規格'
  if (quantity.value < 1 || quantity.value > maxQuantity.value) return '數量不正確'
  return ''
}

function formatPrice(price) {
  return price ? Math.round(price).toLocaleString() : '0'
}

function getImageUrl(url) {
  if (!url) return 'data:image/svg+xml,%3Csvg xmlns="http://www.w3.org/2000/svg" width="500" height="500"%3E%3Crect fill="%23ccc" width="500" height="500"/%3E%3Ctext fill="%23666" x="50%25" y="50%25" text-anchor="middle" dominant-baseline="middle" font-size="24"%3E無圖片%3C/text%3E%3C/svg%3E'
  if (url.startsWith('http')) return url
  if (url.startsWith('/')) return `${import.meta.env.VITE_API_BASE_URL || 'https://localhost:7131'}${url}`
  return url
}

function handleImageError(event) {
  event.target.src = 'data:image/svg+xml,%3Csvg xmlns="http://www.w3.org/2000/svg" width="500" height="500"%3E%3Crect fill="%23e0e0e0" width="500" height="500"/%3E%3Ctext fill="%23666" x="50%25" y="50%25" text-anchor="middle" dominant-baseline="middle" font-size="20"%3E圖片載入失敗%3C/text%3E%3C/svg%3E'
}

// ⭐ 3D 模型相關函數
function get3DTextures() {
  if (!product.value || !selectedOptions.value) {
    return {}
  }

  // 根據使用者選擇的顏色選項，回傳對應的 PBR 貼圖
  const textures = {
    baseColor: null,
    normal: null,
    roughness: null,
    metalness: null,
    ao: null
  }

  // 如果產品有客製化選項，根據選擇組合貼圖
  if (product.value.isCustomizable && customizationParts.value.length > 0) {
    customizationParts.value.forEach(part => {
      const selectedOptionId = selectedOptions.value[part.fPartId]
      if (selectedOptionId) {
        const option = part.colorOptions.find(o => o.fColorOptionId === selectedOptionId)
        if (option && option.textures) {
          // 假設後端提供了每個顏色選項的貼圖 URL
          // 這裡可以根據實際的資料結構調整
          if (option.textures.baseColor) textures.baseColor = getImageUrl(option.textures.baseColor)
          if (option.textures.normal) textures.normal = getImageUrl(option.textures.normal)
          if (option.textures.roughness) textures.roughness = getImageUrl(option.textures.roughness)
          if (option.textures.metalness) textures.metalness = getImageUrl(option.textures.metalness)
          if (option.textures.ao) textures.ao = getImageUrl(option.textures.ao)
        }
      }
    })
  } else if (product.value.textures) {
    // 如果是簡單產品，直接使用產品的貼圖
    textures.baseColor = product.value.textures.baseColor ? getImageUrl(product.value.textures.baseColor) : null
    textures.normal = product.value.textures.normal ? getImageUrl(product.value.textures.normal) : null
    textures.roughness = product.value.textures.roughness ? getImageUrl(product.value.textures.roughness) : null
    textures.metalness = product.value.textures.metalness ? getImageUrl(product.value.textures.metalness) : null
    textures.ao = product.value.textures.ao ? getImageUrl(product.value.textures.ao) : null
  }

  return textures
}

function on3DModelLoaded(model) {
  console.log('✅ 3D 模型載入成功', model)
}

function on3DModelError(err) {
  console.error('❌ 3D 模型載入失敗', err)
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

.color-option-button {
  padding: 0.75rem;
  border: 2px solid #dee2e6;
  border-radius: 8px;
  background-color: #fff;
  cursor: pointer;
  transition: all 0.3s;
  text-align: left;
}

.color-option-button:hover:not(.disabled) {
  border-color: #0d6efd;
  transform: translateY(-2px);
  box-shadow: 0 4px 8px rgba(0,0,0,0.1);
}

.color-option-button.active {
  border-color: #0d6efd;
  background-color: #e7f1ff;
}

.color-option-button.disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.color-circle {
  width: 32px;
  height: 32px;
  border-radius: 50%;
  border: 2px solid #dee2e6;
  flex-shrink: 0;
}

.option-thumbnail {
  width: 100%;
  height: 60px;
  object-fit: cover;
  border-radius: 4px;
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

.variant-thumbnail {
  width: 100%;
  height: 80px;
  object-fit: cover;
  border-radius: 4px;
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

/* ⭐ 3D Viewer 樣式 */
.viewer-3d-wrapper {
  position: relative;
  width: 100%;
  height: 600px;
  border-radius: 8px;
  overflow: hidden;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
}

.btn-group .btn {
  min-width: 100px;
}

@media (max-width: 991px) {
  .product-title {
    font-size: 1.5rem;
  }
  
  .viewer-3d-wrapper {
    height: 400px;
  }
}
</style>