<template>
  <GlobalLoading scope="checkout" message="訂單建立中..."/>
  <div class="container py-4 checkout-page">
    <h2 class="mb-4 fw-bold">結帳資訊</h2>
    <div class="row">
      <!-- 左側主要表單 -->
      <div class="col-lg-8">
        <!-- 收件人資訊 -->
        <section class="mb-4 card p-3">
          <h5 class="mb-3">購買人資訊</h5>
          <div class="row g-3">
            <div class="col-md-6">
              <label class="form-label">聯絡人姓名</label>
              <input v-model="form.contactName" type="text" class="form-control" placeholder="請輸入姓名" />
              <small v-if="errors.contactName" class="text-danger">{{ errors.contactName }}</small>
            </div>
            <div class="col-md-6">
              <label class="form-label">手機號碼</label>
              <input v-model="form.contactPhone" type="tel" class="form-control" placeholder="請輸入手機號碼" />
              <small v-if="errors.contactPhone" class="text-danger">{{ errors.contactPhone }}</small>
            </div>
            <div class="col-md-12">
              <label class="form-label">配送地址</label>
              <input v-model="form.deliveryAddress" type="text" class="form-control" placeholder="請輸入完整地址" />
              <small v-if="errors.deliveryAddress" class="text-danger">{{ errors.deliveryAddress }}</small>
            </div>
          </div>
        </section>

        <!-- 取貨方式 -->
        <section class="mb-4 card p-3">
          <h5 class="mb-3">取貨方式</h5>
          <select v-model="form.pickupMethod" class="form-select">
            <option disabled value="">請選擇取貨方式</option>
            <option v-for="opt in pickupOptions" :key="opt.value" :value="opt.value">
              {{ opt.label }}
            </option>
          </select>
          <small v-if="errors.pickupMethod" class="text-danger">{{ errors.pickupMethod }}</small>
        </section>

        <!-- 物流方式 -->
        <section v-if="form && form.pickupMethod === 2" class="mb-4 card p-3">
          <h5 class="mb-3">物流方式</h5>
          <select v-model="form.logisticsProvider" class="form-select">
            <option disabled :value="null">請選擇物流方式</option>
            <option v-for="opt in logisticsOptions" :key="opt.value" :value="opt.value">
              {{ opt.label }}
            </option>
          </select>
          <small v-if="errors.logisticsProvider" class="text-danger">{{ errors.logisticsProvider }}</small>
        </section>

        <!-- 付款方式 -->
        <section class="mb-4 card p-3">
          <h5 class="mb-3">付款方式</h5>
          <select v-model="form.paymentMethod" class="form-select">
            <option disabled value="">請選擇付款方式</option>
            <option v-for="opt in paymentOptions" :key="opt.value" :value="opt.value">
              {{ opt.label }}
            </option>
          </select>
          <small v-if="errors.paymentMethod" class="text-danger">{{ errors.paymentMethod }}</small>
        </section>

        <!-- 發票資訊 -->
        <section class="mb-4 card p-3">
          <h5 class="mb-3">發票資訊</h5>
          <div class="form-check mb-2">
            <input class="form-check-input" type="radio" value="個人發票" v-model="invoiceType" />
            <label class="form-check-label">個人發票</label>
          </div>
          <div class="form-check mb-2">
            <input class="form-check-input" type="radio" value="公司發票" v-model="invoiceType" />
            <label class="form-check-label">公司發票</label>
          </div>

          <div v-if="invoiceType === '公司發票'" class="row g-3 mt-2">
            <div class="col-md-6">
              <label class="form-label">公司抬頭</label>
              <input v-model="form.invoiceTitle" type="text" class="form-control" placeholder="公司名稱" />
            </div>
            <div class="col-md-6">
              <label class="form-label">統一編號</label>
              <input v-model="form.taxNo" type="text" class="form-control" placeholder="8 碼統編" />
            </div>
          </div>
        </section>

        <!-- 備註 -->
        <section class="mb-4 card p-3">
          <h5 class="mb-3">備註</h5>
          <textarea v-model="form.note" class="form-control" rows="3" placeholder="可填寫其他說明..."></textarea>
        </section>
      </div>

      <!-- ✅ 右側結帳明細 -->
      <div class="col-lg-4">
        <div class="card summary-card p-3">
          <h5 class="fw-bold mb-3">結帳明細</h5>

          <div class="d-flex justify-content-between mb-2">
            <span>商品總金額</span>
            <span class="fw-semibold">{{ totalAmount.toLocaleString() }}</span>
          </div>

          <div class="d-flex justify-content-between mb-2">
            <span>運費</span>
            <span class="fw-semibold">{{ shippingCostDisplay }}</span>
          </div>

          <hr class="divider"></hr>

          <div class="d-flex justify-content-between mb-2">
            <span>折扣</span>
            <span class="fw-semibold"> {{ levelName }}會員: {{ discountText }}</span>
          </div>

          <div class="d-flex justify-content-between border-top pt-2 mb-3">
            <span class="fw-bold">合計</span>
            <span class="fw-bold text-danger">{{ finalAmount.toLocaleString() }}</span>
          </div>

          <button class="btn btn-danger w-100 py-2 fw-bold" @click="submitOrder">
            確認付款
          </button>

          <p class="text-center text-muted mt-2 small">
            確認付款後，即視為同意
            <a href="#" class="text-primary text-decoration-none">條款與規則</a>。
          </p>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, computed, watch, onBeforeUnmount } from 'vue'
