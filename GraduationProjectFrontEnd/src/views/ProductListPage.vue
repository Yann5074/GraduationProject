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
          <div class="card shadow-sm">
            <div class="card-header bg-primary text-white">
              <h5 class="mb-0">
                <i class="bi bi-funnel me-2"></i>篩選條件
              </h5>
            </div>
            <div class="card-body">
              <!-- 類別篩選 -->
              <div class="mb-4">
                <h6 class="fw-bold mb-3">產品類別</h6>
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
                      {{ category.name }}
                    </label>
                  </div>
                  <button
                    v-if="localFilters.categoryId"
                    class="btn btn-sm btn-outline-secondary mt-2"
                    @click="selectCategory(null)"
                  >
                    清除類別
                  </button>
                </div>
                <div v-else class="text-muted small">載入中...</div>
              </div>

              <!-- 價格篩選 -->
              <div class="mb-4">
                <h6 class="fw-bold mb-3">價格範圍</h6>
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
                  套用價格
                </button>
                <div v-if="filterOptions.priceRange" class="text-muted small mt-2">
                  範圍: ${{ formatPrice(filterOptions.priceRange.minPrice) }} - 
                  ${{ formatPrice(filterOptions.priceRange.maxPrice) }}
                </div>
              </div>

              <!-- 狀態篩選 -->
              <div class="mb-4">
                <h6 class="fw-bold mb-3">產品狀態</h6>
                <div v-if="filterOptions.statusOptions.length > 0">
                  <div
                    v-for="status in filterOptions.statusOptions"
                    :key="status.fpStatus"
                    class="form-check mb-2"
                  >
                    <input
                      :id="`status-${status.fpStatus}`"
                      :checked="localFilters.statusId === status.fpStatus"
                      class="form-check-input"
                      type="radio"
                      name="status"
                      @change="selectStatus(status.fpStatus)"
                    />
                    <label
                      class="form-check-label"
                      :for="`status-${status.fpStatus}`"
                    >
                      {{ status.fpStatusName }}
                    </label>
                  </div>
                  <button
                    v-if="localFilters.statusId"
                    class="btn btn-sm btn-outline-secondary mt-2"
                    @click="selectStatus(null)"
                  >
                    清除狀態
                  </button>
                </div>
              </div>

              <!-- 清除所有篩選 -->
              <button class="btn btn-outline-danger w-100" @click="clearAllFilters">
                <i class="bi bi-x-circle me-1"></i>清除所有篩選
              </button>
            </div>
          </div>
        </div>

        <!-- 產品列表區 -->
        <div class="col-lg-9 col-md-8">
          <!-- 工具列 -->
          <div class="d-flex justify-content-between align-items-center mb-3">
            <div class="text-muted">
              <span v-if="!loading && pagination">
                共 <strong>{{ pagination.totalCount }}</strong> 項產品
                (第 {{ pagination.currentPage }} / {{ pagination.totalPages }} 頁)
              </span>
            </div>
            <div class="d-flex align-items-center gap-2">
              <label class="mb-0 me-2">排序:</label>
              <select
                v-model="localFilters.sortBy"
                class="form-select form-select-sm"
                style="width: 180px"
                @change="handleSortChange"
              >
                <option
                  v-for="option in sortOptions"
                  :key="option.value"
                  :value="option.value"
                >
                  {{ option.label }}
                </option>
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
          </div>

          <!-- 無產品 -->
          <div
            v-else-if="products.length === 0"
            class="text-center py-5"
          >
            <i class="bi bi-inbox display-1 text-muted"></i>
            <p class="mt-3 text-muted">目前沒有符合條件的產品</p>
            <button class="btn btn-primary mt-2" @click="clearAllFilters">
              清除篩選條件
            </button>
          </div>

          <!-- 產品網格 -->
          <div v-else>
            <div class="row g-4">
              <div
                v-for="product in products"
                :key="`product-${product.fProductId}`"
                class="col-xl-4 col-lg-6 col-md-6 col-sm-6"
              >
                <div class="card h-100 shadow-sm product-card">
                  <!-- 產品圖片 -->
                  <div class="position-relative image-container">
                    <img
                      :src="product.mainImageUrl"
                      :key="`img-${product.fProductId}`"
                      class="card-img-top"
                      :alt="product.fName"
                      loading="lazy"
                      decoding="async"
                      @error="handleImageError"
                    />
                    <!-- 標籤 -->
                    <div class="position-absolute top-0 start-0 p-2">
                      <span
                        v-if="!product.isAvailable"
                        class="badge bg-danger"
                      >
                        售完
                      </span>
                      <span
                        v-else-if="product.fDiscount"
                        class="badge bg-warning text-dark"
                      >
                        {{ product.fDiscount }}% OFF
                      </span>
                      <span
                        v-if="product.isCustomizable"
                        class="badge bg-info ms-1"
                      >
                        可客製化
                      </span>
                    </div>
                  </div>

                  <!-- 產品資訊 -->
                  <div class="card-body d-flex flex-column">
                    <div class="mb-2">
                      <span class="badge bg-secondary">{{ product.categoryName }}</span>
                    </div>
                    <h5 class="card-title">{{ product.fName }}</h5>
                    <!-- <p class="card-text text-muted small">
                      {{ truncateText(product.fDescription, 80) }}
                    </p> -->

                    <!-- 價格 -->
                    <div class="mt-auto">
                      <div class="d-flex justify-content-between align-items-center mb-2">
                        <div>
                          <span
                            v-if="product.minPrice === product.maxPrice"
                            class="h5 text-primary mb-0"
                          >
                            ${{ formatPrice(product.minPrice) }}
                          </span>
                          <span v-else class="h5 text-primary mb-0">
                            ${{ formatPrice(product.minPrice) }} - 
                            ${{ formatPrice(product.maxPrice) }}
                          </span>
                        </div>
                        <span class="text-muted small">
                          庫存: {{ product.totalStock }}
                        </span>
                      </div>

                      <!-- 額外資訊 -->
                      <div class="d-flex justify-content-between text-muted small mb-3">
                        <span v-if="product.fWarrantyMonth">
                          <i class="bi bi-shield-check"></i> {{ product.fWarrantyMonth }}個月保固
                        </span>
                        <span v-if="product.isCustomizable">
                          <i class="bi bi-palette"></i> {{ product.customizablePartsCount }}個部位
                        </span>
                      </div>

                      <!-- 按鈕 -->
                      <button
                        class="btn btn-primary w-100"
                        :disabled="!product.isAvailable"
                        @click="goToDetail(product.fProductId)"
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
          <nav v-if="pagination && pagination.totalPages > 1" class="mt-4">
            <ul class="pagination justify-content-center">
              <li class="page-item" :class="{ disabled: !pagination.hasPrevious }">
                <a
                  class="page-link"
                  href="#"
                  @click.prevent="goToPage(pagination.currentPage - 1)"
                >
                  上一頁
                </a>
              </li>

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

              <li class="page-item" :class="{ disabled: !pagination.hasNext }">
                <a
                  class="page-link"
                  href="#"
                  @click.prevent="goToPage(pagination.currentPage + 1)"
                >
                  下一頁
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
import { ref, reactive, computed, onMounted } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import ProductAPI from '../api/Product.js'
import { 
  ReqProductFilterDTO, 
  ResFilterOptionsDTO,
  SORT_OPTIONS 
} from '../dtos/ProductDTO.js'

