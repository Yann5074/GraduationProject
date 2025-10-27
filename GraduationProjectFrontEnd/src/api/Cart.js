import http from './axios'
import { handleApiResult } from '@/utils/apiHelper'
import { mapToCartDTOList } from '@/dtos/CartDTO'



// 列出購物車內容
export const getAllCarts = async () => {
    try {
        const result = await http.get('/Cart');
        return mapToCartDTOList(result.data);
    } catch (err) {
        console.error('取得購物車資料錯誤', err);
        throw err
    }
}
// 刪除購物車內某商品
export const deleteItem = async (cartItemId) => {
    try {
        const result = await http.delete(`/Cart/item/${cartItemId}`, cartItemId)
        return handleApiResult(result.data)
    } catch (err) {
        console.error('移除購物車商品失敗', err)
        throw err
    }
}
// 刪除購物車

// 編輯購物車物品數量
export const editCartItem = async (cartItemId, newQty) => {
    const reqDTO = {
        cartItemId: parseInt(cartItemId),
        qty: newQty
    }
    try {
        const result = await http.patch(`/Cart/item/${cartItemId}`, reqDTO)
        return handleApiResult(result.data)
    } catch (err) {
        console.error('編輯購物車物品數量失敗-cart.js', err)
        throw err
    }
}
// 商品加入購物車 (有登入)

// 確認購物車是否正常

// 登入時購物車轉換