import http from '@/api/axios'
import { useAuthStore } from '@/stores/auth'
import { useRouter, useRoute } from 'vue-router'
import { memberCheckOut } from '@/api/Order'
import Swal from 'sweetalert2'
import { useLoading } from '@/stores/useLoading'
import GlobalLoading from '@/components/GlobalLoading.vue'

const auth = useAuthStore()
const router = useRouter()
const route = useRoute()
const totalAmount = ref(0)
//設定loading相關
const {withLoading} = useLoading('checkout') // 建立訂單

const levelName = auth.user.levelName

//最終運費計算
const finalAmount = computed(()=>{
  const total = Number(totalAmount.value) || 0
  const shipping = Number(shippingCost.value) || 0
  const discount = Number(discountRate.value) || 0
  const discounted = total * (1 - discount / 100)
  return Math.round(discounted + shipping)
}
)

const form = ref({
  memberId: 0,
  contactName: '',
  contactPhone: '',
  employeeId: null,
  taxNo: '',
  paymentMethod: '',
  pickupMethod: '',
  deliveryAddress: '',
  shippingCost: 0,
  logisticsProvider: null,
  note: '',
})

onMounted(() =>{
  totalAmount.value = Number(route.query.total) || 0
  console.log('接收到的總金額：', totalAmount.value)
	if(auth.isLoggedIn && auth.user){
		form.value.memberId = auth.user.memberId
		form.value.contactName = auth.user.name
    form.value.contactPhone = auth.user.phone
	}

  window.addEventListener('message', handleECPayMessage)
})

onBeforeUnmount(() =>{
  window.removeEventListener('message', handleECPayMessage)
})

const invoiceType = ref('個人發票')

// 下拉選項
const pickupOptions = [
  { label: '店取', value: 1 },
  { label: '宅配', value: 2 }
]

const logisticsOptions = [
  { label: '郵局', value: 1 },
  { label: '黑貓宅急便', value: 2 },
  { label: '新竹貨運', value: 3 },
  { label: '大榮貨運', value: 4 }
]

// 運費表
const shippingCostMap = {
  1:550,
  2:600,
  3:600,
  4:600
}

// 自動顯示運費
const shippingCost = computed(()=>{
  const id = form.value.logisticsProvider
  return shippingCostMap[id] || 0
})

const shippingCostDisplay = computed(() =>
  new Intl.NumberFormat('zh-TW', {
    style: 'currency',
    currency: 'TWD',
    minimumFractionDigits: 0
  }).format(shippingCost.value)
)

// 監聽調動物流金額
watch(
  () => form.value.pickupMethod,
  (newVal) =>{
    const resquiresLogistics = [2]
    if (!resquiresLogistics.includes(Number(newVal))){
      form.value.logisticsProvider = null
    }
  }
)

const paymentOptions = [
  { label: '現金支付', value: 1 },
  { label: '信用卡支付', value: 2 },
  { label: '網路ATM', value: 3 },
  { label: 'ATM 虛擬帳號', value: 4},
  { label: '超商代碼繳費', value: 5},
  { label: '條碼繳費', value: 6}

]

// 會員等級折扣表
const discountMap = {
  1: 0,
  2: 5,
  3: 10,
  4: 15
}

// 折扣率
const discountRate = computed(() =>{
  const level = auth.user?.levelId
  return discountMap[level] ?? 0
})

//顯示會員折扣
const discountText = computed(() =>{
  return discountRate.value > 0 ? `${discountRate.value}% OFF` : '無折扣'
})

