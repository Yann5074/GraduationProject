<template>
    <div class="order-list container">
        <div v-for="od in orders" :key="od.orderId" class="order-card mb-4 shadow-sm p-3 rounded">
            <!-- 訂單資訊 -->
            <table class="table table-bordered mb-3">
                <thead class="table-header">
                    <tr>
                        <th>訂單編號</th>
                        <th>下單日期</th>
                        <th>銷售員</th>
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
                            {{ od.employeeName}}
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
                            $ {{ od.formatTotalPrice }}
                        </td>
                    </tr>
                    <!-- 第二列 -->
                    <tr class="table-header"> <th colspan="2">統一編號</th> <th colspan="5">配送地址</th> </tr>

                    <tr class="align-top">
                        <td colspan="2" class="tax-no-cell">
                            <div class="d-flex justify-content-between align-items-center">
                                <span>{{ od.taxNo }}</span>
                                <button class="btn btn-sm btn-outline-primary" @click="openModal('taxno', od)">
                                    修改統編
                                </button>
                            </div>
                        </td>

                        <td colspan="5" class="delivery-address-cell">
                            <div class="d-flex justify-content-between align-items-center">
                                <span> {{ od.deliveryAddress }}</span>
                                <button class="btn btn-sm btn-outline-primary" @click="openModal('address', od)">
                                    修改配送地址
                                </button>
                            </div>
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
                                ${{ odd.formatunitPrice }}
                            </td>
                            <td>
                                ${{ odd.formatsubtotal }}
                            </td>
                        </tr>
                    </tbody>
                </table>
            </div>
        </div>
    </div>
    <EditModal ref="editModalRef" :title="modalTitle" :field-label="fieldLabel" :old-value="oldValue" :field-type="fieldType"
    @confirm="handleConfirm"/>
</template>

<style scoped>
.order-card{
    background-color: #fff;
}
.table-header th{
    background-color: #326A66;
    color: #fff;
}
.table-bordered th,
.table-bordered td{
    border: 1px solid #000;
    padding: 0.5rem;
}
.main-order-table .tax-no-cell,
.main-order-table .delivery-address-cell{
    padding: 0.5rem;
}
.tax-no-header,
.address-header{
    margin-bottom: 0.5rem;
}
.table-bordered{
    border-collapse: separate !important;
    border: 1px solid #000;
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
import {getAllOrders, lookupOrder, deleteOrder, EditDeliveryAddress, EditTaxNo} from '@/api/Order';
import EditModal from '@/components/EditModal.vue';

const orders = ref([]);
const editModalRef = ref(null);
const fieldLabel = ref('');
const modalTitle = ref('');
const oldValue = ref('');
const fieldType = ref('');
let currentOrder = null;

onMounted(async () =>{
    const result = await getAllOrders()
    orders.value = result.map(order =>{
        order.showDetails= false
        return order
    })
})

//開啟訂單明細
const toggleDetails = (order) =>{
    order.showDetails = !order.showDetails;
}

//觸發彈跳視窗
function openModal(type, order){
    currentOrder = order
    fieldType.value = type
    modalTitle.value = type === 'taxno' ? '修改統一編號' : '修改配送地址'
    fieldLabel.value = type === 'taxno' ? '統一編號' : '配送地址'
    oldValue.value = type === 'taxno' ? order.taxNo : order.deliveryAddress
    editModalRef.value.open()
}

//確認修改
async function handleConfirm(newValue){
    try{
        if (fieldType.value === 'taxno'){
            const res = await EditTaxNo(currentOrder.orderId, newValue)
            currentOrder.taxNo = newValue;
            alert(`${res.message}`)
        }else{
            const res = await EditDeliveryAddress(currentOrder.orderId, newValue)
            currentOrder.deliveryAddress = newValue;
            alert(`${res.message}`)
        }
    }catch (err){
        console.error('修改失敗: ', err)
        const msg = err.message;
        alert(`${msg}`)
    }
}
</script>