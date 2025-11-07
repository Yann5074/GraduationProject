/**
 * 將後端回傳的原始陣列轉成乾淨的 BestSeller DTO 陣列
 * @param {any[]} raw 從 API 回傳的 items 陣列
 * @returns {BestSellerItem[]}
 */
export const mapToBestSellerDTOList = (raw) => {
  if (!Array.isArray(raw)) return []
  return raw.map(mapBestSellerOne)
}

/**
 * 將單筆原始資料轉成 BestSellerItem
 * @param {any} x
 * @returns {BestSellerItem}
 */
export const mapBestSellerOne = (x) => ({
  productId: Number(x.productId) || 0,
  productName: x.productName ?? '',
  totalQuantity: Number.isFinite(x.totalQuantity) ? x.totalQuantity : 0,
  totalSalesAmount: Number(x.totalSalesAmount) || 0,
  variantCount: Number.isFinite(x.variantCount) ? x.variantCount : 0,
  topVariantId: x.topVariantId ?? null,
  topVariantSKU: x.topVariantSKU ?? null
})

/**
 * @typedef {Object} BestSellerItem
 * @property {number} productId
 * @property {string} productName
 * @property {number} totalQuantity
 * @property {number} totalSalesAmount
 * @property {number} variantCount
 * @property {number|null} [topVariantId]
 * @property {string|null} [topVariantSKU]
 */
