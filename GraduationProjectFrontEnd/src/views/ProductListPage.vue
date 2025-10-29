<!-- 產品列表頁 - 對應 7 個資料表後端 API (無客製化功能) -->
<template>
  <div class="product-list-page">
    <div class="container py-4">
      <!-- 搜尋列 -->
      <div class="row mb-4">
        <div class="col-12">
          <div class="card shadow-sm">
            <div class="card-body">
              <div class="input-group">
                <input
                  v-model="searchKeyword"
                  type="text"
                  class="form-control"
                  placeholder="搜尋產品名稱..."
                  @keyup.enter="handleSearch"
                />
                <button class="btn btn-primary" type="button" @click="handleSearch">
                  <i class="bi bi-search me-1"></i>搜尋
                </button>
              </div>
            </div>
          </div>
        </div>
      </div>

      <div class="row">
        <!-- 側邊篩選欄 -->
        <div class="col-lg-3 col-md-4 mb-4">
          <div class="card shadow-sm sticky-top" style="top: 20px;">
            <div class="card-header bg-primary text-white">
              <h5 class="mb-0">
                <i class="bi bi-funnel me-2"></i>篩選條件
              </h5>
            </div>
            <div class="card-body">
              <!-- 類別篩選 -->
              <!-- <div class="mb-4">
                <h6 class="fw-bold mb-3">
                  <i class="bi bi-grid me-2"></i>產品類別
                </h6>
                <div v-if="filterOptions.categories.length > 0">
                  <div
                    v-for="category in filterOptions.categories"
                    :key="category.categoryId"
                    class="form-check mb-2"
                  >
                    <input
                      :id="`category-${category.categoryId}`"
                      :checked="localFilters.categoryId === category.categoryId"
                      class="form-check-input"
                      type="radio"
                      name="category"
                      @change="selectCategory(category.categoryId)"
                    />
                    <label
                      class="form-check-label"
                      :for="`category-${category.categoryId}`"
                    >
                      {{ category.categoryName }}
                    </label>
                  </div>
                  <button
                    v-if="localFilters.categoryId"
                    class="btn btn-sm btn-outline-secondary mt-2 w-100"
                    @click="selectCategory(null)"
                  >
                    <i class="bi bi-x me-1"></i>清除類別
                  </button>
                </div>
                <div v-else class="text-muted small">
                  <div class="spinner-border spinner-border-sm me-2"></div>
                  載入中...
                </div>
              </div> -->

              <!-- <hr> -->

              <!-- 顏色篩選 -->
              <div v-if="filterOptions.colors && filterOptions.colors.length > 0" class="mb-4">
                <h6 class="fw-bold mb-3">
                  <i class="bi bi-palette me-2"></i>顏色
                </h6>
                <div class="d-flex flex-wrap gap-2">
                  <button
                    v-for="color in filterOptions.colors"
                    :key="color.colorId"
                    class="color-filter-btn"
                    :class="{ active: isColorSelected(color.colorId) }"
                    :style="{ 
                      backgroundColor: color.colorCode || '#ccc',
                      border: isColorSelected(color.colorId) ? '3px solid #0d6efd' : '2px solid #dee2e6'
                    }"
                    :title="color.colorName"
                    @click="toggleColor(color.colorId)"
                  ></button>
                </div>
                <button
                  v-if="localFilters.colorIds && localFilters.colorIds.length > 0"
                  class="btn btn-sm btn-outline-secondary mt-2 w-100"
                  @click="clearColors"
                >
                  <i class="bi bi-x me-1"></i>清除顏色
                </button>
              </div>

              <hr v-if="filterOptions.colors && filterOptions.colors.length > 0">

              <!-- 材質篩選 -->
              <div v-if="filterOptions.textures && filterOptions.textures.length > 0" class="mb-4">
                <h6 class="fw-bold mb-3">
                  <i class="bi bi-layers me-2"></i>材質
                </h6>
                <div class="d-flex flex-wrap gap-2">
                  <button
                    v-for="texture in filterOptions.textures"
                    :key="texture.textureId"
                    class="btn btn-sm"
                    :class="isTextureSelected(texture.textureId) ? 'btn-primary' : 'btn-outline-secondary'"
                    @click="toggleTexture(texture.textureId)"
                  >
                    {{ texture.textureName }}
                  </button>
                </div>
                <button
                  v-if="localFilters.textureIds && localFilters.textureIds.length > 0"
                  class="btn btn-sm btn-outline-secondary mt-2 w-100"
                  @click="clearTextures"
                >
                  <i class="bi bi-x me-1"></i>清除材質
                </button>
              </div>

              <hr v-if="filterOptions.textures && filterOptions.textures.length > 0">

              <!-- 價格篩選 -->
              <div class="mb-4">
                <h6 class="fw-bold mb-3">
                  <i class="bi bi-currency-dollar me-2"></i>價格範圍
                </h6>
                <div class="row g-2">
                  <div class="col-6">
                    <input
                      v-model.number="priceInput.min"
                      type="number"
                      class="form-control form-control-sm"
                      placeholder="最低價"
                      min="0"
                    />
                  </div>
                  <div class="col-6">
                    <input
                      v-model.number="priceInput.max"
                      type="number"
                      class="form-control form-control-sm"
                      placeholder="最高價"
                      min="0"
                    />
                  </div>
                </div>
                <button 
                  class="btn btn-sm btn-primary w-100 mt-2"
                  @click="applyPriceFilter"
                >
                  <i class="bi bi-check me-1"></i>套用價格
                </button>
                <div v-if="filterOptions.priceRange" class="text-muted small mt-2">
                  範圍: NT$ {{ formatPrice(filterOptions.priceRange.minPrice) }} - 
                  NT$ {{ formatPrice(filterOptions.priceRange.maxPrice) }}
                </div>
              </div>

              <hr>

              <!-- 清除所有篩選 -->
              <button class="btn btn-outline-danger w-100" @click="clearAllFilters">
                <i class="bi bi-x-circle me-1"></i>清除所有篩選
              </button>

              <!-- 當前篩選摘要 -->
              <div v-if="hasActiveFilters" class="mt-3 p-2 bg-light rounded">
                <small class="text-muted d-block mb-2">
                  <strong>已套用篩選：</strong>
                </small>
                <div class="d-flex flex-wrap gap-1">
                  <span v-if="localFilters.categoryId" class="badge bg-secondary">
                    類別
                  </span>
                  <span v-if="localFilters.colorIds?.length" class="badge bg-secondary">
                    {{ localFilters.colorIds.length }} 個顏色
                  </span>
                  <span v-if="localFilters.textureIds?.length" class="badge bg-secondary">
                    {{ localFilters.textureIds.length }} 個材質
                  </span>
                  <span v-if="localFilters.minPrice || localFilters.maxPrice" class="badge bg-secondary">
                    價格
                  </span>
                  <span v-if="localFilters.keyword" class="badge bg-secondary">
                    搜尋
                  </span>
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- 產品列表區 -->
        <div class="col-lg-9 col-md-8">
          <!-- 工具列 -->
          <div class="d-flex flex-wrap justify-content-between align-items-center mb-3 gap-2">
            <div class="text-muted">
              <span v-if="!loading && pagination">
                共 <strong>{{ pagination.totalCount }}</strong> 項產品
                <span class="d-none d-md-inline">
                  (第 {{ pagination.currentPage }} / {{ pagination.totalPages }} 頁)
                </span>
              </span>
            </div>
            <div class="d-flex align-items-center gap-2">
              <label class="mb-0 me-2 d-none d-md-inline">排序:</label>
              <select
                v-model="localFilters.sortBy"
                class="form-select form-select-sm"
                style="width: auto; min-width: 150px;"
                @change="handleSortChange"
              >
                <option value="newest">最新上架</option>
                <option value="priceasc">價格低到高</option>
                <option value="pricedesc">價格高到低</option>
                <option value="name">名稱排序</option>
              </select>
            </div>
          </div>

          <!-- 載入中 -->
          <div v-if="loading" class="text-center py-5">
            <div class="spinner-border text-primary" role="status">
              <span class="visually-hidden">載入中...</span>
            </div>
            <p class="mt-3 text-muted">載入產品資料中...</p>
          </div>

          <!-- 錯誤訊息 -->
          <div v-else-if="error" class="alert alert-danger" role="alert">
            <i class="bi bi-exclamation-triangle me-2"></i>{{ error }}
            <button class="btn btn-sm btn-outline-danger ms-3" @click="loadProducts">
              <i class="bi bi-arrow-clockwise me-1"></i>重試
            </button>
          </div>

          <!-- 無產品 -->
          <div
            v-else-if="products.length === 0"
            class="text-center py-5"
          >
            <i class="bi bi-inbox display-1 text-muted"></i>
            <p class="mt-3 text-muted fs-5">目前沒有符合條件的產品</p>
            <p class="text-muted">試試調整篩選條件或清除所有篩選</p>
            <button class="btn btn-primary mt-2" @click="clearAllFilters">
              <i class="bi bi-arrow-clockwise me-2"></i>清除篩選條件
            </button>
          </div>

          <!-- 產品網格 -->
          <div v-else>
            <div class="row g-4">
              <div
                v-for="product in products"
                :key="product.fProductId"
                class="col-xl-4 col-lg-6 col-md-6 col-sm-6"
              >
                <div class="card h-100 shadow-sm product-card" @click="goToDetail(product.fProductId)">
                  <!-- 產品圖片 -->
                  <div class="position-relative image-container">
                    <img
                      :src="getProductImageUrl(product)"
                      class="card-img-top"
                      :alt="product.fName"
                      loading="lazy"
                     
                    />
                    <!-- 標籤 -->
                    <div class="position-absolute top-0 start-0 p-2">
                      <span
                        v-if="!product.isAvailable"
                        class="badge bg-danger"
                      >
                        <i class="bi bi-x-circle me-1"></i>售完
                      </span>
                      <span
                        v-else-if="product.fDiscount && product.fDiscount > 0"
                        class="badge bg-warning text-dark"
                      >
                        <i class="bi bi-tag-fill me-1"></i>
                        {{ Math.round(product.fDiscount * 100) }}% OFF
                      </span>
                    </div>
                  </div>

                  <!-- 產品資訊 -->
                  <div class="card-body d-flex flex-column">
                    <!-- 類別標籤 -->
                    <div class="mb-2">
                      <span class="badge bg-secondary">{{ product.categoryName }}</span>
                      <span v-if="product.isAvailable" class="badge bg-success ms-1">
                        <!-- <i class="bi bi-check-circle"></i> -->
                      </span>
                    </div>
                    
                    <!-- 產品名稱 -->
                    <h5 class="card-title mb-2">{{ product.fName }}</h5>
                    
                    <!-- 產品描述 -->
                    <!-- <p v-if="product.fDescription" class="card-text text-muted small mb-3">
                      {{ truncateText(product.fDescription, 60) }}
                    </p> -->

                    <!-- 價格與庫存 -->
                    <div class="mt-auto">
                      <!-- 價格 -->
                      <div class="mb-2">
                        <div v-if="product.fDiscount && product.fDiscount > 0" class="mb-1">
                          <span class="text-decoration-line-through text-muted small">
                            NT$ {{ formatPrice(calculateOriginalPrice(product.minPrice, product.fDiscount)) }}
                          </span>
                        </div>
                        <div class="d-flex justify-content-between align-items-center">
                          <div>
                            <span
                              v-if="product.minPrice === product.maxPrice"
                              class="h5 text-primary mb-0"
                            >
                              NT$ {{ formatPrice(product.minPrice) }}
                            </span>
                            <span v-else class="h5 text-primary mb-0">
                              NT$ {{ formatPrice(product.minPrice) }} - 
                              {{ formatPrice(product.maxPrice) }}
                            </span>
                          </div>
                          <span class="text-muted small">
                            <i class="bi bi-box-seam me-1"></i>
                            {{ product.totalStock || 0 }}
                          </span>
                        </div>
                      </div>

                      <!-- 額外資訊 -->
                      <div class="d-flex justify-content-between text-muted small mb-3">
                        <span v-if="product.fWarrantyMonth">
                          <i class="bi bi-shield-check me-1"></i>
                          {{ product.fWarrantyMonth }}個月保固
                        </span>
                        <span v-if="product.fAssemblyRequired !== undefined">
                          <i class="bi bi-tools me-1"></i>
                          {{ product.fAssemblyRequired ? '需組裝' : '免組裝' }}
                        </span>
                      </div>

                      <!-- 按鈕 -->
                      <button
                        class="btn btn-primary w-100"
                        :disabled="!product.isAvailable"
                        @click.stop="goToDetail(product.fProductId)"
                      >
                        <i class="bi bi-eye me-1"></i>查看詳情
                      </button>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>

          <!-- 分頁 -->
          <nav v-if="pagination && pagination.totalPages > 1" class="mt-4" aria-label="產品分頁">
            <ul class="pagination justify-content-center flex-wrap">
              <!-- 上一頁 -->
              <li class="page-item" :class="{ disabled: pagination.currentPage === 1 }">
                <a
                  class="page-link"
                  href="#"
                  aria-label="上一頁"
                  @click.prevent="goToPage(pagination.currentPage - 1)"
                >
                  <i class="bi bi-chevron-left"></i>
                  <span class="d-none d-md-inline ms-1">上一頁</span>
                </a>
              </li>

              <!-- 第一頁 -->
              <li v-if="displayPages[0] > 1" class="page-item">
                <a class="page-link" href="#" @click.prevent="goToPage(1)">1</a>
              </li>
              <li v-if="displayPages[0] > 2" class="page-item disabled">
                <span class="page-link">...</span>
              </li>

              <!-- 頁碼 -->
              <li
                v-for="page in displayPages"
                :key="page"
                class="page-item"
                :class="{ active: page === pagination.currentPage }"
              >
                <a
                  class="page-link"
                  href="#"
                  @click.prevent="goToPage(page)"
                >
                  {{ page }}
                </a>
              </li>

              <!-- 最後一頁 -->
              <li v-if="displayPages[displayPages.length - 1] < pagination.totalPages - 1" class="page-item disabled">
                <span class="page-link">...</span>
              </li>
              <li v-if="displayPages[displayPages.length - 1] < pagination.totalPages" class="page-item">
                <a class="page-link" href="#" @click.prevent="goToPage(pagination.totalPages)">
                  {{ pagination.totalPages }}
                </a>
              </li>

              <!-- 下一頁 -->
              <li class="page-item" :class="{ disabled: pagination.currentPage === pagination.totalPages }">
                <a
                  class="page-link"
                  href="#"
                  aria-label="下一頁"
                  @click.prevent="goToPage(pagination.currentPage + 1)"
                >
                  <span class="d-none d-md-inline me-1">下一頁</span>
                  <i class="bi bi-chevron-right"></i>
                </a>
              </li>
            </ul>
          </nav>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive, computed, onMounted, watch } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import ProductAPI from '@/api/Product'

