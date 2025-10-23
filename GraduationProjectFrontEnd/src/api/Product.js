import axios from 'axios'

const API_BASE = import.meta.env.VITE_API_BASE_URL || 'https://localhost:7131'

// 建立 axios 實例
const apiClient = axios.create({
    baseURL: `${API_BASE}/api`,
    timeout: 10000,
    headers: {
        'Content-Type': 'application/json'
    }
})

// 請求攔截器
apiClient.interceptors.request.use(
    (config) => {
        // 可以在這裡加入 token 等認證資訊
        return config
    },
    (error) => {
        return Promise.reject(error)
    }
)

// 回應攔截器
apiClient.interceptors.response.use(
    (response) => {
        return response.data
    },
    (error) => {
        console.error('API Error:', error)
        return Promise.reject(error)
    }
)

/**
 * 產品 API 服務
 */
export const ProductAPI = {
    /**
     * 取得產品列表
     * @param {Object} filter - 篩選條件
     * @returns {Promise<Object>} 產品列表資料
     */
    getProducts(filter = {}) {
        const params = {
            categoryId: filter.categoryId,
            minPrice: filter.minPrice,
            maxPrice: filter.maxPrice,
            statusId: filter.statusId,
            keyword: filter.keyword,
            sortBy: filter.sortBy || 'created_desc',
            pageNumber: filter.pageNumber || 1,
            pageSize: filter.pageSize || 12
        }

        // 移除 undefined 或 null 的參數
        Object.keys(params).forEach(key => {
            if (params[key] === undefined || params[key] === null || params[key] === '') {
                delete params[key]
            }
        })

        return apiClient.get('/Product', { params })
    },

    /**
     * 取得篩選選項
     * @returns {Promise<Object>} 篩選選項資料
     */
    getFilterOptions() {
        return apiClient.get('/Product/filter-options')
    },

    /**
     * 根據關鍵字搜尋產品
     * @param {string} keyword - 搜尋關鍵字
     * @returns {Promise<Object>} 搜尋結果
     */
    searchProductsByKeyword(keyword) {
        if (!keyword || keyword.trim() === '') {
            return Promise.reject(new Error('請提供搜尋關鍵字'))
        }
        return apiClient.get(`/Product/search/${encodeURIComponent(keyword)}`)
    },

    /**
     * 取得產品詳情
     * @param {number} id - 產品 ID
     * @param {boolean} includeCustomization - 是否包含自訂資訊
     * @returns {Promise<Object>} 產品詳情
     */
    getProductById(id, includeCustomization = false) {
        return apiClient.get(`/Product/${id}`, {
            params: { includeCustomization }
        })
    },

    /**
     * 取得相似產品
     * @param {number} id - 產品 ID
     * @param {number} count - 數量
     * @returns {Promise<Object>} 相似產品列表
     */
    getSimilarProducts(id, count = 4) {
        return apiClient.get(`/Product/${id}/similar`, {
            params: { count }
        })
    },

    /**
     * 取得購物車產品資訊（單一）
     * @param {number} variantId - 產品變體 ID
     * @returns {Promise<Object>} 產品資訊
     */
    getCartProduct(variantId) {
        return apiClient.get(`/Product/cart-items/${variantId}`)
    },

    /**
     * 批次取得購物車產品資訊
     * @param {Array<number>} variantIds - 產品變體 ID 列表
     * @returns {Promise<Object>} 產品資訊列表
     */
    getCartProducts(variantIds) {
        return apiClient.post('/Product/cart-items', variantIds)
    },

    /**
     * 根據自訂選項取得價格
     * @param {number} id - 產品 ID
     * @param {Object} selectedOptions - 選中的顏色選項 { partCode: colorOptionId }
     * @returns {Promise<Object>} 價格資訊
     */
    getPriceByCustomization(id, selectedOptions) {
        return apiClient.post(`/Product/${id}/price`, selectedOptions)
    },

    /**
     * 批次檢查庫存
     * @param {Array<Object>} items - 要檢查的商品列表
     * @returns {Promise<Object>} 庫存檢查結果
     */
    checkStock(items) {
        return apiClient.post('/Product/check-stock', items)
    }
}

export default ProductAPI