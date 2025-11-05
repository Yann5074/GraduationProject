<!-- 產品列表頁 - 單選類別版本 -->
<template>
    <!--推薦商品-->
  <RecommendList />
  <GlobalLoading scope="product" message="商品載入中..." />
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
            <div class="card-header  text-black">
              <h5 class="mb-0">
                篩選商品
              </h5>
            </div>
            <div class="card-body">

              <div class="mb-4">
                <h6 class="fw-bold mb-3">
                  <i class="bi bi-grid me-2"></i>商品種類
                </h6>
                <div v-if="filterOptions.categories && filterOptions.categories.length > 0">

                  <div class="form-check mb-2">
                    <input
                      id="category-all"
                      :checked="localFilters.categoryId === null"
                      class="form-check-input"
                      type="radio"
                      name="category"
                      @change="selectCategory(null)"
                    />
                    <label
                      class="form-check-label d-flex justify-content-between align-items-center"
                      for="category-all"
                    >
                      <span>全部商品</span>
                    </label>
                  </div>

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
                      class="form-check-label d-flex justify-content-between align-items-center"
                      :for="`category-${category.categoryId}`"
                    >
                      <span>{{ category.name }}</span>
  
                      <span class="badge text-dark bg-light ms-2" style="font-size: 0.7rem;">
                        {{ category.productCount }}
                      </span>
                    </label>
                  </div>
        
                  <button
                    v-if="localFilters.categoryId !== null"
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
              </div>

              <hr>

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


              <div v-if="hasActiveFilters" class="mt-3 p-2 bg-light rounded">
                <small class="text-muted d-block mb-2">
                  <strong>已套用篩選：</strong>
                </small>
                <div class="d-flex flex-wrap gap-1">
 
                  <span v-if="localFilters.categoryId !== null" class="badge bg-secondary">
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
                <option value="created_desc">最新上架</option>
                <option value="price_asc">價格低到高</option>
                <option value="price_desc">價格高到低</option>
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
                <div class="card product-card shadow-sm h-100" @click="goToDetail(product.fProductId)">
                  <div class="image-container">
                    <img
                      :src="getProductImageUrl(product)"
                      :alt="product.fName"
                      class="card-img-top"
                      loading="lazy"
                      @error="product._imageError = true"
                    />
                    <div v-if="product.fDiscount && product.fDiscount > 0" class="position-absolute top-0 end-0 m-2">
                      <span class="badge bg-danger">
                        -{{ Math.round(product.fDiscount * 100) }}%
                      </span>
                    </div>
                    <div v-if="!product.isAvailable" class="position-absolute top-0 start-0 m-2">
                      <span class="badge bg-secondary">已售完</span>
                    </div>
                  </div>
                  <div class="card-body d-flex flex-column">
                    <h6 class="card-title text-truncate" :title="product.fName">
                      {{ product.fName }}
                    </h6>
                    <!-- <p class="card-text text-muted small flex-grow-1" style="min-height: 40px;">
                      {{ truncateText(product.fDescription, 60) }}
                    </p> -->
                    <div class="d-flex flex-wrap gap-1 mb-2">
                      <span class="badge  text-dark ">
                        {{ product.categoryName }}
                      </span>
                      <span v-if="product.availableColorCount > 0" class="badge  text-dark">
                        {{ product.availableColorCount }} 種顏色
                      </span>
                    </div>
                    <div class="mt-auto">
                      <div v-if="product.minPrice === product.maxPrice" class="d-flex align-items-baseline">
                        <span v-if="product.fDiscount && product.fDiscount > 0" class="text-muted text-decoration-line-through me-2 small">
                          NT$ {{ formatPrice(calculateOriginalPrice(product.minPrice, product.fDiscount)) }}
                        </span>
                        <span class="fs-5 fw-bold text-black">
                          NT$ {{ formatPrice(product.minPrice) }}
                        </span>
                      </div>
                      <div v-else>
                        <span class="fs-6 fw-bold text-black">
                          NT$ {{ formatPrice(product.minPrice) }} - {{ formatPrice(product.maxPrice) }}
                        </span>
                      </div>
                      <div class="mt-2">
                        <!-- <span v-if="product.totalStock > 0" class="badge bg-success">
                          庫存 {{ product.totalStock }}
                        </span>
                        <span v-else class="badge bg-secondary">
                          已售完
                        </span> -->
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </div>

            <!-- 分頁 -->
            <nav v-if="pagination && pagination.totalPages > 1" class="mt-5" aria-label="產品分頁">
              <ul class="pagination justify-content-center flex-wrap">
                <li class="page-item" :class="{ disabled: pagination.currentPage === 1 }">
                  <button class="page-link" @click="goToPage(pagination.currentPage - 1)" :disabled="pagination.currentPage === 1">
                    <i class="bi bi-chevron-left"></i>
                  </button>
                </li>

                <li v-if="pagination.currentPage > 3" class="page-item">
                  <button class="page-link" @click="goToPage(1)">1</button>
                </li>
                <li v-if="pagination.currentPage > 4" class="page-item disabled">
                  <span class="page-link">...</span>
                </li>

                <li
                  v-for="page in visiblePages"
                  :key="page"
                  class="page-item"
                  :class="{ active: page === pagination.currentPage }"
                >
                  <button class="page-link" @click="goToPage(page)">
                    {{ page }}
                  </button>
                </li>

                <li v-if="pagination.currentPage < pagination.totalPages - 3" class="page-item disabled">
                  <span class="page-link">...</span>
                </li>
                <li v-if="pagination.currentPage < pagination.totalPages - 2" class="page-item">
                  <button class="page-link" @click="goToPage(pagination.totalPages)">
                    {{ pagination.totalPages }}
                  </button>
                </li>

                <li class="page-item" :class="{ disabled: pagination.currentPage === pagination.totalPages }">
                  <button class="page-link" @click="goToPage(pagination.currentPage + 1)" :disabled="pagination.currentPage === pagination.totalPages">
                    <i class="bi bi-chevron-right"></i>
                  </button>
                </li>
              </ul>
            </nav>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive, computed, onMounted, watch } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import ProductAPI from '@/api/Product'
