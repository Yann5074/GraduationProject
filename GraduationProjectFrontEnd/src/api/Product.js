import axios from 'axios'

const API_BASE = import.meta.env.VITE_API_BASE_URL || 'https://localhost:7131'

//  axios 實例
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

    // 取得產品詳情
    async getProductById(productId, includeCustomization = true) {  // ⭐ 預設為 true
        try {
            // ⭐ 加入 query parameter
            const url = `/Product/${productId}?includeCustomization=${includeCustomization}`

            const response = await apiClient.get(url)

            console.log('🔍 原始 API 回應:', response.data)

            const data = response.data

            // 如果後端已經有 success 包裝
            if (data.hasOwnProperty('success')) {
                return data
            }

            // 如果後端直接回傳產品物件
            if (data.fProductId) {
                return {
                    success: true,
                    message: '取得產品詳情成功',
                    data: data
                }
            }

            return {
                success: false,
                message: '產品資料格式錯誤',
                data: null
            }

        } catch (error) {
            console.error('取得產品詳情失敗:', error)
            return {
                success: false,
                message: error.response?.data?.message || '取得產品詳情失敗',
                data: null
            }
        }
    },

    // 取得產品變體
    async getProductVariants(productId) {
        const response = await apiClient.get(`/Product/${productId}/variants`)
        return response.data
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