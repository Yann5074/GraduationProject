<!-- src/components/ChatWidget.vue -->
<template>
  <div>
    <!-- 漂浮的開關按鈕 -->
    <div class="floating-button" @click="toggleChat">
      💬
    </div>

    <!-- 彈出聊天室 -->
    <div v-if="showChat" class="chat-popup">
      <ChatRoom />
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import axios from 'axios'
import ChatRoom from "@/components/Chat/ChatRoom.vue"

const showChat = ref(false)
const API_BASE = 'https://localhost:7131'
const LOGIN_URL = '/account/login'  // 組員做好的登入頁路徑

const api = axios.create({
  baseURL: API_BASE,
  withCredentials: true, // 跨域請求時攜帶 Cookie
})






function toggleChat() {
  showChat.value = !showChat.value
}
</script>

<style scoped>
.floating-button {
  position: fixed;
  bottom: 24px;
  right: 24px;
  width: 60px;
  height: 60px;
  border-radius: 50%;
  background-color: #0055ff;
  color: white;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 24px;
  cursor: pointer;
  box-shadow: 0 4px 12px rgba(0,0,0,0.15);
  z-index: 1000;
}

.chat-popup {
  position: fixed;
  bottom: 100px;
  right: 24px;
  width: 420px;
  height: 600px;
  background: white;
  border-radius: 12px;
  box-shadow: 0 8px 24px rgba(0,0,0,0.2);
  z-index: 999;
  overflow: hidden;
}
</style>