import { useLoading } from '@/stores/useLoading'
import GlobalLoading from '@/components/GlobalLoading.vue'
import RecommendList from '@/components/RecommendList.vue'

const route = useRoute()
const router = useRouter()

//loading相關
const {withLoading} = useLoading('product');

// 狀態
const products = ref([])
const filterOptions = ref({
  categories: [],
  colors: [],
  textures: [],
  priceRange: { minPrice: 0, maxPrice: 100000 }
})
const pagination = ref({
  totalCount: 0,
  pageSize: 12,
  currentPage: 1,
  totalPages: 0
})
const loading = ref(false)
const error = ref(null)
const searchKeyword = ref('')
const priceInput = reactive({
  min: null,
  max: null
})

// ⭐ 修改點 3: 本地篩選條件（改回單選）
const localFilters = reactive({
  categoryId: null,  // ⭐ 改為單一 ID
  colorIds: [],
  textureIds: [],
  minPrice: null,
  maxPrice: null,
  searchKeyword: '',
  sortBy: 'newest',
  pageNumber: 1,
  pageSize: 12
})

// 計算可見頁碼
const visiblePages = computed(() => {
  if (!pagination.value) return []
  
  const current = pagination.value.currentPage
  const total = pagination.value.totalPages
  const pages = []
  
  const start = Math.max(1, current - 2)
  const end = Math.min(total, current + 2)
  
  for (let i = start; i <= end; i++) {
    pages.push(i)
  }
  
  return pages
})

// ⭐ 修改點 4: 檢查是否有啟用的篩選（改為檢查單一類別）
const hasActiveFilters = computed(() => {
  return (
    localFilters.categoryId !== null ||  // ⭐ 改為檢查單一值
    (localFilters.colorIds && localFilters.colorIds.length > 0) ||
    (localFilters.textureIds && localFilters.textureIds.length > 0) ||
    localFilters.minPrice ||
    localFilters.maxPrice ||
    localFilters.searchKeyword
  )
})

// 生命週期
onMounted(() => {
  initFiltersFromQuery()
  loadProducts()
  loadFilterOptions()
})

// 監聽路由變化
watch(() => route.query, () => {
  initFiltersFromQuery()
  loadProducts()
}, { deep: true })

// 初始化
onMounted(async () => {
  await withLoading(async () =>{
    await loadFilterOptions()
    syncFiltersFromURL()
    await loadProducts()
  })
})

// ⭐ 修改點 5: 從 URL 查詢參數初始化篩選條件（改回單一類別）
function initFiltersFromQuery() {
  const query = route.query
  
  // ⭐ 改為處理單一類別 ID
  localFilters.categoryId = query.category ? Number(query.category) : null
  
  // 處理多顏色 ID
  if (query.colors) {
    localFilters.colorIds = query.colors.split(',').map(Number).filter(id => !isNaN(id))
  } else {
    localFilters.colorIds = []
  }
  
  // 處理多材質 ID
  if (query.textures) {
    localFilters.textureIds = query.textures.split(',').map(Number).filter(id => !isNaN(id))
  } else {
    localFilters.textureIds = []
  }
  
  localFilters.minPrice = query.minPrice ? Number(query.minPrice) : null
  localFilters.maxPrice = query.maxPrice ? Number(query.maxPrice) : null
  localFilters.searchKeyword = query.keyword || ''
  localFilters.sortBy = query.sortBy || 'newest'
  localFilters.pageNumber = query.page ? Number(query.page) : 1
  
  searchKeyword.value = localFilters.searchKeyword
  priceInput.min = localFilters.minPrice
  priceInput.max = localFilters.maxPrice
}

