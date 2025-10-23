/**
 * 產品篩選請求 DTO
 */
export class ReqProductFilterDTO {
    constructor(data = {}) {
        this.categoryId = data.categoryId || null
        this.minPrice = data.minPrice || null
        this.maxPrice = data.maxPrice || null
        this.statusId = data.statusId || null
        this.keyword = data.keyword || ''
        this.sortBy = data.sortBy || 'created_desc'
        this.pageNumber = data.pageNumber || 1
        this.pageSize = data.pageSize || 12
    }
}

/**
 * 產品列表項 DTO
 */
export class ResProductListDTO {
    constructor(data = {}) {
        this.fProductId = data.fProductId || 0
        this.fName = data.fName || ''
        this.fDescription = data.fDescription || ''
        this.fCategoryId = data.fCategoryId || null
        this.categoryName = data.categoryName || ''
        this.fPstatus = data.fPstatus || null
        this.statusName = data.statusName || ''
        this.fWarrantyMonth = data.fWarrantyMonth || null
        this.fAssemblyRequired = data.fAssemblyRequired || false
        this.fDiscount = data.fDiscount || null
        this.mainImageUrl = data.mainImageUrl || ''
        this.totalStock = data.totalStock || 0
        this.isAvailable = data.isAvailable || false
        this.minPrice = data.minPrice || null
        this.maxPrice = data.maxPrice || null
        this.isCustomizable = data.isCustomizable || false
        this.customizablePartsCount = data.customizablePartsCount || 0
        this.availableCombinationsCount = data.availableCombinationsCount || 0
        this.fCreateTime = data.fCreateTime || null
        this.fUpdateTime = data.fUpdateTime || null
    }
}

/**
 * 分頁資訊 DTO
 */
export class ResPaginationDTO {
    constructor(data = {}) {
        this.currentPage = data.currentPage || 1
        this.pageSize = data.pageSize || 12
        this.totalPages = data.totalPages || 0
        this.totalCount = data.totalCount || 0
        this.hasNext = data.hasNext || false
        this.hasPrevious = data.hasPrevious || false
    }
}

/**
 * 分頁結果 DTO
 */
export class ResultPagedDTO {
    constructor(data = {}) {
        this.data = (data.data || []).map(item => new ResProductListDTO(item))
        this.pagination = new ResPaginationDTO(data.pagination || {})
    }
}

/**
 * API 回應 DTO
 */
export class ResApiResponseDTO {
    constructor(data = {}) {
        this.success = data.success || false
        this.message = data.message || ''
        this.data = data.data || null
    }
}

/**
 * 類別選項 DTO
 */
export class ResCategoryOptionDTO {
    constructor(data = {}) {
        this.categoryId = data.categoryId || 0
        this.name = data.name || ''
        this.parentCategoryId = data.parentCategoryId || null
        this.isActive = data.isActive || false
        this.sortOrder = data.sortOrder || 0
    }
}

/**
 * 價格範圍 DTO
 */
export class ResPriceRangeDTO {
    constructor(data = {}) {
        this.minPrice = data.minPrice || 0
        this.maxPrice = data.maxPrice || 0
    }
}

/**
 * 狀態選項 DTO
 */
export class ResStatusOptionDTO {
    constructor(data = {}) {
        this.statusId = data.statusId || 0
        this.statusName = data.statusName || ''
    }
}

/**
 * 篩選選項 DTO
 */
export class ResFilterOptionsDTO {
    constructor(data = {}) {
        this.categories = (data.categories || []).map(c => new ResCategoryOptionDTO(c))
        this.priceRange = new ResPriceRangeDTO(data.priceRange || {})
        this.statusOptions = (data.statusOptions || []).map(s => new ResStatusOptionDTO(s))
    }
}

/**
 * 產品 DTO（搜尋用）
 */
export class ResProductDTO {
    constructor(data = {}) {
        this.productId = data.productId || 0
        this.name = data.name || ''
        this.categoryId = data.categoryId || null
        this.categoryName = data.categoryName || ''
        this.minPrice = data.minPrice || null
        this.maxPrice = data.maxPrice || null
        this.primaryImageUrl = data.primaryImageUrl || ''
        this.variants = data.variants || []
        this.assets = data.assets || []
    }
}

// 排序選項常數
export const SORT_OPTIONS = [
    { value: 'created_desc', label: '最新上架' },
    { value: 'created_asc', label: '最早上架' },
    { value: 'price_asc', label: '價格由低到高' },
    { value: 'price_desc', label: '價格由高到低' },
    { value: 'name_asc', label: '名稱 A-Z' },
    { value: 'name_desc', label: '名稱 Z-A' }
]

export default {
    ReqProductFilterDTO,
    ResProductListDTO,
    ResPaginationDTO,
    ResultPagedDTO,
    ResApiResponseDTO,
    ResCategoryOptionDTO,
    ResPriceRangeDTO,
    ResStatusOptionDTO,
    ResFilterOptionsDTO,
    ResProductDTO,
    SORT_OPTIONS
}