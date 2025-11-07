<script setup>
import { ref, onMounted } from 'vue'
import { getBestSellers } from '@/api/Analytics.js'

const items = ref([])
const loading = ref(true)
const errorMsg = ref('')

onMounted(async () => {
  const sixtyDaysAgo = new Date(Date.now() - 60 * 24 * 3600 * 1000).toISOString()
  const now = new Date().toISOString()

  try {
    items.value = await getBestSellers({
      from: sixtyDaysAgo,
      to: now,
      top: 5,
      statuses: '2,3,4,5'
    })
  } catch (err) {
    console.error('載入熱銷排行失敗', err)
    errorMsg.value = '載入資料時發生錯誤，請稍後再試'
  } finally {
    loading.value = false
  }
})
</script>

<template>
  <section class="container my-5">
    <h3 class="fw-bold mb-3">熱銷商品排行 🔥</h3>

    <div v-if="loading" class="text-muted">載入中...</div>

    <div v-else-if="errorMsg" class="text-danger">{{ errorMsg }}</div>

    <div v-else-if="!items.length" class="text-muted">目前無資料</div>

    <div v-else class="row g-3">
      <div
        v-for="(p, i) in items"
        :key="p.productId"
        class="col-12 col-md-6 col-lg-4"
      >
        <div class="card h-100 shadow-sm border-0 hover-shadow">
          <div class="card-body">
            <div class="d-flex justify-content-between align-items-center mb-2">
              <h5 class="card-title mb-0">
                #{{ i + 1 }} {{ p.productName }}
              </h5>
              <span class="badge bg-dark">銷量 {{ p.totalQuantity }}</span>
            </div>
            <p class="small text-muted mb-0">
              總金額：
              <span class="text-success fw-bold">
                NT$ {{ p.totalSalesAmount.toLocaleString('zh-TW') }}
              </span>
            </p>
            <p class="small text-muted">
              {{ p.variantCount }} 種規格
              <span v-if="p.topVariantSKU">｜熱銷款 SKU：{{ p.topVariantSKU }}</span>
            </p>
          </div>
        </div>
      </div>
    </div>
  </section>
</template>

<style scoped>
.hover-shadow:hover {
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
  transform: translateY(-2px);
  transition: all 0.2s ease;
}
</style>
