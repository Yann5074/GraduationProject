<template>
  <div class="floating-recommendation shadow-lg p-3">
    <h6 class="fw-bold mb-2">猜你喜歡</h6>
    <div class="recommend-list">
      <div
        v-for="item in items.slice(0,3)"
        :key="item.productVariantId"
        class="recommend-item mb-2"
      >
        <img :src="item.imageUrl" :alt="item.name" />
        <div class="info">
          <p class="mb-0 small fw-bold">{{ item.name }}</p>
          <RouterLink :to="`/product/${item.productId}`" class="text-primary small">
            查看
          </RouterLink>
        </div>
      </div>
    </div>
  </div>
</template>


<script setup>
import { ref, onMounted } from 'vue'
import http from '@/api/axios'
import { useRoute } from 'vue-router'

const items = ref([])
const route = useRoute()
const API = import.meta.env.VITE_BASE_URL

onMounted(async () => {
  try {
    // 取得當前商品 id
    const id = route.params.id
    let result = []
    if (id){
      const { data } = await http.get(`/rec/item/${id}/variants`)
      result = data
    }

    //個人化推薦 (session-based)
    if((!result || result.length === 0) && !id){
      const sessionId = localStorage.getItem('guestSessionId')
      if(sessionId){
        const {data} = await http.get(`/rec/session/${sessionId}`)
        if(data && data.length > 0){
          result = data
        }
      }
    }

    //冷啟動推薦 (Trend)
    if (!result || result.length === 0){
      const trend = await http.get(`/rec/trending`)
      result = trend.data
    }
    items.value = result.map(x => ({
      ...x,
      imageUrl: x.imageUrl?.startsWith('http') ? x.imageUrl : `${API}${x.imageUrl}`
    }))
  } catch (err) {
    console.error('推薦商品載入失敗', err)
  }
})
</script>

<style scoped>
.floating-recommendation {
  position: fixed;
  right: 30px;
  bottom: 100px;
  width: 220px;
  background: white;
  border-radius: 10px;
  z-index: 1050;
  max-height: 500px;
  overflow-y: auto;
  border: 1px solid #eee;
}

.recommend-item {
  display: flex;
  align-items: center;
  gap: 8px;
}

.recommend-item img {
  width: 60px;
  height: 60px;
  object-fit: cover;
  border-radius: 6px;
}

.recommend-item .info {
  flex: 1;
  overflow: hidden;
}
</style>