const router = useRouter()
const route = useRoute()

// 狀態
const loading = ref(false)
const error = ref(null)
const products = ref([])
const pagination = ref(null)
const searchKeyword = ref('')

// 本地篩選條件
const localFilters = reactive({
  categoryId: null,
  colorIds: [],
  textureIds: [],
  minPrice: null,
  maxPrice: null,
  searchKeyword: '',
  sortBy: 'newest',
  pageNumber: 1,
  pageSize: 12
})

// 價格輸入框
const priceInput = reactive({
  min: null,
  max: null
})

// 篩選選項
const filterOptions = reactive({
  categories: [],
  colors: [],
  textures: [],
  priceRange: null
})

// 計算屬性
const displayPages = computed(() => {
  if (!pagination.value) return []
  
  const current = pagination.value.currentPage
  const total = pagination.value.totalPages
  const pages = []
  
  let start = Math.max(1, current - 2)
  let end = Math.min(total, current + 2)
  
  if (current <= 3) {
    end = Math.min(5, total)
  }
  if (current >= total - 2) {
    start = Math.max(1, total - 4)
  }
  
  for (let i = start; i <= end; i++) {
    pages.push(i)
  }
  
  return pages
})

const hasActiveFilters = computed(() => {
  return localFilters.categoryId || 
         localFilters.colorIds?.length > 0 ||
         localFilters.textureIds?.length > 0 ||
         localFilters.minPrice || 
         localFilters.maxPrice ||
         localFilters.searchKeyword
})

