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
     * 取得產品類別列表
     * @returns {Promise<Object>} 類別列表
     */
    async getCategories() {
        try {
            const response = await apiClient.get('/Category')
            return response
        } catch (error) {
            console.error('取得類別列表失敗:', error)
            return {
                success: false,
                message: error.response?.data?.message || '取得類別列表失敗',
                data: []
            }
        }
    },

    /**
     * 取得篩選選項（包含類別和價格範圍）
     * @returns {Promise<Object>} 篩選選項資料
     */
    async getFilterOptions() {
        try {
            // 方法 1: 如果後端有統一的篩選選項 API，取消註解下面這行
            // return apiClient.get('/Product/filter-options')

            // 方法 2: 組合類別 API 和預設價格範圍
            const categoriesRes = await this.getCategories()

            return {
                success: true,
                data: {
                    categories: categoriesRes.data || categoriesRes || [],
                    priceRanges: [
                        { min: 0, max: 5000, label: '5000 以下' },
                        { min: 5000, max: 10000, label: '5000 - 10000' },
                        { min: 10000, max: 20000, label: '10000 - 20000' },
                        { min: 20000, max: 50000, label: '20000 - 50000' },
                        { min: 50000, max: null, label: '50000 以上' }
                    ]
                }
            }
        } catch (error) {
            console.error('取得篩選選項失敗:', error)
            return {
                success: false,
                message: '取得篩選選項失敗',
                data: {
                    categories: [],
                    priceRanges: []
                }
            }
        }
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
     * @param {number} productId - 產品 ID
     * @param {boolean} includeCustomization - 是否包含客製化資訊（包含 3D 模型路徑）
     * @returns {Promise<Object>} 產品詳情
     */
    async getProductById(productId, includeCustomization = true) {
        try {
            // ⭐ includeCustomization=true 會取得 3D 模型路徑
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

    /**
     * 取得產品變體
     * @param {number} productId - 產品 ID
     * @returns {Promise<Object>} 產品變體列表
     */
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
     * 根據自訂選項取得價格（用於 3D 客製化）
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