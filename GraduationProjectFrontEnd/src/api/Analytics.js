import http from './axios'
import { mapToBestSellerDTOList } from '@/dtos/AnalyticsDTO'

/**
 * 取得熱銷排行資料
 * @param {object} [params]
 * @returns {Promise<BestSellerItem[]>}
 */
export async function getBestSellers(params) {
    const res = await http.get('/analytics/best-sellers', { params })
    return mapToBestSellerDTOList(res.data?.items)
}
