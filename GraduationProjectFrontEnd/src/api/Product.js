import axios from 'axios'
import http from '@/api/axios'

// const API_BASE = import.meta.env.VITE_API_BASE_URL || 'https://localhost:7131'

// //  axios 實例
// const apiClient = axios.create({
//     baseURL: `${API_BASE}/api`,
//     timeout: 10000,
//     headers: {
//         'Content-Type': 'application/json'
//     }
// })

// // 請求攔截器
// apiClient.interceptors.request.use(
//     (config) => {
//         // 可以在這裡加入 token 等認證資訊
//         return config
//     },
//     (error) => {
//         return Promise.reject(error)
//     }
// )

// // 回應攔截器
// apiClient.interceptors.response.use(
//     (response) => {
//         return response.data
//     },
//     (error) => {
//         console.error('API Error:', error)
//         return Promise.reject(error)
//     }
// )

/**
 * 產品 API 服務
 */
export const ProductAPI = {
    /**
     * 取得產品列表（分頁）
     */
    async getProducts(filter = {}) {
        try {
            const params = {
                categoryId: filter.categoryId,
                minPrice: filter.minPrice,
                maxPrice: filter.maxPrice,
                colorIds: filter.colorIds,
                textureIds: filter.textureIds,
                searchKeyword: filter.keyword || filter.searchKeyword,
                sortBy: filter.sortBy || 'newest',
                pageNumber: filter.pageNumber || 1,
                pageSize: filter.pageSize || 12
            }

            // 移除空值
            Object.keys(params).forEach(key => {
                if (params[key] === undefined || params[key] === null || params[key] === '') {
                    delete params[key]
                }
            })

            console.log('📡 [Product.js] 呼叫 API:', '/Product/all', params)

            const response = await http.get('/Product/all', { params })

            console.log('📥 [Product.js] 原始 response:', response)
            console.log('📥 [Product.js] response.data:', response.data)
            console.log('📥 [Product.js] response.ok:', response.ok)
            console.log('📥 [Product.js] response.success:', response.success)

            // 🔧 方案1: 如果 axios 攔截器已經返回 response
            // 檢查 response 的結構
            let productData = []
            let paginationData = {}

            // 情況 A: response 本身就是資料
            if (response.ok && response.data) {
                console.log('✅ [路徑A] response.ok = true, 使用 response.data')
                productData = response.data.data || response.data
                paginationData = response.data.pagination || {}
            }
            // 情況 B: response.data 裡有 success
            else if (response.data?.success) {
                console.log('✅ [路徑B] response.data.success = true')
                productData = response.data.data?.data || response.data.data || []
                paginationData = response.data.data?.pagination || response.data.pagination || {}
            }
            // 情況 C: response 直接是陣列
            else if (Array.isArray(response)) {
                console.log('✅ [路徑C] response 是陣列')
                productData = response
            }
            // 情況 D: response.data 是陣列
            else if (Array.isArray(response.data)) {
                console.log('✅ [路徑D] response.data 是陣列')
                productData = response.data
            }
            // 情況 E: response.data.data 是陣列
            else if (Array.isArray(response.data?.data)) {
                console.log('✅ [路徑E] response.data.data 是陣列')
                productData = response.data.data
                paginationData = response.data.pagination || {}
            }

            console.log('🎯 [Product.js] 最終 productData:', productData)
            console.log('📊 [Product.js] 最終 paginationData:', paginationData)
            console.log('🔢 [Product.js] 產品數量:', Array.isArray(productData) ? productData.length : 0)

            return {
                success: true,
                data: productData,
                pagination: {
                    totalCount: paginationData.totalCount || productData.length || 0,
                    pageSize: paginationData.pageSize || params.pageSize,
                    currentPage: paginationData.currentPage || params.pageNumber,
                    totalPages: paginationData.totalPages || Math.ceil((paginationData.totalCount || productData.length) / params.pageSize)
                }
            }
        } catch (error) {
            console.error('❌ [Product.js] 取得產品列表失敗:', error)
            console.error('❌ [Product.js] 錯誤詳情:', {
                message: error.message,
                response: error.response,
                status: error.response?.status
            })
            return {
                success: false,
                message: error.message || '取得產品列表失敗',
                data: [],
                pagination: {
                    totalCount: 0,
                    pageSize: filter.pageSize || 12,
                    currentPage: filter.pageNumber || 1,
                    totalPages: 0
                }
            }
        }
    },

    /**
     * 根據關鍵字搜尋產品
     */
    async searchProductsByKeyword(keyword) {
        try {
            if (!keyword || keyword.trim() === '') {
                return {
                    success: false,
                    message: '請提供搜尋關鍵字',
                    data: []
                }
            }

            // 修正：後端路由是 /Product/search/{keyword}
            const response = await http.get(`/Product/search/${encodeURIComponent(keyword.trim())}`)

            console.log('🔍 搜尋回應:', response)

            // 解析包裝的回應
            if (response && response.success) {
                return {
                    success: true,
                    data: response.data || []
                }
            }

            return {
                success: true,
                data: Array.isArray(response) ? response : []
            }
        } catch (error) {
            console.error('❌ 搜尋產品失敗:', error)
            return {
                success: false,
                message: error.message || '搜尋產品失敗',
                data: []
            }
        }
    },

    /**
     * 取得篩選選項
     */
    async getFilterOptions() {
        try {
            const response = await http.get('/Product/filter-options')

            console.log('🎛️ 篩選選項回應:', response)

            // 解析包裝的回應
            if (response && response.success) {
                const data = response.data
                return {
                    success: true,
                    data: {
                        categories: data?.categories || [],
                        colors: data?.colors || [],
                        textures: data?.textures || [],
                        priceRange: data?.priceRange || { minPrice: 0, maxPrice: 100000 }
                    }
                }
            }

            // 後備
            return {
                success: true,
                data: {
                    categories: response?.categories || [],
                    colors: response?.colors || [],
                    textures: response?.textures || [],
                    priceRange: response?.priceRange || { minPrice: 0, maxPrice: 100000 }
                }
            }
        } catch (error) {
            console.error('❌ 取得篩選選項失敗:', error)
            return {
                success: false,
                message: error.message || '取得篩選選項失敗',
                data: {
                    categories: [],
                    colors: [],
                    textures: [],
                    priceRange: { minPrice: 0, maxPrice: 100000 }
                }
            }
        }
        return http.get(`/Product/search/${encodeURIComponent(keyword)}`)
    },

    /**
     * 取得產品詳情
     */
    async getProductById(productId) {
        try {
            const response = await http.get(`/Product/${productId}`)

            console.log('📦 產品詳情回應:', response)

            // 解析包裝的回應
            if (response && response.data.success) {
                const productData = response.data
                console.log('123', productData)
                if (!productData || !productData.data.fProductId) {
                    return {
                        success: false,
                        message: '產品不存在',
                        data: null
                    }
                }

                return {
                    success: true,
                    data: productData.data
                }
            }

            // 後備：直接回傳
            if (response && response.fProductId) {
                return {
                    success: true,
                    data: response
                }
            }

            return {
                success: false,
                message: '產品不存在',
                data: null
            }
        } catch (error) {
            console.error('❌ 取得產品詳情失敗:', error)
            return {
                success: false,
                message: error.message || '取得產品詳情失敗',
                data: null
            }
        }
    },


    async getProductVariants(productId) {
        const response = await http.get(`/Product/${productId}/variants`)
        return response.data
    },

    /**
     * 取得相似產品
     */
    async getSimilarProducts(productId, count = 4) {
        try {
            const response = await http.get(`/Product/${productId}/similar`, {
                params: { count }
            })

            console.log('🔄 相似產品回應:', response)

            // 解析包裝的回應
            if (response && response.success) {
                return {
                    success: true,
                    data: response.data || []
                }
            }

            return {
                success: true,
                data: Array.isArray(response) ? response : []
            }
        } catch (error) {
            console.error('❌ 取得相似產品失敗:', error)
            return {
                success: false,
                message: error.message || '取得相似產品失敗',
                data: []
            }
        }
    },

    /**
     * 批次取得購物車產品資訊
     */
    getCartProducts(variantIds) {
        return http.post('/Product/cart-items', variantIds)
    },

    /**
     * 根據客製化選項取得價格
     */
    async getPriceByCustomization(productId, selectedOptions) {
        try {
            if (!selectedOptions || Object.keys(selectedOptions).length === 0) {
                return {
                    success: false,
                    message: '請提供客製化選項',
                    data: null
                }
            }

            const response = await http.post(`/Product/${productId}/price`, selectedOptions)

            // 解析包裝的回應
            if (response && response.success) {
                return {
                    success: true,
                    data: response.data
                }
            }

            return {
                success: true,
                data: response
            }
        } catch (error) {
            console.error('❌ 取得客製化價格失敗:', error)
            return {
                success: false,
                message: error.message || '取得客製化價格失敗',
                data: null
            }
        }
    },

    /**
     * 批次檢查庫存
     */
    async checkStock(items) {
        try {
            if (!Array.isArray(items) || items.length === 0) {
                return {
                    success: false,
                    message: '請提供要檢查的商品',
                    data: {
                        allAvailable: false,
                        items: []
                    }
                }
            }

            const response = await http.post('/Product/check-stock', items)

            // 解析包裝的回應
            if (response && response.success) {
                return {
                    success: true,
                    data: response.data || { allAvailable: false, items: [] }
                }
            }

            return {
                success: true,
                data: {
                    allAvailable: response?.allAvailable || false,
                    items: response?.items || []
                }
            }
        } catch (error) {
            console.error('❌ 檢查庫存失敗:', error)
            return {
                success: false,
                message: error.message || '檢查庫存失敗',
                data: {
                    allAvailable: false,
                    items: []
                }
            }
        }
    },

    /**
     * 取得產品類別列表
     */
    async getCategories() {
        try {
            const response = await http.get('/Category')
            return {
                success: true,
                data: Array.isArray(response) ? response : (response.data || [])
            }
        } catch (error) {
            console.error('❌ 取得類別列表失敗:', error)
            return {
                success: false,
                message: error.message || '取得類別列表失敗',
                data: []
            }
        }
    }
}

export default ProductAPI