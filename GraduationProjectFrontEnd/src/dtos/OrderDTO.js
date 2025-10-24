// 定義 Class
export class OrderDTO {
    constructor({
        orderId = null,
        employeeName = null,
        orderTime = '',
        orderStatus = null,
        totalPrice = 0,
        orderDetail = []
    } = {}) {
        this.orderId = orderId
        this.employeeName = employeeName
        this.orderTime = orderTime
        this.orderStatus = orderStatus
        this.totalPrice = totalPrice
        this.orderDetail = orderDetail.map(OrderDetailDTO.fromApi) // 巢狀 DTO 處理
    }

    // 靜態工廠方法 - 替代建構子建立統一入口，並可控制內部邏輯
    static fromApi(data = {}){
        return new OrderDTO(data)
    }
}

export class OrderDetailDTO {
    constructor({
        productName = null,
        productInfo = null,
        unitPrice = 0,
        quantity = 0,
        imageUrl = '',
    } = {}){
        this.productName = productName
        this.productInfo = productInfo
        this.unitPrice = unitPrice
        this.quantity = quantity
        this.imageUrl = imageUrl
    }

    static fromApi(data = {}){
        return new OrderDetailDTO(data)
    }
}

// 資料轉換
export function mapToOrderDTOList(apiData = []) {
    if (!Array.isArray(apiData))
        return []
    return apiData.map(OrderDTO.fromApi)
}