// 監聽路由變化
watch(() => route.query, () => {
  syncFiltersFromURL()
  loadProducts()
}, { deep: true })

// 初始化
onMounted(async () => {
  await loadFilterOptions()
  syncFiltersFromURL()
  await loadProducts()
})

// 從 URL 同步篩選條件
function syncFiltersFromURL() {
  if (route.query.keyword) {
    searchKeyword.value = route.query.keyword
    localFilters.searchKeyword = route.query.keyword
  }
  if (route.query.categoryId) {
    localFilters.categoryId = parseInt(route.query.categoryId)
  }
  if (route.query.colorIds) {
    localFilters.colorIds = route.query.colorIds.split(',').map(id => parseInt(id))
  }
  if (route.query.textureIds) {
    localFilters.textureIds = route.query.textureIds.split(',').map(id => parseInt(id))
  }
  if (route.query.minPrice) {
    const minPrice = parseFloat(route.query.minPrice)
    localFilters.minPrice = minPrice
    priceInput.min = minPrice
  }
  if (route.query.maxPrice) {
    const maxPrice = parseFloat(route.query.maxPrice)
    localFilters.maxPrice = maxPrice
    priceInput.max = maxPrice
  }
  if (route.query.sortBy) {
    localFilters.sortBy = route.query.sortBy
  }
  if (route.query.page) {
    localFilters.pageNumber = parseInt(route.query.page)
  }
}