// 載入產品列表
async function loadProducts() {
  loading.value = true
  error.value = null
  
  try {
    console.log('🔍 [ProductListPage] 開始載入產品，篩選條件:', localFilters)
    
    // ⭐ 修改點 6: 準備 API 參數（改為單一類別）
    const apiParams = {
      categoryId: localFilters.categoryId,  // ⭐ 傳遞單一 ID
      colorIds: localFilters.colorIds.length > 0 ? localFilters.colorIds : undefined,
      textureIds: localFilters.textureIds.length > 0 ? localFilters.textureIds : undefined,
      minPrice: localFilters.minPrice,
      maxPrice: localFilters.maxPrice,
      keyword: localFilters.searchKeyword || undefined,
      sortBy: localFilters.sortBy,
      pageNumber: localFilters.pageNumber,
      pageSize: localFilters.pageSize
    }
    
    console.log('📤 [ProductListPage] API 參數:', apiParams)
    
    const result = await ProductAPI.getProducts(apiParams)
    
    console.log('📥 [ProductListPage] API 回傳結果:', result)
    
    if (result.success) {
      products.value = result.data || []
      pagination.value = result.pagination || {
        totalCount: 0,
        pageSize: localFilters.pageSize,
        currentPage: localFilters.pageNumber,
        totalPages: 0
      }
      
      console.log('✅ [ProductListPage] 成功載入', products.value.length, '項產品')
    } else {
      error.value = result.message || '載入產品失敗'
      products.value = []
      console.error('❌ [ProductListPage] 載入失敗:', result.message)
    }
  } catch (err) {
    console.error('❌ [ProductListPage] 載入產品時發生錯誤:', err)
    error.value = '載入產品時發生錯誤，請稍後再試'
    products.value = []
  } finally {
    loading.value = false
  }
}

// 載入篩選選項
async function loadFilterOptions() {
  try {
    const result = await ProductAPI.getFilterOptions()
    if (result.success && result.data) {
      filterOptions.value = {
        categories: result.data.categories || [],
        colors: result.data.colors || [],
        textures: result.data.textures || [],
        priceRange: result.data.priceRange || { minPrice: 0, maxPrice: 100000 }
      }
      console.log('✅ [ProductListPage] 篩選選項載入成功:', filterOptions.value)
    }
  } catch (err) {
    console.error('❌ [ProductListPage] 載入篩選選項失敗:', err)
  }
}

// ⭐ 修改點 7: 更新 URL 並重新載入產品（改為單一類別）
async function updateAndReload() {
  localFilters.pageNumber = 1
  
  const query = {}
  
  // ⭐ 改為單一類別
  if (localFilters.categoryId !== null) {
    query.category = localFilters.categoryId
  }
  
  // 多顏色
  if (localFilters.colorIds && localFilters.colorIds.length > 0) {
    query.colors = localFilters.colorIds.join(',')
  }
  
  // 多材質
  if (localFilters.textureIds && localFilters.textureIds.length > 0) {
    query.textures = localFilters.textureIds.join(',')
  }
  
  if (localFilters.searchKeyword) query.keyword = localFilters.searchKeyword
  if (localFilters.minPrice) query.minPrice = localFilters.minPrice
  if (localFilters.maxPrice) query.maxPrice = localFilters.maxPrice
  if (localFilters.sortBy && localFilters.sortBy !== 'newest') {
    query.sortBy = localFilters.sortBy
  }
  
  await router.replace({ query })
  await loadProducts()
}

// ⭐ 修改點 8: 選擇類別（改為單選）
function selectCategory(categoryId) {
  localFilters.categoryId = categoryId  // ⭐ 直接賦值
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

// 前往指定頁面
async function goToPage(page) {
  if (page < 1 || page > pagination.value.totalPages) return
  
  localFilters.pageNumber = page
  
  const query = { ...route.query, page }
  if (page === 1) delete query.page
  
  await router.replace({ query })
  await loadProducts()
  
  window.scrollTo(0, 0)
}

// ⭐ 修改點 9: 清除所有篩選（改為 null）
async function clearAllFilters() {
  localFilters.categoryId = null  // ⭐ 改為 null
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

// 取得產品圖片 URL
function getProductImageUrl(product) {
  if (product._imageError) {
    return '/ProductImages/default.png'
  }
  
  if (product.mainImageUrl) {
    const url = product.mainImageUrl
    
    if (url.startsWith('http://') || url.startsWith('https://')) {
      return url
    }
    
    if (url.startsWith('/')) {
      const API_BASE = import.meta.env.VITE_API_BASE_URL || 'https://localhost:7131'
      return `${API_BASE}${url}`
    }
    
    return url
  }
  
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
  background-color: #f5efe3;
  border-color: #cfcabb;
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