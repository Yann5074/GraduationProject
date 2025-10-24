import api from './axios'

// 列出訂單
export const getOrders = async () => {
    try {
        const result = await api.get('/Order');
        return result.data
    } catch (err) {
        console.error('取得訂單資料失敗: ', err);
        throw err
    }
}

// 刪除指定訂單
export const deleteOrder = async (orderId) => {
    try {
        const result = await api.delete(`/Order/${orderId}`);
        return result.data;
    } catch (err) {
        console.error('刪除訂單失敗: ', err);
        throw err;
    }
}

// 修改配送地址
export const EditDeliveryAddress = async (orderId, reqDTO) => {
    try {
        const result = await api.patch(`/Order/address/${orderId}`, reqDTO);
        return result.data;
    } catch (err) {
        console.log('更新配送地址失敗: ', err);
        throw err;
    }
}