// 載入篩選選項
async function loadFilterOptions() {
  try {
    const response = await ProductAPI.getFilterOptions()
    if (response.ok && response.data) {
      filterOptions.categories = response.data.categories || []
      filterOptions.colors = response.data.colors || []
      filterOptions.textures = response.data.textures || []
      filterOptions.priceRange = response.data.priceRange || { minPrice: 0, maxPrice: 100000 }
    }
  } catch (err) {
    console.error('載入篩選選項失敗:', err)
  }
}

// 載入產品列表
async function loadProducts() {
  if (loading.value) return
  
  loading.value = true
  error.value = null
  
  try {
    const response = await ProductAPI.getProducts({
      categoryId: localFilters.categoryId,
      colorIds: localFilters.colorIds?.length > 0 ? localFilters.colorIds : undefined,
      textureIds: localFilters.textureIds?.length > 0 ? localFilters.textureIds : undefined,
      minPrice: localFilters.minPrice,
      maxPrice: localFilters.maxPrice,
      searchKeyword: localFilters.searchKeyword,
      sortBy: localFilters.sortBy,
      pageNumber: localFilters.pageNumber,
      pageSize: localFilters.pageSize
    })
    
    if (response.success) {
      products.value = response.data || []
      pagination.value = response.pagination || {
        totalCount: 0,
        pageSize: localFilters.pageSize,
        currentPage: localFilters.pageNumber,
        totalPages: 0
      }
    } else {
      error.value = response.message || '載入產品失敗'
    }
  } catch (err) {
    console.error('載入產品失敗:', err)
    error.value = '載入產品時發生錯誤，請稍後再試'
  } finally {
    loading.value = false
  }
}

