<template>
  <div class="chat-box">
    <div class="chat-content" ref="box">
      <div
        class="message-bubble"
        v-for="m in messages"
        :key="m.messageId ?? m.createdAt ?? Math.random()"
        :class="getBubbleClass(m.senderType)"
      >
        <!-- 一般文字 -->
        <!-- ★修改：若為模板卡片則顯示卡片，否則顯示原本文字 -->
        <div v-if="m.templateCard" class="template-card"> <!-- ★ -->
          <div class="tc-img-wrap"> <!-- ★ -->
            <img :src="m.templateCard.img" alt="" /> <!-- ★ -->
          </div> <!-- ★ -->
          <div class="tc-info"> <!-- ★ -->
            <div class="tc-title">{{ m.templateCard.title }}</div> <!-- ★ -->
            <div class="tc-price">NT$ {{ Number(m.templateCard.price).toLocaleString() }}</div> <!-- ★ -->
          </div> <!-- ★ -->
        </div> <!-- ★ -->
        <div v-else class="content">{{ m.content }}</div> <!-- ★ -->

        <div class="time">{{ formatTime(m.createdAt) }}</div>

        <!-- ↓ 只有 bot 才有推薦區塊 / 也在看 -->
        <div v-if="m.senderType === 'bot'" class="bot-extra">
          <!-- 推薦商品卡片 -->
          <div v-if="m.items?.length" class="product-grid">
            <a
              v-for="it in m.items"
              :key="it.productId ?? it.name"
              class="product-card"
              :href="it.linkUrl || '#'"
              target="_blank"
              rel="noopener"
            >
              <img :src=" ('https://localhost:7131' + it.imageUrl) || PLACEHOLDER" alt="" />
              <div class="info">
                <div class="name">{{ it.name }}</div>
                <div class="meta">
                  <span class="tag" v-if="it.inStock">現貨</span>
                  <span class="price" v-if="it.price">NT$ {{ Number(it.price).toLocaleString() }}</span>
                </div>
                <!-- ★新增：若有敘述就顯示 -->
                <div class="desc" v-if="it.desc">{{ it.desc }}</div> <!-- ★ -->
                <!-- ★新增：購買連結按鈕（沿用 <a> 外層 href 可點擊） -->
                <div class="buy-row" v-if="it.linkUrl"> <!-- ★ -->
                  <span class="buy-btn">前往購買</span> <!-- ★ -->
                </div> <!-- ★ -->
              </div>
            </a>
          </div>

          <!-- 行動按鈕 -->
          <!-- ★移除：每則訊息內的客服按鈕（統一改為固定在訊息列表下方） -->
          <!-- <div class="actions"> ... </div> --> <!-- ★刪除 -->
          
          <!-- 其他人也在看（寫死範例） -->
          <!-- ★修改：加入可關閉判斷（模板回覆可透過 m.hideAlsoViewed 隱藏） -->
          <div class="also-viewed" v-if="!m.hideAlsoViewed"> <!-- ★ -->
            <div class="title">其他人也在看</div>
            <div class="av-list">
              <a
                v-for="(av, i) in ALSO_VIEWED"
                :key="i"
                class="av-item"
                :href="av.link"
                target="_blank"
                rel="noopener"
              >
                <img :src="av.img" alt="" />
                <div class="txt">{{ av.title }}</div>
              </a>
            </div>
          </div>
        </div>
        <!-- ↑ bot 附加內容 -->
      </div>
    </div>

    <!-- ★新增：固定客服按鈕（在訊息列表下方，模板按鈕上方） -->
    <div class="fixed-actions"> <!-- ★ -->
      <button class="primary" @click="contactStaff()">需要幫你聯繫客服嗎？</button> <!-- ★ -->
    </div> <!-- ★ -->

    <!-- ★保留：問題模板按鈕列（固定顯示於輸入框上方；每次進入聊天室都會看到） -->
    <div v-if="showTemplates" class="quick-templates">
      <button
        v-for="(t, i) in TEMPLATES"
        :key="i"
        class="qt-btn"
        @click="triggerTemplate(t)"
      >
        {{ t.label }}
      </button>
    </div>

    <div class="send-bar">
      <input
        v-model="content"
        @keydown="onKeydown" 
        @compositionstart="onCompositionStart"
        @compositionend="onCompositionEnd"
        placeholder="輸入訊息...."
      />
      <button @click="send" :disabled="!content.trim()">送出</button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted,nextTick, watch } from 'vue'
