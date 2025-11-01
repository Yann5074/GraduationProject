<template>
    <div class="order-list container">
        <!-- 搜尋功能區塊 -->
        <div class="search-bar justify-content-between mb-4">
            <div class="search-title h5 mb-0">
                <i class="bi bi-search"></i>
                訂單查詢
            </div>
            <div class="input-group">
                <input type="text" class="form-control" placeholder="請輸入訂單編號或商品名稱" v-model.trim="keyword" />
                <button class="btn btn-primary" @click="handleSearch">
                    搜尋
                </button>
            </div>
        </div>
        <!-- 訂單資訊 -->
        <div v-for="od in orders" :key="od.orderId" class="order-card mb-4 shadow-sm p-3 rounded">
            <table class="table table-bordered mb-3">
                <thead class="table-header">
                    <tr>
                        <th>訂單編號</th>
                        <th>下單日期</th>
                        <th>銷售員</th>
                        <th>訂單狀態</th>
                        <th>付款方式</th>
                        <th>付款狀態</th>
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
                            {{ od.paymentStatus }}
                        </td>
                        <td class="text-end">
                            $ {{ od.formatTotalPrice }}
                        </td>
                    </tr>
                    <!-- 第二列資訊 -->
                    <tr class="table-header">
                        <th colspan="2">
                            統一編號
                        </th>
                        <th colspan="1">
                            配送狀態
                        </th>
                        <th colspan="4">
                            配送地址
                        </th>
                    </tr>
                    <tr class="align-top">
                        <td colspan="2" class="tax-no-cell">
                            <div class="d-flex justify-content-between align-items-center">
                                <span>{{ od.taxNo }}</span>
                                <button class="btn btn-sm btn-outline-primary" @click="openModal('taxno', od)">
                                    修改統編
                                </button>
                            </div>
                        </td>
                        <td colspan="1">
                            {{ od.deliveryStatus }}
                        </td>
                        <td colspan="4" class="delivery-address-cell">
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
                <div class="progress-line-active" :style="{'--progress-width': od.progressPercent + '%'}"></div>
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
            <div class="d-flex justify-content-between items-center">
                <button class="btn btn-link" @click="toggleDetails(od)">
                    {{ od.showDetails ? '收回' : '看明細' }}
                </button>
                <button class="btn btn-danger mb-1" @click="toggleDelete(od)">
                    取消訂單
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
    <ConfirmModal ref="confirmModalRef" titel="確認取消訂單" :message="`是否確定要取消訂單 ${currentOrder?.orderId || ''} ?`" @confirm="handleDeleteConfirm" />
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
    /* 修改寬度計算：比例式匹配背景線寬度，避免固定減法導致的空隙或溢出 */
    width: calc( (var(--progress-width, 0%) / 100%) * calc(100% - 40px) );
    /* 選用：如果仍有輕微溢出，可添加 transition 讓變化平滑 */
    transition: width 0.3s ease;
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

.search-bar{
    display: flex;
    margin-top: 20px;
    border: 2px solid #326A66;
    height: 100px;
    align-items: center;
}

.search-title{
    margin-left: 10px;
}
.input-group{
    max-width: 400PX;
    margin-right: 10px;
}
</style>

<script setup>
import {ref, onMounted} from 'vue';
import {getAllOrders, lookupOrder, deleteOrder, EditDeliveryAddress, EditTaxNo} from '@/api/Order';
import EditModal from '@/components/EditModal.vue';
import ConfirmModal from '@/components/ConfirmModal.vue'

//顯示訂單相關
const orders = ref([]);
//更改訂單地址及統編相關
const editModalRef = ref(null);
const fieldLabel = ref('');
const modalTitle = ref('');
const oldValue = ref('');
const fieldType = ref('');
let currentOrder = null;
//搜尋訂單相關
const keyword = ref('');
//刪除訂單相關
const confirmModalRef = ref(null);

//初始載入
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

//觸發修改彈跳視窗
function openModal(type, order){
    currentOrder = order
    fieldType.value = type
    modalTitle.value = type === 'taxno' ? '修改統一編號' : '修改配送地址'
    fieldLabel.value = type === 'taxno' ? '統一編號' : '配送地址'
    oldValue.value = type === 'taxno' ? order.taxNo : order.deliveryAddress
    editModalRef.value.open()
}

// 修改訂單
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
        console.error('修改失敗: ', err.message)
        const msg = err.message;
        alert(`${msg}`)
    }
}

// 搜尋訂單
async function handleSearch(){
    try{
        if (!keyword.value.trim()){
            const res = await getAllOrders()
            orders.value = res.map(order =>{
            order.showDetails= false
            return order
        })
        }else{
            const res = await lookupOrder(keyword.value)
            orders.value = res.map(order =>{
            order.showDetails=false
            return order
        })}
    }catch (err){
        console.error('取得訂單資料失敗', err)
        const msg = err.message;
        alert(`${msg}`)
    }
}

// 觸發刪除訂單彈窗
const toggleDelete = (od) =>{
    currentOrder = od;
    confirmModalRef.value.open();
}

// 刪除訂單
const handleDeleteConfirm = async () =>{
    try{
        const res = await deleteOrder(currentOrder.orderId)
        if (res.message == null)
            alert('訂單已取消')
        if (res.ok){
            const result = await getAllOrders()
            orders.value = result.map(order =>{
                order.showDetails= false
                return order
            })
        }
    }catch (err){
        console.log('刪除訂單失敗', err)
        let msg = err.message
        alert(msg)
    }
}
</script>