// 更新篩選並重新載入
async function updateAndReload() {
  localFilters.pageNumber = 1
  
  const query = {}
  if (localFilters.searchKeyword) query.keyword = localFilters.searchKeyword
  if (localFilters.categoryId) query.categoryId = localFilters.categoryId
  if (localFilters.colorIds?.length > 0) query.colorIds = localFilters.colorIds.join(',')
  if (localFilters.textureIds?.length > 0) query.textureIds = localFilters.textureIds.join(',')
  if (localFilters.minPrice) query.minPrice = localFilters.minPrice
  if (localFilters.maxPrice) query.maxPrice = localFilters.maxPrice
  if (localFilters.sortBy && localFilters.sortBy !== 'newest') {
    query.sortBy = localFilters.sortBy
  }
  
  await router.replace({ query })
  await loadProducts()
}

// 選擇類別
function selectCategory(categoryId) {
  localFilters.categoryId = categoryId
  updateAndReload()
}

// 顏色篩選
function isColorSelected(colorId) {
  return localFilters.colorIds?.includes(colorId)
}

function toggleColor(colorId) {
  if (!localFilters.colorIds) {
    localFilters.colorIds = []
  }
  
  const index = localFilters.colorIds.indexOf(colorId)
  if (index > -1) {
    localFilters.colorIds.splice(index, 1)
  } else {
    localFilters.colorIds.push(colorId)
  }
  updateAndReload()
}

function clearColors() {
  localFilters.colorIds = []
  updateAndReload()
}

// 材質篩選
function isTextureSelected(textureId) {
  return localFilters.textureIds?.includes(textureId)
}

function toggleTexture(textureId) {
  if (!localFilters.textureIds) {
    localFilters.textureIds = []
  }
  
  const index = localFilters.textureIds.indexOf(textureId)
  if (index > -1) {
    localFilters.textureIds.splice(index, 1)
  } else {
    localFilters.textureIds.push(textureId)
  }
  updateAndReload()
}

function clearTextures() {
  localFilters.textureIds = []
  updateAndReload()
}

// 套用價格篩選
function applyPriceFilter() {
  localFilters.minPrice = priceInput.min || null
  localFilters.maxPrice = priceInput.max || null
  updateAndReload()
}

// 處理排序變更
function handleSortChange() {
  updateAndReload()
}

