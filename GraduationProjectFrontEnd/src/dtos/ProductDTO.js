/**
 * 商品篩選請求 DTO
 */
export interface ReqProductFilterDTO {
    pageNumber: number;        // 頁碼（從 1 開始）
    pageSize: number;          // 每頁筆數
    keyword?: string;          // 搜尋關鍵字
    categoryId?: number;       // 分類 ID
    minPrice?: number;         // 最低價格
    maxPrice?: number;         // 最高價格
    inStockOnly?: boolean;     // 只顯示有庫存
    customizableOnly?: boolean; // 只顯示可自訂
    sortBy?: string;           // 排序方式
}

/**
 * 商品列表項目 DTO
 */
export interface ResProductListDTO {
    fProductId: number;
    fName: string;
    fDescription: string;
    fCategoryId?: number;
    categoryName: string;
    fPstatus?: number;
    statusName: string;
    fWarrantyMonth?: number;
    fAssemblyRequired?: boolean;
    fDiscount?: number;

    // 圖片
    mainImageUrl: string;

    // 庫存
    totalStock: number;
    isAvailable: boolean;

    // 價格
    minPrice?: number;
    maxPrice?: number;

    // 自訂資訊
    isCustomizable: boolean;
    customizablePartsCount: number;
    availableCombinationsCount: number;

    // 時間
    fCreateTime?: string;
    fUpdateTime?: string;
}

/**
 * 分頁結果 DTO
 */
export interface ResultPagedDTO<T> {
    items: T[];              // 資料列表
    totalCount: number;      // 總筆數
    pageNumber: number;      // 當前頁碼
    pageSize: number;        // 每頁筆數
    totalPages: number;      // 總頁數
    hasPreviousPage: boolean; // 是否有上一頁
    hasNextPage: boolean;    // 是否有下一頁
}

/**
 * API 回應格式
 */
export interface ApiResponse<T> {
    success: boolean;
    message: string;
    data: T;
}