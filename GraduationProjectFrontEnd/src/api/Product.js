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
                sortBy: filter.sortBy || 'created_desc',
                pageNumber: filter.pageNumber || 1,
                pageSize: filter.pageSize || 12
            }

            if (filter.categoryId !== undefined && filter.categoryId !== null) {
                params.categoryId = filter.categoryId
            }




            // 移除空值
            Object.keys(params).forEach(key => {
                if (params[key] === undefined || params[key] === null || params[key] === '') {
                    delete params[key]
                }
            })

            console.log('[Product.js] 呼叫 API:', '/Product/all', params)

            const response = await http.get('/Product/all', { params })



            // 方案1: 如果 axios 攔截器已經返回 response
            // 檢查 response 的結構
            let productData = []
            let paginationData = {}

            // 情況 A: response 本身就是資料
            if (response.ok && response.data) {
                console.log('[路徑A] response.ok = true, 使用 response.data')
                productData = response.data.data || response.data
                paginationData = response.data.pagination || {}
            }
            // 情況 B: response.data 裡有 success
            else if (response.data?.success) {
                console.log('[路徑B] response.data.success = true')
                productData = response.data.data?.data || response.data.data || []
                paginationData = response.data.data?.pagination || response.data.pagination || {}
            }
            // 情況 C: response 直接是陣列
            else if (Array.isArray(response)) {
                console.log('[路徑C] response 是陣列')
                productData = response
            }
            // 情況 D: response.data 是陣列
            else if (Array.isArray(response.data)) {
                console.log('[路徑D] response.data 是陣列')
                productData = response.data
            }
            // 情況 E: response.data.data 是陣列
            else if (Array.isArray(response.data?.data)) {
                console.log('[路徑E] response.data.data 是陣列')
                productData = response.data.data
                paginationData = response.data.pagination || {}
            }


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
            console.error('[Product.js] 取得產品列表失敗:', error)
            console.error('[Product.js] 錯誤詳情:', {
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

            console.log('搜尋回應:', response)

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
            console.error('搜尋產品失敗:', error)
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

            console.log('[Product.js] 篩選選項原始回應:', response)
            console.log('[Product.js] response.data:', response.data)
            console.log('[Product.js] response.data.data:', response.data?.data)
            console.log('Product.js] response.success:', response.success)


            let data = null

            // 情況 A: response.data.success 結構
            if (response.data?.success) {
                console.log('[路徑A] response.data.success = true')
                data = response.data.data
                console.log('[路徑A] data:', data)
            }
            // 情況 B: response.success 結構
            else if (response.success) {
                console.log('[路徑B] response.success = true')
                data = response.data
                console.log('[路徑B] data:', data)
            }
            // 情況 C: 直接是資料
            else if (response.data) {
                console.log('[路徑C] 使用 response.data')
                data = response.data
                console.log('[路徑C] data:', data)
            }
            // 情況 D: response 本身就是資料
            else {
                console.log('[路徑D] 使用 response')
                data = response
                console.log('[路徑D] data:', data)
            }




            const categories = data?.categories || data?.Categories || []
            const colors = data?.colors || data?.Colors || []
            const textures = data?.textures || data?.Textures || []
            const priceRange = data?.priceRange || data?.PriceRange || { minPrice: 0, maxPrice: 100000 }


            // 驗證 categories 是否為陣列
            if (!Array.isArray(categories)) {
                console.error('[Product.js] categories 不是陣列:', categories)
            } else if (categories.length === 0) {

            }

            return {
                success: true,
                data: {
                    categories: Array.isArray(categories) ? categories : [],
                    colors: Array.isArray(colors) ? colors : [],
                    textures: Array.isArray(textures) ? textures : [],
                    priceRange: priceRange
                }
            }
        } catch (error) {
            console.error('[Product.js] 取得篩選選項失敗:', error)
            console.error('[Product.js] 錯誤詳情:', error.response)
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
    },

    /**
     * 取得產品詳情
     */
    async getProductById(productId) {
        try {
            const response = await http.get(`/Product/${productId}`)

            console.log('產品詳情回應:', response)

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
            console.error('取得產品詳情失敗:', error)
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
            const response = await http.get(`/Product/${productId}/similar`, { params: { count } })
            console.log('相似產品回應:', response)

            const payload = response?.data
            if (payload?.success) {
                return {
                    success: true,
                    data: payload.data || []
                }
            }

            // 後備容錯：如果後端哪天改成直接回陣列
            if (Array.isArray(payload)) {
                return { success: true, data: payload }
            }
            if (Array.isArray(response)) {
                return { success: true, data: response }
            }

            return { success: true, data: [] }
        } catch (error) {
            console.error('取得相似產品失敗:', error)
            return { success: false, message: error.message || '取得相似產品失敗', data: [] }
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
            console.error('檢查庫存失敗:', error)
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
            console.error('取得類別列表失敗:', error)
            return {
                success: false,
                message: error.message || '取得類別列表失敗',
                data: []
            }
        }
    },

    async getPBR(productId, variantId = null) {
        const params = {}
        if (variantId) params.variantId = variantId
        const res = await http.get(`/Product/${productId}/pbr`, { params })

        const dto = res?.data || res
        return dto

    }
}

export default ProductAPI