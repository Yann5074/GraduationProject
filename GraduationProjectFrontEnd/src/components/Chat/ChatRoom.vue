<template>
  <div class="chat-box">
<div class="chat-content">
  <div
    class="message-bubble"
    v-for="m in messages"
    :key="m.messageId"
    :class="m.senderType === 'agent' ?  'message-bubble.left': ' message-bubble.right'"
  >
    <div class="content">{{ m.content }}</div>
    <div class="time">{{ formatTime(m.createdAt) }}</div>
    </div>
    <div class="send-bar">
      <input
        v-model="content"
        @keyup.enter="send"
        placeholder="輸入訊息..."
      />
      <button @click="send" :disabled="!content.trim()">送出</button>
    </div>
  </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import axios from 'axios'


const api = axios.create({
  baseURL: 'https://localhost:7131/api/ChatRoom',
  withCredentials: false,
})

const content = ref('')
const selectedId = ref(1) // ✅ 預設一個固定聊天室 ID，如果有需要可改
const messages = ref<any[]>([])

async function loadMessages() {
  const { data } = await api.get("/Index", {
    params: { chatRoomId: selectedId.value }
  })

  messages.value = data.messages ?? []
}

function formatTime(ts?: string | null) {
  if (!ts) return ""
  try {
    const d = new Date(ts)
    return d.toLocaleString()
  } catch {
    return String(ts)
  }
}

async function send() {
  if (!selectedId.value || !content.value.trim()) return

  await api.post('/SendMessage', null, {
    params: { chatRoomId: selectedId.value, content: content.value.trim() },
  })

  content.value = ''
  await loadMessages()
}

import { onMounted } from 'vue'


//初始化  若沒此 就會上次資料留存
onMounted(() => {
  loadMessages()
})
</script>

<style scoped>

.message-bubble.left {
  background: #f0f0f0;
  align-self: flex-start;
}
.message-bubble.right {
  background: #dcf8c6;
  align-self: flex-end;
}
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
  background: #f5f5f5;
  padding: 8px 12px;
  border-radius: 8px;
  margin-bottom: 8px;
  max-width: 80%;
}

.message-bubble .time {
  font-size: 10px;
  color: #888;
  text-align: right;
  margin-top: 4px;
}


</style>
