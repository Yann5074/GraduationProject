<template>
  <div class="product-list-page">
    <!-- 頂部篩選欄 -->
    <div class="filter-bar">
      <div class="container">
        <!-- 主要篩選列 -->
        <div class="filter-row">
          <!-- 搜尋框 -->
          <div class="filter-item search-item">
            <i class="fas fa-search search-icon"></i>
            <input
              v-model="filters.keyword"
              type="text"
              placeholder="搜尋商品名稱..."
              class="search-input"
              @keyup.enter="applyFilters"
            />
          </div>

          <!-- 分類選擇 -->
          <div class="filter-item">
            <select v-model="filters.categoryId" class="filter-select">
              <option :value="null">所有分類</option>
              <option
                v-for="category in categories"
                :key="category.id"
                :value="category.id"
              >
                {{ category.name }}
              </option>
            </select>
          </div>

          <!-- 價格範圍 -->
          <div class="filter-item price-filter">
            <input
              v-model.number="filters.minPrice"
              type="number"
              placeholder="最低價"
              class="price-input"
              min="0"
            />
            <span class="price-separator">-</span>
            <input
              v-model.number="filters.maxPrice"
              type="number"
              placeholder="最高價"
              class="price-input"
              min="0"
            />
          </div>

          <!-- 排序 -->
          <div class="filter-item">
            <select v-model="filters.sortBy" class="filter-select">
              <option value="newest">最新上架</option>
              <option value="popular">最熱門</option>
              <option value="price_asc">價格：低到高</option>
              <option value="price_desc">價格：高到低</option>
              <option value="name_asc">名稱：A-Z</option>
              <option value="name_desc">名稱：Z-A</option>
            </select>
          </div>

          <!-- 篩選按鈕 -->
          <button class="filter-btn" @click="applyFilters">
            <i class="fas fa-filter"></i>
            篩選
          </button>
        </div>

        <!-- 進階篩選 -->
        <div class="advanced-filters">
          <label class="checkbox-label">
            <input v-model="filters.inStockOnly" type="checkbox" />
            <span>只顯示有庫存</span>
          </label>
          <label class="checkbox-label">
            <input v-model="filters.customizableOnly" type="checkbox" />
            <span>只顯示可自訂</span>
          </label>
          <button class="clear-btn" @click="clearFilters">
            <i class="fas fa-times"></i>
            清除篩選
          </button>
        </div>
      </div>
    </div>

    <div class="container">
      <!-- 結果標題 -->
      <div class="result-header">
        <h1>商品列表</h1>
        <p class="result-info">
          <span class="total-count">共 {{ totalCount }} 件商品</span>
          <span v-if="filters.keyword" class="search-keyword">
            搜尋結果：「{{ filters.keyword }}」
          </span>
        </p>
      </div>

      <!-- Loading 狀態 -->
      <div v-if="loading" class="loading-container">
        <div class="spinner"></div>
        <p>載入中...</p>
      </div>

      <!-- 無搜尋結果 -->
      <div v-else-if="products.length === 0" class="no-results">
        <div class="no-results-icon">
          <i class="fas fa-box-open"></i>
        </div>
        <h3>找不到符合的商品</h3>
        <p>試試調整篩選條件或清除篩選</p>
        <button class="btn-primary" @click="clearFilters">
          <i class="fas fa-redo"></i>
          清除所有篩選
        </button>
      </div>

      <!-- 商品網格 -->
      <div v-else class="product-grid">
        <div
          v-for="product in products"
          :key="product.fProductId"
          class="product-card"
          @click="goToProduct(product.fProductId)"
        >
          <!-- 商品圖片 -->
          <div class="product-image-wrapper">
            <img
              :src="product.mainImageUrl"
              :alt="product.fName"
              class="product-image"
            />
            
            <!-- 徽章 -->
            <div class="badges">
              <span v-if="!product.isAvailable" class="badge badge-soldout">
                缺貨
              </span>
              <span v-if="product.fDiscount > 0" class="badge badge-discount">
                -{{ product.fDiscount }}%
              </span>
            </div>
          </div>

          <!-- 商品資訊 -->
          <div class="product-info">
            <!-- 分類 -->
            <p class="product-category">{{ product.categoryName }}</p>
            
            <!-- 商品名稱 -->
            <h3 class="product-name">{{ product.fName }}</h3>
            
            <!-- 商品描述 -->
            <p class="product-description">{{ product.fDescription }}</p>

            <!-- 價格 -->
            <div class="product-price">
              <span v-if="product.minPrice === product.maxPrice" class="price">
                NT$ {{ formatPrice(product.minPrice) }}
              </span>
              <span v-else class="price-range">
                NT$ {{ formatPrice(product.minPrice) }} - {{ formatPrice(product.maxPrice) }}
              </span>
            </div>

            <!-- 標籤 -->
            <div class="product-tags">
              <span v-if="product.isCustomizable" class="tag tag-customizable">
                <i class="fas fa-palette"></i>
                可自訂 ({{ product.customizablePartsCount }}部位)
              </span>
              <span v-if="product.totalStock > 0" class="tag tag-stock">
                <i class="fas fa-box"></i>
                庫存 {{ product.totalStock }}
              </span>
              <span v-if="product.fWarrantyMonth > 0" class="tag tag-warranty">
                <i class="fas fa-shield-alt"></i>
                保固 {{ product.fWarrantyMonth }}個月
              </span>
            </div>
          </div>
        </div>
      </div>

      <!-- 分頁控制 -->
      <div v-if="totalPages > 1" class="pagination">
        <!-- 上一頁 -->
        <button
          class="page-btn"
          :disabled="!hasPreviousPage"
          @click="goToPage(currentPage - 1)"
        >
          <i class="fas fa-chevron-left"></i>
          上一頁
        </button>

        <!-- 頁碼 -->
        <div class="page-numbers">
          <!-- 第一頁 -->
          <button
            v-if="visiblePages[0] > 1"
            class="page-number"
            @click="goToPage(1)"
          >
            1
          </button>
          <span v-if="visiblePages[0] > 2" class="page-ellipsis">...</span>

          <!-- 可見頁碼 -->
          <button
            v-for="page in visiblePages"
            :key="page"
            class="page-number"
            :class="{ active: page === currentPage }"
            @click="goToPage(page)"
          >
            {{ page }}
          </button>

          <!-- 最後一頁 -->
          <span v-if="visiblePages[visiblePages.length - 1] < totalPages - 1" class="page-ellipsis">...</span>
          <button
            v-if="visiblePages[visiblePages.length - 1] < totalPages"
            class="page-number"
            @click="goToPage(totalPages)"
          >
            {{ totalPages }}
          </button>
        </div>

        <!-- 下一頁 -->
        <button
          class="page-btn"
          :disabled="!hasNextPage"
          @click="goToPage(currentPage + 1)"
        >
          下一頁
          <i class="fas fa-chevron-right"></i>
        </button>

        <!-- 頁面資訊 -->
        <div class="page-info">
          第 {{ currentPage }} / {{ totalPages }} 頁
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, watch } from 'vue';
import { useRouter, useRoute } from 'vue-router';
import axios from 'axios';