// 搜尋
function handleSearch() {
  if (!searchKeyword.value.trim()) {
    return
  }
  localFilters.searchKeyword = searchKeyword.value.trim()
  updateAndReload()
}

// ⭐ 前往指定頁面（改為直接跳轉）
async function goToPage(page) {
  if (page < 1 || page > pagination.value.totalPages) return
  
  localFilters.pageNumber = page
  
  const query = { ...route.query, page }
  if (page === 1) delete query.page
  
  await router.replace({ query })
  await loadProducts()
  
  // 直接跳轉到頂部（無動畫）
  window.scrollTo(0, 0)
}

// 清除所有篩選
async function clearAllFilters() {
  localFilters.categoryId = null
  localFilters.colorIds = []
  localFilters.textureIds = []
  localFilters.minPrice = null
  localFilters.maxPrice = null
  localFilters.searchKeyword = ''
  localFilters.sortBy = 'newest'
  localFilters.pageNumber = 1
  
  searchKeyword.value = ''
  priceInput.min = null
  priceInput.max = null
  
  await router.replace({ query: {} })
  await loadProducts()
}

// 前往產品詳情
function goToDetail(productId) {
  router.push(`/products/${productId}`)
}

// 格式化價格
function formatPrice(price) {
  if (price === null || price === undefined) return '0'
  return Math.round(price).toLocaleString()
}

// 計算原價
function calculateOriginalPrice(discountedPrice, discount) {
  if (!discount) return discountedPrice
  return discountedPrice / (1 - discount)
}

// 截斷文字
function truncateText(text, maxLength) {
  if (!text) return ''
  if (text.length <= maxLength) return text
  return text.substring(0, maxLength) + '...'
}

// 取得產品圖片 URL（穩定的，不會閃爍）
function getProductImageUrl(product) {
  // 如果產品已經有錯誤標記，直接返回預設圖
  if (product._imageError) {
    return '/ProductImages/default.png'
  }
  
  // 如果有主圖 URL
  if (product.mainImageUrl) {
    const url = product.mainImageUrl
    
    //  如果已經是完整 URL，直接返回
    if (url.startsWith('http://') || url.startsWith('https://')) {
      return url
    }
    
    // 如果是相對路徑，加上 API Base
    if (url.startsWith('/')) {
      const API_BASE = import.meta.env.VITE_API_BASE_URL || 'https://localhost:7131'
      return `${API_BASE}${url}`
    }
    
    return url
  }
  
  // 預設圖片
  return '/ProductImages/default.png'
}
</script>

<style scoped>
.product-list-page {
  min-height: 100vh;
  background-color: #f8f9fa;
}

.product-card {
  transition: transform 0.3s ease, box-shadow 0.3s ease;
  cursor: pointer;
  border: none;
}

.product-card:hover {
  transform: translateY(-8px);
  box-shadow: 0 8px 20px rgba(0, 0, 0, 0.15) !important;
}

.image-container {
  position: relative;
  width: 100%;
  height: 250px;
  background-color: #f8f9fa;
  overflow: hidden;
}

.card-img-top {
  width: 100%;
  height: 250px;
  object-fit: cover;
  transition: transform 0.3s ease;
}

.product-card:hover .card-img-top {
  transform: scale(1.05);
}

.form-check-input:checked {
  background-color: #0d6efd;
  border-color: #0d6efd;
}

.badge {
  font-size: 0.75rem;
  padding: 0.35em 0.65em;
}

.pagination .page-link {
  color: #0d6efd;
}

.pagination .page-item.active .page-link {
  background-color: #0d6efd;
  border-color: #0d6efd;
}

.pagination .page-item.disabled .page-link {
  cursor: not-allowed;
}

/* 顏色篩選按鈕 */
.color-filter-btn {
  width: 35px;
  height: 35px;
  border-radius: 50%;
  cursor: pointer;
  transition: all 0.2s;
  padding: 0;
  border: 2px solid #dee2e6;
}

.color-filter-btn:hover {
  transform: scale(1.1);
  box-shadow: 0 2px 8px rgba(0,0,0,0.2);
}

.color-filter-btn.active {
  transform: scale(1.15);
  box-shadow: 0 0 0 3px rgba(13, 110, 253, 0.25);
}

/* 響應式 */
@media (max-width: 768px) {
  .image-container {
    height: 200px;
  }
  
  .card-img-top {
    height: 200px;
  }
  
  .sticky-top {
    position: relative !important;
    top: 0 !important;
  }
}
</style>