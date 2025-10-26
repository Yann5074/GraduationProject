<template>
    <div class="order-list container">
        <div v-for="od in orders" :key="od.orderId" class="order-card mb-4 shadow-sm p-3 rounded">
            <!-- 訂單資訊 -->
            <table class="table table-bordered mb-3">
                {{ console.log(od.formatOrderTime, od.formatTotalPrice) }}
                <thead class="table-header">
                    <tr>
                        <th>訂單編號</th>
                        <th>下單日期</th>
                        <th>訂單狀態</th>
                        <th>付款方式</th>
                        <th>配送狀態</th>
                        <th>總金額</th>
                    </tr>
                </thead>
                <tbody>
                    <tr>
                        <td>
                            {{ od.orderId }}
                        </td>
                        <td>
                            {{ od.formatOrderTime }}
                        </td>
                        <td>
                            {{ od.orderStatus }}
                        </td>
                        <td>
                            {{ od.paymentMethod }}
                        </td>
                        <td>
                            {{ od.deliveryStatus }}
                        </td>
                        <td class="text-end">
                            {{ od.totalPrice }}
                        </td>
                    </tr>
                </tbody>
            </table>
            <hr class="my-3" />
            <!-- 進度區塊 -->
            <div class="progress-container my-3">
                <!-- 進度線 -->
                <div class="progress-line-bg"></div>
                <div class="progress-line-active" :style="{width: od.progressPercent + '%'}"></div>
                <!-- 節點 -->
                <div class="progress-steps">
                    <div v-for="(step, index) in od.statusSteps" :key="index" class="progress-step" :class="{active: step.active}" :style="{left: step.leftPercent + '%'}">
                        <div class="circle"></div>
                        <div class="label">
                            {{ step.label }}
                        </div>
                    </div>
                </div>
            </div>
            <hr class="my-3" />
            <!-- 左下角按鈕 -->
            <div class="d-flex justify-content-start">
                <button class="btn btn-link" @click="toggleDetails(od)">
                    {{ od.showDetails ? '收回' : '看明細' }}
                </button>
            </div>
            <!-- 明細表格 -->
            <div v-if="od.showDetails">
                <table class="table table-sm align-middle mb-0">
                    <thead class="table-light">
                        <tr>
                            <th>商品</th>
                            <th>數量</th>
                            <th>單價</th>
                            <th>小計</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr v-for="odd in od.orderDetail" :key="odd.productName">
                            <td>
                                <img :src="odd.imageUrl" class="me-2 rounded" width="60" height="60" />
                                {{ odd.productName }}
                                <div class="text-muted small">
                                    商品規格: {{odd.productInfo}}
                                </div>
                            </td>
                            <td>
                                {{ odd.quantity }}
                            </td>
                            <td>
                                ${{ odd.unitPrice }}
                            </td>
                            <td>
                                ${{ odd.subtotal }}
                            </td>
                        </tr>
                    </tbody>
                </table>
            </div>
        </div>
    </div>
</template>

<style scoped>
.order-card{
    background-color: #fff;
}
.table-header{
    background-color: aqua;
    color: #fff;
}
.table-bordered th,
.table-bordered td{
    border: 1px solid #000;
    padding: 0.5rem;
}
.progress-container{
    position: relative;
    height: 80px;
    margin: 20px 0;
    padding: 0 20px;
    overflow: visible; /* 防止溢出畫面 */
}
.progress-line-bg {
    position: absolute;
    top: 20px;
    left: 20px;
    width: calc(100% - 40px);
    height: 4px;
    background-color: #ccc;
    border-radius: 2px;
}
.progress-line-active {
    position: absolute;
    top: 20px;
    left: 20px;
    height: 4px;
    background-color: #007bff; 
    border-radius: 2px;
    z-index: 1;
}
.progress-steps{
    position: absolute;
    top: 14px;
    left: 20px;
    width: calc(100% - 40px);
    height: 100%;
}
.progress-step {
    position: absolute;
    display: flex;
    flex-direction: column;
    align-items: center;
    transform: translateX(-50%);
    min-width: 50px;
    width: auto;
    padding: 0 5px;
    overflow: hidden;
    text-overflow: ellipsis;
}

.progress-step .circle {
    width: 16px;
    height: 16px;
    border-radius: 50%;
    background-color: #ccc;
    z-index: 3;
}

.progress-step.active .circle {
    background-color: #007bff;
}

.progress-step .label {
    font-size: 12px;
    color: #666;
    margin-top: 10px;
    white-space: nowrap;
    text-align: center;
}

hr{
    border-top: 1px solid #eee;
}
</style>

<script setup>
import {ref, onMounted} from 'vue';
import {getAllOrders, lookupOrder, deleteOrder, EditDeliveryAddress, EditTaxNo, memberCheckOut} from '@/api/Order';

const orders = ref([]);

onMounted(async () =>{
    const result = await getAllOrders()
    orders.value = result.map(order =>{
        order.showDetails= false
        return order
    })
})

const toggleDetails = (order) =>{
    order.showDetails = !order.showDetails;
}

</script>