const router = useRouter();
const route = useRoute();

// 資料狀態
const products = ref([]);
const loading = ref(false);
const totalCount = ref(0);
const currentPage = ref(1);
const pageSize = ref(12);
const totalPages = ref(0);
const hasPreviousPage = ref(false);
const hasNextPage = ref(false);

// 分類列表
const categories = ref([
  { id: 1, name: '客廳家具' },
  { id: 2, name: '臥室家具' },
  { id: 3, name: '辦公家具' }
]);

// 篩選條件
const filters = ref({
  keyword: '',
  categoryId: null,
  minPrice: null,
  maxPrice: null,
  inStockOnly: false,
  customizableOnly: false,
  sortBy: 'newest',
  pageNumber: 1,
  pageSize: 12
});

// 計算可見頁碼
const visiblePages = computed(() => {
  const pages = [];
  const start = Math.max(1, currentPage.value - 2);
  const end = Math.min(totalPages.value, currentPage.value + 2);

  for (let i = start; i <= end; i++) {
    pages.push(i);
  }

  return pages;
});

// 載入商品
async function loadProducts() {
  loading.value = true;

  try {
    const response = await axios.get('https://localhost:7131/api/front', {
      params: filters.value
    });

    
    if (response.data.success) {
      const data = response.data.data;
      products.value = data.items;
      totalCount.value = data.totalCount;
      currentPage.value = data.pageNumber;
      pageSize.value = data.pageSize;
      totalPages.value = data.totalPages;
      hasPreviousPage.value = data.hasPreviousPage;
      hasNextPage.value = data.hasNextPage;

      window.scrollTo({ top: 0, behavior: 'smooth' });
    }
  } catch (error) {
    console.error('載入商品失敗', error);
  } finally {
    loading.value = false;
  }
  
}

// 套用篩選
function applyFilters() {
  filters.value.pageNumber = 1;
  loadProducts();
}

