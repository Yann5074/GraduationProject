import { formatCurrency, formatImageUrl } from "@/utils/format"

// 定義 Class
export class CartDTO{
    constructor({
        cartId = null,
        totalPrice = 0,
        cartItem = []
    } = {}){
        this.cartId = cartId
        this.totalPrice = totalPrice
        this.cartItem = cartItem.map(CartItemDTO.fromApi)
    }

    get formatTotalPrice(){
        return formatCurrency(this.totalPrice)
    }

    static fromApi(data = {}){
        return new CartDTO(data)
    }
}


export class CartItemDTO{
    constructor({
        cartItemId = 0,
        productVariantId = 0,
        productName = '',
        unitPrice = 0,
        qty = 0,
        subtotal = 0,
        imageUrl = '',
        size = ''
    } = {}){
        this.cartItemId = cartItemId
        this.productVariantId = productVariantId
        this.productName = productName
        this.unitPrice = unitPrice
        this.qty = qty
        this.subtotal = this.calculateSubtotal()
        this.imageUrl = formatImageUrl(imageUrl)
        this.size = size
    }

    calculateSubtotal(){
        return this.qty * this.unitPrice
    }

    get formatunitPrice(){
        return formatCurrency(this.unitPrice)
    }

    get formatsubtotal(){
        return formatCurrency(this.subtotal)
    }

    static fromApi(data = {}){
        return new CartItemDTO(data)
    }
}

// 資料轉換
export function mapToCartDTOList(apiData = []){
    if(!Array.isArray(apiData))
        return []
    return apiData.map(CartDTO.fromApi)
}