<template>
  <div class="chat-box">
    <div class="chat-content">
      <div
        class="message-bubble"
        v-for="m in messages"
        :key="m.messageId"
        :class="getBubbleClass(m.senderType)"
      >
        <div class="content">{{ m.content }}</div>
        <div class="time">{{ formatTime(m.createdAt) }}</div>
      </div>
    </div>

    <div class="send-bar">
      <input
        v-model="content"
        @keyup.enter="send"
        placeholder="輸入訊息...."
      />
      <button @click="send" :disabled="!content.trim()">送出</button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import http from '@/api/axios'
import * as singalR from '@microsoft/signalr'


const content = ref('')
const selectedId =  ref<number | null>(1) 
const messages = ref<any[]>([])
const connection  = new singalR.HubConnectionBuilder()
  .withUrl("https://localhost:7131/chathub")
  .withAutomaticReconnect()
  .build();

onMounted(async () => {
  // 2) 監聽後端推播  singalR的傳遞為  '方法名稱', (參數) => {}
  connection.on('ReceiveMessage', (payload) => {
    console.log("Received message via SignalR:", payload)
    // payload = { chatRoomId, content, senderType, senderId, createdAt }
    if (payload.chatRoomId === selectedId.value) {
      messages.value.push(payload)
    }
  })

  await connection.start()
  console.log('SignalR Connected ✅')

  await loadMessages()

  // 3) 進入房間後記得加入群組（這台客戶端的角色是什麼就帶什麼）
  // 範例：這頁是「member」後台在看某一個房
  await connection.invoke('JoinRoom', selectedId.value.toString())
  console.log(`Joined Room ${selectedId.value} ✅`)
})
  

function getBubbleClass(type: string) {
  return type === 'visitor' || type === 'member'
    ? 'message-bubble right'
    : 'message-bubble left'
}

async function loadMessages() {
connection.start().then(() => {
    console.log("SignalR Connected.");
  }).catch((err) => console.log("Error while establishing connection :(" + err));

const { data } = await http.post('/ChatRoom/Index',
 {
  withCredentials: true // ✅ 一定要加這個
})
 selectedId.value = data.selectedId
  messages.value = data.messages ?? []
  console.log("123456")
}

async function send() {
  if (!selectedId.value || !content.value.trim()) return

  await http.post('/ChatRoom/SendMessage', { chatRoomId: selectedId.value, content: content.value.trim() },{ withCredentials: true });
  await connection.invoke("SendToRole", selectedId.value.toString(), "employee", content.value.trim());
  content.value = ''
  await loadMessages()
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

  /* ✨ 新增：這樣 align-self 才會生效 */
  display: flex;
  flex-direction: column;
  gap: 8px; /* 可選：訊息間距 */
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
  padding: 10px 14px;
  border-radius: 18px;
  margin-bottom: 10px;
  max-width: 70%;
  line-height: 1.4;
  word-break: break-word;
  display: inline-block;
  font-size: 15px;
}

/* 🩶 左邊灰色訊息 */
.message-bubble.left {
  background: #f0f0f0;
  align-self: flex-start;
  margin-left: 10px;
  margin-right: auto;
  border-top-left-radius: 4px;
}

/* 🟩 右邊綠色訊息 */
.message-bubble.right {
  background: #dcf8c6;
  align-self: flex-end;
  margin-right: 10px;
  margin-left: auto;
  border-top-right-radius: 4px;
}

/* 🕓 時間文字 */
.message-bubble .time {
  font-size: 11px;
  color: #777;
  text-align: right;
  margin-top: 4px;
}

/* 🗨️ 左邊尾巴 */
.message-bubble.left::after {
  content: "";
  position: absolute;
  left: -6px;
  top: 10px;
  border-width: 6px;
  border-style: solid;
  border-color: transparent #f0f0f0 transparent transparent;
}

/* 🗨️ 右邊尾巴 */
.message-bubble.right::after {
  content: "";
  position: absolute;
  right: -6px;
  top: 10px;
  border-width: 6px;
  border-style: solid;
  border-color: transparent transparent transparent #dcf8c6;
}

</style>
