<template>
    <div class="order-list container">
        <div v-for="od in orders" :key="od.orderId" class="order-card mb-4 shadow-sm p-3 rounded">
            <!-- 訂單資訊 -->
        </div>
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
import {getAllOrders, lookupOrder, deleteOrder, EditDeliveryAddress, EditTaxNo, memberCheckOut} from '@/api/Order';

const orders = ref([]);

onMounted(async () =>{
    orders.value = await getAllOrders()
})
</script>