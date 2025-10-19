// 定義 Class
export class OrderDTO {
    constructor(data = {}) {
        this.orderId = data.orderId,
            this.employeeId = data.employeeId ?? null,
            this.employeeName = data.employeeName ?? null,
            this.orderStatusId = data.orderStatusId,
            this.orderStatusName = data.orderStatus
    }
}

// 資料轉換
export function mapToOrderDTOList(apiData = []) {
    if (!Array.isArray(apiData))
        return []
    return apiData.map(item => new OrderDTO(item))
}