import http from '@/api/axios'
import * as signalR from '@microsoft/signalr'

const content = ref('')
const selectedId = ref<number | null>(1)
const messages = ref<any[]>([])
const isComposing = ref(false)

const PLACEHOLDER =
  'https://placehold.co/300x200?text=Product'
const ALSO_VIEWED = [
  {
    img: 'https://localhost:7131/ProductImages/03.jpeg',
    title: 'EAGO 椅凳',
    link: '#',
  },
  {
    img: 'https://localhost:7131/ProductImages/t01.webp',
    title: 'ROUND 雙色圓桌',
    link: '#',
  },
  {
    img: 'https://localhost:7131/ProductImages/table01.webp',
    title: 'Rey 咖啡桌',
    link: '#',
  },
]

// ★新增：控制模板顯示（預設 true，代表每次進入都顯示）
const showTemplates = ref(true) // ★

// ★新增：三個模板（按鈕 → 依序推送 steps）
const TEMPLATES = [ // ★
  {
    label: '我想要了解SING 單人沙發產品規格',
    steps: [
      { role: 'member', content: '我想要了解SING 單人沙發產品規格' },
      { role: 'bot', content: '您好，您詢問的產品為 SING 單人沙發。', hideAlsoViewed: true },
      { role: 'bot', items: [
          {
            name: 'SING 單人沙發',
            imageUrl: '/ProductImages/STANDARD_SOFA_Brick_Red.jpg',
            price: 25600,
            inStock: true,
            linkUrl: '#',
            desc: '亞麻布加超纖皮提供您舒適座感，白蠟木紋理美觀自然，安全穩固，北歐簡約設計，時尚有型'
          }
        ],
        hideAlsoViewed: true
      }
    ]
  },
  {
    label: 'SING 單人沙發 與 Tonbo 雙人沙發的差異',
    steps: [
      { role: 'member', content: '我想要比較產品：SING 單人沙發 與 Tonbo 雙人沙發的差異' },
      { role: 'bot', content: '您好，以下為這兩款家具產品的敘述與資訊：', hideAlsoViewed: true },
      { role: 'bot', items: [
          {
            name: 'SING 單人沙發',
            imageUrl: '/ProductImages/STANDARD_SOFA_Brick_Red.jpg',
            price: 25600,
            inStock: true,
            linkUrl: '#',
            desc: '亞麻布加超纖皮提供您舒適座感，白蠟木紋理美觀自然，安全穩固，北歐簡約設計，時尚有型'
          },
          {
            name: 'Tonbo 雙人沙發',
            imageUrl: '/ProductImages/Tonbo_Sofa_Black_1-1_1.jpg',
            price: 35600,
            inStock: true,
            linkUrl: '#',
            desc: 'Tonbo Sofa 擁有較小的尺寸與邊緣柔和的方形結構，並配有厚實的軟墊，是十分適合飯店環境和辦公室的理想選擇。'
          }
        ],
        hideAlsoViewed: true
      }
    ]
  },
  {
    label: '保固與聯絡方式',
    steps: [
      { role: 'member', content: '我要詢問保固' },
      {
        role: 'bot',
        content: '購買後請留存您的發票與保固卡。本公司提供 3 年保固。任何問題可留言，或來電 07-1234566。',
        hideAlsoViewed: true
      }
    ]
  }
]

// SignalR 連線
const connection = new signalR.HubConnectionBuilder()
  .withUrl('https://localhost:7131/chathub')
  .withAutomaticReconnect()
  .build()
   
onMounted(async () => {
  // 收訊（後端推的是「一個物件 payload」）
  connection.on('ReceiveMessage', (payload: any) => {
    console.log('SignalR ▶', payload)
    messages.value.push(payload)
    scrollToBottom()
  })

  await connection.start()
  console.log('SignalR Connected ✅')

  await loadMessages()
  if (selectedId.value) {
    await connection.invoke('JoinRoom', selectedId.value.toString())
  }
})

function onKeydown(e: KeyboardEvent) {
  if (e.key === 'Enter' && !e.shiftKey) {
    e.preventDefault()
    send()
  }
}