// 清除篩選
function clearFilters() {
  filters.value = {
    keyword: '',
    categoryId: null,
    minPrice: null,
    maxPrice: null,
    inStockOnly: false,
    customizableOnly: false,
    sortBy: 'newest',
    pageNumber: 1,
    pageSize: 12
  };
  loadProducts();
}

// 前往指定頁面
function goToPage(page) {
  if (page < 1 || page > totalPages.value) return;
  filters.value.pageNumber = page;
  loadProducts();
}

// 前往商品詳情
function goToProduct(productId) {
  console.log('點擊商品:', productId);
  // router.push(`/product/${productId}`);
}

// 格式化價格
function formatPrice(price) {
  return price?.toLocaleString() || '0';
}

// 監聽排序變化
watch(() => filters.value.sortBy, () => {
  applyFilters();
});

// 監聽 checkbox 變化
watch([
  () => filters.value.inStockOnly,
  () => filters.value.customizableOnly
], () => {
  applyFilters();
});

// 監聽分類變化
watch(() => filters.value.categoryId, () => {
  applyFilters();
});

// 初始化
onMounted(() => {
  if (route.query.keyword) {
    filters.value.keyword = route.query.keyword;
  }
  if (route.query.categoryId) {
    filters.value.categoryId = parseInt(route.query.categoryId);
  }
  
  loadProducts();
});
</script>

<style scoped>
/* 基本樣式 */
* {
  box-sizing: border-box;
}

.product-list-page {
  min-height: 100vh;
  background-color: #f5f7fa;
}

.container {
  max-width: 1200px;
  margin: 0 auto;
  padding: 0 20px;
}

/* 篩選欄 */
.filter-bar {
  background: white;
  padding: 25px 0;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.08);
  margin-bottom: 30px;
}

.filter-row {
  display: flex;
  gap: 15px;
  flex-wrap: wrap;
  align-items: center;
  margin-bottom: 15px;
}

.filter-item {
  flex: 1;
  min-width: 150px;
}

.search-item {
  position: relative;
  flex: 2;
  min-width: 250px;
}

.search-icon {
  position: absolute;
  left: 15px;
  top: 50%;
  transform: translateY(-50%);
  color: #909399;
}

.search-input {
  width: 100%;
  padding: 12px 15px 12px 40px;
  border: 2px solid #dcdfe6;
  border-radius: 8px;
  font-size: 14px;
}

.search-input:focus {
  outline: none;
  border-color: #409eff;
}

.filter-select {
  width: 100%;
  padding: 12px 15px;
  border: 2px solid #dcdfe6;
  border-radius: 8px;
  font-size: 14px;
  background: white;
  cursor: pointer;
}

.price-filter {
  display: flex;
  gap: 10px;
  align-items: center;
}

.price-input {
  width: 120px;
  padding: 12px 15px;
  border: 2px solid #dcdfe6;
  border-radius: 8px;
  font-size: 14px;
}

.price-separator {
  color: #909399;
}

.filter-btn {
  padding: 12px 30px;
  background: #409eff;
  color: white;
  border: none;
  border-radius: 8px;
  cursor: pointer;
  font-size: 14px;
  white-space: nowrap;
}

.filter-btn:hover {
  background: #66b1ff;
}

.advanced-filters {
  display: flex;
  gap: 25px;
  align-items: center;
}

.checkbox-label {
  display: flex;
  align-items: center;
  gap: 8px;
  cursor: pointer;
  font-size: 14px;
}

.checkbox-label input[type="checkbox"] {
  width: 18px;
  height: 18px;
  cursor: pointer;
}

.clear-btn {
  padding: 8px 20px;
  background: #f5f7fa;
  border: 1px solid #dcdfe6;
  border-radius: 6px;
  cursor: pointer;
  font-size: 14px;
}

.clear-btn:hover {
  background: #ecf5ff;
  border-color: #409eff;
  color: #409eff;
}

/* 結果標題 */
.result-header {
  margin-bottom: 30px;
}

.result-header h1 {
  font-size: 32px;
  color: #303133;
  margin-bottom: 10px;
}

.result-info {
  display: flex;
  gap: 15px;
  align-items: center;
}

.total-count {
  color: #606266;
  font-size: 16px;
}

.search-keyword {
  color: #909399;
  font-size: 14px;
}

/* Loading */
.loading-container {
  text-align: center;
  padding: 100px 20px;
}

.spinner {
  width: 60px;
  height: 60px;
  border: 5px solid #f3f3f3;
  border-top: 5px solid #409eff;
  border-radius: 50%;
  animation: spin 1s linear infinite;
  margin: 0 auto 20px;
}

@keyframes spin {
  0% { transform: rotate(0deg); }
  100% { transform: rotate(360deg); }
}

