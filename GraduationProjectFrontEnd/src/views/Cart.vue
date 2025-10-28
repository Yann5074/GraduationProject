<template>
  <div class="title d-flex align-items-center mb-2">
    <i class="bi bi-cart"></i>
    <h3>購物車</h3>
  </div>
  <!-- 購物車列表 -->
<div v-if="cartItems.length > 0" class="untree_co-section before-footer-section">
    <div class="container">
      <div class="row mb-5">
        <div class="d-flex justify-content-end">
          <button class="btn btn-warning" @click="toggleClean">
            清空購物車
          </button>
        </div>
        <form class="col-md-12" method="post">
          <div class="site-blocks-table">
            <table class="table">
              <thead>
                <tr>
                  <th class="product-thumbnail"></th>
                  <th class="product-name">商品名稱</th>
                  <th class="product-price">單價</th>
                  <th class="product-quantity">數量</th>
                  <th class="product-total">小計</th>
                  <th class="product-remove">刪除</th>
                </tr>
              </thead>

              <!-- 購物車內容載入 -->
              <tbody>
                <tr v-for="ci in cartItems" :key="ci.cartItemId">
                  <td class="product-thumbnail">
                    <img :src="ci.imageUrl" class="img-fluid"></img>
                  </td>
                  <td class="product-name">
                    <h2 class="h5 text-black">
                      {{ ci.productName }}
                    </h2>
                    <div class="text-muted small">
                      規格: {{ci.size}}
                    </div>
                  </td>
                  <td>
                    $ {{ ci.formatunitPrice }}
                  </td>
                  <td class="text-center align-middle">
                    <QtyControl v-model="ci.qty" :min="1" :max="99" @update:model-value="onQtyChange(ci, $event)"/>
                  </td>
                  <td class="subtotal">
                    $ {{ (ci.qty * ci.unitPrice).toLocaleString() }}
                  </td>
                  <td>
                    <button type="button" class="btn btn-black btn-sm" @click="removeItem(ci.cartItemId)">
                      <i class="bi bi-x-octagon"></i>
                    </button>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
          <!-- 合計欄位 -->
          <div class="text-end mt-4">
            <h5>
              結帳金額:
              <span class="totalPrice">
                {{ formatTotal }}
              </span>
                <button class="btn btn-danger custom-checkout-btn ms-3 me-3">
                  結帳
                </button>
            </h5>
          </div>
        </form>
      </div>
    </div>
  </div>
  <!-- 購物車為空時的畫面 -->
  <div v-else class="text-center p-5 text-muted">
    <i class="bi bi-cart-x" style="font-size: 3rem;"></i>
    <h4 class="mt-3">購物車為空，請繼續購物!</h4>
    <router-link to="/products" class="btn btn-outline-primary mt-3"> <!-- #TODO 加上商品頁正確跳轉-->
      返回商品頁
    </router-link>
  </div>
  <!-- <RouterView /> -->
  <ConfirmModal ref="confirmModalRef" title="清空購物車" message="是否確定要清空購物車 ?" @confirm="cleanCart" />
</template>

<style scoped>
.title{
  margin: 20px;
  font-size: 30px;

}
.totalPrice{
  font-size: bold;
  color: red;
}
.subtotal{
  color:red;
  font-size: bold;
}
.untree_co-section.before-footer-section{
  padding-top: 0rem !important;
  margin-top: 0rem !important;
}
.input-group{
  justify-content: center;
  align-items: center !important;
}
.site-blocks-table table {
  border-collapse: collapse !important;
  border-spacing: 0 !important;
}

.site-blocks-table th,
.site-blocks-table td {
  padding: 0.5rem 0.75rem !important;
  vertical-align: middle;
}

.table td.product-quantity {
  text-align: center;
}
.table td, .table th {
  vertical-align: middle !important;
}
.custom-checkout-btn{
  width: 200px;
  height: 50px;
  font-size: 18px;
}
</style>


<script setup>
import { onMounted, ref, computed } from 'vue'
import { getAllCarts, deleteItem, editCartItem, deleteCart } from '@/api/Cart'
import QtyControl from '@/components/QtyControl.vue'
import ConfirmModal from '@/components/ConfirmModal.vue'

// 儲存購物車資訊
const cartId = ref(null);
// 顯示購物車相關
const cartItems = ref([])
// 清空購物車相關
const confirmModalRef = ref(null)
let currentCart = null;
// 更改數量相關
let isRestoring  = false

// 動態計數&呼叫數量變動API
const onQtyChange = async (ci, newQty) =>{
  if(isRestoring)
    return

  let stock = null;
  // const oldqty = ci.qty // 原本的輸入數量，可選
  ci.qty = newQty
  ci.subtotal = ci.unitPrice * newQty

  try{
    const result = await editCartItem(ci.cartItemId, ci.qty)
    if (!result.ok){
      stock = parseMaxStockFromMessage(result.message)  //庫存的最大數量，可選

      isRestoring = true

      ci.qty = stock
      ci.subtotal = ci.unitPrice * stock

      isRestoring = false
    }
  }catch(err){
    stock = parseMaxStockFromMessage(err.message) //庫存的最大數量，可選
    console.log(stock)
    ci.qty = stock
    ci.subtotal = ci.unitPrice * stock
    console.error('更新購物車商品數量失敗-.vue', err)
    alert(err.message)

    isRestoring = false
  }
}

// 解析訊息中數量
const parseMaxStockFromMessage = (msg) =>{
  const match = msg.match(/最大可選值為\s*(\d+)/)
  const maxStock = match ? parseInt(match[1], 10) : null
  return maxStock
}

// 動態總金額計算
const formatTotal = computed(() =>{
  const sum = cartItems.value.reduce(
    (acc, item) => acc + item.subtotal, 0
  )
  return new Intl.NumberFormat('zh-TW', {
    style: 'currency',
    currency: 'TWD',
    minimumFractionDigits: 0, //不顯示最小小數
    maximumFractionDigits: 0  //不顯示最大小數
  }).format(sum)
})

//初始化載入
onMounted(async () => {
  try{
    const result = await getAllCarts()
    if(!Array.isArray(result)){
      cartId.value = null
      cartItems.value = []
      return
    }
    // console.log(result)
    cartId.value = result[0].cartId
    cartItems.value = result[0]?.cartItem || []
  }catch (err){
    console.error('載入購物車錯誤', err)
    alert(err.message)
    cartItems.value = []
  }
}
)

// 移除商品
const removeItem = async (cartItemId) =>{
  try{
    const result = await deleteItem(cartItemId)
    if (result.ok){
      const result = await getAllCarts()
      cartItems.value = result[0]?.cartItem || []
    }
  }catch(err){
    console.log('刪除購物車商品失敗', err)
    let msg = err.message
    alert(msg)
  }
}

// 清空購物車彈窗
const toggleClean = () =>{
  currentCart = cartId.value;
  confirmModalRef.value.open();
}

// 清空購物車
const cleanCart = async () =>{
  try{
    const result = await deleteCart(currentCart)
    if (result.ok){
      const result = await getAllCarts()
      cartItems.value = result[0]?.cartItem || []
    }
    alert(result.message)
  }catch(err){
    console.log('清空購物車失敗', err)
    alert(err.message)
  }
}
</script>
