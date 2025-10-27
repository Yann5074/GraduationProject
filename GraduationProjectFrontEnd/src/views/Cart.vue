<template>
  <div class="title">
    <p><i class="bi bi-cart"></i> 購物車</p>
  </div>
  <!-- 購物車列表 -->
<div class="untree_co-section before-footer-section">
    <div class="container">
      <div class="row mb-5">
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
                  <td>
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
                <button class="btn btn-danger ms-3 me-3">
                  結帳
                </button>
            </h5>
          </div>
        </form>
      </div>
    </div>
  </div>
  <RouterView />
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
</style>


<script setup>
import { onMounted, ref, computed } from 'vue'
import { getAllCarts, deleteItem, editCartItem } from '@/api/Cart'
import QtyControl from '@/components/QtyControl.vue'


// 顯示購物車相關
const cartItems = ref([])
// 刪除購物車相關
const confirmModalRef = ref(null)

// 動態計數&呼叫數量變動API
const onQtyChange = async (ci, newQty) =>{
  const oldqty = ci.qty
  ci.qty = newQty
  ci.subtotal = ci.unitPrice * newQty

  try{
    const result = await editCartItem(ci.cartItemId, ci.qty)
    if (!result.ok){
      //在錯誤時如何讓數字便回去or庫存最大數量?
    }
  }catch(err){
    console.error('更新購物車商品數量失敗-.vue', err)
    alert(err.message)
  }
}

// 動態總金額計算
const formatTotal = computed(() =>{
  const sum = cartItems.value.reduce(
    (acc, item) => acc + item.subtotal, 0
  )
  return new Intl.NumberFormat('zh-TW', {
    style: 'currency',
    currency: 'TWD'
  }).format(sum)
})

//初始化載入
onMounted(async () => {
  try{
    const result = await getAllCarts()
    cartItems.value = result[0]?.cartItem || []
  }catch (err){
    console.error('載入購物車錯誤', err)
    alert(err.message)
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

</script>
