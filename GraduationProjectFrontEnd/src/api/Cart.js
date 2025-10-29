import http from './axios'
import { handleApiResult } from '@/utils/apiHelper'
import { mapToCartDTOList } from '@/dtos/CartDTO'



// 列出購物車內容
export const getAllCarts = async () => {
    try {
        const result = await http.get('/Cart');
        const carts = mapToCartDTOList(result.data);
        if (!Array.isArray(carts)){
            return [];
        }
        return carts;
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
export const deleteCart = async (cartId) => {
    try{
        const result = await http.delete(`/Cart/${cartId}`)
        return handleApiResult(result.data)
    }catch (err){
        console.error('清空購物車失敗', err)
        throw err
    }
}
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
export const memberAddToCart = async (reqDTO) =>{
    try{
        const result = await http.post(`/Cart/item`, reqDTO)
        return handleApiResult(result.data)
    }catch(err){
        console.error('加入購物車失敗', err)
        throw err
    }
}
// 確認購物車是否正常
export const checkCart = async () =>{
    try{
        const result = await http.get('/Cart/Check')
        return handleApiResult(result.data)
    }catch(err){
        console.error('確認購物車失敗', err)
        throw err
    }
}
// 登入時購物車轉換
export const syncCart = async (reqDTO) =>{
    try{
        const result = await http.post('/Cart/sync', reqDTO)
        return handleApiResult(result.data)
    }catch(err){
        console.error('同步購物車發生錯誤', err)
        throw err
    }
}