<template>
  <div
    class="modal fade"
    :id="modalId"
    tabindex="-1"
    aria-labelledby="editModalLabel"
    aria-hidden="true"
    ref="modalRef"
  >
    <div class="modal-dialog">
      <div class="modal-content">
        <!-- Header -->
        <div class="modal-header">
          <h5 class="modal-title">{{ title }}</h5>
          <button
            type="button"
            class="btn-close"
            data-bs-dismiss="modal"
            aria-label="Close"
          ></button>
        </div>

        <!-- Body -->
        <div class="modal-body">
          <div class="mb-3">
            <label class="form-label">原始{{ fieldLabel }}</label>
            <input type="text" class="form-control" :value="oldValue" readonly />
          </div>
          <div class="mb-3">
            <label class="form-label">新{{ fieldLabel }}</label>
            <input
              type="text"
              class="form-control"
              :class="{'is-invalid': errorMessage}"
              v-model.trim="newValue"
              :placeholder="placeholderText"
              @input="validateInput"
            />
            <!-- 錯誤提示紅字 -->
            <div v-if="errorMessage" class="invalid-feedback">
              {{ errorMessage }}
            </div>
          </div>
        </div>

        <!-- Footer -->
        <div class="modal-footer">
          <button
            type="button"
            class="btn btn-secondary"
            data-bs-dismiss="modal"
            :disabled="isSubmitting"
          >
            取消
          </button>
          <button
            type="button"
            class="btn btn-primary"
            @click="handleConfirm"
            :disabled="!!errorMessage || isSubmitting"
          >
            <span v-if="!isSubmitting">確認修改</span>
            <span v-else class="d-flex align-items-center">
              <span
                class="spinner-border spinner-border-sm me-2"
                role="status"
                aria-hidden="true"
              ></span>
              處理中...
            </span>
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, watch, computed } from 'vue'
import bootstrap from 'bootstrap/dist/js/bootstrap.bundle.min.js'

// props
const props = defineProps({
  modalId: { type: String, default: 'editModal' },
  title: String,
  fieldLabel: String,
  oldValue: String,
  fieldType: { type: String, default: 'text' }, // 判斷要用哪種驗證
})

// emit
const emits = defineEmits(['confirm'])

// refs
const modalRef = ref(null)
const modalInstance = ref(null)
const newValue = ref('')
const isSubmitting = ref(false)
const errorMessage = ref('')

// 依據類型產生不同 placeholder
const placeholderText = computed(() => {
  if (props.fieldType === 'taxno') return '請輸入 8 位數統一編號'
  if (props.fieldType === 'address') return '請輸入正確的配送地址'
  return '請輸入欲更改內容'
})

// Bootstrap modal 初始化
onMounted(() => {
  modalInstance.value = new bootstrap.Modal(modalRef.value)
})

// 開關控制
function open() {
  newValue.value = ''
  errorMessage.value = ''
  modalInstance.value.show()
}

function close() {
  modalInstance.value.hide()
}

// 動態驗證邏輯（根據 fieldType）
function validateInput() {
  const value = newValue.value.trim()

  if (props.fieldType === 'taxno') {
    if (!value) errorMessage.value = '統一編號不得為空'
    else if (!/^\d+$/.test(value)) errorMessage.value = '統一編號只能輸入數字'
    else if (value.length !== 8) errorMessage.value = '統一編號必須為 8 位數'
    else errorMessage.value = ''
  }

  else if (props.fieldType === 'address') {
    if (!value) errorMessage.value = '地址不得為空'
    else if (value.length < 5) errorMessage.value = '地址太短，請輸入更完整資訊'
    else errorMessage.value = ''
  }

  else {
    errorMessage.value = !value ? '此欄位不得為空' : ''
  }
}

// 確認送出
async function handleConfirm() {
  validateInput()
  if (errorMessage.value) return

  isSubmitting.value = true
  try {
    await emits('confirm', newValue.value)
    close()
  } finally {
    isSubmitting.value = false
  }
}

// 對外開放
defineExpose({ open, close })
</script>

<style scoped>
.invalid-feedback {
  display: block;
}
</style>