// 右側（自己）/ 左側（對方/機器人）
function getBubbleClass(type: string) {
  //判斷式：當 type 等於 'bot' 或 等於 'member' 時，條件為真。
  return type === 'member'
    ? 'message-bubble right'
  //若條件為假，回傳左側的樣式 class：'message-bubble left'。
    : 'message-bubble left'
}
const box = ref<HTMLElement | null>(null)

function scrollToBottom() {
  if (!box.value) return
  // 保險做法：放到下一幀，確保 layout 完成
  requestAnimationFrame(() => {
    if (!box.value) return
    box.value.scrollTop = box.value.scrollHeight
  })
}

async function loadMessages() {
  const { data } = await http.post(
    '/chat/Index',
    {},
    { withCredentials: true }
  )
  selectedId.value = data.selectedId
  messages.value = data.messages ?? []
  await nextTick() 
  scrollToBottom()

  showTemplates.value = true // ★每次進入聊天室都顯示模板
}

function onCompositionStart() { isComposing.value = true }
function onCompositionEnd() { isComposing.value = false }

async function send() {
  if (isComposing.value) return            // 組字中不要送
  if (!selectedId.value || !content.value.trim()) return

  // 走「觸發 AI → 推回 bot」
  await http.post(
    '/chat/TriggerAiResponseOnlyMember',
    {
      chatRoomId: selectedId.value,
      senderType: 'member',
      message: content.value.trim(),
    },
    { withCredentials: true }
  )
  content.value = ''
}

// 點「需要幫你聯繫客服嗎？」（固定按鈕）
async function contactStaff() { // ★修改：不再需要接收 m
  if (!selectedId.value) return
  // 廣播一則「我要客服協助」訊息（你也可以換成呼叫後台建立工單）
  await http.post(
    '/chat/UserSendBroadcast',
    {
      chatRoomId: selectedId.value,
      senderType: 'member',
      content: '需要客服協助，請與我聯繫，謝謝！',
    },
    { withCredentials: true }
  )
}

function formatTime(ts?: string | null) {
  if (!ts) return ''
  try {
    const d = new Date(ts)
    return d.toLocaleString()
  } catch {
    return String(ts)
  }
}

// ★新增：點擊模板 → 依序推送 steps（member / bot / bot...）
function triggerTemplate(t: any) { // ★
  for (const step of t.steps ?? []) {
    const base: any = {
      chatRoomId: selectedId.value ?? 0,
      senderType: step.role === 'member' ? 'member' : 'bot',
      createdAt: new Date().toISOString(),
    }
    if (step.items?.length) {
      base.items = step.items
      base.hideAlsoViewed = true
    } else if (step.content) {
      base.content = step.content
      base.hideAlsoViewed = !!step.hideAlsoViewed
    }
    messages.value.push(base)
  }
  scrollToBottom()
  showTemplates.value = false // ★本次對話收起；下次進來再顯示
}
</script>

<style scoped>
.chat-box {
  display: flex;
  flex-direction: column;
  height: 100%;
  max-height: 600px;
  background: white;
  border-radius: 12px;
  overflow: hidden;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
}

