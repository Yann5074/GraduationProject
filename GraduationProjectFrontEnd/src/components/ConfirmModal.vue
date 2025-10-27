<template>
  <div
    class="modal fade"
    :id="modalId"
    tabindex="-1"
    aria-labelledby="confirmModalLabel"
    aria-hidden="true"
    ref="modalRef"
  >
    <div class="modal-dialog modal-dialog-centered">
      <div class="modal-content">
        <div class="modal-header">
          <h5 class="modal-title">{{ title }}</h5>
          <button
            type="button"
            class="btn-close"
            data-bs-dismiss="modal"
            aria-label="Close"
          ></button>
        </div>
        <div class="modal-body">
          <p class="mb-0">{{ message }}</p>
        </div>
        <div class="modal-footer">
          <button
            type="button"
            class="btn btn-secondary"
            data-bs-dismiss="modal"
            :disabled="isLoading"
          >
            取消
          </button>
          <button
            type="button"
            class="btn btn-danger"
            :disabled="isLoading"
            @click="handleConfirm"
          >
            <span v-if="!isLoading">確認</span>
            <span v-else>
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
import { ref, onMounted } from 'vue'
import bootstrap from 'bootstrap/dist/js/bootstrap.bundle.min.js'

// Props
const props = defineProps({
  modalId: { type: String, default: 'confirmModal' },
  title: { type: String, default: '確認動作' },
  message: { type: String, default: '是否確認執行此操作？' }
})

// Emits
const emits = defineEmits(['confirm'])

const modalRef = ref(null)
let modalInstance = null
const isLoading = ref(false)

// 初始化 Bootstrap Modal
onMounted(() => {
  modalInstance = new bootstrap.Modal(modalRef.value)
})

function open() {
  isLoading.value = false
  modalInstance.show()
}

function close() {
  modalInstance.hide()
}

async function handleConfirm() {
  isLoading.value = true
  try {
    await emits('confirm') // 通知父層處理刪除 API
    close()
  } catch (err) {
    console.error('執行確認動作失敗', err)
  } finally {
    isLoading.value = false
  }
}

// 對外暴露方法
defineExpose({ open, close })
</script>