// 送出訂單的主流程
async function submitOrder(){
  if(!validateForm()){
    Swal.fire({
      title: '請完整填寫必要資訊',
      icon: 'error',
      confirmButtonText: '關閉'
    })
    return
  }
  try {
    const payload = {
      memberId: form.value.memberId,
      contactName: form.value.contactName,
      contactPhone: form.value.contactPhone,
      employeeId: null,
      taxNo: form.value.taxNo,
      paymentMethod: form.value.paymentMethod,
      deliveryAddress: form.value.deliveryAddress,
      shippingCost: Number(shippingCost.value),
      logisticsProvider: form.value.logisticsProvider,
      note: form.value.note
    }

    //呼叫API
    const result = await withLoading(async () =>{
      return await memberCheckOut(payload)
    })
    const orderId = result?.data?.orderId
    // 組裝 ECPay 所需DTO
    const ecpay = {
      merchantTradeNo: String(orderId),
      totalAmount: Number(finalAmount.value),
      paymentMethodId: Number(form.value.paymentMethod)
    }
    
      if(!orderId){
        throw new Error('訂單建立失敗，未取得訂單編號')
      }
  
      if (payload.paymentMethod === 1){
        console.log('我的ECPay', ecpay)
        Swal.fire({
          title: '訂單建立成功，請依付款方式完成支付',
          icon: 'success',
          confirmButtonText: '關閉'
          })
        router.push('/Order')
        return
      }
  
      if ([2, 3, 4, 5, 6, 7].includes(payload.paymentMethod)){
        await handleECpay(ecpay)
        return
      }
      Swal.fire({
        title: '未知付款方式，請重新選擇',
        icon: 'error',
        confirmButtonText: '關閉'
      })
  }catch (err){
    console.error('建立訂單失敗', err)
    Swal.fire({
      title: '錯誤',
      text: err.message,
      icon: 'error',
      confirmButtonText: '關閉'
      })
  }
}

// ECpay
async function handleECpay(ecpay){
  try{
    const {data} = await http.post(`/ECPay/checkout`, ecpay, {responseType: 'text'})
    const newWindow = window.open('', '_blank')
    newWindow.document.open()
    newWindow.document.write(data)
    newWindow.document.close()
  }catch (err){
    console.log('ECPay 啟動失敗', err)
    Swal.fire({
      title: 'ECPay付款頁面載入失敗，請稍後再試',
      icon: 'error',
      confirmButtonText: '關閉'
      })
  }
}

//錯誤狀態儲存
const errors = ref({
  contactName: '',
  contactPhone: '',
  deliveryAddress: '',
  pickupMethod: '',
  logisticsProvider: '',
  paymentMethod: '',
})

// 欄位驗證
function validateForm() {
  let isValid = true
  errors.value = { contactName: '', contactPhone: '', deliveryAddress: '', pickupMethod: '', logisticsProvider: '', paymentMethod: '' }

  if (!form.value.contactName.trim()) {
    errors.value.contactName = '請輸入姓名'
    isValid = false
  }
  if (!form.value.contactPhone.trim()) {
    errors.value.contactPhone = '請輸入手機號碼'
    isValid = false
  }
  if (!form.value.deliveryAddress.trim()){
    errors.value.deliveryAddress = '請填寫運送地址'
    isValid = false
  }
  if (!form.value.pickupMethod) {
    errors.value.pickupMethod = '請選擇取貨方式'
    isValid = false
  }
  // 僅在選擇「宅配」時檢查物流方式
  if (form.value.pickupMethod === 2 && !form.value.logisticsProvider) {
    errors.value.logisticsProvider = '請選擇物流方式'
    isValid = false
  }
  if (!form.value.paymentMethod) {
    errors.value.paymentMethod = '請選擇付款方式'
    isValid = false
  }

  return isValid
}

// 監控錯誤提示
watch(form, () => {
  Object.keys(errors.value).forEach(key => {
    if (form.value[key]) errors.value[key] = ''
  })
}, { deep: true })

//付款監聽
function handleECPayMessage(event){
  if (event.data?.type !== 'ecpayPayment') return
  if (event.data.status === 'success'){
    Swal.fire({
      title: '付款成功',
      icon: 'success',
      confirmButtonText: '關閉'
    }).then(() =>{
      router.push('/Order')
    })
  }else{
    Swal.fire({
      title: '付款失敗，請重新操作',
      icon: 'error',
      confirmButtonText: '關閉'
    })
  }
}

</script>

<style scoped>
.summary-card {
  position: sticky;
  top: 100px;
  background-color: #fff;
  border-radius: 10px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.08);
}

.summary-card h5 {
  font-weight: 700;
  border-bottom: 1px solid #eee;
  padding-bottom: 0.5rem;
}

.summary-card .btn-danger {
  background-color: #e60012;
  border: none;
  transition: 0.2s ease-in-out;
}

.summary-card .btn-danger:hover {
  background-color: #cc0010;
}

.summary-card a {
  font-size: 0.85rem;
}

.divider {
  border: none;
  border-top: 2px solid #000; /* 黑色粗線 */
  margin: 12px 0;
}
</style>