/* 無結果 */
.no-results {
  text-align: center;
  padding: 100px 20px;
}

.no-results-icon {
  font-size: 80px;
  color: #dcdfe6;
  margin-bottom: 30px;
}

.no-results h3 {
  font-size: 24px;
  color: #606266;
  margin-bottom: 15px;
}

.no-results p {
  color: #909399;
  margin-bottom: 40px;
}

.btn-primary {
  padding: 14px 40px;
  background: #409eff;
  color: white;
  border: none;
  border-radius: 8px;
  font-size: 16px;
  cursor: pointer;
  display: inline-flex;
  align-items: center;
  gap: 10px;
}

.btn-primary:hover {
  background: #66b1ff;
}

/* 商品網格 */
.product-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
  gap: 30px;
  margin-bottom: 60px;
}

.product-card {
  background: white;
  border-radius: 12px;
  overflow: hidden;
  cursor: pointer;
  transition: all 0.3s;
  box-shadow: 0 2px 12px rgba(0, 0, 0, 0.08);
}

.product-card:hover {
  transform: translateY(-8px);
  box-shadow: 0 8px 30px rgba(0, 0, 0, 0.12);
}

.product-image-wrapper {
  position: relative;
  width: 100%;
  height: 300px;
  overflow: hidden;
  background: #f5f7fa;
}

.product-image {
  width: 100%;
  height: 100%;
  object-fit: cover;
  transition: transform 0.3s;
}

.product-card:hover .product-image {
  transform: scale(1.08);
}

.badges {
  position: absolute;
  top: 15px;
  right: 15px;
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.badge {
  padding: 6px 14px;
  border-radius: 20px;
  font-size: 12px;
  font-weight: 600;
}

.badge-soldout {
  background: rgba(0, 0, 0, 0.75);
  color: white;
}

.badge-discount {
  background: #f56c6c;
  color: white;
}

.product-info {
  padding: 20px;
}

.product-category {
  font-size: 12px;
  color: #909399;
  margin-bottom: 8px;
}

.product-name {
  font-size: 18px;
  font-weight: 600;
  color: #303133;
  margin-bottom: 10px;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.product-description {
  font-size: 14px;
  color: #909399;
  margin-bottom: 15px;
  overflow: hidden;
  text-overflow: ellipsis;
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  min-height: 45px;
}

.product-price {
  margin-bottom: 15px;
}

.price,
.price-range {
  font-size: 22px;
  font-weight: 700;
  color: #409eff;
}

.product-tags {
  display: flex;
  gap: 8px;
  flex-wrap: wrap;
}

.tag {
  padding: 5px 12px;
  border-radius: 15px;
  font-size: 11px;
  display: inline-flex;
  align-items: center;
  gap: 5px;
}

.tag-customizable {
  background: #ecf5ff;
  color: #409eff;
}

.tag-stock {
  background: #f0f9ff;
  color: #67c23a;
}

.tag-warranty {
  background: #fef0f0;
  color: #f56c6c;
}

/* 分頁 */
.pagination {
  display: flex;
  justify-content: center;
  align-items: center;
  gap: 15px;
  margin: 60px 0;
}

.page-btn {
  padding: 10px 20px;
  background: white;
  border: 2px solid #dcdfe6;
  border-radius: 8px;
  color: #606266;
  cursor: pointer;
  font-size: 14px;
  display: inline-flex;
  align-items: center;
  gap: 8px;
}

.page-btn:hover:not(:disabled) {
  border-color: #409eff;
  color: #409eff;
}

.page-btn:disabled {
  opacity: 0.4;
  cursor: not-allowed;
}

.page-numbers {
  display: flex;
  gap: 8px;
}

.page-number {
  width: 45px;
  height: 45px;
  background: white;
  border: 2px solid #dcdfe6;
  border-radius: 8px;
  color: #606266;
  cursor: pointer;
  font-size: 14px;
}

.page-number:hover {
  border-color: #409eff;
  color: #409eff;
}

.page-number.active {
  background: #409eff;
  border-color: #409eff;
  color: white;
}

.page-ellipsis {
  color: #909399;
  padding: 0 5px;
  display: flex;
  align-items: center;
}

.page-info {
  color: #909399;
  font-size: 14px;
}

/* 響應式 */
@media (max-width: 768px) {
  .filter-row {
    flex-direction: column;
  }

  .filter-item,
  .search-item {
    width: 100%;
  }

  .product-grid {
    grid-template-columns: repeat(auto-fill, minmax(160px, 1fr));
    gap: 15px;
  }

  .product-image-wrapper {
    height: 200px;
  }
}
</style>