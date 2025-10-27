import http from './axios'
import { handleApiResult } from '@/utils/apiHelper';
import { mapToOrderDTOList } from '@/dtos/OrderDTO';

// 列出訂單
export const getAllOrders = async () => {
    try {
        const result = await http.get('/Order');
        return mapToOrderDTOList(result.data);
    } catch (err) {
        console.error('取得訂單資料失敗: ', err);
        throw err
    }
}

// 尋找指定訂單
export const lookupOrder = async (keyword) => {
    try {
        const result = await http.get(`/Order/${encodeURIComponent(keyword)}`);
        return mapToOrderDTOList(result.data);
    } catch (err) {
        console.error('查無指定訂單:', err);
        throw err
    }
}

// 刪除指定訂單
export const deleteOrder = async (orderId) => {
    try {
        const result = await http.delete(`/Order/${orderId}`);
        return handleApiResult(result.data) // 使用result.data是因回傳值為雙層結構
    } catch (err) {
        console.error('刪除訂單失敗: ', err);
        throw err;
    }
}

// 更改配送地址
export const EditDeliveryAddress = async (orderId, newAddress) => {
    const reqDTO = {
        orderId: parseInt(orderId),
        address: newAddress
    }
    try {
        const result = await http.patch(`/Order/address`, reqDTO);
        return handleApiResult(result.data);
    } catch (err) {
        console.log('更新配送地址失敗: ', err);
        throw err;
    }
}

// 更改統編
export const EditTaxNo = async (orderId, newTaxNo) => {
    const reqDTO = {
        orderId: parseInt(orderId),
        taxNo: newTaxNo
    }
    try {
        const result = await http.patch('/Order/taxno', reqDTO);
        return handleApiResult(result.data);
    } catch (err) {
        console.log('更新統編失敗', err);
        throw err;
    }
}
// 會員結帳
export const memberCheckOut = async (reqDTO) => {
    try {
        const result = await http.post('/Order/CheckOut', reqDTO);
        return handleApiResult(result.data);
    } catch (err) {
        console.log('新增訂單失敗: ', err);
        throw err;
    }
}

//訪客新增訂單 #TODO
