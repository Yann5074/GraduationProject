<!-- src/components/ChatWidget.vue -->
<template>
  <div>
    <!-- 浮動按鈕：圖片輪播 + 滑過切第三張 -->
    <button
      class="floating-button"
      @click="toggleChat"
      @mouseenter="isHover = true"
      @mouseleave="isHover = false"
      aria-label="開啟客服聊天室"
    >
      <img :src="currentImg" alt="客服頭像" />
    </button>

    <!-- 流式對話框 -->
    <transition name="pop">
      <div v-if="tipVisible" class="tip-bubble" role="status">
        <button class="tip-close" @click="hideTip">✕</button>
        <StreamingText :text="tipText" :enabled="true" :speed="24" @done="onStreamDone" />
        <span class="tip-caret"></span>
      </div>
    </transition>

    <!-- 聊天室彈窗 -->
    <div v-if="showChat" class="chat-popup">
      <ChatRoom />
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, onBeforeUnmount, defineComponent, watch, h } from 'vue'
import ChatRoom from '@/components/Chat/ChatRoom.vue'

/* ---------------- StreamingText 子組件（內嵌版，無需額外檔案/JSX） ---------------- */
const StreamingText = defineComponent({
  name: 'StreamingText',
  props: {
    text: { type: String, required: true },
    enabled: { type: Boolean, default: true },
    speed: { type: Number, default: 28 }
  },
  emits: ['done'],
  setup(props, { emit }) {
    const display = ref('')
    const finished = ref(false)
    let stop = false
    let timer: number | null = null

    const PUNCT_PAUSE: Record<string, number> = {
      ',': 180, '.': 260, '?': 280, '!': 280,
      '，': 220, '。': 320, '、': 160, '！': 300, '？': 300, '：': 180
    }

    const clearTimer = () => { if (timer) { clearTimeout(timer); timer = null } }

    const skipAll = () => {
      if (!props.enabled || finished.value) return
      stop = true
      clearTimer()
      display.value = props.text
      finished.value = true
      emit('done')
    }

    const play = () => {
      clearTimer()
      display.value = ''
      finished.value = false
      stop = false

      if (!props.enabled) {
        display.value = props.text
        finished.value = true
        return
      }

      let i = 0
      const base = Math.max(8, props.speed)
      const tick = () => {
        if (stop) return
        const ch = props.text[i++]
        display.value += ch ?? ''
        if (i >= props.text.length) {
          finished.value = true
          emit('done')
          return
        }
        const extra = PUNCT_PAUSE[ch as keyof typeof PUNCT_PAUSE] ?? 0
        const jitter = Math.random() * 20
        timer = window.setTimeout(tick, base + extra + jitter)
      }
      timer = window.setTimeout(tick, base)
    }

    watch(() => props.text, play, { immediate: true })
    watch(() => props.enabled, play)
    onMounted(play)
    onBeforeUnmount(() => { stop = true; clearTimer() })

    // 使用 h() 返回節點，避免 JSX/TSX 需求
    return () =>
      h('span',
        {
          class: 'streaming-text',
          title: props.enabled ? '點我顯示完整' : '',
          onClick: skipAll
        },
        [
          display.value,
          (props.enabled && !finished.value)
            ? h('span', { class: 'caret' }, '▋')
            : null
        ]
      )
  }
})
/* ---------------- end StreamingText ---------------- */

/** ============ 圖片路徑（放在 public/chatroom/ 下） ============ */
const IMG46 = '/chatroom/ChatGPT Image 2025年11月10日 上午11_49_46.png' // 首圖
const IMG48 = '/chatroom/ChatGPT Image 2025年11月10日 上午11_49_48.png' // 次圖
const IMG50 = '/chatroom/ChatGPT Image 2025年11月10日 上午11_49_50.png' // 舉手（hover）

/** ============ 聊天視窗開關 ============ */
const showChat = ref(false)
function toggleChat() { showChat.value = !showChat.value }

/** ============ 頭像輪播（46⇄48 每4秒；hover 固定 50） ============ */
const isHover = ref(false)
const rotateIdx = ref(0)
let rotateTimer: number | null = null