.chat-content {
  flex: 1;
  padding: 12px;
  overflow-y: auto;
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.send-bar {
  display: flex;
  gap: 8px;
  padding: 12px;
  border-top: 1px solid #eee;
  background: #fff;
}

.send-bar input {
  flex: 1;
  padding: 10px;
  border: 1px solid #ccc;
  border-radius: 8px;
}

.send-bar button {
  padding: 10px 16px;
  background-color: #4f83ff;
  color: white;
  border: none;
  border-radius: 8px;
  cursor: pointer;
}

.send-bar button:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.message-bubble {
  position: relative;
  padding: 12px 14px; /* ★微調：更厚一點的卡片感 */
  border-radius: 18px;
  margin-bottom: 10px;
  max-width: 78%;     /* ★微調：讓內容略寬 */
  line-height: 1.5;   /* ★微調：更舒適行距 */
  word-break: break-word;
  display: inline-block;
  font-size: 15px;
  box-shadow: 0 1px 2px rgba(0,0,0,0.04); /* ★淡淡陰影更像卡片 */
}

.message-bubble.left {
  background: #f5f7fb; /* ★微調：更接近截圖的淡灰藍底 */
  align-self: flex-start;
  margin-left: 10px;
  margin-right: auto;
  border-top-left-radius: 4px;
}

.message-bubble.right {
  background: #dcf8c6;
  align-self: flex-end;
  margin-right: 10px;
  margin-left: auto;
  border-top-right-radius: 4px;
}

.message-bubble .time {
  font-size: 11px;
  color: #777;
  text-align: right;
  margin-top: 4px;
}

/* bot 附加內容容器 */
.bot-extra {
  margin-top: 8px;
}

/* 推薦商品卡片（套用於單品或兩品比較） */
.product-grid {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 10px;
  margin-top: 6px;
}

.product-card {
  display: block;
  background: #fff;
  border-radius: 12px;
  overflow: hidden;
  border: 1px solid #eee;
  text-decoration: none;
  color: inherit;
}

.product-card img {
  width: 100%;
  height: 120px;
  object-fit: cover;
  display: block;
}

.product-card .info {
  padding: 8px;
}

.product-card .name {
  font-size: 14px;
  font-weight: 600;
  line-height: 1.2;
  margin-bottom: 4px;
}

.product-card .meta {
  display: flex;
  gap: 8px;
  align-items: center;
  font-size: 12px;
}

.product-card .tag {
  background: #e8f5e9;
  color: #2e7d32;
  padding: 2px 6px;
  border-radius: 999px;
}

.product-card .price {
  font-weight: 700;
}

/* ★新增：敘述、購買按鈕 */
.product-card .desc {
  font-size: 12px;
  color: #4b5563;
  margin-top: 6px;
  line-height: 1.4;
}

.product-card .buy-row {
  margin-top: 8px;
}

.product-card .buy-btn {
  display: inline-block;
  padding: 6px 10px;
  border-radius: 999px;
  background: #4f83ff;
  color: #fff;
  font-size: 12px;
}

/* ★新增：固定客服按鈕列 */
.fixed-actions {
  display: flex;
  justify-content: flex-start;
  padding: 8px 12px;
  border-top: 1px solid #eee;
  background: #fff;
}

.fixed-actions .primary {
  background: #4f83ff;
  color: #fff;
  border: 0;
  padding: 8px 12px;
  border-radius: 10px;
  cursor: pointer;
}

/* 其他人也在看 */
.also-viewed {
  background: #f8fafc;
  border: 1px solid #e5e7eb;
  border-radius: 12px;
  padding: 10px;
  margin-top: 10px;
}

.also-viewed .title {
  font-weight: 700;
  margin-bottom: 8px;
}

.av-list {
  display: grid;
  grid-template-columns: 1fr;
  gap: 8px;
}

.av-item {
  display: grid;
  grid-template-columns: 48px 1fr;
  gap: 8px;
  align-items: center;
  text-decoration: none;
  color: inherit;
  background: #fff;
  border: 1px solid #eee;
  border-radius: 10px;
  padding: 6px 8px;
}

.av-item img {
  width: 48px;
  height: 48px;
  object-fit: cover;
  border-radius: 6px;
}

.av-item .txt {
  font-size: 14px;
  line-height: 1.2;
}

/* ★模板卡片樣式：單品展示（保留以備需要） */
.template-card {
  width: 180px;
  border-radius: 16px;
  overflow: hidden;
  background: #fff;
  border: 1px solid #eee;
  box-shadow: 0 2px 6px rgba(0,0,0,0.06);
}

.tc-img-wrap {
  background: #f3f4f6;
  width: 100%;
  height: 120px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.tc-img-wrap img {
  max-width: 90%;
  max-height: 90%;
  object-fit: contain;
  display: block;
}

.tc-info {
  background: #fff;
  padding: 8px 10px;
  border-top: 1px solid #eee;
}

.tc-title {
  font-size: 14px;
  font-weight: 600;
  line-height: 1.2;
  margin-bottom: 4px;
}

.tc-price {
  font-size: 13px;
  font-weight: 700;
  color: #111827;
}

/* ★新增：問題模板按鈕列樣式（膠囊鈕） */
.quick-templates {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
  padding: 10px 12px;
  border-top: 1px solid #eee;
  border-bottom: 1px solid #eee;
  background: #fafafa;
}

.qt-btn {
  padding: 8px 12px;
  border-radius: 999px;
  border: 1px solid #e5e7eb;
  background: #fff;
  cursor: pointer;
  font-size: 13px;
  line-height: 1;
}

.qt-btn:hover {
  background: #f3f4f6;
}
</style>