const router = useRouter()
const route = useRoute()

// 狀態
const loading = ref(false)
const error = ref(null)
const products = ref([])
const pagination = ref(null)
const searchKeyword = ref('')
const sortOptions = SORT_OPTIONS


const localFilters = reactive({
  categoryId: null,
  minPrice: null,
  maxPrice: null,
  statusId: null,
  keyword: '',
  sortBy: 'created_desc',
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
  priceRange: null,
  statusOptions: []
})

// 計算顯示的頁碼
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

// 初始化
onMounted(async () => {
  // 載入篩選選項
  await loadFilterOptions()
  
  // 從 URL 讀取參數
  syncFiltersFromURL()
  
  // 載入產品
  await loadProducts()
})

// 從 URL 同步篩選條件
function syncFiltersFromURL() {
  if (route.query.keyword) {
    searchKeyword.value = route.query.keyword
    localFilters.keyword = route.query.keyword
  }
  if (route.query.categoryId) {
    localFilters.categoryId = parseInt(route.query.categoryId)
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
  if (route.query.statusId) {
    localFilters.statusId = parseInt(route.query.statusId)
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
    if (response.success) {
      const options = new ResFilterOptionsDTO(response.data)
      filterOptions.categories = options.categories
      filterOptions.priceRange = options.priceRange
      filterOptions.statusOptions = options.statusOptions
    }
  } catch (err) {
    console.error('載入篩選選項失敗:', err)
  }
}

// 載入產品列表（核心函數，只在這裡呼叫 API）
async function loadProducts() {
  // 防止重複呼叫
  if (loading.value) {
    console.log('🔄 已經在載入中，跳過此次請求')
    return
  }
  
  loading.value = true
  error.value = null
  
  console.log('📡 載入產品:', localFilters)
  
  try {
    const filterDTO = new ReqProductFilterDTO(localFilters)
    const response = await ProductAPI.getProducts(filterDTO)
    
    if (response.success) {
      // 處理產品資料，修正圖片 URL
      const productsData = (response.data.data || []).map(product => {
        // 支援 mainImageUrl 或 MainImageUrl（大小寫不一致）
        const imageUrl = product.mainImageUrl || product.MainImageUrl
        console.log('🖼️ 產品圖片 URL:', product.fName, '→', imageUrl)
        
        return {
          ...product,
          mainImageUrl: getImageUrl(imageUrl)
        }
      })
      
      products.value = productsData
      pagination.value = response.data.pagination
      
      console.log('✅ 產品載入成功:', products.value.length, '個產品')
      console.log('📸 第一個產品的圖片:', products.value[0]?.mainImageUrl)
    } else {
      error.value = response.message || '載入產品失敗'
    }
  } catch (err) {
    console.error('❌ 載入產品失敗:', err)
    error.value = err.message || '載入產品時發生錯誤，請稍後再試'
  } finally {
    loading.value = false
  }
}

// 處理圖片 URL（修正相對路徑、檢查有效性）
function getImageUrl(url) {
  // 如果沒有 URL，返回佔位符
  if (!url) {
    console.warn('⚠️ 產品沒有圖片 URL，使用佔位符')
    return 'https://via.placeholder.com/250/cccccc/ffffff?text=No+Image'
  }
  

  if (url.startsWith('http://') || url.startsWith('https://')) {
    return url
  }
  

  if (url.startsWith('/')) {
  
    const API_BASE = import.meta.env.VITE_API_BASE_URL || 'https://localhost:7131'
    const fullUrl = `${API_BASE}${url}`
    console.log('🔗 轉換相對路徑:', url, '→', fullUrl)
    return fullUrl
  }
  

  console.warn('⚠️ 無法識別的圖片格式:', url)
  return 'https://via.placeholder.com/250/cccccc/ffffff?text=Invalid+URL'
}


async function updateAndReload() {
  // 重置到第一頁
  localFilters.pageNumber = 1
  
  // 更新 URL
  const query = {}
  if (localFilters.keyword) query.keyword = localFilters.keyword
  if (localFilters.categoryId) query.categoryId = localFilters.categoryId
  if (localFilters.minPrice) query.minPrice = localFilters.minPrice
  if (localFilters.maxPrice) query.maxPrice = localFilters.maxPrice
  if (localFilters.statusId) query.statusId = localFilters.statusId
  if (localFilters.sortBy && localFilters.sortBy !== 'created_desc') {
    query.sortBy = localFilters.sortBy
  }
  
  // 使用 replace 而不是 push，避免歷史紀錄過多
  await router.replace({ query })
  
  // 重新載入產品
  await loadProducts()
}

// 選擇類別
function selectCategory(categoryId) {
  localFilters.categoryId = categoryId
  updateAndReload()
}

// 選擇狀態
function selectStatus(statusId) {
  localFilters.statusId = statusId
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
  localFilters.keyword = searchKeyword.value.trim()
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
  
  window.scrollTo({ top: 0, behavior: 'smooth' })
}

// 清除所有篩選
async function clearAllFilters() {
  // 重置本地篩選
  localFilters.categoryId = null
  localFilters.minPrice = null
  localFilters.maxPrice = null
  localFilters.statusId = null
  localFilters.keyword = ''
  localFilters.sortBy = 'created_desc'
  localFilters.pageNumber = 1
  
  // 重置輸入框
  searchKeyword.value = ''
  priceInput.min = null
  priceInput.max = null
  
  // 清除 URL
  await router.replace({ query: {} })
  
  // 重新載入
  await loadProducts()
}

// 前往產品詳情
function goToDetail(productId) {
  router.push({ name: 'ProductDetail', params: { id: productId } })
}

// 格式化價格
function formatPrice(price) {
  if (!price) return '0'
  return new Intl.NumberFormat('zh-TW').format(price)
}

// 截斷文字
function truncateText(text, maxLength) {
  if (!text) return ''
  if (text.length <= maxLength) return text
  return text.substring(0, maxLength) + '...'
}

// 圖片載入失敗處理
function handleImageError(event) {
  console.error('❌ 圖片載入失敗:', event.target.src)
  // 使用佔位符圖片
  event.target.src = 'https://via.placeholder.com/250/e0e0e0/666666?text=Image+Not+Found'
  event.target.style.backgroundColor = '#f0f0f0'
}
</script>

<style scoped>
.product-list-page {
  min-height: 100vh;
  background-color: #f8f9fa;
}

.product-card {
  transition: transform 0.2s, box-shadow 0.2s;
  cursor: pointer;
}

.product-card:hover {
  transform: translateY(-5px);
  box-shadow: 0 0.5rem 1rem rgba(0, 0, 0, 0.15) !important;
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

/* 圖片容器 - 固定高度，避免閃爍 */
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
  display: block;
}

/* 移除淡入淡出動畫 */

</style>