const currentImg = computed(() => (isHover.value ? IMG50 : (rotateIdx.value ? IMG48 : IMG46)))

function startRotate() {
  stopRotate()
  rotateTimer = window.setInterval(() => {
    if (!isHover.value) rotateIdx.value = 1 - rotateIdx.value
  }, 4000)
}
function stopRotate() { if (rotateTimer) { clearInterval(rotateTimer); rotateTimer = null } }

/** ============ 三段循環對話：打完→等4秒收起→等6秒下一段 ============ */
const SCRIPTS = [
  '遇到什麼問題嗎？我一直在這裡～ 請註冊為我們會員，立即協助您解決問題！！',
  '對怎樣的產品有興趣呢～要不要小助手推薦給您？',
  '想添購怎樣的家具呢～ 小助手在眾多產品中替您找到真愛^^'
]

const tipVisible = ref(false)
const tipText = ref('')
const tipIndex = ref(0)

let waitCloseT: number | null = null
let waitNextT: number | null = null

function showTip(text: string) { tipText.value = text; tipVisible.value = true }
function hideTip() { tipVisible.value = false; clearTimers() }
function clearTimers() {
  if (waitCloseT) { clearTimeout(waitCloseT); waitCloseT = null }
  if (waitNextT) { clearTimeout(waitNextT); waitNextT = null }
}

function onStreamDone() {
  // 打完字後等 4 秒收起
  waitCloseT = window.setTimeout(() => {
    tipVisible.value = false
    // 收起後等 6 秒播下一則
    waitNextT = window.setTimeout(playNext, 6000)
  }, 4000)
}

function playNext() {
  tipIndex.value = (tipIndex.value + 1) % SCRIPTS.length
  showTip(SCRIPTS[tipIndex.value])
}

onMounted(() => {
  startRotate()
  tipIndex.value = 0
  showTip(SCRIPTS[0])   // 一進頁播第一段
})

onBeforeUnmount(() => {
  stopRotate()
  clearTimers()
})
</script>

<style scoped>
/* StreamingText 樣式 */
.streaming-text { white-space: pre-wrap; word-break: break-word; }
.caret { display: inline-block; animation: blink 1s steps(1, end) infinite; }
@keyframes blink { 50% { opacity: 0; } }

/* 浮動按鈕 */
.floating-button {
  position: fixed;
  bottom: 24px;
  right: 24px;
  width: 64px;
  height: 64px;
  border-radius: 50%;
  background: #fff;
  border: none;
  padding: 0;
  cursor: pointer;
  box-shadow: 0 6px 16px rgba(0,0,0,0.18);
  z-index: 1000;
  overflow: hidden;
}
.floating-button img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

/* 流式對話泡泡（靠著按鈕左上） */
.tip-bubble {
  position: fixed;
  bottom: 80px;
  right: 70px;
  max-width: 280px;
  background: #fff;
  color: #333;
  border-radius: 12px;
  padding: 12px 14px 14px 14px;
  box-shadow: 0 8px 24px rgba(0,0,0,0.18);
  font-size: 14px;
  line-height: 1.55;
  z-index: 1001;
}
.tip-caret {
  position: absolute;
  bottom: -8px;
  right: 18px;
  width: 0; height: 0;
  border-left: 8px solid transparent;
  border-right: 8px solid transparent;
  border-top: 8px solid #fff;
  filter: drop-shadow(0 2px 2px rgba(0,0,0,0.1));
}
.tip-close {
  position: absolute;
  right: 8px;
  top: 6px;
  width: 22px;
  height: 22px;
  border: none;
  background: transparent;
  color: #6b7280;
  cursor: pointer;
}

/* 進出動畫 */
.pop-enter-active, .pop-leave-active {
  transition: transform .18s ease, opacity .18s ease;
}
.pop-enter-from, .pop-leave-to {
  opacity: 0;
  transform: translateY(8px) scale(0.98);
}

/* 聊天室彈窗 */
.chat-popup {
  position: fixed;
  bottom: 150px;
  right: 24px;
  width: 420px;
  height: 550px;
  background: white;
  border-radius: 12px;
  box-shadow: 0 8px 24px rgba(0,0,0,0.2);
  z-index: 999;
  overflow: hidden;
}
</style>
