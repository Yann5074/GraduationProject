<template>
    <h1>這是訂單的測試頁面</h1>
        <div class="container mt-4">
    <h2 class="mb-3">訂單列表</h2>

    <div v-if="loading" class="text-muted">載入中...</div>
    <div v-if="error" class="text-danger">{{ error }}</div>

    <table v-if="!loading && orders.length > 0" class="table table-striped">
        <thead>
        <tr>
            <th>訂單編號</th>
            <th>負責員工</th>
            <th>訂單狀態</th>
        </tr>
        </thead>
        <tbody>
        <tr v-for="order in orders" :key="order.orderId">
            <td>{{ order.orderId }}</td>
            <td>{{ order.employeeName }}</td>
            <td>{{ order.orderStatusName }}</td>
        </tr>
        </tbody>
    </table>
    </div>
</template>

<style scoped>
/* 若有需要特調在輸入 */
.container {
    max-width: 800px;
}
</style>

<script setup>
import {ref, onMounted} from 'vue';
import {getOrders, deleteOrder} from '../api/Order';
import { mapToOrderDTOList } from '../dtos/OrderDto';

const orders = ref([]);
const loading = ref(false);
const error = ref(null);

onMounted(async () =>{
    loading.value = true;
    error.value = null;

    try{
        const apiData = await getOrders();
        orders.value = mapToOrderDTOList(apiData);
    }catch (err){
        error.value = "無法取得訂單資料";
    }finally{
        loading.value = false;
    }
}
)
</script>