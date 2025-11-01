import { formatCurrency, formatDateTime, formatImageUrl } from "@/utils/format"

// 訂單狀態的固定清單
const STATUS_STEPS = [
    { id: 1, label: '處理中' },
    { id: 2, label: '訂單成立' },
    { id: 3, label: '付款資訊確認' },
    { id: 4, label: '訂單出貨' },
    { id: 5, label: '訂單完成' },
]

// 定義 Class
export class OrderDTO {
    constructor({
        orderId = null,
        employeeName = null,
        orderTime = '',
        orderStatusId = 0,
        orderStatus = null,
        paymentMethod = null,
        paymentStatus = '',
        taxNo = '',
        deliveryAddress = '',
        deliveryStatus = null,
        totalPrice = 0,
        orderDetail = []
    } = {}) {
        this.orderId = orderId
        this.employeeName = employeeName
        this.orderTime = orderTime
        this.orderStatusId = orderStatusId
        this.orderStatus = orderStatus
        this.paymentMethod = paymentMethod
        this.paymentStatus = paymentStatus
        this.taxNo = taxNo
        this.deliveryAddress = deliveryAddress
        this.deliveryStatus = deliveryStatus
        this.totalPrice = totalPrice
        this.orderDetail = orderDetail.map(OrderDetailDTO.fromApi) // 巢狀 DTO 處理
    }

    //透過Getter產生進度條節點
    get statusSteps() {
        const currentId = this.orderStatusId;
        const total = STATUS_STEPS.length - 1;
        return STATUS_STEPS.map((step, index) => ({
            label: step.label,
            active: step.id <= currentId,
            leftPercent: (index / total) * 100
        }))
    }

    // 透過Getter產生進度線百分比
    get progressPercent() {
        if (!this.orderStatusId)
            return 0
        const currentIndex = STATUS_STEPS.findIndex(
            (s) => s.id == this.orderStatusId
        )
        if (currentIndex === -1)
            return 0
        return ((currentIndex / (STATUS_STEPS.length - 1)) * 100) - 0.5
    }

    //時間格式轉換
    get formatOrderTime() {
        return formatDateTime(this.orderTime);
    }
    //金錢格式轉換
    get formatTotalPrice() {
        return formatCurrency(this.totalPrice);
    }

    // 靜態工廠方法 - 替代建構子建立統一入口，並可控制內部邏輯
    static fromApi(data = {}) {
        return new OrderDTO(data)
    }
}

export class OrderDetailDTO {
    constructor({
        productName = null,
        productInfo = null,
        unitPrice = 0,
        quantity = 0,
        subtotal = 0,
        imageUrl = '',
    } = {}) {
        this.productName = productName
        this.productInfo = productInfo
        this.unitPrice = unitPrice
        this.quantity = quantity
        this.subtotal = this.calculateSubtotal()
        this.imageUrl = formatImageUrl(imageUrl)
    }

    // method
    calculateSubtotal() {
        return this.quantity * this.unitPrice
    }

    get formatunitPrice() {
        return formatCurrency(this.unitPrice)
    }

    get formatsubtotal() {
        return formatCurrency(this.subtotal)
    }

    static fromApi(data = {}) {
        return new OrderDetailDTO(data)
    }
}

// 資料轉換
export function mapToOrderDTOList(apiData = []) {
    if (!Array.isArray(apiData))
        return []
    return apiData.map(OrderDTO.fromApi)
}